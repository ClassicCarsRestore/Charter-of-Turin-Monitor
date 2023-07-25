using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace tasklist.Models
{
    public class Project
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; }

        [BsonElement("make")]
        public string Make { get; set; }

        [BsonElement("model")]
        public string Model { get; set; }

        [BsonElement("year")]
        public int Year { get; set; }

        [BsonElement("licencePlate")]
        public string LicencePlate { get; set; }

        [BsonElement("country")]
        public string Country { get; set; }

        [BsonElement("chassisNo")]
        public string ChassisNo { get; set; }

        [BsonElement("engineNo")]
        public string EngineNo { get; set; }

        [BsonElement("ownerEmail")]
        public string OwnerEmail { get; set; }

        [BsonElement("startDate")]
        [DataType(DataType.Date)]
        public DateTime StartDate { get; set; }

        [BsonElement("endDate")]
        [DataType(DataType.Date)]
        public DateTime EndDate { get; set; }

        [BsonElement("isComplete")]
        public bool IsComplete { get; set; }

        [BsonElement("caseInstanceId")]
        public string CaseInstanceId { get; set; }

        [BsonElement("processInstanceIds")]
        public List<string> ProcessInstanceIds { get; set; }

        [BsonElement("lastDiagramId")]
        public string LastDiagramId { get; set; }

        [BsonElement("photoId")]
        public string PhotoId { get; set; }

        [BsonElement("pinterestBoardId")]
        public string PinterestBoardId { get; set; }

        [BsonElement("pinterestBoardUrl")]
        public string PinterestBoardUrl { get; set; }

        [BsonElement("pinterestBoardAccessUrl")]
        public string PinterestBoardAccessUrl { get; set; }

        [BsonElement("paintRecordNumber")]
        public string PaintRecordNumber { get; set; }

        [BsonElement("paintDesignation")]
        public string PaintDesignation { get; set; }

        [BsonElement("paintTechnique")]
        public string PaintTechnique { get; set; }

        [BsonElement("paintOriginalYear")]
        public string PaintOriginalYear { get; set; }

        [BsonElement("paintDate")]
        public string PaintDate { get; set; }

        public Project(string make, string model, int year, string licencePlate, string country, string chassisNo, string engineNo, string ownerEmail, DateTime startDate, string caseInstanceId, string photoId, string pinterestBoardId, string pinterestBoardUrl, string pinterestBoardAccessUrl, string paintRecordNumber, string paintDesignation, string paintTechnique, string paintOriginalYear, string paintDate)
        {
            Make = make;
            Model = model;
            Year = year;
            LicencePlate = licencePlate;
            Country = country;
            ChassisNo = chassisNo;
            EngineNo = engineNo;
            OwnerEmail = ownerEmail;
            StartDate = startDate;
            IsComplete = false;
            EndDate = startDate;
            CaseInstanceId = caseInstanceId;
            ProcessInstanceIds = new List<string>();
            LastDiagramId = null;
            PhotoId = photoId;
            PinterestBoardId = pinterestBoardId;
            PinterestBoardUrl = pinterestBoardUrl;
            PinterestBoardAccessUrl = pinterestBoardAccessUrl;
            PaintRecordNumber = paintRecordNumber;
            PaintDesignation = paintDesignation;
            PaintTechnique = paintTechnique;
            PaintOriginalYear = paintOriginalYear;
            PaintDate = paintDate;
        }

        public Project(Project oldProject, ProjectDTO newProject)
        {
            Id = oldProject.Id;
            Make = newProject.Make;
            Model = newProject.Model;
            Year = newProject.Year;
            LicencePlate = newProject.LicencePlate;
            Country = newProject.Country;
            ChassisNo = newProject.ChassisNo;
            EngineNo = newProject.EngineNo;
            OwnerEmail = newProject.OwnerEmail;
            StartDate = oldProject.StartDate;
            IsComplete = oldProject.IsComplete;
            EndDate = oldProject.StartDate;
            CaseInstanceId = oldProject.CaseInstanceId;
            ProcessInstanceIds = oldProject.ProcessInstanceIds;
            LastDiagramId = oldProject.LastDiagramId;
            PhotoId = oldProject.PhotoId;
            PinterestBoardId = oldProject.PinterestBoardId;
            PinterestBoardUrl = oldProject.PinterestBoardUrl;
            PinterestBoardAccessUrl = newProject.PinterestBoardAccessUrl;
            PaintRecordNumber = newProject.PaintRecordNumber;
            PaintDesignation = newProject.PaintDesignation;
            PaintTechnique = newProject.PaintTechnique;
            PaintOriginalYear = newProject.PaintOriginalYear;
            PaintDate = newProject.PaintDate;
        }
    }
}
