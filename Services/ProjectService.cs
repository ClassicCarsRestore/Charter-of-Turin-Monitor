using MongoDB.Driver;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tasklist.Models;

namespace tasklist.Services
{
    public class ProjectService
    {
        private readonly IMongoCollection<Project> _projects;

        public ProjectService(IProjectsDatabaseSettings settings)
        {
            var client = new MongoClient(settings.ConnectionString);
            var database = client.GetDatabase(settings.DatabaseName);

            _projects = database.GetCollection<Project>(settings.ProjectsCollectionName);
        }

        public List<Project> Get() =>
            _projects.Find(project => true).ToList();

        public Project Get(string id) =>
            _projects.Find<Project>(project => project.Id == id).FirstOrDefault();

        /// <summary>
        /// Get all the ongoing Projects in the database. 
        /// </summary>
        /// <returns>A List with all the open projects.</returns>
        public List<Project> GetOpenProjects() =>
            _projects.Find(project => !project.IsComplete).ToList();

        /// <summary>
        /// Get all the ongoing Projects in the database from a specific user. 
        /// </summary>
        /// <returns>A List with all the open projects from a specific user.</returns>
        public List<Project> GetOpenProjects(string email) =>
            _projects.Find(project => !project.IsComplete && project.OwnerEmail == email).ToList();

        /// <summary>
        /// Get all the completed Projects in the database. 
        /// </summary>
        /// <returns>A List with all the completed projects.</returns>
        public List<Project> GetClosedProjects() =>
            _projects.Find(project => project.IsComplete).ToList();

        /// <summary>
        /// Get all the completed Projects in the database from a specific user. 
        /// </summary>
        /// <returns>A List with all the completed projects from a specific user.</returns>
        public List<Project> GetClosedProjects(string email) =>
            _projects.Find(project => project.IsComplete && project.OwnerEmail == email).ToList();

        /// <summary>
        /// Get the Project with CaseInstanceId equal to the requested caseInstanceId.
        /// </summary>
        /// <param name="caseInstanceId"></param>
        /// <returns>The Project with the requested CaseInstanceId.</returns>
        public Project GetByCaseInstanceId(string caseInstanceId) =>
            _projects.Find<Project>(project => project.CaseInstanceId == caseInstanceId).FirstOrDefault();

        /// <summary>
        /// Get the Project that contains the ProcessInstanceId.
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <returns>The Project that contains the ProcessInstanceId.</returns>
        public Project GetByProcessInstanceId(string processInstanceId) =>
            _projects.Find<Project>(project => project.ProcessInstanceIds.Contains(processInstanceId)).FirstOrDefault();

        /// <summary>
        /// Get the Project with LicencePlate equal to the requested licencePlate.
        /// </summary>
        /// <param name="licencePlate"></param>
        /// <returns>The Project with the requested LicencePlate.</returns>
        public Project GetByLicencePlate(string licencePlate) =>
            _projects.Find<Project>(project => project.LicencePlate == licencePlate).FirstOrDefault();

        public Project Create(Project project)
        {
            _projects.InsertOne(project);
            return project;
        }

        public void Update(string id, Project projectIn) =>
            _projects.ReplaceOne(project => project.Id == id, projectIn);

        /// <summary>
        /// Update the Project object with the property 'IsComplete' changed to the boolean 'true' and 
        /// update the the property 'EndDate' with the current DateTime.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="projectIn"></param>
        public void Complete(string id, Project projectIn)
		{
            projectIn.IsComplete = true;
            projectIn.EndDate = DateTime.Now;

            _projects.ReplaceOne(project => project.Id == id, projectIn);
        }

        public void Remove(Project projectIn) =>
            _projects.DeleteOne(project => project.Id == projectIn.Id);

        public void Remove(string id) =>
            _projects.DeleteOne(project => project.Id == id);
    }
}
