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

        public ActivityAndLocationHistory AddNewActivityAndLocationToCar(string caseInstanceId, ActivityAndLocation newActivity)
        {
                // Find the existing document
                var existingHistory = _activityAndLocationHistory
                    .Find(activityAndLocationHistory => activityAndLocationHistory.CaseInstanceId == caseInstanceId)
                    .FirstOrDefault();

                if (existingHistory == null)
                {
                    // If no document exists, create a new one
                    existingHistory = new ActivityAndLocationHistory(caseInstanceId, new List<ActivityAndLocation> { newActivity });
                    _activityAndLocationHistory.InsertOne(existingHistory);
                }
                else
                {
                   
                    if (existingHistory.History == null)
                    {
                    existingHistory.History = new List<ActivityAndLocation>();
                    }
                    // Add the new activity to the existing history
                    existingHistory.History.Add(newActivity);

                    // Update the document in the database
                    var filter = Builders<ActivityAndLocationHistory>.Filter.Eq(h => h.CaseInstanceId, caseInstanceId);
                    var update = Builders<ActivityAndLocationHistory>.Update.Set(h => h.History, existingHistory.History);
                    _activityAndLocationHistory.UpdateOne(filter, update);
                }

                return existingHistory;
        }

        public ActivityAndLocationHistory UpdateActivityAndLocationInHistory(string caseInstanceId, string activityAndLocationId, ActivityAndLocation updatedActivity)
        {
            if (string.IsNullOrEmpty(caseInstanceId))
            {
                throw new ArgumentException("caseInstanceId cannot be null or empty.", nameof(caseInstanceId));
            }

            if (string.IsNullOrEmpty(activityAndLocationId))
            {
                throw new ArgumentException("activityAndLocationId cannot be null or empty.", nameof(activityAndLocationId));
            }

            if (updatedActivity == null)
            {
                throw new ArgumentNullException(nameof(updatedActivity), "updatedActivity cannot be null.");
            }

            try
            {
                var filter = Builders<ActivityAndLocationHistory>.Filter.And(
                    Builders<ActivityAndLocationHistory>.Filter.Eq(h => h.CaseInstanceId, caseInstanceId),
                    Builders<ActivityAndLocationHistory>.Filter.ElemMatch(h => h.History, a => a.Id == activityAndLocationId)
                );

                var update = Builders<ActivityAndLocationHistory>.Update
                    .Set(h => h.History[-1].ActivityId, updatedActivity.ActivityId)
                    .Set(h => h.History[-1].LocationId, updatedActivity.LocationId);

                var options = new FindOneAndUpdateOptions<ActivityAndLocationHistory>
                {
                    ReturnDocument = ReturnDocument.After,
                    IsUpsert = false
                };

                var updatedDocument = _activityAndLocationHistory.FindOneAndUpdate(filter, update, options);

                return updatedDocument;
            }
            catch (Exception ex)
            {
                // Log the exception
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw new Exception("An error occurred while updating the activity and location in history.", ex);
            }
        }

        public void Update(string caseInstanceId, ActivityAndLocationHistory historyLocationCarsIn) =>
            _activityAndLocationHistory.ReplaceOne(activityAndLocationHistory => activityAndLocationHistory.CaseInstanceId == caseInstanceId, historyLocationCarsIn);

        public void Remove(string id) =>
            _activityAndLocationHistory.DeleteOne(activityAndLocationHistory => activityAndLocationHistory.Id == id);

        public void DeleteByCaseInstanceId(string caseInstanceId) =>
            _activityAndLocationHistory.DeleteOne(activityAndLocationHistory => activityAndLocationHistory.CaseInstanceId == caseInstanceId);


    }
} 