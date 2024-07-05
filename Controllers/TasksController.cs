using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using tasklist.Models;
using tasklist.Services;
using Task = tasklist.Models.Task;

namespace tasklist.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Authorize]
	public class TasksController : ControllerBase
	{
		private readonly TaskService _taskService;
		private readonly ProjectService _projectService;
		private readonly CamundaService _camundaService;
		private readonly SensorTaskService _sensorTaskService;
		private readonly AmazonS3Service _amazonS3Service;
		private readonly ActivityMapService _activityMapService;
		private readonly PinterestService _pinterestService;
		private readonly ActivityAndLocationHistoryService _activityAndLocationHistoryService;

		public TasksController(TaskService taskService, ProjectService projectService, CamundaService camundaService,
			SensorTaskService sensorTaskService, AmazonS3Service amazonS3Service, ActivityMapService activityMapService, PinterestService pinterestService,
			ActivityAndLocationHistoryService activityAndLocationHistoryService)
		{
			_taskService = taskService;
			_projectService = projectService;
			_camundaService = camundaService;
			_sensorTaskService = sensorTaskService;
			_amazonS3Service = amazonS3Service;
			_activityMapService = activityMapService;
			_pinterestService = pinterestService;
			_activityAndLocationHistoryService = activityAndLocationHistoryService;
		}

		// GET: api/Tasks
		[HttpGet]
		[Authorize(Roles = "admin, manager")]
		public ActionResult<List<Task>> Get() =>
			_taskService.Get();

		// GET: api/Tasks/5
		[HttpGet("{processInstanceId}/{activityId}", Name = "GetTask")]
		[Authorize(Roles = "admin, manager")]
		public ActionResult<TaskDTO> Get(string processInstanceId, string activityId)
		{
			var task = _taskService.GetByActivityId(processInstanceId, activityId);
			if (task == null)
				return NotFound();

			return new TaskDTO(task);
		}

		// GET: api/Tasks/invoice:112/Diagram/History
		/// <summary>
		/// Method that retrieves the 'HistoryTasks' object for the Diagram in which the task from the requested caseInstanceId 
		/// is currently in. This object is composed by an array of the current activity id's, the previously completed activity 
		/// id's, and some completed sequence flows infered from the variables existing in the system (variables used to pick 
		/// paths in gateways).
		/// </summary>
		/// <param name="caseInstanceId"></param>
		/// <returns>A List with the id's of the tasks completed in the diagram, current tasks, and sequence flows in the 
		/// system.</returns>
		[HttpGet("{processInstanceId}/Diagram/History", Name = "GetCurrentDiagramHistory")]
		[Authorize(Roles = "admin, manager")]
		public async Task<ActionResult<HistoryTasks>> GetCurrentDiagramHistoryAsync(string processInstanceId)
		{
			List<CamundaHistoryTask> historyTasks = await _camundaService.GetDiagramTaskHistoryAsync(processInstanceId);

			List<CamundaHistoryTask> currentTasks = historyTasks.Where(t => t.EndTime == null).ToList();

			List<string> currentTasksActivityIds = new();

			foreach (CamundaHistoryTask task in currentTasks)
			{
				currentTasksActivityIds.Add(task.ActivityId);
				historyTasks.Remove(task);

			}

			List<CamundaHistoryVariables> historyVariables = await _camundaService.GetDiagramVariableHistoryAsync(processInstanceId);

			return new HistoryTasks(currentTasksActivityIds, historyTasks.Select(t => t.ActivityId).ToList(), 
				historyVariables.Select(v => (string)Convert.ToString(v.Value)).ToList());
		}

		// GET: api/Tasks/invoice:112/Diagram/Predictions
		/// <summary>
		/// Method that receives the 'caseInstanceId' of a project and returns the tasks "predicted" based on sensor information. 
		/// These predictions are made by comparing the possible tasks to be predicted in the current diagram (mapped before in 
		/// form of 'ActivityMap' objects) to the recently detected sensor information.
		/// </summary>
		/// <param name="caseInstanceId"></param>
		/// <returns>A List of task predictions containing 'ActivityId', 'StartTime', and 'EndTime'</returns>
		[HttpGet("{caseInstanceId}/Diagram/Predictions", Name = "GetCurrentDiagramPredictions")]
		[Authorize(Roles = "admin, manager")]
		public ActionResult<List<TaskPredictionsDTO>> GetCurrentDiagramPredictions(string caseInstanceId)
		{
			Project currentProject = _projectService.GetByCaseInstanceId(caseInstanceId);

			if (currentProject == null) return NotFound();

			// get the last processDefinitionId from the project with the requested caseInstanceId
			string processInstanceId = currentProject.ProcessInstanceIds.LastOrDefault();

			if (processInstanceId == null) return NotFound();

			IEnumerable<SensorTask> predictions = _sensorTaskService.GetByProjectId(currentProject.Id).OrderBy(pred => pred.StartTime);

			List<ActivityMap> mappings = _activityMapService.GetByDiagramId(currentProject.LastDiagramId).OrderBy(map => map.DiagramOrder).ToList();

			List<TaskPredictionsDTO> predictionsToReturn = new();

			if (mappings.Count > 0)
			{
				foreach (SensorTask task in predictions)
				{

					foreach (ActivityMap activity in mappings)
					{
						// check if a mapping has any of the found sensor events
						if (activity.SensorsRelated.Exists(s => task.Events.Contains(s)))
						{
							predictionsToReturn.Add(new TaskPredictionsDTO { ActivityId = activity.ActivityId, StartTime = task.StartTime, EndTime = task.EndTime });
							mappings.Remove(activity); // remove the mapping to avoid correlating another SensorTask to it

							task.ActivityId = activity.ActivityId;
							_sensorTaskService.Update(task.Id, task); // update the object with the id to indicate it was a prediction for it
						}

						break;
					}
				}
			}

			return predictionsToReturn;
		}

		// GET: api/Tasks/invoice:112/History
		/// <summary>
		/// Method that retrieves all the tasks that have been completed in Camunda Workflow Engine, joined with their extra
		/// information objects in our database ('Task' objects) to build the final restoration document.
		/// </summary>
		/// <param name="caseInstanceId"></param>
		/// <returns>A List of 'FullHistoryTaskDTO'.</returns>
		[HttpGet("{caseInstanceId}/History", Name = "GetFullProjectHistory")]
		public async Task<ActionResult<List<FullHistoryTaskDTO>>> GetFullProjectHistoryAsync(string caseInstanceId)
		{
			ClaimsPrincipal claims = JwtManager.GetPrincipal(JwtManager.GetToken(Request));
			string role = claims.FindFirst(c => c.Type == ClaimTypes.Role).Value;

			Project currentProject = _projectService.GetByCaseInstanceId(caseInstanceId);

			if (currentProject == null) return NotFound();
			if (role == "owner" && currentProject.OwnerEmail !=claims.FindFirst(c => c.Type == ClaimTypes.Email).Value)
			{
				return Forbid();
			}

			// get the last processDefinitionId from the project with the requested caseInstanceId
			List<CamundaProcessInstance> processInstances = await _camundaService.GetProccessInstancesFromCaseAsync(caseInstanceId);
			//List<string> processInstanceIds = currentProject.ProcessInstanceIds;

			if (processInstances.Count == 0) return new List<FullHistoryTaskDTO>();

			List<CamundaHistoryTask> historyTasks = new();
			List<Task> relatedTasks = new();

			// gather the whole history by joining the history of the multiple diagrams
			foreach (CamundaProcessInstance processInstance in processInstances)
			{
				List<CamundaHistoryTask> diagramHistoryTasks = await _camundaService.GetDiagramTaskHistoryAsync(processInstance.Id);
				historyTasks = historyTasks.Concat(diagramHistoryTasks).ToList();

				List<Task> diagramRelatedTasks = _taskService.GetByProcessInstanceId(processInstance.Id);
				relatedTasks = relatedTasks.Concat(diagramRelatedTasks).ToList();
			}

			// organize the entire history by 'StartTime'
			historyTasks = historyTasks.OrderBy(t => t.StartTime).ToList();

			// remove current tasks if they exist
			historyTasks = historyTasks.Where(t => t.EndTime != null).ToList();

			List<FullHistoryTaskDTO> history = new();

			// make the connection between the history tasks from camunda and the tasks saved in our database
			foreach(CamundaHistoryTask historyTask in historyTasks)
			{
				Task taskFound = relatedTasks.FirstOrDefault(rt => (rt.ProcessInstanceId == historyTask.ProcessInstanceId 
					&& rt.ActivityId == historyTask.ActivityId) );
				if(taskFound != null)
					history.Add(new FullHistoryTaskDTO(historyTask, taskFound) );
				
			}

			return history;
		}

		private async Task<PathNode> BuildPathAsync(PathNode n, Dictionary<string, PathNode> dict)
        {
			if (dict.ContainsKey(n.Self.InstanceId))
				return null;
			dict.Add(n.Self.InstanceId, n);
			var parent = await _camundaService.GetSuperProcessAsync(n.Self.InstanceId);
			if (parent == null)
				return n;
			PathNode parentNode;
			if (dict.ContainsKey(parent.Id))
			{
				parentNode = dict.GetValueOrDefault(parent.Id);
				parentNode.Children.Add(n);
				return null;
			}
            var list = new List<PathNode> {n};
            parentNode = new PathNode(new PathProcess(parent, (await _camundaService.GetProcessDefinitionAsync(parent.DefinitionId)).Name), list);
			return await BuildPathAsync(parentNode, dict);
        }

		[HttpGet("{caseInstanceId}/Path", Name = "GetPath")]
		[Authorize(Roles = "admin, manager")]
		public async Task<ActionResult<PathNode>> GetPath(string caseInstanceId)
		{
			List<CamundaTask> l = await _camundaService.GetOpenTasksFromProcessAsync(caseInstanceId);
			Dictionary<string, PathNode> dict = new();
			PathNode root = null;

			foreach (var k in l)
            {
				var processInstance = await _camundaService.GetProcessInstanceAsync(k.ProcessInstanceId);
				PathNode n = await BuildPathAsync(new PathNode(new PathProcess(processInstance, (await _camundaService.GetProcessDefinitionAsync(processInstance.DefinitionId)).Name), new List<PathNode>()), dict);
				if (n != null)
					root = n;
            }
			return root;
        }

		[HttpGet("{caseInstanceId}/Root", Name = "GetProjectRoot")]
		[Authorize(Roles = "admin, manager")]
		public async Task<ActionResult<PathNode>> GetProjectRoot(string caseInstanceId)
		{
			var processInstance = await _camundaService.GetRootProcessAsync(caseInstanceId);
			var processDefinition = await _camundaService.GetProcessDefinitionAsync(processInstance.ProcessDefinitionId);
			return new PathNode(new PathProcess(processInstance, processDefinition.Name), new List<PathNode>());
        }

		[HttpGet("Called/{processInstanceId}/{taskId}", Name = "GetCalledProcess")]
		[Authorize(Roles = "admin, manager")]
		public async Task<string> GetCalledProcessAsync(string processInstanceId, string taskId)
		{
			var task = await _camundaService.GetHistoryTask(processInstanceId, taskId);
			
			return task?.CalledProcessInstanceId;
        }

		/// <summary>
		/// Method to be used to Process Mining purposes that returns all the tasks completed in Camunda Workflow Engine, joined
		/// with their respective 'ProjectName' and 'LicencePlate'
		/// </summary>
		/// <returns>A List of 'ActivityMiningRow'.</returns>
		[HttpGet("Mining", Name = "GetActivityDataForMining")]
		[Authorize(Roles = "admin, manager")]
		public async Task<ActionResult<List<ActivityMiningRow>>> GetActivityDataForMiningAsync()
		{
			// the list of activity events to return
			List<ActivityMiningRow> history = new();

			List<Project> projects = _projectService.Get();

			foreach(Project project in projects)
			{
				if (project == null) return NotFound();
				// get the last processDefinitionId from the project with the requested caseInstanceId
				List<string> processInstanceIds = project.ProcessInstanceIds;

				if (processInstanceIds.Count == 0) return new List<ActivityMiningRow>();

				List<CamundaHistoryTask> historyTasks = new();
				List<Task> relatedTasks = new();

				// gather the whole history by joining the history of the multiple diagrams
				foreach (string processInstanceId in processInstanceIds)
				{
					List<CamundaHistoryTask> diagramHistoryTasks = await _camundaService.GetDiagramTaskHistoryAsync(processInstanceId);
					historyTasks = historyTasks.Concat(diagramHistoryTasks).ToList();

					List<Task> diagramRelatedTasks = _taskService.GetByProcessInstanceId(processInstanceId);
					relatedTasks = relatedTasks.Concat(diagramRelatedTasks).ToList();
				}

				// organize the entire history by 'StartTime'
				historyTasks = historyTasks.OrderBy(t => t.StartTime).ToList();

				// remove current tasks if they exist
				historyTasks = historyTasks.Where(t => t.EndTime != null).ToList();

				// make the connection between the history tasks from camunda and the tasks saved in our database
				foreach (CamundaHistoryTask historyTask in historyTasks)
				{
					Task taskFound = relatedTasks.FirstOrDefault(rt => (rt.ProcessInstanceId == historyTask.ProcessInstanceId
						&& rt.ActivityId == historyTask.ActivityId));

					history.Add(new ActivityMiningRow(project, historyTask, taskFound));

				}
			}

			return history;
		}

		// PUT: api/Tasks/5
		[HttpPut("{id:length(24)}")]
		[Authorize(Roles = "admin, manager")]
		public IActionResult Update(string id, Task taskIn)
		{
			var task = _taskService.Get(id);

			if (task == null)
			{
				return NotFound();
			}

			_taskService.Update(id, taskIn);

			return NoContent();
		}

		// POST: api/Tasks
		[HttpPost]
		[Authorize(Roles = "admin, manager")]
		public ActionResult<Task> Create(Task task)
		{
			_taskService.Create(task);

			return CreatedAtRoute("GetTask", new { id = task.Id.ToString() }, task);
		}

		// POST: api/Tasks/5/Approve
		/// <summary>
		/// Method that follows the tasks in the 'tasks' object received, approving one by one in the Camunda Workflow Engine
		/// and creating the 'Task' objects in the database to save the inputed start and completion time.
		/// </summary>
		/// <param name="projectId">the id of the project related to the tasks</param>
		/// <param name="tasks">the tasks, necessary workflow variables, and task timings to approve</param>
		/// <returns>NoContent if successful, or NotFound if at least one task to be approved is not currently approvable</returns>
		[DisableRequestSizeLimit]
		[HttpPost("{projectId}/Approve")]
		[Authorize(Roles = "admin, manager")]
		public async Task<IActionResult> ApproveCamundaTasksAsync(string projectId, TasksToApprove tasks)
		{
			Project currentProject = _projectService.Get(projectId);
			// get the process instance from the Project object
			string currentProcessInstanceId = tasks.ProcessInstanceId;

			if (currentProcessInstanceId == null)
				return NotFound();

			// start by triggering the Signal Start Events of the subprocesses
			foreach (string signalRefName in tasks.StartEventTriggers)
			{
				string triggerResult = await _camundaService.TriggerCamundaSignalStartEvent(signalRefName, currentProcessInstanceId);

				if (triggerResult != currentProcessInstanceId)
					return NotFound();
			}

			if(!await _pinterestService.CheckAndUpdateCredentialsAsync())
				return Unauthorized();

			List<SensorTask> predictions = _sensorTaskService.GetByProjectId(currentProject.Id).ToList();
			
			foreach (TaskToApprove task in tasks.Tasks)
			{
				List<CamundaTask> currentTasks = await _camundaService.GetOpenTasksByProcessInstanceIDAsync(currentProcessInstanceId);

				string id = null;
				var taskToApprove = currentTasks.Find(t => t.TaskDefinitionKey == task.Id);

				// if no current open task corresponds, check if a message task is waiting
				if (taskToApprove == null)
				{
					// check if the task is a 'ReceiveTask' to approve accordingly
					if (task.Message != "")
                    {
						id = await _camundaService.SignalCamundaReceiveTask(task.Message, currentProcessInstanceId, tasks.Variables);
						taskToApprove = await _camundaService.GetTask(id);
                    }
				}
				else
					id = await _camundaService.CompleteCamundaTask(taskToApprove.Id, tasks.Variables);

				if (id == null)
					return NotFound();
				else
				{
					var sections = await _pinterestService.GetBoardSections(currentProject.PinterestBoardId);
					string boardSectionId = null;
					string sectionUrl = null;
					var pins = new List<string>();

					if ((task.Media.Any() || task.ExtraMedia.Any()) && sections != null) {
						HttpResponseMessage result;
						var n = 0;
						do
						{
							if (n > sections.Count)
								return BadRequest();
							result = await _pinterestService.CreateBoardSection(currentProject.PinterestBoardId, (n == 0 ? "" : n + "_") + taskToApprove.Name);
							n++;
						}
						while (!result.IsSuccessStatusCode);

						var boardSection = await result.Content.ReadAsAsync<PinterestBoardSection>();


						boardSectionId = boardSection.Id;
						sectionUrl = Regex.Replace(boardSection.Name.ToLower().Replace(" ", "-"), @"-+", "-").ToLower();
						if (sectionUrl.Length > 50)
							sectionUrl = sectionUrl[..50];
						sectionUrl = currentProject.PinterestBoardUrl + "/" + sectionUrl;

						// upload each media file into the new board section
						foreach (string media in task.Media) {
							var response = await _pinterestService.CreatePin(media, currentProject.PinterestBoardId, boardSectionId);
							string mediaId = (await response.Content.ReadAsAsync<dynamic>()).id;
							pins.Add(mediaId);
						}
						foreach (string media in task.ExtraMedia)
							_pinterestService.CreatePin(media, currentProject.PinterestBoardId, boardSectionId);
					}

					
					var newTask = _taskService.Create(new Task(task.Id, currentProcessInstanceId, task.StartTime, task.CompletionTime, task.CommentReport, task.CommentExtra, boardSectionId, sectionUrl, pins, ""));
					_activityAndLocationHistoryService.AddNewActivityAndLocationToCar(currentProject.CaseInstanceId, new ActivityAndLocation(newTask.Id,null));

					// delete the prediction if it was submitted
					SensorTask prediction = predictions.Find(p => p.ActivityId != null && p.ActivityId == taskToApprove.TaskDefinitionKey);
					if (prediction != null)
						_sensorTaskService.Remove(prediction);

					// complete the current project
					if (taskToApprove.TaskDefinitionKey == "Activity_0eelwrr") // last task of the diagram
						_projectService.Complete(currentProject.Id, currentProject);
				}
			}
			return Ok();
		}

		[DisableRequestSizeLimit]
		[HttpPost("{processInstanceId}/{taskId}/Update")]
		[Authorize(Roles = "admin, manager")]
		public async Task<IActionResult> UpdateTask(string processInstanceId, string taskId, TaskUpdate updates)
        {
			var task = _taskService.GetByActivityId(processInstanceId, taskId);
			var camundaTask = await _camundaService.GetTask(processInstanceId, taskId);
			var project = _projectService.GetByProcessInstanceId(processInstanceId);
			var sections = await _pinterestService.GetBoardSections(project.PinterestBoardId);

			if ((task.BoardSectionId == null || !sections.Exists(sec => sec.Id == task.BoardSectionId)) && (updates.Media.Length > 0 || updates.ExtraMedia.Length > 0))
			{
				//var name = Regex.Replace(camundaTask.Name.Trim(), @" +", " ").Replace("/", "").Replace(":", "");
				//var n = 0;
				// get a unique name
				//while (sections.Any(section => section.Name == name))
				//name =  n++ + "_" + Regex.Replace(camundaTask.Name.Trim(), @" +", " ").Replace("/", "").Replace(":", "");

				HttpResponseMessage result;
				var n = 0;
				do
				{
					if (n > sections.Count)
						return BadRequest();
					result = await _pinterestService.CreateBoardSection(project.PinterestBoardId, (n == 0 ? "" : n + "_") + camundaTask.Name);
					n++;
				}
				while (!result.IsSuccessStatusCode);

				//Console.WriteLine(name);
				//Console.WriteLine(await result.Content.ReadAsStringAsync());

				var boardSection = await result.Content.ReadAsAsync<PinterestBoardSection>();
				//sections = await _pinterestService.GetBoardSections(project.PinterestBoardId);
				//var boardSection = sections.Find(s => s.Name == name);

				task.BoardSectionId = boardSection.Id;
				var sectionUrl = Regex.Replace(boardSection?.Name.ToLower().Replace(" ", "-"), @"-+", "-");
				if (sectionUrl.Length > 50)
					sectionUrl = sectionUrl[..50];
				task.BoardSectionUrl = project.PinterestBoardUrl + "/" + sectionUrl;

				_taskService.Update(task.Id, task);
			}
			foreach (var file in updates.Media)
			{
				string[] sep = { ":", ";", "," };
				string[] data = file.Split(sep, StringSplitOptions.RemoveEmptyEntries);

				var mediaType = data[1].Split("/")[0];

				if (mediaType == "video")
					_pinterestService.CreatePin(file, project.PinterestBoardId, task.BoardSectionId);
				else if (mediaType == "image")
				{
					var response = await _pinterestService.CreatePin(file, project.PinterestBoardId, task.BoardSectionId);
					string mediaId = (await response.Content.ReadAsAsync<dynamic>()).id;
					task.Pins.Add(mediaId);
				}
			}
			foreach (var file in updates.ExtraMedia)
				_pinterestService.CreatePin(file, project.PinterestBoardId, task.BoardSectionId);
			task.StartTime = DateTime.Parse(updates.StartDate);
			task.CompletionTime = DateTime.Parse(updates.CompletionDate);
			task.CommentReport = updates.CommentReport;
			task.CommentExtra = updates.CommentExtra;
			_taskService.Update(task.Id, task);

			return Ok();
        }

		// POST: api/Tasks/Sensor/SensorBox_01/processed_SensorBox_01_data_07_09_2021_2.json
		/// <summary>
		/// Method that serves as a trigger to retrieve sensor information from the Amazon servers.
		/// </summary>
		/// <param name="folderName">the name of the folder to get the file from (i.e. 'SensorBox_01')</param>
		/// <param name="fileName">the name of the file to retrieve (i.e. 'processed_SensorBox_01_data_07_09_2021_1.json')</param>
		/// <returns>a List containing the parsed SensorInfo objects</returns>
		[HttpPost("Sensor/{folderName}/{fileName}")]
		[Authorize(Roles = "admin, manager")]
		public async Task<List<AmazonS3SensorInfo>> NewSensorInformationAvailable(string folderName, string fileName)
		{
			List<AmazonS3SensorInfo> list = await _amazonS3Service.GetSensorInformation(folderName, fileName);

			List<AmazonS3SensorInfo> tmp = list;

			list.ForEach(si =>
			{
				Project p = _projectService.GetByLicencePlate(si.Car);
				if (p != null)
				{
					SensorTask st = new SensorTask(si, p.Id);
					_sensorTaskService.Create(st);
				}
			});

			return tmp;
		}

		// DELETE: api/Tasks/5
		[HttpDelete("{id:length(24)}")]
		[Authorize(Roles = "admin")]
		public IActionResult Delete(string id)
		{
			var task = _taskService.Get(id);

			if (task == null)
			{
				return NotFound();
			}

			_taskService.Remove(task.Id);

			return NoContent();
		}

		[DisableRequestSizeLimit]
		[HttpGet("getBC/{processInstanceId}/{activityId}/")]
		[Authorize(Roles = "admin, manager")]
		public async Task<ActionResult<CamundaTaskDTO>> GetTaskBC(string processInstanceId, string activityId)
		{
			var camundaTask = await _camundaService.GetTaskForBC(processInstanceId, activityId);
			if (camundaTask == null)
				return NotFound();

			return new CamundaTaskDTO(camundaTask);
		}

		[DisableRequestSizeLimit]
		[HttpPut("{processInstanceId}/{taskId}/updateWithBcId/{taskBcId}")]
		[Authorize(Roles = "admin, manager")]
		public async Task<IActionResult> updateWithBcId(string processInstanceId, string taskId, string taskBcId)
        {
			var task = _taskService.GetByActivityId(processInstanceId, taskId);
			task.BlockChainId = taskBcId;
			_taskService.Update(task.Id, task);

			return Ok();
		}

			// GET: api/Task/5
        [HttpGet("{id:length(24)}", Name = "GetTaskById")]
        [Authorize]
        public ActionResult<Task> Get(string id)
        {
            var task = _taskService.Get(id);

            if (task == null)
            {
                return NotFound();
            }

            return task;
        }
	}
}
