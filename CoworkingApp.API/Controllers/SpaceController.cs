using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CoworkingApp.API.DTOs;
using CoworkingApp.API.Interfaces;

namespace CoworkingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SpaceController : ControllerBase
    {
        private readonly ISpaceService _spaceService;

        public SpaceController(ISpaceService spaceService)
        {
            _spaceService = spaceService;
        }

        // --- PUBLIC ENDPOINTS ---

        [HttpGet]
        public async Task<IActionResult> SearchSpaces([FromQuery] SpaceSearchRequest request)
        {
            var spaces = await _spaceService.SearchSpacesAsync(request);
            return Ok(spaces);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetSpaceById(Guid id)
        {
            var space = await _spaceService.GetSpaceByIdAsync(id);
            return Ok(space);
        }
        
        [HttpGet("locations")]
        public async Task<IActionResult> GetLocations()
        {
            var locations = await _spaceService.GetAllLocationsAsync();
            return Ok(locations);
        }

        // --- ADMIN ENDPOINTS ---

        [Authorize(Policy = "AdminOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateSpace([FromBody] CreateSpaceRequest request)
        {
            var result = await _spaceService.CreateSpaceAsync(request);
            return CreatedAtAction(nameof(GetSpaceById), new { id = result.SpaceId }, result);
        }

        [Authorize(Policy = "AdminOnly")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSpace(Guid id)
        {
            var success = await _spaceService.DeleteSpaceAsync(id);
            if (!success) return NotFound();
            return NoContent();
        }
        [Authorize(Policy = "AdminOnly")]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSpace(Guid id, [FromBody] UpdateSpaceRequest request)
        {
            var result = await _spaceService.UpdateSpaceAsync(id, request);
            return Ok(result);
        }
    }
}