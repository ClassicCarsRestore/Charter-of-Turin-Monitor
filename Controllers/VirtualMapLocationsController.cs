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
	public class VirtualMapLocationsController : ControllerBase
	{
		private readonly VirtualMapLocationService _virtualMapLocationService;

	public VirtualMapLocationsController(VirtualMapLocationService virtualMapLocationService)
		{
			_virtualMapLocationService = virtualMapLocationService;
		}


		// GET: api/VirtualMapLocations
		[HttpGet]
		[Authorize]
		public ActionResult<List<VirtualMapLocation>> Get() =>
			_virtualMapLocationService.Get();


			// GET: api/VirtualMapLocations/5
        [HttpGet("{id:length(24)}", Name = "GetVirtualMapLocation")]
        [Authorize]
        public ActionResult<VirtualMapLocation> Get(string id)
        {
            var virtualMapLocation = _virtualMapLocationService.Get(id);

            if (virtualMapLocation == null)
            {
                return NotFound();
            }

            return virtualMapLocation;
        }

		[HttpPost]
		[Authorize(Roles = "admin")]
		public ActionResult<VirtualMapLocation> CreateVirtualMapLocation(VirtualMapLocation request)
		{
        	_virtualMapLocationService.Create(request);

        	return CreatedAtRoute("GetVirtualMapLocation", new { id = request.Id.ToString() }, request);
		}

		[HttpDelete("{id:length(24)}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(string id)
        {
			var virtualMapLocation = _virtualMapLocationService.Get(id);

            if (virtualMapLocation == null)
            {
                return NotFound();
            }

			 //_virtualMapLocationService.Remove(id);
			 await System.Threading.Tasks.Task.Run(() => _virtualMapLocationService.Remove(id));

			return NoContent();
		}


		[HttpPut("{id:length(24)}")]
		[Authorize(Roles = "admin")]
		public async Task<IActionResult> Update(string id, VirtualMapLocation updatedVirtualMapLocation)
		{
    		var existingVirtualMapLocation = _virtualMapLocationService.Get(id);

    		if (existingVirtualMapLocation == null)
    		{
        		return NotFound();
    		}

    		existingVirtualMapLocation.Name = updatedVirtualMapLocation.Name;
    		existingVirtualMapLocation.CoordinateX = updatedVirtualMapLocation.CoordinateX;
    		existingVirtualMapLocation.CoordinateY = updatedVirtualMapLocation.CoordinateY;
			existingVirtualMapLocation.CoordinateZ = updatedVirtualMapLocation.CoordinateZ;
			existingVirtualMapLocation.ActivityIds = updatedVirtualMapLocation.ActivityIds;
			existingVirtualMapLocation.Vertices = updatedVirtualMapLocation.Vertices;
			existingVirtualMapLocation.Color = updatedVirtualMapLocation.Color;
			existingVirtualMapLocation.Capacity = updatedVirtualMapLocation.Capacity;

    		await System.Threading.Tasks.Task.Run(() => _virtualMapLocationService.Update(id,existingVirtualMapLocation));

    		return NoContent();
		}

			// GET: api/VirtualMapLocations/5
        [HttpGet("activity/{activityId}", Name = "GetLocationsWithActivity")]
        [Authorize]
        public ActionResult<List<string>> GetLocationIdsByActivityId(string activityId)
        {
            var virtualMapLocation = _virtualMapLocationService.GetLocationIdsByActivityId(activityId);

            if (virtualMapLocation == null)
            {
                return NotFound();
            }

            return virtualMapLocation;
        }

		
	}
}