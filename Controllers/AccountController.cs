using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tasklist.Models;
using tasklist.Services;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;

namespace tasklist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountController : ControllerBase
    {

        private readonly LoginCredentialsService _credentialsService;
        private readonly PinterestService _pinterestService;
        public AccountController(LoginCredentialsService credentialsService, PinterestService pinterestService)
        {
            _credentialsService = credentialsService;
            _pinterestService = pinterestService;
        }

        [HttpGet("Pinterest/Check", Name = "PinterestCheck")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<string>> PinterestCheckAsync()
        {
            if (await _pinterestService.CheckAndUpdateCredentialsAsync())
                return "";
            return $"https://www.pinterest.com/oauth/?client_id={Settings.Pinterest_ID}&redirect_uri=http://{Request.Host}/{Settings.Pinterest_Redirect_URI}&response_type=code&scope={Settings.Pinterest_Scope}";
        }

        [HttpGet("PinterestOauth/{code}", Name = "PinterestOauth")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> PinterestOauthAsync(string code)
        {
            var response = await _pinterestService.Authenticate(code, Request.Host);

            if(response)
                return Ok();
            return BadRequest();
        }

        [HttpPost("Create", Name = "Create")]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult> Create(Account account)
        {
            var trimmedEmail = account.Email.Trim();

            if (trimmedEmail.EndsWith("."))
            {
                return BadRequest();
            }
            try
            {
                var addr = new System.Net.Mail.MailAddress(trimmedEmail);
                if(addr.Address != trimmedEmail)
                    return BadRequest();
            }
            catch
            {
                return BadRequest();
            }
            if (account.Role != "owner" && account.Role != "manager" && account.Role != "admin")
            {
                return BadRequest();
            }

            var password = UtilManager.RandString(10);

            var authentikCreated = await CreateAuthentikUserAsync(trimmedEmail, account.Name, password, account.Role);
            if (!authentikCreated)
                return StatusCode(500, "Failed to create user in Authentik");

            if (_credentialsService.Create(new LoginCredentials(trimmedEmail, UtilManager.EncryptPassword(password), account.Role, account.Name))) {
                
                var messageSubject = "Charter of Turin Monitor Credentials / Moniteur de la Charte de Turin Accréditation";
                var messageBody = $"[EN]\nYour credentials for the Charter of Turin Monitor platform are:\nUsername: {trimmedEmail}\nPassword: {password}\n\n" +
                    $"You can access it with the following link: {Settings.Platform_URL} \n\n" +
                    $"----------------------------------------------------------------------------\n" +
                    $"[FR]\nVotre accréditation pour la plateforme du Moniteur de la Charte de Turin est la suivante: \nNom d'utilisateur: {trimmedEmail}\n Mot de passe: {password}\n\n"+
                    $"Vous pouvez y acceder avec le lien suivant: {Settings.Platform_URL}";


                if (UtilManager.SendEmail(trimmedEmail, messageSubject, messageBody)) {
                    //Blockchain
                    var blockchainApiUrl = Settings.Blockchain_API_URL;
                    if (!string.IsNullOrEmpty(blockchainApiUrl))
                    {
                        using (HttpClient client = new HttpClient())
                        {
                            string[] accountName = account.Name.Split(' ');
                            var formData = new
                            {
                                email = trimmedEmail,
                                password = password,
                                orgname = "Org1",
                                firstname = accountName[0],
                                surname = accountName.Length > 1 ? accountName[1] : "",
                                country = "Portugal"
                            };
                            string jsonContent = JsonConvert.SerializeObject(formData);
                            StringContent content = new StringContent(jsonContent, System.Text.Encoding.UTF8, "application/json");
                            try {
                                HttpResponseMessage response = await client.PostAsync(blockchainApiUrl, content);
                            }
                            catch (Exception ex){
                                Console.WriteLine("Exception: " + ex.Message);
                            }
                        }
                    }
                    //Blockchain-End
                    return Ok();
                }
                return BadRequest();
            }
            return Conflict();
        }

        private static async Task<bool> CreateAuthentikUserAsync(string email, string name, string password, string group)
        {
            try
            {
                string token = await GetAuthentikApiTokenAsync();

                using var client = new HttpClient();
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

                var userData = new
                {
                    username = email,
                    email = email,
                    name = name,
                    is_active = true,
                    groups = Array.Empty<string>()
                };

                var userJson = JsonConvert.SerializeObject(userData);
                var userContent = new StringContent(userJson, Encoding.UTF8, "application/json");

                var baseUrl = Settings.AuthentikTokenUrl.Replace("/application/o/token/", "");
                var response = await client.PostAsync($"{baseUrl}/api/v3/core/users/", userContent);

                if (!response.IsSuccessStatusCode)
                {
                    var error = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to create Authentik user: {response.StatusCode} - {error}");
                    return false;
                }

                var responseBody = JsonConvert.DeserializeObject<dynamic>(await response.Content.ReadAsStringAsync());
                int userId = responseBody.pk;

                var passwordData = new { password = password };
                var passwordJson = JsonConvert.SerializeObject(passwordData);
                var passwordContent = new StringContent(passwordJson, Encoding.UTF8, "application/json");
                await client.PostAsync($"{baseUrl}/api/v3/core/users/{userId}/set_password/", passwordContent);

                var groupsResponse = await client.GetAsync($"{baseUrl}/api/v3/core/groups/?name={Uri.EscapeDataString(group)}");
                if (groupsResponse.IsSuccessStatusCode)
                {
                    var groupsBody = JsonConvert.DeserializeObject<dynamic>(await groupsResponse.Content.ReadAsStringAsync());
                    if (groupsBody.results.Count > 0)
                    {
                        string groupPk = groupsBody.results[0].pk;
                        List<int> existingUsers = JsonConvert.DeserializeObject<List<int>>(
                            JsonConvert.SerializeObject(groupsBody.results[0].users));
                        existingUsers.Add(userId);

                        var groupUpdate = new { users = existingUsers };
                        var groupJson = JsonConvert.SerializeObject(groupUpdate);
                        var groupContent = new StringContent(groupJson, Encoding.UTF8, "application/json");
                        await client.PatchAsync($"{baseUrl}/api/v3/core/groups/{groupPk}/", groupContent);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error creating Authentik user: {ex.Message}");
                return false;
            }
        }

        private static async Task<string> GetAuthentikApiTokenAsync()
        {
            using var client = new HttpClient();
            var authBytes = Encoding.UTF8.GetBytes($"{Settings.Authentik_Client_ID}:{Settings.Authentik_Client_Secret}");
            client.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Basic", Convert.ToBase64String(authBytes));

            var requestData = new List<KeyValuePair<string, string>>
            {
                new("grant_type", "client_credentials"),
                new("scope", "openid profile email")
            };

            var response = await client.PostAsync(Settings.AuthentikTokenUrl, new FormUrlEncodedContent(requestData));
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var doc = System.Text.Json.JsonDocument.Parse(json);
            return doc.RootElement.GetProperty("access_token").GetString();
        }

        [HttpPost("Details", Name = "ChangeDetails")]
        [Authorize]
        public ActionResult ChangeDetails(Details detailsForm)
        {
            string email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            _credentialsService.ChangeDetails(email, detailsForm);

            return Ok();
        }

        [HttpGet]
        [Authorize(Roles = "admin")]
        public ActionResult<IEnumerable<Account>> Get() =>
            _credentialsService.Get();

        [HttpGet("Self", Name = "GetSelf")]
        [Authorize]
        public ActionResult<Account> GetSelf()
        {
            string email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            string role = User.FindFirst(c => c.Type == ClaimTypes.Role)?.Value;
            var account = _credentialsService.GetAccount(email);
            if (account == null)
                return NotFound();
            if (role != account.Role)
                return null;
            return account;
        }

        [HttpGet("Details", Name = "GetDetails")]
        [Authorize]
        public ActionResult<Details> GetDetails()
        {
            string email = User.FindFirst(c => c.Type == ClaimTypes.Email)?.Value;
            return _credentialsService.GetDetails(email);
        }

        [HttpPost("Delete", Name = "Delete")]
        [Authorize(Roles = "admin")]
        public ActionResult Delete(Account account)
        {
            if (_credentialsService.Delete(account))
                return NoContent();
            return BadRequest();
        }
    }
}
