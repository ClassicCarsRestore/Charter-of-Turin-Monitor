using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace tasklist.Models
{
	public class LoginCredentials
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("email")]
        public string Email { get; set; }

        [BsonElement("password")]
        public string Password { get; set; }

        [BsonElement("role")]
        public string Role { get; set; }

        [BsonElement("name")]
        public string Name { get; set; }

        [BsonElement("address")]
        public string Address { get; set; }

        [BsonElement("phone")]
        public string Phone { get; set; }

        public LoginCredentials(string email, string password, string role, string name)
        {
            Email = email;
            Password = password;
            Role = role;
            Name = name;
            Address = null;
            Phone = null;
        }
    }
}
