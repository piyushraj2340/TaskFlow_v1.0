using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskMonitoringApp.Exceptions;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;
using TaskMonitoringApp.Models.Repositories;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class NotesRepository(ApplicationDbContext context) : INotesRepository
    {
        private readonly ApplicationDbContext _context = context;

        public async Task AddNotes(Notes notes)
        {
            await _context.Notes.AddAsync(notes);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<T>> GetAllNotesAsync<T>(string userId, Status status, ResponseDataMode mode) where T : class
        {
            // keep existing non-paged behavior
            return await GetAllNotesAsync<T>(userId, status, pageNumber: 0, pageSize: 0, mode);
        }

        // New: paged implementation
        public async Task<IEnumerable<T>> GetAllNotesAsync<T>(string userId, Status status, int pageNumber, int pageSize, ResponseDataMode mode) where T : class
        {
            // normalize paging flags: treat pageNumber/pageSize <= 0 as "no paging"
            var applyPaging = pageNumber > 0 && pageSize > 0;
            switch (mode)
            {
                case ResponseDataMode.Model:
                    {
                        var query = _context.Notes
                            .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && !n.IsDeleted)
                            .OrderByDescending(n => n.TimeStamp)
                            .AsQueryable();

                        if (applyPaging)
                        {
                            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                        }

                        var list = await query.ToListAsync() as IEnumerable<T>;
                        return list ?? throw new NotFoundException("Notes with userId Not Found!");
                    }

                case ResponseDataMode.ModelDTO:
                    {
                        var query = _context.Notes
                            .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && !n.IsDeleted)
                            .OrderByDescending(n => n.TimeStamp)
                            .AsQueryable();

                        if (applyPaging)
                        {
                            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                        }

                        var projected = await query
                            .Select(n => new NoteDTO()
                            {
                                Id = n.Id,
                                Title = n.Title,
                                Content = n.Content,
                                Tags = n.Tags,
                                IsModified = n.IsModified,
                                ModifiedOn = n.ModifiedOn,
                                ParentNoteId = n.ParentNoteId,
                                Status = n.Status,
                                IsPinned = n.IsPinned,
                                TimeStamp = n.TimeStamp,
                                UserId = n.UserId
                            })
                            .ToListAsync() as IEnumerable<T>;

                        return projected ?? throw new NotFoundException("NoteDTO with userId Data Not Found!");
                    }

                default:
                    throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.");
            }
        }

        public async Task<IEnumerable<T>> GetAllNotesForGoalIdAsync<T>(string userId, int goalId, Status status, ResponseDataMode mode) where T : class
        {
            // delegate to paged variant (no paging by default)
            return await GetAllNotesForGoalIdAsync<T>(userId, goalId, status, pageNumber: 0, pageSize: 0, mode);
        }

        // New: paged overload for goal specific notes
        public async Task<IEnumerable<T>> GetAllNotesForGoalIdAsync<T>(string userId, int goalId, Status status, int pageNumber, int pageSize, ResponseDataMode mode) where T : class
        {
            var applyPaging = pageNumber > 0 && pageSize > 0;

            return mode switch
            {
                ResponseDataMode.Model => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.GoalId == goalId && n.IsDeleted == false)
                    .Include(g => g.Goal)
                    .AsSingleQuery()
                    .OrderByDescending(n => n.TimeStamp)
                    .Skip(applyPaging ? (pageNumber - 1) * pageSize : 0)
                    .Take(applyPaging ? pageSize : int.MaxValue)
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Notes with userId Not Found!"),

                ResponseDataMode.ModelDTO => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.GoalId == goalId && n.IsDeleted == false)
                    .Include(g => g.Goal)
                    .OrderByDescending(n => n.TimeStamp)
                    .Skip(applyPaging ? (pageNumber - 1) * pageSize : 0)
                    .Take(applyPaging ? pageSize : int.MaxValue)
                    .Select(n => new NoteDTOWithGoalDTO()
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Content = n.Content,
                        Tags = n.Tags,
                        IsModified = n.IsModified,
                        ModifiedOn = n.ModifiedOn,
                        ParentNoteId = n.ParentNoteId,
                        Status = n.Status,
                        IsPinned = n.IsPinned,
                        TimeStamp = n.TimeStamp,
                        UserId = n.UserId,
                        Goal = new GoalDTO()
                        {
                            Id = n.Goal.Id,
                            Name = n.Goal.Name,
                            Description = n.Goal.Description,
                            Priority = n.Goal.Priority,
                            EndDate = n.Goal.EndDate,
                            UserId = n.Goal.UserId,
                            GoalStatus = n.Goal.GoalStatus
                        }
                    })
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("NoteDTOWithGoalDTO with userId Data Not Found!"),

                _ => throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.")
            };
        }

        public async Task<IEnumerable<T>> GetAllNotesForTaskIdAsync<T>(string userId, int taskId, Status status, ResponseDataMode mode) where T : class
        {
            // delegate to paged variant
            return await GetAllNotesForTaskIdAsync<T>(userId, taskId, status, pageNumber: 0, pageSize: 0, mode);
        }

        // New: paged overload for task specific notes
        public async Task<IEnumerable<T>> GetAllNotesForTaskIdAsync<T>(string userId, int taskId, Status status, int pageNumber, int pageSize, ResponseDataMode mode) where T : class
        {
            var applyPaging = pageNumber > 0 && pageSize > 0;

            return mode switch
            {
                ResponseDataMode.Model => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.TaskId == taskId && n.IsDeleted == false)
                    .Include(t => t.Task)
                    .AsSingleQuery()
                    .OrderByDescending(n => n.TimeStamp)
                    .Skip(applyPaging ? (pageNumber - 1) * pageSize : 0)
                    .Take(applyPaging ? pageSize : int.MaxValue)
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Notes with userId Not Found!"),

                ResponseDataMode.ModelDTO => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.TaskId == taskId && n.IsDeleted == false)
                    .Include(t => t.Task)
                    .OrderByDescending(n => n.TimeStamp)
                    .Skip(applyPaging ? (pageNumber - 1) * pageSize : 0)
                    .Take(applyPaging ? pageSize : int.MaxValue)
                    .Select(n => new NoteDTOWithTaskDTO()
                    {
                        Id = n.Id,
                        Title = n.Title,
                        Content = n.Content,
                        Tags = n.Tags,
                        IsModified = n.IsModified,
                        ModifiedOn = n.ModifiedOn,
                        ParentNoteId = n.ParentNoteId,
                        Status = n.Status,
                        IsPinned = n.IsPinned,
                        TimeStamp = n.TimeStamp,
                        UserId = n.UserId,
                        Task = new TaskDTO()
                        {
                            Id = n.Task.Id,
                            Name = n.Task.Name,
                            Description = n.Task.Description,
                            Repeat = n.Task.Repeat,
                            RepeatWeekList = n.Task.RepeatWeekList,
                            Priority = n.Task.Priority,
                            EndDate = n.Task.EndDate,
                            UserId = n.Task.UserId,
                            TaskStatus = n.Task.TaskStatus
                        }
                    })
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("NoteDTOWithTaskDTO with userId Data Not Found!"),

                _ => throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.")
            };
        }

        // New: Get all notes including Goal and Task information (non-paged)
        public async Task<IEnumerable<T>> GetAllNotesWithGoalAndTaskAsync<T>(string userId, Status status, ResponseDataMode mode) where T : class
        {
            return await GetAllNotesWithGoalAndTaskAsync<T>(userId, status, pageNumber: 0, pageSize: 0, mode);
        }

        // New: paged overload for WithGoalAndTask
        public async Task<IEnumerable<T>> GetAllNotesWithGoalAndTaskAsync<T>(string userId, Status status, int pageNumber, int pageSize, ResponseDataMode mode) where T : class
        {
            var applyPaging = pageNumber > 0 && pageSize > 0;

            switch (mode)
            {
                case ResponseDataMode.Model:
                    {
                        var query = _context.Notes
                            .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && !n.IsDeleted)
                            .Include(g => g.Goal)
                            .Include(t => t.Task)
                            .AsSingleQuery()
                            .OrderByDescending(n => n.TimeStamp)
                            .AsQueryable();

                        if (applyPaging)
                        {
                            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                        }

                        var list = await query.ToListAsync() as IEnumerable<T>;
                        return list ?? throw new NotFoundException("Notes with userId Not Found!");
                    }

                case ResponseDataMode.ModelDTO:
                    {
                        var query = _context.Notes
                            .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && !n.IsDeleted)
                            .Include(g => g.Goal)
                            .Include(t => t.Task)
                            .AsSingleQuery()
                            .OrderByDescending(n => n.TimeStamp)
                            .AsQueryable();

                        if (applyPaging)
                        {
                            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                        }

                        var projected = await query
                            .Select(n => new NoteDTOWithGoalAndTaskDTO()
                            {
                                Id = n.Id,
                                Title = n.Title,
                                Content = n.Content,
                                Tags = n.Tags,
                                IsModified = n.IsModified,
                                ModifiedOn = n.ModifiedOn,
                                ParentNoteId = n.ParentNoteId,
                                Status = n.Status,
                                IsPinned = n.IsPinned,
                                TimeStamp = n.TimeStamp,
                                UserId = n.UserId,
                                TaskId = n.TaskId ?? 0,
                                Task = n.Task == null ? null : new TaskDTO()
                                {
                                    Id = n.Task.Id,
                                    Name = n.Task.Name,
                                    Description = n.Task.Description,
                                    Repeat = n.Task.Repeat,
                                    RepeatWeekList = n.Task.RepeatWeekList,
                                    Priority = n.Task.Priority,
                                    EndDate = n.Task.EndDate,
                                    StartDate = n.Task.StartDate,
                                    EndedOn = n.Task.EndedOn,
                                    CompletedOn = n.Task.CompletedOn,
                                    IsScheduled = n.Task.IsScheduled,
                                    StartOptionType = n.Task.StartOptionType,
                                    IsStarted = n.Task.IsStarted,
                                    UserId = n.Task.UserId,
                                    TaskStatus = n.Task.TaskStatus
                                },
                                GoalId = n.GoalId ?? 0,
                                Goal = n.Goal == null ? null : new GoalDTO()
                                {
                                    Id = n.Goal.Id,
                                    Name = n.Goal.Name,
                                    Description = n.Goal.Description,
                                    Priority = n.Goal.Priority,
                                    EndDate = n.Goal.EndDate,
                                    StartDate = n.Goal.StartDate,
                                    IsScheduled = n.Goal.IsScheduled,
                                    StartOptionType = n.Goal.StartOptionType,
                                    IsStarted = n.Goal.IsStarted,
                                    UserId = n.Goal.UserId,
                                    GoalStatus = n.Goal.GoalStatus
                                },
                            })
                            .ToListAsync() as IEnumerable<T>;

                        return projected ?? throw new NotFoundException("NoteDTOWithGoalAndTaskDTO with userId Data Not Found!");
                    }

                default:
                    throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.");
            }
        }

        public async Task<T> GetNotesByIdAsync<T>(string userId, int id, ResponseDataMode mode) where T : class
        {
            switch (mode)
            {
                case ResponseDataMode.Model:
                    var dataModel = await _context.Notes
                        .Where(n => n.UserId == userId && n.Id == id && n.IsDeleted == false)
                        .OrderByDescending(n => n.TimeStamp)
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                    return dataModel.FirstOrDefault()
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                case ResponseDataMode.ModelDTO:
                    var dataModelDTO = await _context.Notes
                       .Where(n => n.UserId == userId && n.Id == id && n.IsDeleted == false)
                       .OrderByDescending(n => n.TimeStamp)
                       .Select(n => new NoteDTO()
                       {
                           Id = n.Id,
                           Title = n.Title,
                           Content = n.Content,
                           Tags = n.Tags,
                           IsModified = n.IsModified,
                           ModifiedOn = n.ModifiedOn,
                           ParentNoteId = n.ParentNoteId,
                           Status = n.Status,
                           IsPinned = n.IsPinned,
                           TimeStamp = n.TimeStamp,
                           UserId = n.UserId
                       })
                       .ToListAsync() as IEnumerable<T>
                           ?? throw new NotFoundException("Notes with userId Not Found!");

                    return dataModelDTO.FirstOrDefault()
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                default:
                    throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.");
            }
        }

        public async Task<T> GetNotesByIdWithGoalIdAsync<T>(string userId, int id, int goalId, ResponseDataMode mode) where T : class
        {
            switch (mode)
            {
                case ResponseDataMode.Model:
                    var dataModel = await _context.Notes
                        .Where(n => n.UserId == userId && n.GoalId == goalId && n.IsDeleted == false)
                        .Include(g => g.Goal)
                        .AsSingleQuery()
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                    return dataModel.FirstOrDefault()
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                case ResponseDataMode.ModelDTO:
                    var dataModelDTO = await _context.Notes
                       .Where(n => n.UserId == userId && n.GoalId == goalId && n.IsDeleted == false)
                       .Include(g => g.Goal)
                       .AsSingleQuery()
                       .Select(n => new NoteDTOWithGoalDTO()
                       {
                           Id = n.Id,
                           Title = n.Title,
                           Content = n.Content,
                           Tags = n.Tags,
                           IsModified = n.IsModified,
                           ModifiedOn = n.ModifiedOn,
                           ParentNoteId = n.ParentNoteId,
                           Status = n.Status,
                           IsPinned = n.IsPinned,
                           TimeStamp = n.TimeStamp,
                           UserId = n.UserId,
                           Goal = new GoalDTO()
                           {
                               Id = n.Goal.Id,
                               Name = n.Goal.Name,
                               Description = n.Goal.Description,
                               Priority = n.Goal.Priority,
                               EndDate = n.Goal.EndDate,
                               UserId = n.Goal.UserId,
                               GoalStatus = n.Goal.GoalStatus
                           }
                       })
                       .ToListAsync() as IEnumerable<T>
                           ?? throw new NotFoundException("Notes with userId Not Found!");

                    return dataModelDTO.FirstOrDefault()
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                default:
                    throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.");
            }
        }

        public async Task<T> GetNotesByIdWithTaskIdAsync<T>(string userId, int id, int taskId, ResponseDataMode mode) where T : class
        {
            switch (mode)
            {
                case ResponseDataMode.Model:
                    var dataModel = await _context.Notes
                        .Where(n => n.UserId == userId && n.TaskId == taskId)
                        .Include(t => t.Task)
                        .AsSingleQuery()
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                    return dataModel.FirstOrDefault()
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                case ResponseDataMode.ModelDTO:
                    var dataModelDTO = await _context.Notes
                       .Where(n => n.UserId == userId && n.TaskId == taskId)
                       .Include(t => t.Task)
                        .AsSingleQuery()
                        .Select(n => new NoteDTOWithTaskDTO()
                        {
                            Id = n.Id,
                            Title = n.Title,
                            Content = n.Content,
                            Tags = n.Tags,
                            IsModified = n.IsModified,
                            ModifiedOn = n.ModifiedOn,
                            ParentNoteId = n.ParentNoteId,
                            Status = n.Status,
                            IsPinned = n.IsPinned,
                            TimeStamp = n.TimeStamp,
                            UserId = n.UserId,
                            Task = new TaskDTO()
                            {
                                Id = n.Task.Id,
                                Name = n.Task.Name,
                                Description = n.Task.Description,
                                Repeat = n.Task.Repeat,
                                RepeatWeekList = n.Task.RepeatWeekList,
                                Priority = n.Task.Priority,
                                EndDate = n.Task.EndDate,
                                UserId = n.Task.UserId,
                                TaskStatus = n.Task.TaskStatus
                            }
                        })
                       .ToListAsync() as IEnumerable<T>
                           ?? throw new NotFoundException("Notes with userId Not Found!");

                    return dataModelDTO.FirstOrDefault()
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                default:
                    throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.");
            }
        }

        public async Task UpdateNotes(string userId, Notes notes)
        {
            _context.Notes.Update(notes);
            await _context.SaveChangesAsync();
        }

        // MODIFIED: Update this method to support search string and specific filtering
        public async Task<IEnumerable<T>> GetAllNotesWithGoalAndTaskAsync<T>(string userId, Status status, int pageNumber, int pageSize, ResponseDataMode mode, IEnumerable<int>? filterGoalIds = null, IEnumerable<int>? filterTaskIds = null, string? searchQuery = null, bool? onlyPinned = null, bool includeGoalRelatedTasks = false) where T : class
        {
            var applyPaging = pageNumber > 0 && pageSize > 0;

            switch (mode)
            {
                case ResponseDataMode.ModelDTO:
                    {
                        var query = _context.Notes
                            .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && !n.IsDeleted)
                            .Include(g => g.Goal)
                            .Include(t => t.Task)
                            .AsSingleQuery()
                            .AsQueryable();

                        // APPLY FILTERS (Combined Logic)
                        // Using a predicate builder approach or just combining expressions
                        
                        // We want to filter notes that match (Goal Filters) OR (Task Filters) OR (Search)
                        // Typically UI filters are AND between sections (must match Goal criteria AND Task critera).
                        // However, since a Note usually belongs to ONE parent, selecting Goal A and Task B (unrelated) 
                        // in an AND filter would result in 0 notes.
                        // So for this specific domain, OR logic between Goal/Task selections makes more sense for "Show me stuff about Goal A and Task B".
                        // BUT, if I select multiple Goals, that is OR (Goal A or Goal B).
                        
                        // Let's implement OR logic between the Goal block and Task block if both are present.
                        // (Match Goal Condition) OR (Match Task Condition)
                        
                        var hasGoalFilter = filterGoalIds != null && filterGoalIds.Any();
                        var hasTaskFilter = filterTaskIds != null && filterTaskIds.Any();
                        
                        if (hasGoalFilter || hasTaskFilter)
                        {
                            // We need to build a dynamic OR predicate or just use a big Where clause
                            // Since EF Core translates this well:
                            
                            List<int> validGoalIds = new List<int>();
                            List<int> relatedTaskIds = new List<int>();
                            bool includeJournal = false;
                            
                            if (hasGoalFilter)
                            {
                                includeJournal = filterGoalIds.Contains(-1);
                                var initialGoalIds = filterGoalIds.Where(id => id != -1).ToList();

                                // NEW: Expand to include all descendant goal IDs
                                if (initialGoalIds.Any())
                                {
                                    // Helper function logic inline or separate
                                    // Fetch all goals to build hierarchy relation
                                    var allGoals = await _context.Goals
                                        .Where(g => g.UserId == userId && !g.IsDeleted)
                                        .Select(g => new { g.Id, g.ParentId })
                                        .ToListAsync();

                                    var parentMap = allGoals
                                        .Where(g => g.ParentId.HasValue)
                                        .GroupBy(g => g.ParentId.Value)
                                        .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

                                    var distinctGoals = new HashSet<int>(initialGoalIds);
                                    var queue = new Queue<int>(initialGoalIds);

                                    while (queue.Count > 0)
                                    {
                                        var current = queue.Dequeue();
                                        if (parentMap.TryGetValue(current, out var children))
                                        {
                                            foreach (var child in children)
                                            {
                                                if (distinctGoals.Add(child))
                                                {
                                                    queue.Enqueue(child);
                                                }
                                            }
                                        }
                                    }
                                    validGoalIds = distinctGoals.ToList();
                                }
                                
                                if (includeGoalRelatedTasks && validGoalIds.Any())
                                {
                                     var rIds = await _context.Set<GoalTask>()
                                        .Where(gt => validGoalIds.Contains(gt.GoalId))
                                        .Select(gt => gt.TaskId)
                                        .ToListAsync();
                                     relatedTaskIds.AddRange(rIds);
                                }
                            }
                            
                            // We need to construct the Where clause carefully. 
                            // Note: We cannot use `await` inside the Where expression obviously. 
                            // I moved the relatedTaskIds fetch outside.
                            
                            query = query.Where(n => 
                                // Goal Condition
                                (hasGoalFilter && (
                                    (n.GoalId.HasValue && validGoalIds.Contains(n.GoalId.Value)) || 
                                    (includeJournal && n.GoalId == null && n.TaskId == null && n.TodoId == null) ||
                                    (includeGoalRelatedTasks && validGoalIds.Any() && n.TaskId.HasValue && relatedTaskIds.Contains(n.TaskId.Value))
                                ))
                                ||
                                // Task Condition
                                (hasTaskFilter && n.TaskId.HasValue && filterTaskIds.Contains(n.TaskId.Value))
                            );
                        }

                        if (!string.IsNullOrEmpty(searchQuery))
                        {
                            string q = searchQuery.ToLower();
                            query = query.Where(n => n.Title.ToLower().Contains(q) || n.Content.ToLower().Contains(q));
                        }

                        if (onlyPinned.HasValue && onlyPinned.Value)
                        {
                            query = query.Where(n => n.IsPinned);
                        }

                        // Order by Pinned then Date usually, but for timeline strict date is better. 
                        // For this specific method (timeline), we stick to date.
                        query = query.OrderByDescending(n => n.TimeStamp);

                        if (applyPaging)
                        {
                            query = query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
                        }

                        var projected = await query
                            .Select(n => new NoteDTOWithGoalAndTaskDTO()
                            {
                                Id = n.Id,
                                Title = n.Title,
                                Content = n.Content,
                                Tags = n.Tags,
                                IsModified = n.IsModified,
                                ModifiedOn = n.ModifiedOn,
                                ParentNoteId = n.ParentNoteId,
                                Status = n.Status,
                                IsPinned = n.IsPinned,
                                TimeStamp = n.TimeStamp,
                                UserId = n.UserId,
                                TaskId = n.TaskId ?? 0,
                                Task = n.Task == null ? null : new TaskDTO()
                                {
                                    Id = n.Task.Id,
                                    Name = n.Task.Name,
                                    Description = n.Task.Description,
                                    Repeat = n.Task.Repeat,
                                    RepeatWeekList = n.Task.RepeatWeekList,
                                    Priority = n.Task.Priority,
                                    EndDate = n.Task.EndDate,
                                    StartDate = n.Task.StartDate,
                                    EndedOn = n.Task.EndedOn,
                                    CompletedOn = n.Task.CompletedOn,
                                    IsScheduled = n.Task.IsScheduled,
                                    StartOptionType = n.Task.StartOptionType,
                                    IsStarted = n.Task.IsStarted,
                                    UserId = n.Task.UserId,
                                    TaskStatus = n.Task.TaskStatus
                                },
                                GoalId = n.GoalId ?? 0,
                                Goal = n.Goal == null ? null : new GoalDTO()
                                {
                                    Id = n.Goal.Id,
                                    Name = n.Goal.Name,
                                    Description = n.Goal.Description,
                                    Priority = n.Goal.Priority,
                                    EndDate = n.Goal.EndDate,
                                    StartDate = n.Goal.StartDate,
                                    IsScheduled = n.Goal.IsScheduled,
                                    StartOptionType = n.Goal.StartOptionType,
                                    IsStarted = n.Goal.IsStarted,
                                    UserId = n.Goal.UserId,
                                    GoalStatus = n.Goal.GoalStatus
                                },
                            })
                            .ToListAsync() as IEnumerable<T>;

                        return projected ?? throw new NotFoundException("NoteDTOWithGoalAndTaskDTO with userId Data Not Found!");
                    }
                // ... handle Model case similarly if needed, or throw not implemented for now
                default:
                    // Fallback to existing logic if not DTO
                     throw new InvalidOperationException("Only ModelDTO supported for filtered queries currently.");
            }
        }

        public async Task<T> GetNoteByIdWithGoalAndTaskAsync<T>(string userId, int id, ResponseDataMode mode) where T : class
        {
            switch (mode)
            {
                case ResponseDataMode.ModelDTO:
                     var query = _context.Notes
                            .Where(n => n.UserId == userId && n.Id == id && !n.IsDeleted)
                            .Include(g => g.Goal)
                            .Include(t => t.Task)
                            .AsSingleQuery();

                     var projected = await query
                            .Select(n => new NoteDTOWithGoalAndTaskDTO()
                            {
                                Id = n.Id,
                                Title = n.Title,
                                Content = n.Content,
                                Tags = n.Tags,
                                IsModified = n.IsModified,
                                ModifiedOn = n.ModifiedOn,
                                ParentNoteId = n.ParentNoteId,
                                Status = n.Status,
                                IsPinned = n.IsPinned,
                                TimeStamp = n.TimeStamp,
                                UserId = n.UserId,
                                TaskId = n.TaskId ?? 0,
                                Task = n.Task == null ? null : new TaskDTO() { 
                                    Id = n.Task.Id, 
                                    Name = n.Task.Name, 
                                    Description = n.Task.Description 
                                },
                                GoalId = n.GoalId ?? 0,
                                Goal = n.Goal == null ? null : new GoalDTO() { 
                                    Id = n.Goal.Id, 
                                    Name = n.Goal.Name, 
                                    Description = n.Goal.Description 
                                },
                            })
                            .FirstOrDefaultAsync();

                     return (projected as T) ?? throw new NotFoundException("Note not found");
                
                default: 
                     throw new NotImplementedException("Only ModelDTO supported for this method");
            }
        }

        public async Task<int> GetNotePositionAsync(string userId, int noteId, Status status, IEnumerable<int>? filterGoalIds = null, IEnumerable<int>? filterTaskIds = null, string? searchQuery = null, bool includeGoalRelatedTasks = false)
        {
            // Base query (must match the main list query logic exactly)
            var query = _context.Notes
                .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && !n.IsDeleted);

            // APPLY FILTERS (Combined Logic OR)
            
            // Check if ANY filter is active
            var hasGoalFilter = filterGoalIds != null && filterGoalIds.Any();
            var hasTaskFilter = filterTaskIds != null && filterTaskIds.Any();

            if (hasGoalFilter || hasTaskFilter)
            {
                List<int> validGoalIds = new List<int>();
                List<int> relatedTaskIds = new List<int>();
                bool includeJournal = false;

                if (hasGoalFilter)
                {
                    includeJournal = filterGoalIds.Contains(-1);
                    var initialGoalIds = filterGoalIds.Where(id => id != -1).ToList();

                    // NEW: Expand to include all descendant goal IDs
                    if (initialGoalIds.Any())
                    {
                        var allGoals = await _context.Goals
                            .Where(g => g.UserId == userId && !g.IsDeleted)
                            .Select(g => new { g.Id, g.ParentId })
                            .ToListAsync();

                        var parentMap = allGoals
                            .Where(g => g.ParentId.HasValue)
                            .GroupBy(g => g.ParentId.Value)
                            .ToDictionary(g => g.Key, g => g.Select(x => x.Id).ToList());

                        var distinctGoals = new HashSet<int>(initialGoalIds);
                        var queue = new Queue<int>(initialGoalIds);

                        while (queue.Count > 0)
                        {
                            var current = queue.Dequeue();
                            if (parentMap.TryGetValue(current, out var children))
                            {
                                foreach (var child in children)
                                {
                                    if (distinctGoals.Add(child))
                                    {
                                        queue.Enqueue(child);
                                    }
                                }
                            }
                        }
                        validGoalIds = distinctGoals.ToList();
                    }
                    
                    if (includeGoalRelatedTasks && validGoalIds.Any())
                    {
                         // Fetch related tasks for goal filtering
                         var rIds = await _context.Set<GoalTask>()
                            .Where(gt => validGoalIds.Contains(gt.GoalId))
                            .Select(gt => gt.TaskId)
                            .ToListAsync();
                         relatedTaskIds.AddRange(rIds);
                    }
                }

                 query = query.Where(n =>
                    // Goal Condition 
                    (hasGoalFilter && (
                        (n.GoalId.HasValue && validGoalIds.Contains(n.GoalId.Value)) ||
                        (includeJournal && n.GoalId == null && n.TaskId == null && n.TodoId == null) ||
                        (includeGoalRelatedTasks && validGoalIds.Any() && n.TaskId.HasValue && relatedTaskIds.Contains(n.TaskId.Value))
                    ))
                    ||
                    // Task Condition
                    (hasTaskFilter && n.TaskId.HasValue && filterTaskIds.Contains(n.TaskId.Value))
                 );
            }

             if (!string.IsNullOrEmpty(searchQuery))
            {
                string q = searchQuery.ToLower();
                query = query.Where(n => n.Title.ToLower().Contains(q) || n.Content.ToLower().Contains(q));
            }

            // We need to count how many items are NEWER than our target note
            // Since we order by TimeStamp Descending (Newest first)
            
            // 1. Get target note timestamp
            var targetNote = await _context.Notes
                .Where(n => n.Id == noteId)
                .Select(n => new { n.TimeStamp })
                .FirstOrDefaultAsync();

            if (targetNote == null) return -1; // Note not found

            // 2. Count items with TimeStamp > target.TimeStamp
            // Note: If timestamps are identical, ID tie-breaking would be needed for perfect precision, 
            // but for now > is sufficient.
            var count = await query
                .Where(n => n.TimeStamp > targetNote.TimeStamp)
                .CountAsync();

            // Position is count + 1 (1-based index)
            return count + 1;
        }

        public async Task<FilterMenuDataDTO> GetFilterOptionsWithCountsAsync(string userId)
        {
            // 1. Fetch Goals with hierarchy info
            var goalsRaw = await _context.Goals
                .Where(g => g.UserId == userId && !g.IsDeleted)
                .Select(g => new { g.Id, g.Name, g.ParentId })
                .ToListAsync();

            // 2. Direct Note Counts (Group by GoalId)
            var noteCountsByGoal = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted && n.GoalId.HasValue)
                .GroupBy(n => n.GoalId)
                .Select(g => new { GoalId = g.Key.Value, Count = g.Count() })
                .ToDictionaryAsync(k => k.GoalId, v => v.Count);

            // 3. Task Note Counts (Group by TaskId)
            // We need this to calculate how many notes are attached to tasks related to a goal
            var noteCountsByTask = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted && n.TaskId.HasValue)
                .GroupBy(n => n.TaskId)
                .Select(t => new { TaskId = t.Key.Value, Count = t.Count() })
                .ToDictionaryAsync(t => t.TaskId, t => t.Count);

            // 4. Map Goal -> Related Tasks (via GoalTask table)
            // We need to know which tasks belong to which goal to add their note counts
            var goalTasksMap = await _context.Set<GoalTask>()
                .GroupBy(gt => gt.GoalId)
                .Select(g => new { GoalId = g.Key, TaskIds = g.Select(x => x.TaskId).ToList() })
                .ToDictionaryAsync(k => k.GoalId, v => v.TaskIds);

            // 5. Build Tree Structure to Aggregate Counts
            // Map: GoalId -> (DirectCount, TaskNoteCount, Children)
            var countMap = goalsRaw.ToDictionary(g => g.Id, g =>
            {
                // Count from direct notes on goal
                int directGoalNotes = noteCountsByGoal.ContainsKey(g.Id) ? noteCountsByGoal[g.Id] : 0;

                // Count from notes on tasks related to this goal
                int directTaskNotes = 0;
                if (goalTasksMap.TryGetValue(g.Id, out var taskIds))
                {
                    foreach (var tid in taskIds)
                    {
                        if (noteCountsByTask.TryGetValue(tid, out int c))
                        {
                            directTaskNotes += c;
                        }
                    }
                }

                return new
                {
                    g.ParentId,
                    DirectVal = directGoalNotes + directTaskNotes, // Sum of Goal Notes + Task Notes
                    Children = new List<int>()
                };
            });

            // Populate Children
            foreach (var g in goalsRaw)
            {
                if (g.ParentId.HasValue && countMap.ContainsKey(g.ParentId.Value))
                {
                    countMap[g.ParentId.Value].Children.Add(g.Id);
                }
            }

            // Recursive function to calculate total count (Memoized)
            var memo = new Dictionary<int, int>();

            int GetTotalCount(int goalId)
            {
                if (memo.ContainsKey(goalId)) return memo[goalId];

                var node = countMap[goalId];
                int total = node.DirectVal; // Own notes + Own tasks' notes

                foreach (var childId in node.Children)
                {
                    total += GetTotalCount(childId); // Add children's total (which includes their tasks)
                }

                memo[goalId] = total;
                return total;
            }

            // 6. Transform to DTO
            var goals = goalsRaw.Select(g => new FilterOptionDTO
            {
                Id = g.Id,
                Name = g.Name,
                Count = GetTotalCount(g.Id)
            })
            .OrderByDescending(g => g.Count)
            .ThenBy(g => g.Name)
            .ToList();

            // 7. Task List for Filter (Standard logic)
            var tasks = await _context.Tasks
                .Where(t => t.UserId == userId && !t.IsDeleted)
                .Select(t => new FilterOptionDTO 
                { 
                    Id = t.Id, 
                    Name = t.Name 
                })
                .ToListAsync();

            foreach (var t in tasks)
            {
                if (noteCountsByTask.TryGetValue(t.Id, out int count))
                {
                    t.Count = count;
                }
            }
             
            // 8. Journal Count
            var journalCount = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted && n.GoalId == null && n.TaskId == null && n.TodoId == null)
                .CountAsync();

            return new FilterMenuDataDTO
            {
                Goals = goals,
                Tasks = tasks.OrderByDescending(t => t.Count).ThenBy(t => t.Name),
                JournalCount = journalCount
            };
        }
    }
}
