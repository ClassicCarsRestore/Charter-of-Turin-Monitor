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

		
	}
}