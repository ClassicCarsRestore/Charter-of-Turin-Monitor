using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using tasklist.Models;
using tasklist.Services;
using System.Xml;

namespace tasklist.Controllers
{
	[Route("api/[controller]")]
    [ApiController]
    public class ProjectsController : ControllerBase
    {
        private readonly LoginCredentialsService _credentialsService;
        private readonly ProjectService _projectService;
        private readonly CamundaService _camundaService;
        private readonly PinterestService _pinterestService;
        private readonly TaskService _taskService;
        private readonly ActivityAndLocationHistoryService _activityAndLocationHistoryService;


        public ProjectsController(LoginCredentialsService credentialsService, ProjectService projectService, CamundaService camundaService, 
            PinterestService pinterestService, TaskService taskService, ActivityAndLocationHistoryService activityAndLocationHistoryService)
        {
            _credentialsService = credentialsService;
            _projectService = projectService;
            _camundaService = camundaService;
            _pinterestService = pinterestService;
            _taskService = taskService;
            _activityAndLocationHistoryService = activityAndLocationHistoryService;

        }

        // GET: api/Projects
        /// <summary>
        /// Method that retrieves all open Projects and the currently open Tasks in Camunda to build objects containing both the 
        /// Project properties and the name of the next Task awaiting approval.
        /// </summary>
        /// <returns>A list of Projects with the next Task name.</returns>
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetOpenProjectsAsync()
        {
            ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));
            string role = claims.FindFirst(c => c.Type == ClaimTypes.Role).Value;

            List<Project> projects;

            // get the projects created in the system
            if (role == "owner")
            {
                string email = claims.FindFirst(c => c.Type == ClaimTypes.Email).Value;
                projects = _projectService.GetOpenProjects(email);
            }
            else
                projects = _projectService.GetOpenProjects();

            // get the tasks from Camunda
            List<CamundaTask> tasks = await _camundaService.GetOpenTasksAsync();

            List<ProjectDTO> projectDTOs = new();

            foreach (Project project in projects)
            {
                List<CamundaTask> foundTasks = tasks.FindAll(t => t.CaseInstanceId == project.CaseInstanceId);

                if (foundTasks.Count > 0)
                    projectDTOs.Add(new ProjectDTO(project, foundTasks));
                else
                { // the case when it's waiting for a message trigger
                    CamundaTask waitingForMessage = new CamundaTask { Name = "Waiting for message"};
                    projectDTOs.Add(new ProjectDTO(project, new List<CamundaTask>() { waitingForMessage }));
                }
            }
            return projectDTOs;
        }

        [HttpGet("Closed", Name = "GetClosedProjects")]
        [Authorize]
        public ActionResult<IEnumerable<ProjectDTO>> GetClosedProjectsAsync()
        {
            ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));
            string role = claims.FindFirst(c => c.Type == ClaimTypes.Role).Value;

            List<Project> projects;

            // get the projects created in the system
            if (role == "owner")
            {
                string email = claims.FindFirst(c => c.Type == ClaimTypes.Email).Value;
                projects = _projectService.GetClosedProjects(email);
            }
            else
                projects = _projectService.GetClosedProjects();

            List<ProjectDTO> projectDTOs = new();

            foreach (Project project in projects)
            {
                projectDTOs.Add(new ProjectDTO(project, new List<CamundaTask>() ) );
            }

            return projectDTOs;
        }

        // GET: api/Projects/5
        [HttpGet("{id:length(24)}", Name = "GetProject")]
        [Authorize(Roles = "admin, manager")]
        public ActionResult<Project> Get(string id)
        {
            var project = _projectService.Get(id);

            if (project == null)
            {
                return NotFound();
            }

            return project;
        }

        // GET: api/Projects/DTO/5
        [HttpGet("{id:length(24)}/DTO", Name = "GetProjectDTO")]
        [Authorize]
        public async Task<ActionResult<ProjectDTO>> GetProjectDTO(string id)
        {
            ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));
            string role = claims.FindFirst(c => c.Type == ClaimTypes.Role).Value;

            Project project = _projectService.Get(id);
            if (role == "owner" && project.OwnerEmail != claims.FindFirst(c => c.Type == ClaimTypes.Email).Value)
            {
                return Forbid();
            }

            if (project == null)
            {
                return NotFound();
            }

            // get the tasks from Camunda
            List<CamundaTask> tasks = await _camundaService.GetOpenTasksAsync();

            List<CamundaTask> foundTasks = tasks.FindAll(t => t.CaseInstanceId == project.CaseInstanceId);

            ProjectDTO projectDTO = null;

            if (foundTasks.Count > 0)
                projectDTO = new ProjectDTO(project, foundTasks);
            else
            { // the case when it's waiting for a message trigger
                CamundaTask waitingForMessage = new CamundaTask { Name = "Waiting for message" };
                projectDTO = new ProjectDTO(project, new List<CamundaTask>() { waitingForMessage });
            }

            return projectDTO;
        }

        [HttpGet("Super/{processInstanceId}", Name = "GetSuperProcess")]
        [Authorize(Roles = "admin, manager")]
        public async Task<string> GetSuperProcessAsync(string processInstanceId)
        {
            var processInstance = await _camundaService.GetHistoryProcessInstanceAsync(processInstanceId);

            return processInstance?.SuperProcessInstanceId;
        }

        // GET: api/Projects/invoice:112/Diagram
        /// <summary>
        /// Method that retrieves the current XML Diagram in which the task from the requested caseInstanceId is currently in.
        /// </summary>
        /// <param name="caseInstanceId"></param>
        /// <returns>The XML of the Diagram of the current found task.</returns>
        [HttpGet("{processInstanceId}/Diagram", Name = "GetCurrentDiagram")]
        [Authorize(Roles = "admin, manager")]
        public async Task<ActionResult<string>> GetCurrentDiagramAsync(string processInstanceId)
        {
            CamundaHistoryProcessInstance processInstance = await _camundaService.GetHistoryProcessInstanceAsync(processInstanceId);
            string caseInstanceId = processInstance.CaseInstanceId;
            string processDefinitionId = processInstance.ProcessDefinitionId;

            // get the project with the requested caseInstanceId
            Project project = _projectService.GetByCaseInstanceId(caseInstanceId);

            // get the task from Camunda
            CamundaTask task = (await _camundaService.GetOpenTasksAsync()).Find(t => t.CaseInstanceId == caseInstanceId);
            bool updateLastDiagramId = false;

            if (task == null)
            {
                if (project.ProcessInstanceIds.Count == 0)
                    return NotFound();
            }
            else
            {
                // if the current ProcessInstanceId is not on the list add it and update the object (useful when retrieving history)
                if (!project.ProcessInstanceIds.Contains(task.ProcessInstanceId))
                {
                    updateLastDiagramId = true;

                    project.ProcessInstanceIds.Add(task.ProcessInstanceId);

                    _projectService.Update(project.Id, project);
                }
            }

            // get the diagram xml from Camunda
            CamundaDiagramXML xml = await _camundaService.GetXMLAsync(processDefinitionId);

            // update the last diagram name on the 'Project' object if needed
            if (updateLastDiagramId)
            {
                project.LastDiagramId = xml.DiagramId;
                _projectService.Update(project.Id, project);
            }

            return xml.Bpmn20Xml;
        }

        // PUT: api/Projects/5
        [HttpPut("{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> UpdateAsync(string id, ProjectDTO projectIn)
        {
            var project = _projectService.Get(id);

            if (project == null)
                return NotFound();

            if(projectIn.Photo != "")
            {
                var result = await _pinterestService.CreatePin(projectIn.Photo, project.PinterestBoardId, null);
                if(result.IsSuccessStatusCode)
                    project.PhotoId = (await result.Content.ReadAsAsync<dynamic>()).id;
            }
            _projectService.Update(id, new Project(project, projectIn));

            return Ok();
        }

        // POST: api/Projects
        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<Project>> CreateAsync(ProjectFormDTO projectForm)
        {
            // create a new pinterest board
            HttpResponseMessage result = await _pinterestService.CreateBoard(projectForm);
            var board = result.Content.ReadAsAsync<PinterestBoard>().Result;

            result = await _pinterestService.CreatePin(projectForm.Photo, board.Id, null);
            string photoId = (await result.Content.ReadAsAsync<dynamic>()).id;

            // make a unique key to use while starting the process in Camunda
            int noProjects = _projectService.Get().Count;
            string generatedId;
            do generatedId = projectForm.Make + "_" + projectForm.Model + "_" + noProjects++;
            while (_projectService.GetByCaseInstanceId(generatedId) != null);

            // clean the string input of licence plate from possible unecessary characters
            string sanitizedLicencePlate = projectForm.LicencePlate.Replace("-", "").Trim();

            string boardUrl = Settings.Pinterest_Photos_URL + Settings.Pinterest_Account + "/" + Regex.Replace(board.Name.Trim().Replace("/", "").Replace(" ", "-"), @"-+", "-").ToLower();

            Project project = new Project(projectForm.Make, projectForm.Model, projectForm.Year, sanitizedLicencePlate, projectForm.Country, projectForm.ChassisNo, projectForm.EngineNo, projectForm.OwnerEmail, projectForm.StartDate, generatedId, photoId, board.Id, boardUrl, null, "", "", "", "", "");

            _projectService.Create(project);

            ActivityAndLocationHistory activityAndLocationHistory = new ActivityAndLocationHistory(generatedId,null);

            _activityAndLocationHistoryService.Create(activityAndLocationHistory);

            // start the process in Camunda with the generated ID
            await _camundaService.StartProcessInstanceAsync("restoration_base", generatedId, projectForm);

            return CreatedAtRoute("GetProject", new { id = project.Id.ToString() }, project);
        }

        // DELETE: api/Projects/5
        [HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
            var project = _projectService.Get(id);

            if (project == null)
                return NotFound();


            List<CamundaProcessInstance> processInstances = await _camundaService.GetProccessInstancesFromCaseAsync(project.CaseInstanceId);

            // remove the tasks related to the Project
            foreach (CamundaProcessInstance processInstance in processInstances)
            {
                _taskService.RemoveManyByProcessInstanceId(processInstance.Id);
            }

            // terminate the process in Camunda with the caseInstanceId
            _camundaService.Delete(project.CaseInstanceId);

            _projectService.Remove(project.Id);

            _pinterestService.DeleteBoard(project.PinterestBoardId);

            return NoContent();
        }

        [HttpGet("Evidence/{caseInstanceId}", Name = "EvidenceDownload")]
        [Authorize]
        public async Task<ActionResult<string>> EvidenceDownloadAsync(string caseInstanceId)
        {
            Console.WriteLine($"[EvidenceDownload] Starting evidence generation for caseInstanceId: {caseInstanceId}");

            ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));
            string role = claims.FindFirst(c => c.Type == ClaimTypes.Role).Value;
            Console.WriteLine($"[EvidenceDownload] User role: {role}");

            Project currentProject = _projectService.GetByCaseInstanceId(caseInstanceId);

            if (currentProject == null)
            {
                Console.WriteLine($"[EvidenceDownload] ERROR: Project not found for caseInstanceId: {caseInstanceId}");
                return NotFound();
            }

            Console.WriteLine($"[EvidenceDownload] Project found: {currentProject.Id}, Owner: {currentProject.OwnerEmail}");

            if (role == "owner" && currentProject.OwnerEmail != claims.FindFirst(c => c.Type == ClaimTypes.Email).Value && currentProject.IsComplete)
            {
                Console.WriteLine($"[EvidenceDownload] ERROR: Authorization failed for owner");
                return Forbid();
            }

            var account = _credentialsService.GetAccount(currentProject.OwnerEmail);
            if (account == null)
            {
                Console.WriteLine($"[EvidenceDownload] ERROR: Account not found for email: {currentProject.OwnerEmail}");
                return NotFound();
            }

            Console.WriteLine($"[EvidenceDownload] Account found: {account.Name}");

            Console.WriteLine($"[EvidenceDownload] Checking Pinterest credentials...");
            if(!await _pinterestService.CheckAndUpdateCredentialsAsync())
            {
                Console.WriteLine($"[EvidenceDownload] ERROR: Pinterest credentials check failed");
                return Unauthorized();
            }
            Console.WriteLine($"[EvidenceDownload] Pinterest credentials OK");

            Console.WriteLine($"[EvidenceDownload] Fetching pins from Pinterest board: {currentProject.PinterestBoardId}");
            var boardPins= await _pinterestService.GetPinsFromBoard(currentProject.PinterestBoardId);

            if (boardPins == null)
            {
                Console.WriteLine($"[EvidenceDownload] WARNING: No pins found for board: {currentProject.PinterestBoardId}");
            }
            else
            {
                Console.WriteLine($"[EvidenceDownload] Found {boardPins.Count} pins on board");
            }

            var mainImage = boardPins?.FindLast(i => i.Id == currentProject.PhotoId);
            if(mainImage != null)
            {
                Console.WriteLine($"[EvidenceDownload] Downloading main image: {mainImage.Id}");
                try
                {
                    using (WebClient webClient = new WebClient())
                    {
                        byte[] dataArr = webClient.DownloadData(mainImage.Media.Images["1200x"].Url);
                        System.IO.File.WriteAllBytes($"data/images/{mainImage.Id}.jpg", dataArr);
                        Console.WriteLine($"[EvidenceDownload] Main image saved successfully");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[EvidenceDownload] ERROR downloading main image: {ex.Message}");
                }
            }
            else
            {
                Console.WriteLine($"[EvidenceDownload] WARNING: Main image not found with PhotoId: {currentProject.PhotoId}");
            }

            string clientExpectation = null;

            Console.WriteLine($"[EvidenceDownload] Fetching root process instance for case: {caseInstanceId}");
            var processInstance = await _camundaService.GetRootProcessAsync(caseInstanceId);
            if (processInstance == null)
            {
                Console.WriteLine($"[EvidenceDownload] ERROR: Root process instance not found");
                return NotFound();
            }
            Console.WriteLine($"[EvidenceDownload] Root process instance found: {processInstance.Id}");

            clientExpectation = await _camundaService.GetProcessInstanceVariable(processInstance.Id, "clientExpectation");
            Console.WriteLine($"[EvidenceDownload] Client expectation: {clientExpectation}");

            Console.WriteLine($"[EvidenceDownload] Building LaTeX document content...");
            StringBuilder contents = new StringBuilder();

            contents.AppendLine(@"\def\carphotoid{" + currentProject.PhotoId + "}");
            contents.AppendLine(@"\def\carowner{" + account.Name + "}");
            contents.AppendLine(@"\def\carintervention{" + clientExpectation + "}");
            contents.AppendLine(@"\def\carmake{" + currentProject.Make + "}");
            contents.AppendLine(@"\def\carmodel{" + currentProject.Model + "}");
            contents.AppendLine(@"\def\caryear{" + currentProject.Year + "}");
            contents.AppendLine(@"\def\carlicenceplate{" + currentProject.LicencePlate + "}");
            contents.AppendLine(@"\def\carcountry{" + currentProject.Country + "}");
            contents.AppendLine(@"\def\carchassisnumber{" + currentProject.ChassisNo + "}");
            contents.AppendLine(@"\def\carenginenumber{" + currentProject.EngineNo + "}");
            contents.AppendLine(@"\def\carstartdate{" + currentProject.StartDate.ToShortDateString() + "}");
            var endDate = "";
            if (currentProject.IsComplete)
                endDate = currentProject.EndDate.ToShortDateString();
            contents.AppendLine(@"\def\carenddate{" + endDate + "}");
            contents.AppendLine(@"\def\carpinterestaccess{" + currentProject.PinterestBoardAccessUrl + "}");
            contents.AppendLine(@"\def\paintRecordNumber{" + currentProject.PaintRecordNumber?.Replace("#", "##")  + "}");
            contents.AppendLine(@"\def\paintDesignation{" + currentProject.PaintDesignation  + "}");
            contents.AppendLine(@"\def\paintTechnique{" + currentProject.PaintTechnique  + "}");
            contents.AppendLine(@"\def\paintOriginalYear{" + currentProject.PaintOriginalYear  + "}");
            contents.AppendLine(@"\def\paintDate{" + currentProject.PaintDate + "}");
            contents.AppendLine(@"\input{intro}");
            contents.AppendLine(@"\newpage");
            contents.AppendLine(@"\section{Record of evidence of the intervention}");
            Console.WriteLine($"[EvidenceDownload] LaTeX header built successfully");

            List<CamundaHistoryTask> historyTasks = new();
            List<Models.Task> relatedTasks = new();

            Console.WriteLine($"[EvidenceDownload] Fetching process instances from case...");
            List<CamundaProcessInstance> processInstances = await _camundaService.GetProccessInstancesFromCaseAsync(caseInstanceId);
            Console.WriteLine($"[EvidenceDownload] Found {processInstances.Count} process instances");

            foreach (CamundaProcessInstance instance in processInstances)
            {
                Console.WriteLine($"[EvidenceDownload] Processing instance: {instance.Id}");
                List<CamundaHistoryTask> diagramHistoryTasks = await _camundaService.GetDiagramTaskHistoryAsync(instance.Id);
                historyTasks = historyTasks.Concat(diagramHistoryTasks).ToList();
                Console.WriteLine($"[EvidenceDownload] Added {diagramHistoryTasks.Count} history tasks from instance");

                List<Models.Task> diagramRelatedTasks = _taskService.GetByProcessInstanceId(instance.Id);
                relatedTasks = relatedTasks.Concat(diagramRelatedTasks).ToList();
                Console.WriteLine($"[EvidenceDownload] Added {diagramRelatedTasks.Count} related tasks from instance");
            }

            historyTasks = historyTasks.Where(t => t.EndTime != null).ToList();
            Console.WriteLine($"[EvidenceDownload] Total completed history tasks: {historyTasks.Count}");

            historyTasks = historyTasks.OrderBy(t => t.StartTime).ToList();
            Console.WriteLine($"[EvidenceDownload] History tasks sorted by start time");

            var definitions = new Dictionary<string, DefinitionAssociations>();

            var changes = false;
            var tempContents = new StringBuilder();

            Console.WriteLine($"[EvidenceDownload] Starting to process {historyTasks.Count} history tasks...");
            int taskCounter = 0;

            // make the connection between the history tasks from camunda and the tasks saved in our database
            foreach (CamundaHistoryTask historyTask in historyTasks)
            {
                taskCounter++;
                Console.WriteLine($"[EvidenceDownload] Processing history task {taskCounter}/{historyTasks.Count}: {historyTask.ActivityName} (ActivityId: {historyTask.ActivityId})");

                var sub = "sub";
                if (historyTask.RootProcessInstanceId == historyTask.ProcessInstanceId)
                {
                    if (changes)
                        contents.Append(tempContents);
                    changes = false;
                    tempContents = new StringBuilder();
                    if(historyTask.ActivityType == "callActivity")
                        tempContents.AppendLine(@"\subsection{" + historyTask.ActivityName + "}");
                    sub = "";
                }

                Models.Task taskFound = relatedTasks.FirstOrDefault(rt => (rt.ProcessInstanceId == historyTask.ProcessInstanceId
                    && rt.ActivityId == historyTask.ActivityId));

                if (taskFound == null)
                {
                    Console.WriteLine($"[EvidenceDownload] No matching task found in database, skipping");
                    continue;
                }

                Console.WriteLine($"[EvidenceDownload] Task found in database: {taskFound.Id}");

                var videos = new List<PinterestPin>();
                var images = new List<PinterestPin>();
                if(taskFound.BoardSectionId != null)
                {
                    Console.WriteLine($"[EvidenceDownload] Fetching pins from section: {taskFound.BoardSectionId}");
                    var pins = await _pinterestService.GetPinsFromSection(currentProject.PinterestBoardId, taskFound.BoardSectionId);
                    if (pins != null)
                    {
                        Console.WriteLine($"[EvidenceDownload] Found {pins.Count} pins in section");
                        foreach (var pin in pins)
                        {
                            var media = pin.Media;
                            var type = "";
                            type = media.Media_type;
                            if (type == "image" && taskFound.Pins.Contains(pin.Id))
                                images.Add(pin);
                            else if(type == "video")
                                videos.Add(pin);
                        }
                        Console.WriteLine($"[EvidenceDownload] Categorized: {images.Count} images, {videos.Count} videos");
                    }
                    else
                    {
                        Console.WriteLine($"[EvidenceDownload] WARNING: No pins returned from section");
                    }
                }
                else
                {
                    Console.WriteLine($"[EvidenceDownload] Task has no BoardSectionId");
                }

                if (videos.Count == 0 && images.Count == 0 && (taskFound.CommentReport == null || taskFound.CommentReport == ""))
                {
                    Console.WriteLine($"[EvidenceDownload] Task has no content (no videos, images, or comments), skipping");
                    continue;
                }

                Console.WriteLine($"[EvidenceDownload] Task has content, adding to LaTeX document");
                changes = true;

                var processDefinition = await _camundaService.GetHistoryProcessInstanceAsync(taskFound.ProcessInstanceId);
                var processDefinitionId = processDefinition.ProcessDefinitionId;
                if (!definitions.ContainsKey(processDefinitionId))
                {
                    Console.WriteLine($"[EvidenceDownload] Fetching process definition diagram: {processDefinitionId}");
                    try
                    {
                        var xml = await GetDefinitionDiagram(processDefinitionId);
                        using (var reader = new StringReader(xml))
                        {
                            var textAnnotations = new Dictionary<string, string>();
                            var associations = new Dictionary<string, string>();
                            var textAnnotationId = "";
                            var xmlReader = new XmlTextReader(reader);
                            while (xmlReader.Read())
                            {
                                switch (xmlReader.NodeType)
                                {
                                    case XmlNodeType.Element:
                                        if (xmlReader.Name == "textAnnotation")
                                            while (xmlReader.MoveToNextAttribute())
                                                if (xmlReader.Name == "id")
                                                    textAnnotationId = xmlReader.Value;
                                        if (xmlReader.Name == "association")
                                        {
                                            var sourceRef = "";
                                            var targetRef = "";
                                            while (xmlReader.MoveToNextAttribute())
                                                if (xmlReader.Name == "sourceRef")
                                                    sourceRef = xmlReader.Value;
                                                else if (xmlReader.Name == "targetRef")
                                                    targetRef = xmlReader.Value;
                                            associations.Add(sourceRef, targetRef);
                                        }
                                        break;
                                    case XmlNodeType.Text:
                                        if (textAnnotationId != "")
                                            textAnnotations.Add(textAnnotationId, xmlReader.Value);
                                        break;
                                    case XmlNodeType.EndElement:
                                        if (xmlReader.Name == "textAnnotation")
                                            textAnnotationId = "";
                                        break;
                                }
                            }
                            definitions.Add(processDefinitionId, new DefinitionAssociations() { Associations = associations, TextAnnotations = textAnnotations });
                            Console.WriteLine($"[EvidenceDownload] Parsed XML - Found {textAnnotations.Count} annotations and {associations.Count} associations");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[EvidenceDownload] ERROR parsing diagram XML: {ex.Message}");
                    }
                }
                var definitionAssociations = definitions[processDefinitionId];

                tempContents.AppendLine(@"\" + sub + "subsection{" + historyTask.ActivityName.Replace("µ", @"$\mu$").Replace("<", @"$<$") + "}");
                
                if (definitionAssociations.Associations.ContainsKey(taskFound.ActivityId))
                    tempContents.AppendLine(@"\chartercomment{" + definitionAssociations.TextAnnotations[definitionAssociations.Associations[taskFound.ActivityId]] + "}");

                tempContents.AppendLine(@"\taskdiagram{" + Request.Host + "/diagram/" + processDefinition.ProcessDefinitionId + "/}");

                if (videos.Count > 0)
                {
                    foreach(var video in videos)
                        tempContents.AppendLine(@"\taskvideo{https://www.pinterest.pt/pin/" + video.Id + "/}");
                }

                if(taskFound.CommentReport != null && taskFound.CommentReport != "")
                    tempContents.AppendLine(@"\taskcomment{" + taskFound.CommentReport.Replace(@"\", @"$\backslash$").Replace("&", @"\&") + "}");

                if (images.Count > 0)
                {
                    Console.WriteLine($"[EvidenceDownload] Processing {images.Count} images for task");
                    tempContents.AppendLine(@"\begin{multicols}{2}");
                    int imageCounter = 0;
                    foreach (var i in images) {
                        imageCounter++;
                        Console.WriteLine($"[EvidenceDownload] Downloading image {imageCounter}/{images.Count}: {i.Id}");
                        try
                        {
                            using (WebClient webClient = new WebClient())
                            {
                                byte[] dataArr = webClient.DownloadData(i.Media.Images["1200x"].Url);
                                System.IO.File.WriteAllBytes($"data/images/{i.Id}.jpg", dataArr);
                                Console.WriteLine($"[EvidenceDownload] Image saved: {i.Id}.jpg ({dataArr.Length} bytes)");
                            }
                            tempContents.AppendLine(@"\begin{figure}[H]");
                            tempContents.AppendLine(@"\centering");
                            tempContents.AppendLine(@"\href{https://www.pinterest.pt/pin/" + i.Id + @"}{\includegraphics[width=\linewidth]{" + i.Id + "}}");
                            tempContents.AppendLine(@"\caption{"+ i.Note + "}");
                            tempContents.AppendLine(@"\vspace{10pt}");
                            tempContents.AppendLine(@"\end{figure}");
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[EvidenceDownload] ERROR downloading image {i.Id}: {ex.Message}");
                        }
                    }
                    tempContents.AppendLine(@"\end{multicols}");
                }
                tempContents.AppendLine(@"\newpage");
            }
            if (changes)
                contents.Append(tempContents);

            Console.WriteLine($"[EvidenceDownload] Finished processing history tasks");
            Console.WriteLine($"[EvidenceDownload] LaTeX document length: {contents.Length} characters");

            string path = "data/document.tex";
            try
            {
                Console.WriteLine($"[EvidenceDownload] Writing LaTeX file to: {path}");
                using (FileStream fs = System.IO.File.Create(path))
                {
                    byte[] info = new UTF8Encoding(true).GetBytes(contents.ToString());
                    fs.Write(info, 0, info.Length);
                    Console.WriteLine($"[EvidenceDownload] LaTeX file written successfully ({info.Length} bytes)");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[EvidenceDownload] ERROR: Tex file creation error:");
                Console.WriteLine(ex.ToString());
                return StatusCode(500, "Failed to create LaTeX file");
            }

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            {
                Console.WriteLine($"[EvidenceDownload] Running pdflatex to generate PDF (2 passes)...");
                ProcessStartInfo startInfo = new ProcessStartInfo() { FileName = "/bin/bash", Arguments = "pdflatex.sh", WorkingDirectory = "data" };
                Process proc = new Process() { StartInfo = startInfo };
                for(int i = 0; i<2; i++)
                {
                    Console.WriteLine($"[EvidenceDownload] Starting pdflatex pass {i+1}/2");
                    proc.Start();
                    proc.WaitForExit();
                    Console.WriteLine($"[EvidenceDownload] pdflatex pass {i+1} completed with exit code: {proc.ExitCode}");
                }
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Console.WriteLine($"[EvidenceDownload] Windows platform detected - LaTeX compilation skipped");
                Console.WriteLine(contents.ToString());
            }

            Console.WriteLine($"[EvidenceDownload] Cleaning up image files...");
            string[] files = Directory.GetFiles("data/images");
            foreach (string file in files)
            {
                System.IO.File.Delete(file);
            }
            Console.WriteLine($"[EvidenceDownload] Deleted {files.Length} image files");

            string base64 = "";
            if (System.IO.File.Exists($"data/template.pdf"))
            {
                Console.WriteLine($"[EvidenceDownload] Reading generated PDF file...");
                byte[] bytes = System.IO.File.ReadAllBytes($"data/template.pdf");
                base64 = Convert.ToBase64String(bytes);
                Console.WriteLine($"[EvidenceDownload] PDF converted to base64 ({bytes.Length} bytes, base64 length: {base64.Length})");
            }
            else
            {
                Console.WriteLine($"[EvidenceDownload] ERROR: PDF file not found at data/template.pdf");
            }

            Console.WriteLine($"[EvidenceDownload] Cleaning up temporary files...");
            files = Directory.GetFiles("data");
            int deletedCount = 0;
            foreach (string file in files)
            {
                if (!file.Contains("template.tex") && !file.Contains("intro.tex") && !file.Contains("pdflatex.sh"))
                {
                    System.IO.File.Delete(file);
                    deletedCount++;
                }
            }
            Console.WriteLine($"[EvidenceDownload] Deleted {deletedCount} temporary files");

            Console.WriteLine($"[EvidenceDownload] Evidence generation complete, returning {(base64.Length > 0 ? "PDF" : "empty response")}");
            return base64;
        }

        public class DefinitionAssociations
        {
            public Dictionary<string, string> TextAnnotations { get; set; }
            public Dictionary<string, string> Associations { get; set; }
        }

        [HttpGet("Root")]
        public async Task<PathNode> GetRoot()
        {
            return new PathNode(new PathProcess(await _camundaService.GetRoot()), new());
        }

        [HttpGet("Node/{processDefinitionId}/{activityId}")]
        public async Task<PathNode> GetNode(string processDefinitionId, string activityId)
        {
            var calledProcessDefinitions = await _camundaService.GetCalledProcessDefinitions(processDefinitionId);
            foreach (var calledProcessDefinition in calledProcessDefinitions)
                if (calledProcessDefinition.CalledFromActivityIds.Contains(activityId))
                    return new PathNode(new PathProcess(calledProcessDefinition), new());
            return null;
        }

        [HttpGet("Diagram/{processDefinitionId}")]
        public async Task<string> GetDefinitionDiagram(string processDefinitionId)
        {
            CamundaDiagramXML xml = await _camundaService.GetXMLAsync(processDefinitionId);
            return xml.Bpmn20Xml;
        }

        [HttpGet("Definition/{processDefinitionId}")]
        public async Task<PathNode> DefinitionSearch(string processDefinitionId)
        {
            var root = await GetRoot();
            if (root.Self.DefinitionId == processDefinitionId)
                return root;
            var calledProcessDefinitions = await _camundaService.GetCalledProcessDefinitions(root.Self.DefinitionId);
            foreach (var calledProcessDefinition in calledProcessDefinitions)
                if (calledProcessDefinition.Id != root.Self.DefinitionId)
                {
                    var child = await RecursiveDefinitionSearch(calledProcessDefinition, processDefinitionId);
                    if (child != null)
                    {
                        root.Children.Add(child);
                        return root;
                    }
                }
            return null;
        }
        private async Task<PathNode> RecursiveDefinitionSearch(CamundaCalledProcessDefinition processDefinition, string processDefinitionId)
        {
            if (processDefinition.Id == processDefinitionId)
                return new PathNode(new PathProcess(processDefinition), new());
            var calledProcessDefinitions = await _camundaService.GetCalledProcessDefinitions(processDefinition.Id);
            foreach (var calledProcessDefinition in calledProcessDefinitions)
                if (calledProcessDefinition.Id != processDefinition.Id)
                {
                    var child = await RecursiveDefinitionSearch(calledProcessDefinition, processDefinitionId);
                    if (child != null)
                    {
                        var children = new List<PathNode>();
                        children.Add(child);
                        return new PathNode(new PathProcess(processDefinition), children);
                    }
                }
            return null;
        }
    }
}
