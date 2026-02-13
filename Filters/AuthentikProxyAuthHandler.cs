using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace tasklist.Filters
{
    public class AuthentikProxyAuthHandler
        : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public AuthentikProxyAuthHandler(
            IOptionsMonitor<AuthenticationSchemeOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder)
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var email = Request.Headers["X-Authentik-Email"].FirstOrDefault();
            if (string.IsNullOrEmpty(email))
                return Task.FromResult(AuthenticateResult.NoResult());

            var username = Request.Headers["X-Authentik-Username"].FirstOrDefault() ?? email;
            var name = Request.Headers["X-Authentik-Name"].FirstOrDefault() ?? username;
            var groups = Request.Headers["X-Authentik-Groups"].FirstOrDefault() ?? "";

            var claims = new List<Claim>
            {
                new(ClaimTypes.Email, email),
                new(ClaimTypes.Name, name),
                new("username", username),
            };

            foreach (var group in groups.Split('|',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, group));
            }

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
