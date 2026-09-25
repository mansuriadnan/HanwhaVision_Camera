using HanwhaClient.Application.Interfaces;
using HanwhaClient.Model.DbEntities;
using HanwhaClient.Model.SSM;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Services
{    
    public class SsmClientService : ISsmClientService
    {   
        private HttpClient _http;
        private readonly ILogger<SsmClientService> _logger;
        private readonly IServiceProvider _serviceProvider;

        public string? SessionId { get; private set; }
        public string? LoginPassword { get; private set; }   // plain-text, used for HMAC signing
        public SsmClientService(ILogger<SsmClientService> logger,
            IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }


        // ── Login flow ────────────────────────────────────────────
        public async Task<bool> LoginAsync(string baseAddress, string id, string password)
        {
            try
            {
                // Skip SSL cert validation for self-signed LAN certs
                var handler = new HttpClientHandler
                {
                    ServerCertificateCustomValidationCallback = (_, _, _, _) => true
                };
                _http = new HttpClient(handler) { BaseAddress = new Uri(baseAddress) };
                // 1. Get public key
                var pubKeyResponse = await _http.GetAsync("/V1/report/status");

                if (!pubKeyResponse.Headers.TryGetValues("PublicKey", out var values))
                {
                    Console.WriteLine("PublicKey not found in headers");
                    return false;
                }

                string publicKeyBase64 = values.FirstOrDefault();

                if (string.IsNullOrEmpty(publicKeyBase64))
                {
                    Console.WriteLine("PublicKey header is empty");
                    return false;
                }

                Console.WriteLine($"[PublicKey] {publicKeyBase64.Substring(0, 50)}...");
                if (string.IsNullOrEmpty(publicKeyBase64))
                {
                    Console.WriteLine("Could not parse PublicKey from /V1/report/status");
                    return false;
                }

                // 2. RSA-encrypt id and password with the public key (PKCS#1 v1.5)
                string encId = RsaEncrypt(publicKeyBase64, id);
                string encPwd = RsaEncrypt(publicKeyBase64, password);

                // 3. POST to /V1/Session
                var loginPayload = new
                {
                    id = encId,
                    password = encPwd,
                    ip = "0",
                    service = 1,
                    clientVersion = "2.18.00"
                };

                var content = new StringContent(JsonSerializer.Serialize(loginPayload), Encoding.UTF8, "application/json");
                var response = await _http.PostAsync("/V1/Session", content);
                string body = await response.Content.ReadAsStringAsync();

                Console.WriteLine($"  [Login response] {body}");

                if (!response.IsSuccessStatusCode)
                {
                    Console.WriteLine($"Login HTTP {(int)response.StatusCode}");
                    return false;
                }

                var loginResult = JsonSerializer.Deserialize<SSMLoginResponse>(body,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (loginResult?.SessionId == null)
                {
                    Console.WriteLine("SessionId missing in login response.");
                    return false;
                }

                SessionId = loginResult.SessionId;
                LoginPassword = password;           // keep plain-text for HMAC signing
                return true;
            }
            catch (Exception ex)
            {
                // Log exception here instead of sending it to Global Exception Handler
                _logger.LogError(
                    ex,
                    "Error occurred while logging in to SSM. BaseAddress: {BaseAddress}",
                    baseAddress);
                var exceptionLog = new ExceptionLog();
                using (var scope = _serviceProvider.CreateScope())
                {
                    var exceptionLogService = scope.ServiceProvider.GetRequiredService<IExceptionLogService>();
                    exceptionLog.ExceptionMessage = $"Error occurred while logging in to SSM. BaseAddress {baseAddress}: {ex.Message}";
                    exceptionLog.StackTrace = ex.StackTrace;
                    exceptionLog.ExceptionType = ex.GetType().Name;
                    exceptionLog.LoggedAt = DateTime.Now;
                    exceptionLog.RequestPath = "SSM Server login exception in SsmClientService";
                    exceptionLog.ResponseTime = DateTime.Now;
                    exceptionLog.IsSuccess = false;
                    await exceptionLogService.SaveExceptionLogAsync(exceptionLog);
                }

                return false;
            }
            
        }

        // ── Authenticated GET helper ──────────────────────────────
        public async Task<T?>   GetAsync<T>(string path)
        {
            if (SessionId == null || LoginPassword == null)
                throw new InvalidOperationException("Not logged in.");

            long xSsmDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string auth = GenerateAuth(path, xSsmDate.ToString(), SessionId, LoginPassword);

            var request = new HttpRequestMessage(HttpMethod.Get, path);
            request.Headers.Add("x-ssm-date", xSsmDate.ToString());
            request.Headers.Add("Authorization", $"TSM {auth}");

            //Console.WriteLine($"  → GET {path}");
            //Console.WriteLine($"     x-ssm-date   : {xSsmDate}");
            //Console.WriteLine($"     Authorization: TSM {auth[..Math.Min(40, auth.Length)]}...");

            var response = await _http.SendAsync(request);
            string body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP {(int)response.StatusCode}: {body}");
                return default;
            }

            return JsonSerializer.Deserialize<T>(body,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task<bool> LogoutAsync(string path)
        {
            if (SessionId == null || LoginPassword == null)
                throw new InvalidOperationException("Not logged in.");

            long xSsmDate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            string auth = GenerateAuth(path, xSsmDate.ToString(), SessionId, LoginPassword);

            var request = new HttpRequestMessage(HttpMethod.Delete, path);
            request.Headers.Add("x-ssm-date", xSsmDate.ToString());
            request.Headers.Add("Authorization", $"TSM {auth}");

            Console.WriteLine($"  → GET {path}");
            Console.WriteLine($"     x-ssm-date   : {xSsmDate}");
            Console.WriteLine($"     Authorization: TSM {auth[..Math.Min(40, auth.Length)]}...");

            var response = await _http.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        // ── Auth signature generation ─────────────────────────────
        // Logic (from guide):
        //  1. Make string  → "{URI}:{x-ssm-date}"
        //  2. Encode loginPassword as UTF-8  (this is the HMAC key)
        //  3. HMAC-SHA256( key=step2, data=step1 )
        //  4. Encode SessionId as UTF-8
        //  5. Base64( "{SessionId}:{hmac-bytes}" )     

        public static string GenerateAuth(string uri, string xSsmDate, string sessionId, string loginPassword)
        {
            // Step 1
            string message = $"{uri}:{xSsmDate}";
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);

            // Step 2
            byte[] keyBytes = Encoding.UTF8.GetBytes(loginPassword);

            // Step 3 → HMAC
            byte[] hashBytes;
            using (var hmac = new HMACSHA256(keyBytes))
            {
                hashBytes = hmac.ComputeHash(messageBytes);
            }

            // IMPORTANT → Convert to HEX (NOT Base64)
            string hex = BitConverter.ToString(hashBytes)
                .Replace("-", "")
                .ToLower();

            // Step 4 + 5
            string final = $"{sessionId}:{hex}";
            return Convert.ToBase64String(Encoding.UTF8.GetBytes(final));
        }

        // ── RSA encrypt with PKCS#1 v1.5 ─────────────────────────
        static string RsaEncrypt(string base64PublicKey, string plainText)
        {
            byte[] keyBytes = Convert.FromBase64String(base64PublicKey);

            using var rsa = RSA.Create();
            rsa.ImportSubjectPublicKeyInfo(keyBytes, out _);

            byte[] data = Encoding.UTF8.GetBytes(plainText);
            byte[] encrypted = rsa.Encrypt(data, RSAEncryptionPadding.Pkcs1);
            return Convert.ToBase64String(encrypted);
        }

        
    }

}
