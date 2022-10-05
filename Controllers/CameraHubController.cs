using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using tasklist.Models;
using tasklist.Services;

namespace tasklist.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CameraHubController : ControllerBase
    {

        private readonly CameraHubService _cameraHubService;
        private readonly ProjectService _projectService;
        private readonly PinterestService _pinterestService;
        public CameraHubController(CameraHubService cameraHubService, ProjectService projectService, PinterestService pinterestService)
        {
            _cameraHubService = cameraHubService;
            _projectService = projectService;
            _pinterestService = pinterestService;
        }

        [HttpPost("Authenticate", Name = "Authenticate")]
        public ActionResult<CameraHub> Authenticate(CameraHubCredentials credentials)
        {
            if (Request.Headers["Authorization"] != Settings.Camera_Hub_Secret)
                return Unauthorized();
            var cameraHub = _cameraHubService.Get(credentials.ProjectName, credentials.Password);
            if (cameraHub == null)
                return NotFound();
            return cameraHub;
        }

        [HttpPost("Snapshot", Name = "TakeSnapshot")]
        public async Task<ActionResult> TakeSnapshot(SnapshotRequest snap)
        {
            if (Request.Headers["Authorization"] != Settings.Camera_Hub_Secret)
                return Unauthorized();
            var cameraHub = _cameraHubService.Get(snap.Id);
            if (cameraHub == null)
                return NotFound();
            var project = _projectService.Get(cameraHub.ProjectId);
            if (cameraHub.BoardSectionId == null)
            {
                var result = await _pinterestService.CreateBoardSection(project.PinterestBoardId, "Meeting " + DateTime.Today.ToString("d").Replace("/", "_") + " Snapshots");
                var boardSection = await result.Content.ReadAsAsync<PinterestBoardSection>();
                cameraHub.BoardSectionId = boardSection.Id;
                _cameraHubService.Update(cameraHub.Id, cameraHub);
            }
            var credentials = new NetworkCredential(snap.Username, snap.Password);
            using (var handler = new HttpClientHandler { Credentials = credentials })
            using (var client = new HttpClient(handler))
            {
                var bytes = await client.GetByteArrayAsync(snap.Url);
                _pinterestService.CreatePin("data:image/jpeg;base64," + Convert.ToBase64String(bytes), project.PinterestBoardId, cameraHub.BoardSectionId);
            }
            return Ok();
        }

        [DisableRequestSizeLimit]
        [HttpPost("Video", Name = "UploadVideo")]
        public async Task<ActionResult> UploadVideo(VideoUpload video)
        {
            if (Request.Headers["Authorization"] != Settings.Camera_Hub_Secret)
                return Unauthorized();
            var cameraHub = _cameraHubService.Get(video.Id);
            if (cameraHub == null)
                return NotFound();
            var project = _projectService.Get(cameraHub.ProjectId);
            if (cameraHub.BoardSectionId == null)
            {
                var result = await _pinterestService.CreateBoardSection(project.PinterestBoardId, "Meeting " + DateTime.Today.ToString("d").Replace("/", "_") + " Snapshots");
                var boardSection = await result.Content.ReadAsAsync<PinterestBoardSection>();
                cameraHub.BoardSectionId = boardSection.Id;
                _cameraHubService.Update(cameraHub.Id, cameraHub);
            }
            _pinterestService.CreatePin("data:video/mkv;base64," + video.Data, project.PinterestBoardId, cameraHub.BoardSectionId);
            return Ok();
        }

        [HttpPost("Credentials", Name = "SendCredentials")]
        [Authorize(Roles = "admin")]
        public ActionResult SendCredentials(SendCredentials send)
        {
            var project = _projectService.Get(send.ProjectId);
            if (project == null)
                return NotFound();

            var projectName = (project.Make + "_" + project.Model + "_" + project.Year).Replace(" ", "_");

            var password = UtilManager.RandString(12);
            var cameraHub = new CameraHub() { ProjectId = send.ProjectId, ProjectName = projectName, Password = password, StartTime = send.StartTime, EndTime = send.EndTime, BoardSectionId = null};

            _cameraHubService.Schedule(cameraHub);

            var messageSubject = "Camera Hub Credentials";
            var messageBody = $"In order to access to the Camera Hub platform use the following credentials at the scheduled time:\nProject: {projectName}\nPassword: {password}";

            if (UtilManager.SendEmail(project.OwnerEmail, messageSubject, messageBody))
                return Ok();
            return BadRequest();
        }
    }
}
