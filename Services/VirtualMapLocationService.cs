using MongoDB.Driver;
using System;
using tasklist.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace tasklist.Services
{
    public class VirtualMapLocationService
    {
        private readonly IMongoCollection<VirtualMapLocation> _virtualmaplocations;

        public VirtualMapLocationService(IVirtualMapLocationsDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _virtualmaplocations = database.GetCollection<VirtualMapLocation>(settings.VirtualMapLocationsCollectionName);
        }

        public List<VirtualMapLocation> Get() =>
            _virtualmaplocations.Find(virtualMapLocation => true).ToList();

        public VirtualMapLocation Get(string id) =>
            _virtualmaplocations.Find<VirtualMapLocation>(virtualMapLocation => virtualMapLocation.Id == id).FirstOrDefault();

        public VirtualMapLocation Create(VirtualMapLocation virtualMapLocation)
        {
            _virtualmaplocations.InsertOne(virtualMapLocation);
            return virtualMapLocation;
        }

        public void Update(string id, VirtualMapLocation virtualMapLocationIn) =>
            _virtualmaplocations.ReplaceOne(virtualMapLocation => virtualMapLocation.Id == id, virtualMapLocationIn);

        public void Remove(string id) =>
            _virtualmaplocations.DeleteOne(virtualMapLocation => virtualMapLocation.Id == id);

        public List<string> GetLocationIdsByActivityId(string activityId)
        {
            var filter = Builders<VirtualMapLocation>.Filter.AnyEq(location => location.ActivityIds, activityId);
            var locations = _virtualmaplocations.Find(filter).ToList();
            return locations.Select(location => location.Id).ToList();
        }

    }
}