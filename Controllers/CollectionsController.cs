using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CollectionsController : ControllerBase
    {
        private readonly ICollectionService _collectionService;

        public CollectionsController(ICollectionService collectionService)
        {
            _collectionService = collectionService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: api/collections
        [HttpGet]
        public async Task<IActionResult> GetCollections()
        {
            var collections = await _collectionService.GetUserCollectionsAsync(GetUserId());
            return Ok(collections);
        }

        // GET: api/collections/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetCollection(int id)
        {
            var collection = await _collectionService.GetUserCollectionByIdAsync(id, GetUserId());
            if (collection == null)
            {
                return NotFound(new { message = "Collection not found." });
            }
            return Ok(collection);
        }

        // POST: api/collections
        [HttpPost]
        public async Task<IActionResult> CreateCollection([FromBody] CreateCollectionDto collectionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (newCollection, error) = await _collectionService.CreateCollectionAsync(collectionDto, GetUserId());

            if (error != null)
            {
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetCollection), new { id = newCollection.CollectionId }, newCollection);
        }

        // PUT: api/collections/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCollection(int id, [FromBody] UpdateCollectionDto collectionDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error) = await _collectionService.UpdateCollectionAsync(id, collectionDto, GetUserId());

            if (error != null)
            {
                // Distinguish between Not Found and other errors
                if (error.Contains("not found"))
                    return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }

            return NoContent();
        }

        // DELETE: api/collections/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCollection(int id)
        {
            var (success, error) = await _collectionService.DeleteCollectionAsync(id, GetUserId());

            if (error != null)
            {
                return NotFound(new { message = error });
            }

            return NoContent();
        }
    }
}
