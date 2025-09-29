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
    }
}
