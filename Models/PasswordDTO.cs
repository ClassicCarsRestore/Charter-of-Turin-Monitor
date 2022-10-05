using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace tasklist.Models
{
	public class PasswordDTO
	{
        public string OldPassword { get; set; }
        
        public string Password { get; set; }
    }
}
