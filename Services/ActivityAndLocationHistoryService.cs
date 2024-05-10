using MongoDB.Driver;
using System;
using tasklist.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace tasklist.Services
{
    public class ActivityAndLocationHistoryService
    {
        private readonly IMongoCollection<ActivityAndLocationHistory> _activityAndLocationHistory;

        public ActivityAndLocationHistoryService(IActivityAndLocationHistoryDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _activityAndLocationHistory = database.GetCollection<ActivityAndLocationHistory>(settings.ActivityAndLocationHistoryCollectionName);
        }

        public List<ActivityAndLocationHistory> Get() =>
            _activityAndLocationHistory.Find(activityAndLocationHistory => true).ToList();

        public ActivityAndLocationHistory GetByCaseInstanceId(string caseInstanceId) =>
            _activityAndLocationHistory.Find<ActivityAndLocationHistory>(activityAndLocationHistory => activityAndLocationHistory.CaseInstanceId == caseInstanceId).FirstOrDefault();

        public ActivityAndLocationHistory Get(string id) =>
            _activityAndLocationHistory.Find<ActivityAndLocationHistory>(activityAndLocationHistory => activityAndLocationHistory.Id == id).FirstOrDefault();


        public ActivityAndLocationHistory Create(ActivityAndLocationHistory activityAndLocationHistory)
        {
            _activityAndLocationHistory.InsertOne(activityAndLocationHistory);
            return activityAndLocationHistory;
        }

        public ActivityAndLocationHistory AddNewActivityAndLocationToCar(string caseInstanceId, ActivityAndLocation newActivity)
        {
            var filter = Builders<ActivityAndLocationHistory>.Filter.Eq(h => h.CaseInstanceId, caseInstanceId);
            var update = Builders<ActivityAndLocationHistory>.Update.Push(h => h.History, newActivity);

            _activityAndLocationHistory.UpdateOne(filter, update);

            return _activityAndLocationHistory.Find<ActivityAndLocationHistory>(filter).FirstOrDefault();
        }

        public ActivityAndLocationHistory UpdateActivityInCarHistory(string caseInstanceId, string activityId, ActivityAndLocation updatedActivity)
        {
            var filter = Builders<ActivityAndLocationHistory>.Filter.And(
            Builders<ActivityAndLocationHistory>.Filter.Eq(h => h.CaseInstanceId, caseInstanceId),
            Builders<ActivityAndLocationHistory>.Filter.ElemMatch(h => h.History, a => a.Id == activityId)
            );

            var update = Builders<ActivityAndLocationHistory>.Update.Set("History.$", updatedActivity);

            _activityAndLocationHistory.UpdateOne(filter, update);

            return _activityAndLocationHistory.Find<ActivityAndLocationHistory>(filter).FirstOrDefault();
        }

        public void Update(string caseInstanceId, ActivityAndLocationHistory historyLocationCarsIn) =>
            _activityAndLocationHistory.ReplaceOne(activityAndLocationHistory => activityAndLocationHistory.CaseInstanceId == caseInstanceId, historyLocationCarsIn);

        public void Remove(string id) =>
            _activityAndLocationHistory.DeleteOne(activityAndLocationHistory => activityAndLocationHistory.Id == id);

        public void DeleteByCaseInstanceId(string caseInstanceId) =>
            _activityAndLocationHistory.DeleteOne(activityAndLocationHistory => activityAndLocationHistory.CaseInstanceId == caseInstanceId);

        public List<ActivityAndLocationHistory> GetUnfinishedActivities()
        {
            var filter = Builders<ActivityAndLocationHistory>.Filter.ElemMatch(
                h => h.History,
                Builders<ActivityAndLocation>.Filter.Eq(a => a.EndDate, null)
            );

            return _activityAndLocationHistory.Find(filter).ToList();
        }


         public List<ActivityAndLocationHistory> GetUnfinishedActivitiesByCar(string caseInstanceId)
        {
            var filter = Builders<ActivityAndLocationHistory>.Filter.And(
                Builders<ActivityAndLocationHistory>.Filter.Eq(h => h.CaseInstanceId, caseInstanceId),
                Builders<ActivityAndLocationHistory>.Filter.ElemMatch(
                    h => h.History,
                    Builders<ActivityAndLocation>.Filter.Eq(a => a.EndDate, null)
                )
            );

            return _activityAndLocationHistory.Find(filter).ToList();
        }




    }
} 