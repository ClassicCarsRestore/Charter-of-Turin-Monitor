using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tasklist.Models;
using Task = tasklist.Models.Task;

namespace tasklist.Services
{
    public class TaskService
    {
        private readonly IMongoCollection<Task> _tasks;

        public TaskService(ITasksDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _tasks = database.GetCollection<Task>(settings.TasksCollectionName);
        }

        public List<Task> Get() =>
            _tasks.Find(task => true).ToList();

        public Task Get(string id) =>
            _tasks.Find<Task>(task => task.Id == id).FirstOrDefault();

        public Task GetByActivityId(string processInstanceId, string activityId) =>
            _tasks.Find<Task>(task => task.ProcessInstanceId == processInstanceId && task.ActivityId == activityId).FirstOrDefault();

        public List<Task> GetByProcessInstanceId(string processInstanceId) =>
            _tasks.Find(task => task.ProcessInstanceId == processInstanceId).ToList();

        public Task Create(Task task)
        {
            _tasks.InsertOne(task);
            return task;
        }

        public void Update(string id, Task taskIn) =>
            _tasks.ReplaceOne(task => task.Id == id, taskIn);

        public void Remove(string id) =>
            _tasks.DeleteOne(task => task.Id == id);

        public void RemoveManyByProcessInstanceId(string processInstanceId) =>
            _tasks.DeleteMany(task => task.ProcessInstanceId == processInstanceId);
    }
}
