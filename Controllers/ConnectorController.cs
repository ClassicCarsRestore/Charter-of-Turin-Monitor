using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using tasklist.Models;
using tasklist.Services;
using Task = tasklist.Models.Task;

namespace tasklist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ConnectorController : Controller
    {
        private readonly ProjectService _projectService;
        private readonly CamundaService _camundaService;
        private readonly TaskService _taskService;

        public ConnectorController(ProjectService projectService, CamundaService camundaService, TaskService taskService)
        {
            _projectService = projectService;
            _camundaService = camundaService;
            _taskService = taskService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDTO>>> GetOpenProjectsAsync()
        {
            if (Request.Headers["Authorization"] != Settings.Connector_Secret)
                return Unauthorized();
            var projects = _projectService.GetOpenProjects();

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
                    CamundaTask waitingForMessage = new CamundaTask { Name = "Waiting for message" };
                    projectDTOs.Add(new ProjectDTO(project, new List<CamundaTask>() { waitingForMessage }));
                }
            }
            return projectDTOs;
		}

		[HttpGet("{caseInstanceId}", Name = "GetHistory")]
		public async Task<ActionResult<List<string>>> GetFullProjectHistoryAsync(string caseInstanceId)
		{
			if (Request.Headers["Authorization"] != Settings.Connector_Secret)
				return Unauthorized();

			Project currentProject = _projectService.GetByCaseInstanceId(caseInstanceId);

			if (currentProject == null) return NotFound();

			// get the last processDefinitionId from the project with the requested caseInstanceId
			List<CamundaProcessInstance> processInstances = await _camundaService.GetProccessInstancesFromCaseAsync(caseInstanceId);
			//List<string> processInstanceIds = currentProject.ProcessInstanceIds;

			if (processInstances.Count == 0) return new List<string>();

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

			List<string> activities = new();

			// make the connection between the history tasks from camunda and the tasks saved in our database
			foreach (CamundaHistoryTask historyTask in historyTasks)
			{
				Task taskFound = relatedTasks.FirstOrDefault(rt => (rt.ProcessInstanceId == historyTask.ProcessInstanceId
					&& rt.ActivityId == historyTask.ActivityId));
				if (taskFound != null)
					activities.Add(historyTask.ActivityName);
			}

			return activities;
		}
	}
}
