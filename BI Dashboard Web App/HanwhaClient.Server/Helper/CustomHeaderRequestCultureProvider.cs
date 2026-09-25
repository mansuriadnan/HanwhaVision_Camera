using Microsoft.AspNetCore.Localization;

namespace HanwhaClient.Server.Helper
{
    public class CustomHeaderRequestCultureProvider : RequestCultureProvider
    {
        public override Task<ProviderCultureResult?> DetermineProviderCultureResult(HttpContext httpContext)
        {
            if (httpContext.Request.Headers.TryGetValue("Accept-Language", out var language))
            {
                var culture = language.ToString().Split(',')[0].Trim();
                if (!string.IsNullOrWhiteSpace(culture))
                {
                    return Task.FromResult<ProviderCultureResult?>(new ProviderCultureResult(culture));
                }
            }
            return Task.FromResult<ProviderCultureResult?>(null);
        }
    }
}
