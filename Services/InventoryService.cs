using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
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
            using var loggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddConsole();
            });
            _logger = loggerFactory.CreateLogger<InventoryService>();
        }

        public async Task<bool> NotifyProjectCreatedAsync(InventoryProjectDTO projectData)
        {
            try 
            {
                string token = await GetAccessTokenAsync();

                var request = new HttpRequestMessage(HttpMethod.Post, Settings.Inventory_API_URL);
                request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(projectData);

                var response = await _client.SendAsync(request);
                
                if (!response.IsSuccessStatusCode)
                {
                    _logger.LogError($"Inventory API returned {response.StatusCode}");
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to notify inventory project creation.");
                return false;
            }
        }

        private static async Task<string> GetAccessTokenAsync()
        {
            var clientId = Settings.Authentik_Client_ID;
            var clientSecret = Settings.Authentik_Client_Secret; 
            
            var authBytes = Encoding.UTF8.GetBytes($"{clientId}:{clientSecret}");
            var base64Auth = Convert.ToBase64String(authBytes);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post, Settings.AuthentikTokenUrl);
            
            requestMessage.Headers.Authorization = new AuthenticationHeaderValue("Basic", base64Auth);

            var requestData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("scope", "openid profile email")
            };

            requestMessage.Content = new FormUrlEncodedContent(requestData);

            var response = await _client.SendAsync(requestMessage);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError($"Failed to get token from Authentik: {response.StatusCode}, {errorContent}");
                throw new Exception($"Could not authenticate with Authentik. Status: {response.StatusCode}");
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