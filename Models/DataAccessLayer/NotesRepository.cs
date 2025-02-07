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

        public async Task<IEnumerable<T>> GetAllNotesAsync<T>(string userId, ResponseDataMode mode) where T : class
        {
            return mode switch
            {
                ResponseDataMode.Model => await _context.Notes
                    .Where(n => n.UserId == userId && n.IsDeleted == false)
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Notes with userId Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Notes
                    .Where(n => n.UserId == userId && n.IsDeleted == false)
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
                        ?? throw new NotFoundException("NoteDTO with userId Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.")
            };
        }

        public async Task<IEnumerable<T>> GetAllNotesAsync<T>(string userId, Status status, ResponseDataMode mode) where T : class
        {
            return mode switch
            {
                ResponseDataMode.Model => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All ||  n.Status == status)&& !n.IsDeleted)
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Notes with userId Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.IsDeleted == false)
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
                        ?? throw new NotFoundException("NoteDTO with userId Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.")
            };
        }

        public async Task<IEnumerable<T>> GetAllNotesForBothGoalAndTaskIdAsync<T>(string userId, int goalId, int taskId, Status status, ResponseDataMode mode) where T : class
        {
            return mode switch
            {
                ResponseDataMode.Model => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.GoalId == goalId && n.TaskId == taskId && n.IsDeleted == false)
                    .Include(g => g.Goal)
                    .Include(t => t.Task)
                    .AsSingleQuery()
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Notes with userId Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.GoalId == goalId && n.TaskId == taskId && n.IsDeleted == false)
                    .Include(g => g.Goal)
                    .Include(t => t.Task)
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
                        },
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
                    //.AsSingleQuery()
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("NoteDTOWithGoalAndTaskDTO with userId Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.")
            };
        }

        public async Task<IEnumerable<T>> GetAllNotesForGoalIdAsync<T>(string userId, int goalId, Status status, ResponseDataMode mode) where T : class
        {
            return mode switch
            {
                ResponseDataMode.Model => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.GoalId == goalId && n.IsDeleted == false)
                    .Include(g => g.Goal)
                    .AsSingleQuery()
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Notes with userId Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.GoalId == goalId && n.IsDeleted == false)
                    .Include(g => g.Goal)
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
                    .AsSingleQuery()
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("NoteDTOWithGoalAndTaskDTO with userId Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.")
            };
        }

        public async Task<IEnumerable<T>> GetAllNotesForTaskIdAsync<T>(string userId, int taskId, Status status, ResponseDataMode mode) where T : class
        {
            return mode switch
            {
                ResponseDataMode.Model => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.TaskId == taskId && n.IsDeleted == false)
                    .Include(t => t.Task)
                    .AsSingleQuery()
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("Notes with userId Not Found!"),
                ResponseDataMode.ModelDTO => await _context.Notes
                    .Where(n => n.UserId == userId && (status == Status.All || n.Status == status) && n.TaskId == taskId && n.IsDeleted == false)
                    .Include(t => t.Task)
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
                    .AsSingleQuery()
                    .ToListAsync() as IEnumerable<T>
                        ?? throw new NotFoundException("NoteDTOWithGoalAndTaskDTO with userId Data Not Found!"),
                _ => throw new InvalidOperationException("Invalid Operations While Fetching Notes Data.")
            };
        }

        public async Task<T> GetNotesByIdAsync<T>(string userId, int id, ResponseDataMode mode) where T : class
        {
            switch (mode)
            {
                case ResponseDataMode.Model:
                    var dataModel = await _context.Notes
                        .Where(n => n.UserId == userId && n.IsDeleted == false)
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                    return dataModel.FirstOrDefault()
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                case ResponseDataMode.ModelDTO:
                    var dataModelDTO = await _context.Notes
                       .Where(n => n.UserId == userId && n.IsDeleted == false)
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

        public async Task<T> GetNotesByIdWithBothGoalAndTaskIdAsync<T>(string userId, int id, int goalId, int taskId, ResponseDataMode mode) where T : class
        {
            switch (mode)
            {
                case ResponseDataMode.Model:
                    var dataModel = await _context.Notes
                        .Where(n => n.UserId == userId && n.GoalId == goalId && n.TaskId == taskId && n.IsDeleted == false)
                        .Include(g => g.Goal)
                        .Include(t => t.Task)
                        .AsSingleQuery()
                        .ToListAsync() as IEnumerable<T>
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                    return dataModel.FirstOrDefault()
                            ?? throw new NotFoundException("Notes with userId Not Found!");

                case ResponseDataMode.ModelDTO:
                    var dataModelDTO = await _context.Notes
                       .Where(n => n.UserId == userId && n.GoalId == goalId && n.TaskId == taskId && n.IsDeleted == false)
                       .Include(g => g.Goal)
                       .Include(t => t.Task)
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
                           },
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
                       .AsSingleQuery()
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
