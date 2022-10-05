using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace tasklist.Models
{
	public class Account
	{
        public string Email { get; set; }

        public string Role { get; set; }

        public string Name { get; set; }

        public Account(string email, string role, string name)
        {
            Email = email;
            Role = role;
            Name = name;
        }
    }
}
