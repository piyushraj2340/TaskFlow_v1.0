using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/collections/{collectionId}/[controller]")]
    public class ItemsController : ControllerBase
    {
        private readonly IItemService _itemService;

        public ItemsController(IItemService itemService)
        {
            _itemService = itemService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: api/collections/5/items
        [HttpGet]
        public async Task<IActionResult> GetItems(int collectionId)
        {
            var items = await _itemService.GetCollectionItemsAsync(collectionId, GetUserId());
            if (items == null)
            {
                return NotFound(new { message = "Collection not found or access denied." });
            }
            return Ok(items);
        }

        // GET: api/collections/5/items/1
        [HttpGet("{itemId}")]
        public async Task<IActionResult> GetItem(int collectionId, int itemId)
        {
            var item = await _itemService.GetItemByIdAsync(itemId, collectionId, GetUserId());
            if (item == null)
            {
                return NotFound(new { message = "Item or Collection not found." });
            }
            return Ok(item);
        }

        // POST: api/collections/5/items
        [HttpPost]
        public async Task<IActionResult> CreateItem(int collectionId, [FromBody] CreateItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (newItem, error) = await _itemService.CreateItemAsync(collectionId, itemDto, GetUserId());

            if (error != null)
            {
                if (error.Contains("not found"))
                    return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }

            return CreatedAtAction(nameof(GetItem), new { collectionId, itemId = newItem.ItemId }, newItem);
        }

        // PUT: api/collections/5/items/1
        [HttpPut("{itemId}")]
        public async Task<IActionResult> UpdateItem(int collectionId, int itemId, [FromBody] UpdateItemDto itemDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error) = await _itemService.UpdateItemAsync(itemId, collectionId, itemDto, GetUserId());

            if (error != null)
            {
                if (error.Contains("not found"))
                    return NotFound(new { message = error });
                return BadRequest(new { message = error });
            }

            return NoContent();
        }

        // DELETE: api/collections/5/items/1
        [HttpDelete("{itemId}")]
        public async Task<IActionResult> DeleteItem(int collectionId, int itemId)
        {
            var (success, error) = await _itemService.DeleteItemAsync(itemId, collectionId, GetUserId());

            if (error != null)
            {
                return NotFound(new { message = error });
            }

            return NoContent();
        }
    }
}
