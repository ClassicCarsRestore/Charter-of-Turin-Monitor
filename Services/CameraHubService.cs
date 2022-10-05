using MongoDB.Driver;
using System;
using tasklist.Models;

namespace tasklist.Services
{
	public class CameraHubService
	{
        private readonly IMongoCollection<CameraHub> _cameraHub;

        public CameraHubService(ICameraHubDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _cameraHub = database.GetCollection<CameraHub>(settings.CameraHubCollectionName);
        }

        public CameraHub Get(string id) =>
            _cameraHub.Find(cameraHub => cameraHub.Id == id).FirstOrDefault();

        public CameraHub Get(string projectName, string password) =>
            _cameraHub.Find(cameraHub => cameraHub.ProjectName == projectName && cameraHub.Password == password).FirstOrDefault();

        public void Update(string id, CameraHub cameraHubIn) =>
            _cameraHub.ReplaceOne(cameraHub => cameraHub.Id == id, cameraHubIn);

        public CameraHub Schedule(CameraHub cameraHub)
        {
            _cameraHub.InsertOne(cameraHub);
            return cameraHub;
        }
    }
}
