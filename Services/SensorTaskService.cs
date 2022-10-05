using MongoDB.Driver;
using System.Collections.Generic;
using System.Linq;
using tasklist.Models;

namespace tasklist.Services
{
	public class SensorTaskService
	{
        private readonly IMongoCollection<SensorTask> _sensorTasks;

        public SensorTaskService(ISensorTasksDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _sensorTasks = database.GetCollection<SensorTask>(settings.SensorTasksCollectionName);
        }

        public List<SensorTask> Get() =>
            _sensorTasks.Find(sensorTask => true).ToList();

        public List<SensorTask> GetByProjectId(string projectId) =>
           _sensorTasks.Find(sensorTask => sensorTask.ProjectId == projectId).ToList();

        public SensorTask Get(string id) =>
            _sensorTasks.Find<SensorTask>(sensorTask => sensorTask.Id == id).FirstOrDefault();

        public SensorTask Create(SensorTask sensorTask)
        {
            _sensorTasks.InsertOne(sensorTask);
            return sensorTask;
        }

        public void Update(string id, SensorTask sensorTaskIn) =>
            _sensorTasks.ReplaceOne(sensorTask => sensorTask.Id == id, sensorTaskIn);

        public void Remove(SensorTask sensorTaskIn) =>
            _sensorTasks.DeleteOne(sensorTask => sensorTask.Id == sensorTaskIn.Id);

        public void Remove(string id) =>
            _sensorTasks.DeleteOne(sensorTask => sensorTask.Id == id);
    }

}
