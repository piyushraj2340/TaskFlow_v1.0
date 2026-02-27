using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using TaskMonitoringApp.Exceptions;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
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
        public async Task<IEnumerable<T>> GetAllNotesWithGoalAndTaskAsync<T>(string userId, Status status, int pageNumber, int pageSize, ResponseDataMode mode, int? filterGoalId = null, int? filterTaskId = null, string? searchQuery = null, bool? onlyPinned = null) where T : class
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

                        // APPLY FILTERS
                        if (filterGoalId.HasValue)
                        {
                            if (filterGoalId.Value == -1) // NEW: Journal/Independent mode
                            {
                                // Filter for notes that have NO parent associations
                                query = query.Where(n => n.GoalId == null && n.TaskId == null && n.TodoId == null);
                            }
                            else
                            {
                                // Standard Goal filter
                                query = query.Where(n => n.GoalId == filterGoalId.Value);
                            }
                        }
                        else if (filterTaskId.HasValue)
                        {
                            query = query.Where(n => n.TaskId == filterTaskId.Value);
                        }
                        
                        // NEW: "Journal Only" Logic
                        // We can use a special flag, e.g., filterGoalId = 0 OR filterGoalId = -1 to mean "No Goal/Task"
                        // Or passed as a separate bool/enum parameter.
                        // Let's assume passed via specific logic: if 'filterGoalId == -1' (Journal mode)
                        else if (filterGoalId == -1) 
                        {
                             query = query.Where(n => n.GoalId == null && n.TaskId == null && n.TodoId == null);
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
                                    // ... map only necessary fields for performance if possible
                                    Description = n.Task.Description
                                },
                                GoalId = n.GoalId ?? 0,
                                Goal = n.Goal == null ? null : new GoalDTO()
                                {
                                    Id = n.Goal.Id,
                                    Name = n.Goal.Name,
                                    Description = n.Goal.Description
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

        public async Task<int> GetNotePositionAsync(string userId, int noteId, Status status, int? filterGoalId = null, int? filterTaskId = null, string? searchQuery = null)
        {
            // Base query (must match the main list query logic exactly)
            var query = _context.Notes
                .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && !n.IsDeleted);

            // Apply same filters
            if (filterGoalId.HasValue) query = query.Where(n => n.GoalId == filterGoalId.Value);
            else if (filterTaskId.HasValue) query = query.Where(n => n.TaskId == filterTaskId.Value);

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
            // 1. Goal Counts
            // Simple approach: Group notes by GoalId and count
            var goalCounts = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted && n.GoalId.HasValue)
                .GroupBy(n => n.GoalId)
                .Select(g => new { GoalId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(g => g.GoalId.Value, g => g.Count);

            // Fetch actual goals to get names (and include those with 0 notes if desired, though usually filters show only what has data or all active goals)
            var goals = await _context.Goals
                .Where(g => g.UserId == userId && !g.IsDeleted)
                .Select(g => new FilterOptionDTO 
                { 
                    Id = g.Id, 
                    Name = g.Name, 
                    // ParentId = g.ParentGoalId // If you have parent goal logic
                })
                .ToListAsync();

            // Map counts
            foreach (var g in goals)
            {
                if (goalCounts.TryGetValue(g.Id, out int count))
                {
                    g.Count = count;
                }
            }

            // 2. Task Counts
            var taskCounts = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted && n.TaskId.HasValue)
                .GroupBy(n => n.TaskId)
                .Select(t => new { TaskId = t.Key, Count = t.Count() })
                .ToDictionaryAsync(t => t.TaskId.Value, t => t.Count);

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
                if (taskCounts.TryGetValue(t.Id, out int count))
                {
                    t.Count = count;
                }
            }

            // 3. Journal (Standalone) Count
            var journalCount = await _context.Notes
                .Where(n => n.UserId == userId && !n.IsDeleted && n.GoalId == null && n.TaskId == null && n.TodoId == null)
                .CountAsync();

            return new FilterMenuDataDTO
            {
                Goals = goals.OrderByDescending(g => g.Count).ThenBy(g => g.Name),
                Tasks = tasks.OrderByDescending(t => t.Count).ThenBy(t => t.Name),
                JournalCount = journalCount
            };
        }
    }
}
