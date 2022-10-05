using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tasklist.Models;

namespace tasklist.Services
{
	public class ActivityMapService
	{
        private readonly IMongoCollection<ActivityMap> _activityMaps;

        public ActivityMapService(IActivityMapsDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _activityMaps = database.GetCollection<ActivityMap>(settings.ActivityMapsCollectionName);
        }

        public List<ActivityMap> Get() =>
            _activityMaps.Find(activityMap => true).ToList();

        public ActivityMap Get(string id) =>
            _activityMaps.Find<ActivityMap>(activityMap => activityMap.Id == id).FirstOrDefault();

        public List<ActivityMap> GetByDiagramId(string diagramId) =>
            _activityMaps.Find(activityMap => activityMap.DiagramId == diagramId).ToList();

        public ActivityMap Create(ActivityMap activityMap)
        {
            _activityMaps.InsertOne(activityMap);
            return activityMap;
        }

        public void Update(string id, ActivityMap activityMapIn) =>
            _activityMaps.ReplaceOne(activityMap => activityMap.Id == id, activityMapIn);

        public void Remove(ActivityMap activityMapIn) =>
            _activityMaps.DeleteOne(activityMap => activityMap.Id == activityMapIn.Id);

        public void Remove(string id) =>
            _activityMaps.DeleteOne(activityMap => activityMap.Id == id);
    }
}
