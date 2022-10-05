using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System.Collections.Generic;

namespace tasklist.Models
{
	public class Details
	{
        public string Address { get; set; }
        
        public string Phone { get; set; }

        public string Name { get; set; }

        public Details(string address, string phone, string name)
        {
            Address = address;
            Phone = phone;
            Name = name;
        }
    }

}
