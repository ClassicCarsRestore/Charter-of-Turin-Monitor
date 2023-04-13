using Microsoft.AspNetCore.Http;
using MongoDB.Driver;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using tasklist.Models;

namespace tasklist.Services
{
    public class PinterestService
    {
        private static readonly HttpClient client = new HttpClient();

        private readonly IMongoCollection<Pinterest> _pinterestCreds;

        private readonly string creds;

        public PinterestService(IPinterestDatabaseSettings settings)
        {
            var c = new MongoClient(settings.ConnectionString);
            var database = c.GetDatabase(settings.DatabaseName);

            _pinterestCreds = database.GetCollection<Pinterest>(settings.PinterestCollectionName);

            creds = Convert.ToBase64String(Encoding.UTF8.GetBytes($"{Settings.Pinterest_ID}:{Settings.Pinterest_Secret}"));

            var p = _pinterestCreds.Find(p => true).FirstOrDefault();
            if (p != null)
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", p.AccessToken);
        }

        public async Task<bool> Authenticate(string code, HostString host)
        {
            var request = new HttpRequestMessage(new HttpMethod("POST"), Settings.Pinterest_API_URL + "oauth/token");
            request.Headers.TryAddWithoutValidation("Authorization", "Basic " + creds);

            var contentList = new List<string>();
            contentList.Add($"grant_type={Uri.EscapeDataString("authorization_code")}");
            contentList.Add($"code={Uri.EscapeDataString(code)}");
            contentList.Add($"redirect_uri={Uri.EscapeDataString($"http://{host}/{Settings.Pinterest_Redirect_URI}")}");
            request.Content = new StringContent(string.Join("&", contentList));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return false;
            var auth = await response.Content.ReadAsAsync<PinterestOauth>();
            
            var p = new Pinterest(auth);

            _pinterestCreds.DeleteMany(p => true);
            _pinterestCreds.InsertOne(p);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", p.AccessToken);

            return true;
        }

        public async Task<bool> CheckAndUpdateCredentialsAsync()
        {
            var pint = _pinterestCreds.Find(p => true).FirstOrDefault();
            if (pint == null)
                return false;
            var tomorrow = DateTime.UtcNow.AddDays(1);
            if (pint.ExpireDate > tomorrow)
                return true;
            if (pint.RefreshTokenExpireDate < tomorrow)
                return false;

            //refresh token
            var request = new HttpRequestMessage(new HttpMethod("POST"), Settings.Pinterest_API_URL + "oauth/token");
            request.Headers.TryAddWithoutValidation("Authorization", "Basic " + creds);

            var contentList = new List<string>();
            contentList.Add($"grant_type={Uri.EscapeDataString("refresh_token")}");
            contentList.Add($"refresh_token={Uri.EscapeDataString(pint.RefreshToken)}");
            contentList.Add($"scope={Uri.EscapeDataString(Settings.Pinterest_Scope)}");
            request.Content = new StringContent(string.Join("&", contentList));
            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/x-www-form-urlencoded");

            var response = await client.SendAsync(request);

            if (!response.IsSuccessStatusCode)
                return false;

            var auth = await response.Content.ReadAsAsync<PinterestOauth>();

            var p = new Pinterest(pint, auth);

            _pinterestCreds.DeleteMany(p => true);
            _pinterestCreds.InsertOne(p);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", p.AccessToken);

            return true;
        }

        public async Task<HttpResponseMessage> CreatePin(string media, string boardId, string boardSectionId)
        {
            string[] sep = { ":", ";", "," };
            string[] data = media.Split(sep, StringSplitOptions.RemoveEmptyEntries);

            object mediaSource = null;
            var mediaType = data[1].Split("/")[0];

            if (mediaType == "image")
                mediaSource = new { source_type = "image_base64", content_type = data[1], data = data[3] };
            else if (mediaType == "video")
                mediaSource = await CreateVideo(Convert.FromBase64String(data[3]));

            StringContent content;
            if (boardSectionId != null)
                content = new StringContent(JsonConvert.SerializeObject(new
                {
                    board_id = boardId,
                    board_section_id = boardSectionId,
                    media_source = mediaSource
                }), Encoding.UTF8, "application/json");
            else
                content = new StringContent(JsonConvert.SerializeObject(new
                {
                    board_id = boardId,
                    media_source = mediaSource
                }), Encoding.UTF8, "application/json");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return await client.PostAsync(Settings.Pinterest_API_URL + "pins", content);
        }

        public async Task<object> CreateVideo(byte[] video)
        {
            var content = new StringContent(JsonConvert.SerializeObject(new { media_type = "video" }), Encoding.UTF8, "application/json");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            Media media = null;
            HttpResponseMessage response;
            var b = true;
            do
            {
                response = await client.PostAsync(Settings.Pinterest_API_URL + "media", content);
                try
                {
                    media = await response.Content.ReadAsAsync<Media>();
                    b = false;
                }
                catch
                {
                    System.Threading.Thread.Sleep(10000);
                }
            }
            while (b);


            using (var contentForm = new MultipartFormDataContent())
            {
                contentForm.Add(new StringContent(media.upload_parameters.x_amz_date), "x-amz-date");
                contentForm.Add(new StringContent(media.upload_parameters.x_amz_signature), "x-amz-signature");
                contentForm.Add(new StringContent(media.upload_parameters.x_amz_security_token), "x-amz-security-token");
                contentForm.Add(new StringContent(media.upload_parameters.x_amz_algorithm), "x-amz-algorithm");
                contentForm.Add(new StringContent(media.upload_parameters.key), "key");
                contentForm.Add(new StringContent(media.upload_parameters.policy), "policy");
                contentForm.Add(new StringContent(media.upload_parameters.x_amz_credential), "x-amz-credential");
                contentForm.Add(new StringContent(media.upload_parameters.Content_Type), "Content-Type");
                contentForm.Add(new ByteArrayContent(video), "file");
                using (var c = new HttpClient())
                    response = await c.PostAsync(media.upload_url, contentForm);
            }


            var status = "registered";
            for (var i = 0; i < 12; i++)
            {
                System.Threading.Thread.Sleep(5000);
                response = await client.GetAsync(Settings.Pinterest_API_URL + "media/" + media.media_id);
                var res = await response.Content.ReadAsAsync<MediaDetails>();
                status = res.status;
                if (status != "registered")
                    break;
            }
            while (status == "processing")
            {
                System.Threading.Thread.Sleep(5000);
                response = await client.GetAsync(Settings.Pinterest_API_URL + "media/" + media.media_id);
                var res = await response.Content.ReadAsAsync<MediaDetails>();
                status = res.status;
            }
            if (status != "succeeded")
            {
                return null;
            }

            return new { source_type = "video_id", cover_image_url = "http://194.210.120.34:5000/Play.jpg", media_id = media.media_id };
        }

        public Task<HttpResponseMessage> CreateBoard(ProjectFormDTO projectForm)
        {
            var projectName = projectForm.Make + " " + projectForm.Model + " " + projectForm.LicencePlate;
            if (projectName.Length > 41)
                projectName = projectName.Substring(0, 41);
            projectName += " " + projectForm.StartDate.ToString("yy/MM/dd");

            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            // remove unwanted characters
            projectName = Regex.Replace(Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-8").GetBytes(projectName)), "[^a-zA-Z0-9_~/ -]", string.Empty);

            var projectDescription = "Make: " + projectForm.Make + "\nModel: " + projectForm.Model + "\nYear of Manifacture: " + projectForm.Year + "\nLicence Plate: " + projectForm.LicencePlate + 
                "\nCountry: " + projectForm.Country + "\nChassis/Frame number: " + projectForm.ChassisNo + "\nEngine make and number: " + projectForm.EngineNo + "\nCreated on: " + projectForm.StartDate;
            
            var content = new StringContent(JsonConvert.SerializeObject(new { name = projectName, description = projectDescription, privacy = "SECRET" }), Encoding.UTF8, "application/json");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");

            return client.PostAsync(Settings.Pinterest_API_URL + "boards", content);
        }

        public Task<HttpResponseMessage> DeleteBoard(string boardId)
        {
            return client.DeleteAsync(Settings.Pinterest_API_URL + "boards/" + boardId);
        }

        public Task<HttpResponseMessage> CreateBoardSection(string boardId, string name)
        {
            EncodingProvider provider = CodePagesEncodingProvider.Instance;
            Encoding.RegisterProvider(provider);

            var sectionName = Regex.Replace(Encoding.UTF8.GetString(Encoding.GetEncoding("ISO-8859-8").GetBytes(name)), "[^a-zA-Z0-9_~ -]", string.Empty);
            var content = new StringContent(JsonConvert.SerializeObject(new { name = sectionName }), Encoding.UTF8, "application/json");
            content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json");
            
            return client.PostAsync(Settings.Pinterest_API_URL + "boards/" + boardId + "/sections", content);
        }

        public async Task<List<PinterestBoardSection>> GetBoardSections(string boardId)
        {
            List<PinterestBoardSection> sections = new();
            PinterestBoardSectionList list = null;
            do
            {
                var response = await client.GetAsync(Settings.Pinterest_API_URL + "boards/" + boardId + "/sections?page_size=100" + (list != null ? "&bookmark=" + list.Bookmark : ""));
                if (!response.IsSuccessStatusCode)
                    return null;
                list = await response.Content.ReadAsAsync<PinterestBoardSectionList>();
                sections.AddRange(list.Items);
            }
            while (list.Bookmark != null);
            return sections;
        }

        public async Task<List<PinterestPin>> GetPinsFromSection(string boardId, string boardSectionId)
        {
            List<PinterestPin> sections = new();
            PinterestPinList list = null;
            do
            {
                var response = await client.GetAsync(Settings.Pinterest_API_URL + "boards/" + boardId + "/sections/" + boardSectionId + "/pins?page_size=100" + (list != null ? "&bookmark=" + list.Bookmark : ""));
                if (!response.IsSuccessStatusCode)
                    return null;
                list = await response.Content.ReadAsAsync<PinterestPinList>();
                sections.AddRange(list.Items);
            }
            while (list.Bookmark != null);
            return sections;
        }

        public async Task<List<PinterestPin>> GetPinsFromBoard(string boardId)
        {
            List<PinterestPin> sections = new();
            PinterestPinList list = null;
            do
            {
                var response = await client.GetAsync(Settings.Pinterest_API_URL + "boards/" + boardId + "/pins?page_size=100" + (list != null ? "&bookmark=" + list.Bookmark : ""));
                if (!response.IsSuccessStatusCode)
                    return null;
                list = await response.Content.ReadAsAsync<PinterestPinList>();
                sections.AddRange(list.Items);
            }
            while (list.Bookmark != null);
            return sections;
        }

        //public async Task<PinterestPin> GetPinAsync(string id)
        //{
        //    var response = await client.GetAsync(Settings.Pinterest_API_URL + "pins/" + id);
        //    Console.WriteLine(Settings.Pinterest_API_URL + "pins/" + id);
        //    Console.WriteLine(await response.Content.ReadAsStringAsync());
        //    if (!response.IsSuccessStatusCode)
        //        return null;
        //    return await response.Content.ReadAsAsync<PinterestPin>();
        //}
    }
}
