using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using tasklist.Models;

namespace tasklist.Services
{
    public class InventoryService
    {
        private static readonly HttpClient _client;
        private static readonly ILogger<InventoryService> _logger;

        static InventoryService()
        {
            _client = new HttpClient();
            _logger = LoggerFactory.Create(builder => 
            {
                builder.AddConsole();
            }).CreateLogger<InventoryService>();
        }

        public async Task<bool> NotifyProjectCreatedAsync(InventoryProjectDTO projectData)
        {
            string token = await GetAccessTokenAsync();

            var request = new HttpRequestMessage(HttpMethod.Post, Settings.Inventory_API_URL);
            
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

            request.Content = JsonContent.Create(projectData);

            var response = await _client.SendAsync(request);

            return response.IsSuccessStatusCode;
        }

        private static async Task<string> GetAccessTokenAsync()
        {
        var requestData = new List<KeyValuePair<string, string>>
        {
            new("grant_type", "client_credentials"),
            new("client_id", Settings.Inventory_Client_ID),
            new("username", Settings.Service_Account_Username),
            new("password", Settings.Service_Account_Password),
            new("scope", "openid profile email")
        };

        var requestContent = new FormUrlEncodedContent(requestData);

        var response = await _client.PostAsync(Settings.AuthentikTokenUrl, requestContent);

        if (!response.IsSuccessStatusCode)
        {
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError($"Failed to get token: {response.StatusCode}, {errorContent}");
            throw new Exception("Could not authenticate with Authentik.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(json);
        
        if (document.RootElement.TryGetProperty("access_token", out var accessToken))
        {
            return accessToken.GetString();
        }

        throw new Exception("Token response did not contain an access_token.");
        }
    }

}