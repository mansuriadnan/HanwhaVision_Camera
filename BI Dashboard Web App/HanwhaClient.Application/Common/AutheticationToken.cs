using HanwhaClient.Model.Auth;
using HanwhaClient.Model.Common;
using HanwhaClient.Model.DbEntities;
using Newtonsoft.Json;
using PuppeteerSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HanwhaClient.Application.Utilities
{
    public static class AutheticationToken
    {
        //public async static Task<StandardAPIResponse<FloorZoneDataAccessPermissionForLinkedServer>> GetFloorZoneLinkedServer(HttpClient httpClient, string username, string password)
        //{
        //    try
        //    {
        //        var loginRequest = new
        //        {
        //            Username = username,
        //            Password = password
        //        };

        //        CancellationToken cancellationToken = default(CancellationToken);

        //        var content = new StringContent(
        //            System.Text.Json.JsonSerializer.Serialize(loginRequest),
        //            Encoding.UTF8,
        //            "application/json");

        //        using var loginResponse = await httpClient.PostAsync(
        //  "api/Auth/GetFloorZoneDataAccessPermission",
        //        content,
        //  cancellationToken);

        //        if (!loginResponse.IsSuccessStatusCode)
        //            return null;

        //        loginResponse.EnsureSuccessStatusCode();

        //        var jsonString = await loginResponse.Content.ReadAsStringAsync();
        //        var loginResult = JsonConvert.DeserializeObject<StandardAPIResponse<FloorZoneDataAccessPermissionForLinkedServer>>(jsonString);
        //        return loginResult;
        //    }
        //    catch (Exception ex)
        //    {
        //        // Log error if needed
        //        // _logger.LogError(ex, "Error during authentication");
        //        throw new UnauthorizedAccessException("Authentication failed", ex);
        //    }
        //}

        public async static Task<StandardAPIResponse<FloorZoneDataAccessPermissionForLinkedServer>?> GetFloorZoneLinkedServer(
        HttpClient httpClient,
        string username,
        string password,
        CancellationToken cancellationToken)
        {
            try
            {
                var loginRequest = new
                {
                    Username = username,
                    Password = password
                };

                var content = new StringContent(
                    JsonConvert.SerializeObject(loginRequest),
                    Encoding.UTF8,
                    "application/json");

                using var response = await httpClient.PostAsync(
                    "api/Auth/GetFloorZoneDataAccessPermission",
                    content,
                    cancellationToken);

                if (!response.IsSuccessStatusCode)
                    return null;

                var json = await response.Content.ReadAsStringAsync(cancellationToken);

                return JsonConvert.DeserializeObject<
                    StandardAPIResponse<FloorZoneDataAccessPermissionForLinkedServer>>(json);
            }
            catch (Exception)
            {
                // IMPORTANT: swallow failure here → do NOT break app flow
                return null;
            }
        }

    }
}
