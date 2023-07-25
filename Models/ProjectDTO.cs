using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace tasklist.Models
{
    public class ProjectDTO
    {
        public string Id { get; set; }
        public string Make { get; set; }
        public string Model { get; set; }
        public int Year { get; set; }
        public string LicencePlate { get; set; }
        public string Country { get; set; }
        public string ChassisNo { get; set; }
        public string EngineNo { get; set; }
        public string OwnerEmail { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool IsComplete { get; set; }
        public string CaseInstanceId { get; set; }
        public List<string> NextTaskName { get; set; }
        public string Photo { get; set; }
        public string PinterestBoardUrl { get; set; }
        public string PinterestBoardAccessUrl { get; set; }
        public string PaintRecordNumber { get; set; }
        public string PaintDesignation { get; set; }
        public string PaintTechnique { get; set; }
        public string PaintOriginalYear { get; set; }
        public string PaintDate { get; set; }

        public ProjectDTO(Project project, List<CamundaTask> nextTasks)
        {
            Id = project.Id;
            Make = project.Make;
            Model = project.Model;
            Year = project.Year;
            LicencePlate = project.LicencePlate;
            Country = project.Country;
            ChassisNo = project.ChassisNo;
            EngineNo = project.EngineNo;
            OwnerEmail = project.OwnerEmail;
            StartDate = project.StartDate;
            EndDate = project.EndDate;
            IsComplete = project.IsComplete;
            CaseInstanceId = project.CaseInstanceId;

            NextTaskName = new List<string>();
            foreach (CamundaTask task in nextTasks)
                NextTaskName.Add(task.Name);

            Photo = "";
            PinterestBoardUrl = project.PinterestBoardUrl;
            PinterestBoardAccessUrl = project.PinterestBoardAccessUrl;
            PaintRecordNumber = project.PaintRecordNumber;
            PaintDesignation = project.PaintDesignation;
            PaintTechnique = project.PaintTechnique;
            PaintOriginalYear = project.PaintOriginalYear;
            PaintDate = project.PaintDate;
        }
        [JsonConstructor]
        public ProjectDTO() { }
    }
}
