using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace tasklist.Models
{
	public class LoginCredentialsResponse
	{
        public string Role { get; set; }

        public string Token { get; set; }

        public LoginCredentialsResponse(string role, string token)
        {
            Role = role;
            Token = token;
        }
    }
}
