using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace tasklist.Models
{
	public class LoginCredentialsDTO
	{
        public string Email { get; set; }
        
        public string Password { get; set; }
    }
}
