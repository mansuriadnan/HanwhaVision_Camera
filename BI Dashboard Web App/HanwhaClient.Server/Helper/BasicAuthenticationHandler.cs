using HanwhaClient.Application.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;

namespace HanwhaClient.Server.Helper
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly IConfiguration _configuration;
        private readonly IExposeApiUserService _exposeApiUserService;

        public BasicAuthenticationHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock,
            IConfiguration configuration,
            IExposeApiUserService exposeApiUserService)
            : base(options, logger, encoder, clock)
        {
            _configuration = configuration;
            _exposeApiUserService = exposeApiUserService;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");

            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
                var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

                var username = credentials[0];
                var password = credentials[1];

                var response = await _exposeApiUserService.GetUserByUsernameAsync(username);
                if (response == null || !response.IsSuccess || response.Data == null)
                    return AuthenticateResult.Fail("Invalid Username or Password");

                var user = response.Data;

                if (!string.Equals(user.Password, password, StringComparison.Ordinal) || !user.IsActive)
                    return AuthenticateResult.Fail("Invalid Username or Password");

                //var claims = new[]
                //{
                //    new Claim(ClaimTypes.Name, username),
                //    new Claim(ClaimTypes.Role, "Admin")
                //};
                var claims = new[]
                {
                    new Claim(ClaimTypes.Name, user.Username),
                    new Claim(ClaimTypes.Role, user.Role)
                };

                var identity = new ClaimsIdentity(claims, Scheme.Name);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);

                return AuthenticateResult.Success(ticket);
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }
        }
    }

    // public class BasicAuthUser
    // {
    //     public string Username { get; set; }
    //     public string Password { get; set; }
    //     public string Role { get; set; }
    // }
}
