using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using tasklist.Models;
using Task = System.Threading.Tasks.Task;

namespace tasklist.Services
{
    /// <summary>
    /// Service that establishes the communication between the Camunda Workflow Engine and our application by 
    /// using REST calls made by HttpClient.
    /// </summary>
    public class CamundaService
    {
        private static readonly string BASE_URL = "http://194.210.120.34:591/engine-rest/"; //"http://localhost:591/engine-rest/"

        private static readonly HttpClient _client;

        static CamundaService()
        {
            _client = new HttpClient();
        }

        /// <summary>
        /// Make a request to Camunda Workflow Engine to retrieve all open tasks awaiting completion.
        /// </summary>
        /// <returns>A List containing all the open tasks.</returns>
        public async Task<List<CamundaTask>> GetOpenTasksAsync()
        {
            List<CamundaTask> tasks = new();

            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "task");
            if (response.IsSuccessStatusCode)
            {
                tasks = await response.Content.ReadFromJsonAsync<List<CamundaTask>>();
            }

            return tasks;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="caseInstanceId"></param>
        /// <returns></returns>
        public async Task<List<CamundaTask>> GetOpenTasksFromProcessAsync(string caseInstanceId)
        {
            List<CamundaTask> tasks = new();

            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "task");
            if (response.IsSuccessStatusCode)
            {
                tasks = await response.Content.ReadFromJsonAsync<List<CamundaTask>>();
            }

            tasks = tasks.FindAll(t => t.CaseInstanceId == caseInstanceId);

            return tasks;
        }

        public async Task<CamundaProcessInstance> GetProcessInstanceAsync(string processInstanceId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "process-instance/" + processInstanceId);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CamundaProcessInstance>();
            return null;
        }

        public async Task<CamundaHistoryProcessInstance> GetHistoryProcessInstanceAsync(string processInstanceId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/process-instance/" + processInstanceId);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CamundaHistoryProcessInstance>();
            return null;
        }

        public async Task<List<CamundaProcessInstance>> GetProccessInstancesFromCaseAsync(string caseInstanceId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/process-instance?caseInstanceId=" + caseInstanceId);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<List<CamundaProcessInstance>>();
            return null;
        }

        public async Task<CamundaProcessInstance> GetSuperProcessAsync(string processInstanceId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "process-instance?subProcessInstance=" + processInstanceId);
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadFromJsonAsync<List<CamundaProcessInstance>>()).LastOrDefault();
            return null;
        }

        public async Task<CamundaProcessDefinition> GetProcessDefinitionAsync(string definitionId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "process-definition/" + definitionId);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<CamundaProcessDefinition>();
            return null;
        }

        public async Task<CamundaTask> GetTask(string activityId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "task/" + activityId);
            if (response.IsSuccessStatusCode) {
                return await response.Content.ReadFromJsonAsync<CamundaTask>();
            }
            return null;
        }

        public async Task<CamundaTask> GetTask(string processInstanceId, string activityId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/task?processInstanceId=" + processInstanceId + "&activityId=" + activityId);
            if (response.IsSuccessStatusCode) {
                var t = await response.Content.ReadFromJsonAsync<List<CamundaTask>>();
                return t.FirstOrDefault();
            }
            return null;
        }

        public async Task<CamundaTask> GetTaskForBC(string processInstanceId, string activityId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/task?processInstanceId=" + processInstanceId + "&activityId=" + activityId);
            if (response.IsSuccessStatusCode) {
                var t = await response.Content.ReadFromJsonAsync<List<CamundaTask>>();
                var taskBC = t.Where(item => item.TaskDefinitionKey == activityId).LastOrDefault();
                return taskBC;
            }
            return null;
        }

        public async Task<CamundaHistoryTask> GetHistoryTask(string processInstanceId, string activityId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/activity-instance?processInstanceId=" + processInstanceId + "&activityId=" + activityId);
            if (response.IsSuccessStatusCode) {
                var t = await response.Content.ReadFromJsonAsync<List<CamundaHistoryTask>>();
                return t.FirstOrDefault();
            }
            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <returns></returns>
        public async Task<List<CamundaTask>> GetOpenTasksByProcessInstanceIDAsync(string processInstanceId)
        {
            List<CamundaTask> tasks = new();

            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "task?processInstanceId=" + processInstanceId);
            if (response.IsSuccessStatusCode)
            {
                tasks = await response.Content.ReadFromJsonAsync<List<CamundaTask>>();
            }

            return tasks;
        }

        /// <summary>
        /// Make a request to Camunda Workflow Engine to start a process with the given 'processId' and a 
        /// given 'caseInstanceId'.
        /// </summary>
        /// <param name="processId">The id of the process to begin.</param>
        /// <param name="caseInstanceIdToCreate">The caseInstantId to identify the process.</param>
        /// <returns>The caseInstanceId used to begin the process.</returns>
        public async Task<string> StartProcessInstanceAsync(string processId, string caseInstanceIdToCreate, ProjectFormDTO projectForm)
        {
            Dictionary<string, PairKeyValueType> variables = new Dictionary<string, PairKeyValueType>();

            // Add necessary variables to the object
            variables.Add("clientExpectation", new PairKeyValueType() { Value = projectForm.ClientExpectation, Type = "String" });
            variables.Add("originalMaterials", new PairKeyValueType() { Value = projectForm.OriginalMaterials, Type = "Boolean" });
            variables.Add("carDocuments", new PairKeyValueType() { Value = projectForm.CarDocuments, Type = "Boolean" });

            var processArgs = new CamundaStartProcess()
            {
                Variables = variables,
                CaseInstanceId = caseInstanceIdToCreate
            };
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var processArgsSerialized = System.Text.Json.JsonSerializer.Serialize(processArgs, serializeOptions);
            var requestContent = new StringContent(processArgsSerialized, Encoding.UTF8, "application/json");

            string caseInstanceId = null;

            HttpResponseMessage response = await _client.PostAsync(BASE_URL + "process-definition/key/" + processId
                + "/start", requestContent);
            if (response.IsSuccessStatusCode)
            {
                caseInstanceId = caseInstanceIdToCreate;
                var res = response.Content.ReadAsStringAsync();
            }


            return caseInstanceId;
        }

        /// <summary>
        /// Make a request to Camunda Workflow Engine to retrieve the diagram related to the requested 
        /// processDefinitionId.
        /// </summary>
        /// <param name="processDefinitionId"></param>
        /// <returns>An object containing an ID and the XML of the diagram requested</returns>
        public async Task<CamundaDiagramXML> GetXMLAsync(string processDefinitionId)
        {
            CamundaDiagramXML xml = null;

            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "process-definition/" + processDefinitionId + "/xml");
            if (response.IsSuccessStatusCode)
            {
                xml = await response.Content.ReadFromJsonAsync<CamundaDiagramXML>();
            }

            return xml;
        }

        /// <summary>
        /// Make a request to Camunda Workflow Engine to retrieve the task history of the diagram related 
        /// to the requested processDefinitionId.
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <returns>A List of id's of the tasks in the diagram related to the processInstanceId.</returns>
        public async Task<List<CamundaHistoryTask>> GetDiagramTaskHistoryAsync(string processInstanceId)
        {
            List<CamundaHistoryTask> history = new();

            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/activity-instance?processInstanceId=" + processInstanceId);
            if (response.IsSuccessStatusCode)
            {
                history = await response.Content.ReadFromJsonAsync<List<CamundaHistoryTask>>();
            }

            // sort the list to order the elements by execution
            history = history.OrderBy(t => t.StartTime).ToList();

            /*
            // get all activityId's that correspond to UserTasks, ManualTasks or unassigned Tasks
            List<string> taskIds = history.Where(t => t.ActivityType == "userTask" || t.ActivityType == "manualTask" 
                || t.ActivityType == "task").Select(t => t.ActivityId).ToList();
            */

            // get all activityId's that correspond to UserTasks, ManualTasks or unassigned Tasks
            history = history.Where(t => t.ActivityType == "userTask" || t.ActivityType == "manualTask"
                || t.ActivityType == "task" || t.ActivityType == "callActivity" || t.ActivityType == "businessRuleTask"
                || t.ActivityType == "sendTask" || t.ActivityType == "receiveTask").ToList();

            return history;
        }

        /// <summary>
        /// Method that fetches all the variables from a 'processInstanceId' and returns a list with all of 
        /// them that have a 'Value' property different than an empty string. This is used to infer which of
        /// the paths was followed on that given 'processInstanceId' by making use of the generated variables
        /// using the methods described previously.
        /// </summary>
        /// <param name="processInstanceId"></param>
        /// <returns></returns>
        public async Task<List<CamundaHistoryVariables>> GetDiagramVariableHistoryAsync(string processInstanceId)
        {
            List<CamundaHistoryVariables> history = new();

            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/variable-instance?processInstanceId=" + processInstanceId);
            if (response.IsSuccessStatusCode)
            {
                history = await response.Content.ReadFromJsonAsync<List<CamundaHistoryVariables>>();
            }

            // sort the list to order the elements by creation time
            history = history.OrderBy(v => v.CreateTime).ToList();

            history = history.Where(t => (t.Type == "String") && (Convert.ToString(t.Value) != "")).ToList();

            return history;
        }

        /// <summary>
        /// Method that completes a Task inside Camunda Workflow Engine.
        /// </summary>
        /// <param name="id">the id of the task to complete</param>
        /// <param name="vars">the variables needed for completion</param>
        /// <returns>the id of the completed task</returns>
		public async Task<string> CompleteCamundaTask(string id, string[][] vars)
        {
            Dictionary<string, PairKeyValue> variables = new Dictionary<string, PairKeyValue>();

            foreach (string[] var in vars)
            {
                variables.Add(var[0], new PairKeyValue() { Value = var[1] });
            }

            var processArgs = new CamundaTaskApprove()
            {
                Variables = variables,
                WithVariablesInReturn = true
            };
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var processArgsSerialized = System.Text.Json.JsonSerializer.Serialize(processArgs, serializeOptions);
            var requestContent = new StringContent(processArgsSerialized, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(BASE_URL + "task/" + id
                + "/complete", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var res = response.Content.ReadAsStringAsync();
                return id;
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="id"></param>
        /// <param name="vars"></param>
        /// <returns></returns>
        public async Task<string> SignalCamundaReceiveTask(string messageName, string processInstanceId, string[][] vars)
        {
            Dictionary<string, PairKeyValueType> processVariables = new Dictionary<string, PairKeyValueType>();

            foreach (string[] var in vars)
            {
                processVariables.Add(var[0], new PairKeyValueType() { Value = var[1], Type = "String" }); // if var[0] == "" do nothing
            }

            var processArgs = new CamundaSignalReceiveTask()
            {
                MessageName = messageName,
                ProcessInstanceId = processInstanceId,
                ProcessVariables = processVariables
            };
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var processArgsSerialized = System.Text.Json.JsonSerializer.Serialize(processArgs, serializeOptions);
            var requestContent = new StringContent(processArgsSerialized, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(BASE_URL + "message", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var res = response.Content.ReadAsStringAsync();
                return processInstanceId;
            }
            else
            {
                var errorContent = JsonConvert.DeserializeObject<dynamic>(response.Content.ReadAsStringAsync().Result);
            }

            return null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="messageName"></param>
        /// <param name="processInstanceId"></param>
        /// <returns></returns>
        public async Task<string> TriggerCamundaSignalStartEvent(string messageName, string processInstanceId)
        {
            var processArgs = new CamundaTriggerSignalStartEvent()
            {
                Name = messageName,
                ExecutionId = processInstanceId
            };
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var processArgsSerialized = System.Text.Json.JsonSerializer.Serialize(processArgs, serializeOptions);
            var requestContent = new StringContent(processArgsSerialized, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(BASE_URL + "signal", requestContent);
            if (response.IsSuccessStatusCode)
            {
                var res = response.Content.ReadAsStringAsync();
                return processInstanceId;
            }

            return null;
        }

        public async Task TerminateProcess(string processInstanceId)
        {
            var processArgs = new CamundaTerminateProcess()
            {
                DeleteReason = "Process was cancelled by the Manager.",
                ProcessInstanceIds = new string[] { processInstanceId },
                SkipCustomListeners = false,
                SkipSubprocesses = false
            };
            var serializeOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                WriteIndented = true
            };
            var processArgsSerialized = System.Text.Json.JsonSerializer.Serialize(processArgs, serializeOptions);
            var requestContent = new StringContent(processArgsSerialized, Encoding.UTF8, "application/json");

            HttpResponseMessage response = await _client.PostAsync(BASE_URL + "process-instance/delete", requestContent);

            response.EnsureSuccessStatusCode();

        }

        public async void Delete(string caseInstanceId)
        {
            var instances = await GetProccessInstancesFromCaseAsync(caseInstanceId);
            foreach(var instance in instances)
                _client.DeleteAsync(BASE_URL + "process-instance/" + instance.Id);
        }

        public async Task<List<CamundaCalledProcessDefinition>> GetCalledProcessDefinitions(string processDefinitionId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "process-definition/" + processDefinitionId + "/static-called-process-definitions");
            if (response.IsSuccessStatusCode)
            {
                var calledProcessDefinitions = await response.Content.ReadFromJsonAsync<List<CamundaCalledProcessDefinition>>();
                return calledProcessDefinitions;
            }
            return null;
        }

        public async Task<CamundaProcessDefinition> GetRoot()
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "process-definition?latestVersion=true");
            if (response.IsSuccessStatusCode)
            {
                var processDefinitions = await response.Content.ReadFromJsonAsync<List<CamundaProcessDefinition>>();
                return processDefinitions.Find(x => x.Name.Contains("Base"));
            }
            return null;
        }

		public class Variable
        {
            public string VariableName { get; set; }
            public string Value { get; set; }
        }

        public async Task<string> GetProcessInstanceVariable(string processInstanceId, string variableName)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/variable-instance?processInstanceId=" + processInstanceId + "&variableName=" + variableName);
            if (response.IsSuccessStatusCode)
            {
                var variables = await response.Content.ReadFromJsonAsync<List<Variable>>();
                if (variables.Count == 0)
                    return " ";
                return variables[0].Value;
            }
            return null;
        }

        public async Task<CamundaHistoryProcessInstance> GetRootProcessAsync(string caseInstanceId)
        {
            HttpResponseMessage response = await _client.GetAsync(BASE_URL + "history/process-instance?caseInstanceId=" + caseInstanceId + "&rootProcessInstances=true");
            if (response.IsSuccessStatusCode)
                return (await response.Content.ReadFromJsonAsync<List<CamundaHistoryProcessInstance>>())[0];
            return null;
        }
    }
}
