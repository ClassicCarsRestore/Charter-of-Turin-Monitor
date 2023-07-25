using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using tasklist.Models;
using tasklist.Services;
using System.Collections.Generic;
using System.Threading.Tasks;

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

        [HttpPost("Login", Name = "Login")]
        public ActionResult<LoginCredentialsResponse> Login(LoginCredentialsDTO creds)
        {
            creds.Password = UtilManager.EncryptPassword(creds.Password);
            string role = _credentialsService.Login(creds);

            if (role == null)
                return Unauthorized();

            return new LoginCredentialsResponse(role, JwtManager.GenerateToken(creds.Email, role));
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
        public ActionResult Create(Account account)
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

            if (_credentialsService.Create(new LoginCredentials(trimmedEmail, UtilManager.EncryptPassword(password), account.Role, account.Name))) {
                var messageSubject = "Charter of Turin Monitor Credentials / Moniteur de la Charte de Turin Accréditation";
                var messageBody = $"[EN]\nYour credentials for the Charter of Turin Monitor platform are:\nUsername: {trimmedEmail}\nPassword: {password}\n\n" +
                    $"You can access it with the following link: http://194.210.120.34:5000/ \n\n" +
                    $"----------------------------------------------------------------------------\n" +
                    $"[FR]\nVotre accréditation pour la plateforme du Moniteur de la Charte de Turin est la suivante: \nNom d'utilisateur: {trimmedEmail}\n Mot de passe: {password}\n\n"+
                    $"Vous pouvez y acceder avec le lien suivant: http://194.210.120.34:5000/";


                if (UtilManager.SendEmail(trimmedEmail, messageSubject, messageBody))
                    return Ok();
                return BadRequest();
            }
            return Conflict();
        }

        [HttpPost("Password", Name = "ChangePassword")]
        [Authorize]
        public ActionResult ChangePassword(PasswordDTO passwordForm)
        {
            ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));

            var creds = new LoginCredentialsDTO
            {
                Email = claims.FindFirst(c => c.Type == ClaimTypes.Email).Value,
                Password = UtilManager.EncryptPassword(passwordForm.OldPassword)
            };

            if(_credentialsService.Login(creds) == null)
                return BadRequest();

            creds.Password = UtilManager.EncryptPassword(passwordForm.Password);
            _credentialsService.ChangePassword(creds);

            return Ok();
        }

        [HttpPost("Details", Name = "ChangeDetails")]
        [Authorize]
        public ActionResult ChangeDetails(Details detailsForm)
        {
            ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));
            _credentialsService.ChangeDetails(claims.FindFirst(c => c.Type == ClaimTypes.Email).Value, detailsForm);

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
            ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));
            string email = claims.FindFirst(c => c.Type == ClaimTypes.Email).Value;
            var account = _credentialsService.GetAccount(email);
            if(claims.FindFirst(c => c.Type == ClaimTypes.Role).Value != account.Role)
                return null;
            return account;
        }

        [HttpGet("Details", Name = "GetDetails")]
        [Authorize]
        public ActionResult<Details> GetDetails()
        {
            ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));
            return _credentialsService.GetDetails(claims.FindFirst(c => c.Type == ClaimTypes.Email).Value);
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
