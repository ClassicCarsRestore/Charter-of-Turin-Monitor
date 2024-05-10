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

namespace tasklist.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	public class ActivityAndLocationHistoryController : ControllerBase
	{
		private readonly ActivityAndLocationHistoryService _activityAndLocationHistoryService;

	public ActivityAndLocationHistoryController(ActivityAndLocationHistoryService activityAndLocationHistoryService)
		{
			_activityAndLocationHistoryService = activityAndLocationHistoryService;
		}

        // GET: api/HistoryLocationCars
		[HttpGet]
		[Authorize]
		public ActionResult<List<ActivityAndLocationHistory>> Get() =>
			_activityAndLocationHistoryService.Get();


        [HttpPost]
		[Authorize(Roles = "admin")]
		public ActionResult<ActivityAndLocationHistory> Create(ActivityAndLocationHistory request)
		{
        	_activityAndLocationHistoryService.Create(request);

        	return CreatedAtRoute("GetActivityAndLocationHistory", new { id = request.Id.ToString() }, request);
		}

        /**
	    // GET: api/HistoryLocationCars/5
        [HttpGet("{caseInstanceId}", Name = "GetActivityAndLocationHistoryByCar")]
        [Authorize]
        public ActionResult<ActivityAndLocationHistory> GetByCar(string caseInstanceId)
        {
            var historyLocationCars = _activityAndLocationHistoryService.GetByCaseInstanceId(caseInstanceId);

            if (historyLocationCars == null)
            {
                return NotFound();
            }

            return historyLocationCars;
        }
**/
	// GET: api/VirtualMapLocations/5
        [HttpGet("{id}", Name = "GetActivityAndLocationHistory")]
        [Authorize]
        public ActionResult<ActivityAndLocationHistory> Get(string id)
        {
            var virtualMapLocation = _activityAndLocationHistoryService.Get(id);

            if (virtualMapLocation == null)
            {
                return NotFound();
            }

            return virtualMapLocation;
        }


    }
}