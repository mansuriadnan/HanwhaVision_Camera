using HanwhaClient.Application.Interfaces;
using HanwhaClient.Model.Dto;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace HanwhaClient.Application.Services
{
    public class IdracClientService : IIdracClientService
    {
        public string XAuthToken { get; private set; }
        public string SessionId { get; set; }

        public IdracClientService() { }

        public async Task<bool> IdracLoginAsync(string ipAddress, string username, string password)
        {
            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                        (msg, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var url = $"https://{ipAddress}/redfish/v1/Sessions";
            
            var payload = new
            {
                UserName = username,
                Password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(payload),
                Encoding.UTF8,
                "application/json");

            // API Call
            var response = await client.PostAsync(url, content);

            // Read response body
            var responseContent = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return false;
            }

            // Deserialize response
            var sessionObj = JsonSerializer.Deserialize<IdracLogin>(responseContent, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            // Read token from header
            if (response.Headers.TryGetValues("X-Auth-Token", out var tokenValues))
            {
                XAuthToken = tokenValues.FirstOrDefault();
            }
            SessionId = sessionObj.Id;
            return true;
        }

        public async Task<T?> GetIdracAsync<T>(string url, string ipAddress)
        {
            var dataUrl = "https://" + ipAddress + url;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (msg, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var request = new HttpRequestMessage(HttpMethod.Get, dataUrl);

            // Required headers
            request.Headers.Add("X-Auth-Token", XAuthToken);

            request.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request);

            var body = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"HTTP {(int)response.StatusCode}: {body}");
                return default;
            }

            var data = JsonSerializer.Deserialize<T>(body, new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
            return data;
        }

        public async Task<T?> PatchIdracAsync<T>(string url, object body, string ipAddress)
        {
            var dataUrl = "https://" + ipAddress + url;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (msg, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var jsonBody = JsonSerializer.Serialize(body);

            var request = new HttpRequestMessage(HttpMethod.Patch, dataUrl)
            {
                Content = new StringContent(
                    jsonBody,
                    Encoding.UTF8,
                    "application/json")
            };

            request.Headers.Add("X-Auth-Token", XAuthToken);

            request.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            // Throw exception if API failed
            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"iDRAC PATCH API failed. Status: {(int)response.StatusCode}, Response: {responseBody}");
            }

            // Handle empty response
            if (string.IsNullOrWhiteSpace(responseBody))
                return default;

            var data = JsonSerializer.Deserialize<T>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return data;
        }

        public async Task<T?> PostIdracAsync<T>(string url, object body, string ipAddress)
        {
            var dataUrl = "https://" + ipAddress + url;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (msg, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var jsonBody = JsonSerializer.Serialize(body);

            var request = new HttpRequestMessage(HttpMethod.Post, dataUrl)
            {
                Content = new StringContent(
                    jsonBody,
                    Encoding.UTF8,
                    "application/json")
            };

            // Required headers
            request.Headers.Add("X-Auth-Token", XAuthToken);

            request.Headers.Accept.Add(
                new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.SendAsync(request);

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                throw new Exception(
                    $"iDRAC POST API failed. Status: {(int)response.StatusCode}, Response: {responseBody}");
            }

            // If API returns empty response
            if (string.IsNullOrWhiteSpace(responseBody))
                return default;

            var data = JsonSerializer.Deserialize<T>(
                responseBody,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

            return data;
        }
        public async Task<bool> DeleteIdracAsync(string url, string ipAddress)
        {
            var dataUrl = "https://" + ipAddress + url;

            var handler = new HttpClientHandler
            {
                ServerCertificateCustomValidationCallback =
                    (msg, cert, chain, errors) => true
            };

            using var client = new HttpClient(handler);

            var request = new HttpRequestMessage(
                HttpMethod.Delete,
                dataUrl);

            request.Headers.Add(
                "X-Auth-Token",
                XAuthToken);

            request.Headers.Accept.Add(
                new MediaTypeWithQualityHeaderValue(
                    "application/json"));

            var response =
                await client.SendAsync(request);

            var responseBody =
                await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine(
                    $"HTTP {(int)response.StatusCode}: {responseBody}");

                return false;
            }

            return true;
        }

        public async Task<bool> IdracLogoutAsync(string ipAddress, string? sessionId = null)
        {
            var targetSessionId = sessionId ?? SessionId;
            if (string.IsNullOrEmpty(targetSessionId))
            {
                return false;
            }

            var url = $"/redfish/v1/SessionService/Sessions/{targetSessionId}";
            var result = await DeleteIdracAsync(url, ipAddress);

            if (result && targetSessionId == SessionId)
            {
                SessionId = null;
                XAuthToken = null;
            }

            return result;
        }
    }
}
