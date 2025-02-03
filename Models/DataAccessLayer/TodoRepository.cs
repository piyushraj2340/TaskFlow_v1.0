using Microsoft.Build.Framework;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.VisualBasic;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Intrinsics.X86;
using System;
using System.Threading.Tasks;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Repositories;
using TaskMonitoringApp.Utility;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Microsoft.Data.SqlClient;
using TaskMonitoringApp.Models.DTOs;
using AutoMapper;

namespace TaskMonitoringApp.Models.DataAccessLayer
{
    public class TodoRepository : ITodoRepository
    {
        private readonly ApplicationDbContext _context;
        private readonly IMapper _mapper;

        public TodoRepository(ApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task AddTodoAsync(string UserId, Todo todo)
        {
            var user = todo.User ?? throw new ArgumentNullException(nameof(todo.User), "UserId Is Required!.");

            if (user.Id != UserId)
            {
                throw new ArgumentException("UserId and Todo.User.Id must be same!.");
            }

            await _context.Todo.AddAsync(todo);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTodoAsync(string UserId, int Id)
        {
            var todo = await _context.Todo.FirstOrDefaultAsync(T => T.User.Id == UserId && T.Id == Id) ??
                throw new InvalidOperationException("Error: attempts to delete todo that does not exits."); ;

            _context.Todo.Remove(todo);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Todo>> GetAllTodoAsync(string UserId)
        {
            // Fetch the todo list for today and project it to a list of Todo objects
            var allTodoList = await _context.Todo
                .Where(t => t.CreatedOn == DateTime.Now.Date && t.User.Id == UserId)  // Ensure we only consider today
                .ToListAsync();


            // Fetch all running tasks, ensuring that the task is not associated with any Todo for today
            var allRunningTask = await _context.Tasks
                .Where(t => t.User.Id == UserId &&
                    t.TaskStatus == Status.Running &&
                    (
                        t.Repeat == RepeatType.Daily ||
                        (
                            t.Repeat == RepeatType.Weekly &&
                            t.RepeatWeekList != null &&
                            t.RepeatWeekList.Contains(DateTimeUtility.GetTodayDayName())
                        )
                    )
                )
                .Include(U => U.User)
                .ToListAsync();

            allRunningTask = allRunningTask.Where(task => task.User.Id == UserId && !allTodoList.Any(todo => todo?.Task?.Id == task.Id))
                .ToList();

            foreach (var task in allRunningTask)
            {
                Todo todo = new Todo();
                todo.Task = task;
                todo.User = task.User;
                allTodoList.Add(todo);
                await _context.Todo.AddAsync(todo);
            }
            await _context.SaveChangesAsync();
            allTodoList.Sort((a, b) => a.Id.CompareTo(b.Id)); // sort the result...

            return allTodoList;
        }

        public async Task<IEnumerable<Todo>> GetAllTodoAsync(string UserId, Status status)
        {
            var startOfDay = DateTime.Today;
            var endOfDay = DateTime.Today.AddDays(1);

            //var task = from t in _context.Tasks
            //           where t.User.Id == UserId &&
            //           t.TaskStatus == Status.Running &&
            //           (
            //                t.Repeat == RepeatType.Daily ||
            //                (
            //                    t.Repeat == RepeatType.Weekly &&
            //                    t.RepeatWeekList.Contains(DateTimeUtility.GetTodayDayName())
            //                )
            //           ) &&
            //           (
            //                !_context.Todo.Any(td => td.Task.Id == t.Id && t.CreatedOn >= startOfDay && t.CreatedOn < endOfDay)
            //           )
            //           select t;

            if (status == Status.Running)
            {
                var resultSet = await _context.TodoSpWithTaskDTOs
                .FromSqlRaw("EXEC usp_AddTodoFromTask @UserId", new SqlParameter("@UserId", UserId))
                .ToListAsync();

                var result = _mapper.Map<List<Todo>>(resultSet);

                return result;
            }

            return await _context.Todo.Include(t => t.Task)
                .Where(td => td.User.Id == UserId && td.Status == status && (td.CreatedOn >= startOfDay && td.CreatedOn < endOfDay))
                .ToListAsync();
        }

        public async Task<IEnumerable<Todo>> GetAllTodoAsync(string UserId, Status status, string backup = "true")
        {
            // Step 1: Fetch all Todo items for today
            // We will also include the Task information and filter by status
            var startOfDay = DateTime.Today;
            var endOfDay = DateTime.Today.AddDays(1); // This will give you the start of the next day, i.e., 00:00:00 tomorrow

            var allTodoList = await _context.Todo
                        .Where(t => t.User.Id == UserId && t.CreatedOn >= startOfDay && t.CreatedOn < endOfDay)
                        .Include(t => t.Task)
                        .ToListAsync();

            if (status == Status.Running)
            { // only create the new instance when it is running...

                // Step 2: Get task IDs already assigned to Todo items for today
                var todoTaskIds = allTodoList.Select(t => t?.Task?.Id).ToList();

                // Fetch all running tasks, ensuring that the task is not associated with any Todo for today
                // Step 3: Fetch all running tasks for today that aren't already assigned to Todo
                var allRunningTasks = await _context.Tasks
                    .Where(t => t.User.Id == UserId &&
                        t.TaskStatus == Status.Running &&
                        (
                            t.Repeat == RepeatType.Daily ||
                            (
                                t.Repeat == RepeatType.Weekly &&
                                t.RepeatWeekList != null &&
                                t.RepeatWeekList.Contains(DateTimeUtility.GetTodayDayName())
                            )
                        ) &&
                        !todoTaskIds.Contains(t.Id)) // Ensure task is not already in a Todo for today
                    .Include(U => U.User)
                    .ToListAsync();


                // Step 4: Create new Todo instances for tasks not already assigned
                var newTodos = allRunningTasks.Select(task => new Todo
                {
                    Task = task,
                    User = task.User,
                    CreatedOn = DateTime.Now,
                    Status = Status.Running
                }).ToList();

                // Step 5: Add new Todos to the context and save changes
                if (newTodos.Any())
                {
                    await _context.Todo.AddRangeAsync(newTodos);
                    await _context.SaveChangesAsync();
                }
            }

            allTodoList = await _context.Todo.Where(t => t.User.Id == UserId &&
                t.Status == status && (t.CreatedOn >= startOfDay && t.CreatedOn < endOfDay))
                .ToListAsync();
            // sort the result...

            return allTodoList;
        }

        public async Task<Todo> GetTodoByIdAsync(string UserId, int Id)
        {
            var todo = await _context.Todo.FirstOrDefaultAsync(u => u.User.Id == UserId && u.Id == Id);

            if (todo == null)
            {
                throw new KeyNotFoundException($"Todo with ID {Id} not found.");
            }

            return todo;
        }

        public async Task UpdateTodoAsync(string UserId, Todo todo)
        {
            var user = todo.User ?? throw new ArgumentNullException(nameof(todo.User), "UserId Is Required!.");

            if (user.Id != UserId)
            {
                throw new ArgumentException("UserId and Todo.User.Id must be same!.");
            }

            _context.Todo.Update(todo);
            await _context.SaveChangesAsync();
        }

        public async Task<int> GetTodoCountByTodoStatus(string UserId, Status status)
        {
            return await _context.Todo.CountAsync(t => t.User.Id == UserId && t.Status == status);
        }

        public async Task<int> GetTodoCountByTodoStatusAndDateTimeRange(string UserId, Status status, DateTime from, DateTime end)
        {
            return await _context.Todo.CountAsync(t => t.User.Id == UserId && (t.CreatedOn >= from && t.CreatedOn < end && t.Status == status));
        }
    }
}
