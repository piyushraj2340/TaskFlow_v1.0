using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class SearchRepository(ApplicationDbContext context) : ISearchRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task<IEnumerable<SearchResultDTO>> SearchAsync(string userId, string query, int pageNumber, int pageSize)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Enumerable.Empty<SearchResultDTO>();
            }

            query = query.Trim();

            // Project each entity to the DTO and materialize separately to avoid EF set-operation translation issues.
            var goalsList = await _context.Goals
                .Where(g => g.UserId == userId && !g.IsDeleted &&
                            (EF.Functions.Like(g.Name!, $"%{query}%") ||
                             (g.Description != null && EF.Functions.Like(g.Description, $"%{query}%"))))
                .Select(g => new SearchResultDTO
                {
                    EntityId = g.Id,
                    Title = g.Name ?? string.Empty,
                    Url = "/Goals/Details/" + g.Id,
                    Type = "goal",
                    Status = g.GoalStatus,
                    Priority = g.Priority,
                    TimeStamp = g.CreatedOn,
                    IsPinned = null,
                    Snippet = g.Description != null ? (g.Description.Length > 200 ? g.Description.Substring(0, 200) + "…" : g.Description) : null
                })
                .ToListAsync();

            var tasksList = await _context.Tasks
                .Where(t => t.UserId == userId && !t.IsDeleted &&
                            (EF.Functions.Like(t.Name!, $"%{query}%") ||
                             (t.Description != null && EF.Functions.Like(t.Description, $"%{query}%"))))
                .Select(t => new SearchResultDTO
                {
                    EntityId = t.Id,
                    Title = t.Name ?? string.Empty,
                    Url = "/Tasks/Details/" + t.Id,
                    Type = "task",
                    Status = t.TaskStatus,
                    Priority = t.Priority,
                    TimeStamp = t.CreatedOn,
                    IsPinned = null,
                    Snippet = t.Description != null ? (t.Description.Length > 200 ? t.Description.Substring(0, 200) + "…" : t.Description) : null
                })
                .ToListAsync();

            var todosList = Enumerable.Empty<SearchResultDTO>();
            // Only query Todos if DbSet exists in context
            try
            {
                if (_context.Model.FindEntityType(typeof(TaskMonitoringApp.Models.Entities.Todo)) != null)
                {
                    todosList = await _context.Todo
                        .Where(td => td.UserId == userId && !td.IsDeleted &&
                                     (td.Notes != null && EF.Functions.Like(td.Notes, $"%{query}%")))
                        .Select(td => new SearchResultDTO
                        {
                            EntityId = td.Id,
                            Title = td.Notes ?? $"Todo #{td.Id}",
                            Url = td.TaskId > 0 ? ("/Tasks/Details/" + td.TaskId + $"?todoId={td.Id}") : ("/Tasks/Details/0?todoId=" + td.Id),
                            Type = "todo",
                            Status = td.Status,
                            Priority = null,
                            TimeStamp = td.CreatedOn,
                            IsPinned = null,
                            Snippet = td.Notes != null ? (td.Notes.Length > 200 ? td.Notes.Substring(0, 200) + "…" : td.Notes) : null
                        })
                        .ToListAsync();
                }
            }
            catch
            {
                // If Todos are not present or something fails, fallback to empty
                todosList = Enumerable.Empty<SearchResultDTO>();
            }

            var notesList = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted &&
                            ((n.Title != null && EF.Functions.Like(n.Title, $"%{query}%")) ||
                             (n.Content != null && EF.Functions.Like(n.Content, $"%{query}%"))))
                .Select(n => new SearchResultDTO
                {
                    EntityId = n.Id,
                    Title = n.Title ?? string.Empty,
                    Url = "/Notes/Details/" + n.Id,
                    Type = "note",
                    Priority = null,
                    TimeStamp = n.TimeStamp,
                    IsPinned = n.IsPinned,
                    Snippet = n.Content != null ? (n.Content.Length > 200 ? n.Content.Substring(0, 200) + "…" : n.Content) : null
                })
                .ToListAsync();

            // Combine results in-memory, order and then page
            var combined = goalsList
                .Concat(tasksList)
                .Concat(todosList)
                .Concat(notesList)
                .OrderByDescending(r => r.TimeStamp ?? DateTime.MinValue)
                .ToList();

            // Normalize paging values
            var p = Math.Max(1, pageNumber);
            var s = Math.Max(1, pageSize);

            var paged = combined
                .Skip((p - 1) * s)
                .Take(s)
                .ToList();

            return paged;
        }
    }
}