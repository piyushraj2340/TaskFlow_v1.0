using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Services;

namespace TaskMonitoringApp.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CategoriesController : ControllerBase
    {
        private readonly ICategoryService _categoryService;

        public CategoriesController(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        private string GetUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

        // GET: api/categories
        [HttpGet]
        public async Task<IActionResult> GetUserCategories()
        {
            var categories = await _categoryService.GetUserCategoriesAsync(GetUserId());
            return Ok(categories);
        }

        // PUT: api/categories/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateCategoryDto categoryDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var (success, error) = await _categoryService.UpdateUserCategoryAsync(id, categoryDto, GetUserId());

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return NoContent();
        }

        // DELETE: api/categories/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var (success, error) = await _categoryService.DeleteUserCategoryAsync(id, GetUserId());

            if (!success)
            {
                return BadRequest(new { message = error });
            }

            return NoContent();
        }
    }
}
