using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace tasklist.Models
{
    public class Pinterest
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("accessToken")]
        public string AccessToken { get; set; }

        [BsonElement("expireDate")]
        public DateTime ExpireDate { get; set; }

        [BsonElement("refreshToken")]
        public string RefreshToken { get; set; }

        [BsonElement("refreshTokenExpireDate")]
        public DateTime RefreshTokenExpireDate { get; set; }

        public Pinterest(PinterestOauth auth)
		{
            this.AccessToken = auth.access_token;
            this.ExpireDate = DateTime.UtcNow.AddSeconds(auth.expires_in);
            this.RefreshToken = auth.refresh_token;
            this.RefreshTokenExpireDate = DateTime.UtcNow.AddSeconds(auth.refresh_token_expires_in);
        }

        public Pinterest(Pinterest p, PinterestOauth auth)
		{
            this.AccessToken = auth.access_token;
            this.ExpireDate = DateTime.UtcNow.AddSeconds(auth.expires_in);
            this.RefreshToken = p.RefreshToken;
            this.RefreshTokenExpireDate = p.RefreshTokenExpireDate;
        }
    }
}
