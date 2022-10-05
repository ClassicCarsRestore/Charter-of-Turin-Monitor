using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tasklist.Models;

namespace tasklist.Services
{
	public class LoginCredentialsService
	{
        private readonly IMongoCollection<LoginCredentials> _credentials;

        public LoginCredentialsService(ILoginCredentialsDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _credentials = database.GetCollection<LoginCredentials>(settings.LoginCredentialsCollectionName);
        }

        public List<Account> Get() => 
            _credentials.Find(creds => true).ToList().ConvertAll<Account>(a => new Account(a.Email, a.Role, a.Name));

        public Account GetAccount(string email) => 
            _credentials.Find(creds => creds.Email == email).ToList().ConvertAll<Account>(a => new Account(a.Email, a.Role, a.Name)).FirstOrDefault();

        public string Login(LoginCredentialsDTO creds)
        {
            var res = _credentials.Find<LoginCredentials>(credentials => credentials.Email == creds.Email && credentials.Password == creds.Password).FirstOrDefault();
            if (res == null)
                return null;
            return res.Role;
        }

        public bool Create(LoginCredentials creds)
        {
            if(_credentials.Find(credentials => credentials.Email == creds.Email).CountDocuments() == 0) {
                _credentials.InsertOne(creds);
                return true; 
            }
            return false;
        }

        public void ChangePassword(LoginCredentialsDTO creds)
        {
            var update = Builders<LoginCredentials>.Update.Set(credentials => credentials.Password, creds.Password);
            _credentials.UpdateOne(credentials => credentials.Email == creds.Email, update);
        }

        public bool Delete(Account account)
        {
            if (_credentials.DeleteOne(credentials => credentials.Email == account.Email && credentials.Role == account.Role).DeletedCount == 0)
                return false;
            return true;
        }

        internal void ChangeDetails(string email, Details detailsForm)
        {
            var update = Builders<LoginCredentials>.Update.Set(credentials => credentials.Address, detailsForm.Address).Set(credentials => credentials.Phone, detailsForm.Phone).Set(credentials => credentials.Name, detailsForm.Name);
            _credentials.UpdateOne(credentials => credentials.Email == email, update);
        }

        internal Details GetDetails(string email)
        {
            var res = _credentials.Find(creds => creds.Email == email).FirstOrDefault();
            return new Details(res.Address, res.Phone, res.Name);
        }
    }
}
