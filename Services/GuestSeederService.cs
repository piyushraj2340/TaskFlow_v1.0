using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.Data;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.Enums;

using Microsoft.Extensions.Configuration;

namespace TaskMonitoringApp.Services
{
    public class GuestSeederService : IGuestSeederService
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<Users> _userManager;
        private readonly IConfiguration _configuration;
        private readonly ILogger<GuestSeederService> _logger;

        public GuestSeederService(
            ApplicationDbContext dbContext,
            UserManager<Users> userManager,
            IConfiguration configuration,
            ILogger<GuestSeederService> logger)
        {
            _dbContext = dbContext;
            _userManager = userManager;
            _configuration = configuration;
            _logger = logger;
        }

        public async Task EnsureGuestDataExistsAsync()
        {
            var guestEmail = "guest@taskmonitorapp.com";
            var guestUser = await _userManager.FindByEmailAsync(guestEmail);
            
            if (guestUser == null)
            {
                await CleanupAndReseedGuestAsync();
                return;
            }

            // Check the oldest goal's creation date to determine if the seed data is older than 24 hours
            var firstGoal = await _dbContext.Goals
                .Where(g => g.UserId == guestUser.Id)
                .FirstOrDefaultAsync();

            var hasTasks = await _dbContext.Tasks.AnyAsync(t => t.UserId == guestUser.Id);

            if (firstGoal == null || !hasTasks)
            {
                await CleanupAndReseedGuestAsync();
                return;
            }

            // If the guest data was created/seeded more than 24 hours ago, force a clean and re-seed
            if (firstGoal.CreatedOn < DateTime.Now.AddDays(-1))
            {
                _logger.LogInformation("Guest data is older than 24 hours. Cleaning and re-seeding guest data on login...");
                await CleanupAndReseedGuestAsync();
            }
        }

        public async Task CleanupAndReseedGuestAsync()
        {
            _logger.LogInformation("Starting Guest account cleanup and data seeding...");

            // 1. Ensure Guest User exists
            var guestEmail = "guest@taskmonitorapp.com";
            var guestUser = await _userManager.FindByEmailAsync(guestEmail);
            if (guestUser == null)
            {
                guestUser = new Users
                {
                    UserName = "guest",
                    Email = guestEmail,
                    FirstName = "Guest",
                    LastName = "User",
                    EmailConfirmed = true
                };
                var guestPassword = _configuration["GuestSettings:Password"] ?? "GuestPass123!";
                var createResult = await _userManager.CreateAsync(guestUser, guestPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(", ", createResult.Errors.Select(e => e.Description));
                    _logger.LogError("Failed to create guest user: {Errors}", errors);
                    throw new Exception($"Failed to create guest user: {errors}");
                }
            }

            var userId = guestUser.Id;

            // 2. Clean up existing guest data (in correct order of dependencies)
            
            var notes = await _dbContext.Notes.Where(n => n.UserId == userId).ToListAsync();
            _dbContext.Notes.RemoveRange(notes);

            var todos = await _dbContext.Todo.Where(t => t.UserId == userId).ToListAsync();
            _dbContext.Todo.RemoveRange(todos);

            // GoalTask join records
            var goalTasks = await _dbContext.GoalTasks
                .Include(gt => gt.Goal)
                .Where(gt => gt.Goal.UserId == userId)
                .ToListAsync();
            _dbContext.GoalTasks.RemoveRange(goalTasks);

            var tasks = await _dbContext.Tasks.Where(t => t.UserId == userId).ToListAsync();
            _dbContext.Tasks.RemoveRange(tasks);

            var goals = await _dbContext.Goals.Where(g => g.UserId == userId).ToListAsync();
            _dbContext.Goals.RemoveRange(goals);

            var items = await _dbContext.Items
                .Include(i => i.Collection)
                .Where(i => i.Collection.UserId == userId)
                .ToListAsync();
            _dbContext.Items.RemoveRange(items);

            var collections = await _dbContext.Collections.Where(c => c.UserId == userId).ToListAsync();
            _dbContext.Collections.RemoveRange(collections);

            var progressAnalyses = await _dbContext.TodoProgressAnalyses.Where(pa => pa.UserId == userId).ToListAsync();
            _dbContext.TodoProgressAnalyses.RemoveRange(progressAnalyses);

            await _dbContext.SaveChangesAsync();
            _logger.LogInformation("Successfully cleaned up old Guest database records.");

            // 3. Seed new guest data

            // A. Ensure Categories exist (Shared table)
            var personalCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == "Personal");
            if (personalCategory == null)
            {
                personalCategory = new Category { Name = "Personal" };
                _dbContext.Categories.Add(personalCategory);
                await _dbContext.SaveChangesAsync();
            }

            var workCategory = await _dbContext.Categories.FirstOrDefaultAsync(c => c.Name == "Work");
            if (workCategory == null)
            {
                workCategory = new Category { Name = "Work" };
                _dbContext.Categories.Add(workCategory);
                await _dbContext.SaveChangesAsync();
            }

            // B. Seed Goals
            var masterGoal = new Goals
            {
                Name = "Master the Task Monitoring App",
                Description = "Learn all the core features of TMA to boost your daily productivity.",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(7).Date.AddHours(23).AddMinutes(59),
                GoalStatus = Status.Running,
                Priority = Priority.High,
                UserId = userId,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };
            _dbContext.Goals.Add(masterGoal);
            await _dbContext.SaveChangesAsync();

            // C. Seed Tasks
            var exploreTask = new Tasks
            {
                Name = "Explore the Dashboard Analytics",
                Description = "Look at the productivity percentages and growth charts on the home page.",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(2).Date.AddHours(23).AddMinutes(59),
                TaskStatus = Status.Running,
                Priority = Priority.High,
                UserId = userId,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                RepeatWeekList = new List<Weekly>()
            };

            var firstTodoTask = new Tasks
            {
                Name = "Complete your first daily To-Do",
                Description = "Mark a task todo as completed and verify your score grows.",
                StartDate = DateTime.Today,
                EndDate = DateTime.Today.AddDays(1).Date.AddHours(23).AddMinutes(59),
                TaskStatus = Status.Running,
                Priority = Priority.Medium,
                UserId = userId,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                RepeatWeekList = new List<Weekly>()
            };

            _dbContext.Tasks.AddRange(exploreTask, firstTodoTask);
            await _dbContext.SaveChangesAsync();

            // D. Link Goals and Tasks
            _dbContext.GoalTasks.AddRange(
                new GoalTask { GoalId = masterGoal.Id, TaskId = exploreTask.Id, UserId = userId },
                new GoalTask { GoalId = masterGoal.Id, TaskId = firstTodoTask.Id, UserId = userId }
            );

            // E. Seed To-Dos
            var watchTourTodo = new Todo
            {
                Notes = "Watch the TMA website tour to learn the layout.",
                EndDate = DateTime.Today.AddDays(1).Date,
                Status = Status.Running,
                TaskId = exploreTask.Id,
                UserId = userId,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };

            var stickyNoteTodo = new Todo
            {
                Notes = "Create a sticky note for urgent reminders.",
                EndDate = DateTime.Today.AddDays(1).Date,
                Status = Status.Running,
                TaskId = firstTodoTask.Id,
                UserId = userId,
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now
            };

            _dbContext.Todo.AddRange(watchTourTodo, stickyNoteTodo);

            // F. Seed Notes
            var tmaNote = new Notes
            {
                Title = "TMA Quick Start Guide",
                Content = @"<h3><strong>Welcome to Task Monitoring App!</strong></h3>
<p>As a guest, you have full access to TMA features. You can:
<ul>
  <li>Create high-level <strong>Goals</strong> and split them into actionable <strong>Tasks</strong>.</li>
  <li>Manage daily workflows under the <strong>To-Do</strong> tab.</li>
  <li>Write and pin sticky <strong>Notes</strong> like this one!</li>
</ul>
<p><em>Note: All changes and records on this guest account reset daily at 12:00 AM midnight.</em></p>",
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                TimeStamp = DateTime.Now,
                IsPinned = true,
                UserId = userId
            };

            var goalNote = new Notes
            {
                Title = "Mastering TMA Action Steps",
                Content = @"<h4><strong>Action Plan for Goal Success</strong></h4>
<p>To master the application, complete the following milestones:</p>
<ol>
  <li><strong>Explore Analytics:</strong> Look at your growth indicators on the dashboard.</li>
  <li><strong>Check off staging items:</strong> Complete linked tasks to boost your score.</li>
  <li><strong>Create Notes:</strong> Attach reminders directly to tasks or goals.</li>
</ol>",
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                TimeStamp = DateTime.Now,
                IsPinned = false,
                GoalId = masterGoal.Id,
                UserId = userId
            };

            var taskNote = new Notes
            {
                Title = "Dashboard Analytics Guide",
                Content = @"<h5><strong>Productivity Score Indicators</strong></h5>
<p>Here is how your indicators are calculated:</p>
<ul>
  <li><strong>Goals Achieved:</strong> High-level goals marked complete.</li>
  <li><strong>Tasks Completed:</strong> Sub-tasks checked off.</li>
  <li><strong>Growth:</strong> Dynamic weekly average velocity compared to past performance.</li>
</ul>",
                CreatedOn = DateTime.Now,
                UpdatedOn = DateTime.Now,
                TimeStamp = DateTime.Now,
                IsPinned = false,
                TaskId = exploreTask.Id,
                UserId = userId
            };

            _dbContext.Notes.AddRange(tmaNote, goalNote, taskNote);
            await _dbContext.SaveChangesAsync();

            // G. Seed Collections and Items
            var workCollection = new Collection
            {
                Name = "Project Work Tasks",
                UserId = userId
            };
            
            var personalCollection = new Collection
            {
                Name = "Personal Reminders",
                UserId = userId
            };
            
            _dbContext.Collections.AddRange(workCollection, personalCollection);
            await _dbContext.SaveChangesAsync();

            var migrationItem = new Item
            {
                Name = "Database Migration Setup",
                Description = "Run and test EF Core migration on Development database environment.",
                CollectionId = workCollection.CollectionId
            };
            migrationItem.Categories.Add(workCategory);

            var checkBackupItem = new Item
            {
                Name = "Verify Azure App Settings Backup",
                Description = "Confirm automated nightly backup settings are active on Azure portal.",
                CollectionId = personalCollection.CollectionId
            };
            checkBackupItem.Categories.Add(personalCategory);

            _dbContext.Items.AddRange(migrationItem, checkBackupItem);
            await _dbContext.SaveChangesAsync();

            _logger.LogInformation("Successfully completed Guest account data seeding.");
        }
    }
}
