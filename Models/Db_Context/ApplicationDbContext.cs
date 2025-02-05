using AutoMapper;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TodoMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Models.Data
{
    public class ApplicationDbContext : IdentityDbContext<Users>
    {
        private readonly ILogger<ApplicationDbContext> _logger;

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ILogger<ApplicationDbContext> logger) : base(options)
        {
            _logger = logger;
        }

        public DbSet<Goals> Goals { get; set; }

        public DbSet<Tasks> Tasks { get; set; }

        public DbSet<GoalTask> GoalTasks { get; set; }

        public DbSet<Todo> Todo { get; set; }

        public DbSet<TodoProgressAnalysis> TodoProgressAnalyses { get; set; }


        // Adding the sp_entity_data
        

        public DbSet<InsertUpdateSpWithIdDTO> InsertUpdateSpWithIdDTO { get; set; }

        public DbSet<GoalDTO> GoalDTOs { get; set; }
        public DbSet<GoalNameDTO> GoalNameDTOs { get; set; }

        public DbSet<TaskDTO> TaskDTOs { get; set; }
        public DbSet<TaskNameDTO> TaskNameDTOs { get; set; }

        public DbSet<TodoWithTask> TodoWithTask { get; set; }
        public DbSet<TodoDTOWithTaskDTO> TodoWithTaskDTO { get; set; }

        public DbSet<TodoProgressAnalysisDTO> TodoProgressAnalysesDTO { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Ensure CalculateDateFor is unique
            modelBuilder.Entity<TodoProgressAnalysis>()
                .HasIndex(c => c.CalculateDateFor)
                .IsUnique(); // Enforce uniqueness of the date

            // Define many-to-many relationship using the GoalTask table
            modelBuilder.Entity<GoalTask>()
                .HasKey(gt => new { gt.GoalId, gt.TaskId });

            modelBuilder.Entity<GoalTask>()
                .HasOne(gt => gt.Goal)
                .WithMany(g => g.GoalTasks)
                .HasForeignKey(gt => gt.GoalId);

            modelBuilder.Entity<GoalTask>()
                .HasOne(gt => gt.Task)
                .WithMany(t => t.GoalTasks)
                .HasForeignKey(gt => gt.TaskId);

            modelBuilder.Entity<InsertUpdateSpWithIdDTO>().ToView(null);
            modelBuilder.Entity<GoalDTO>().ToView(null);
            modelBuilder.Entity<GoalNameDTO>().ToView(null);
            modelBuilder.Entity<TaskDTO>().ToView(null);
            modelBuilder.Entity<TaskNameDTO>().ToView(null);
            modelBuilder.Entity<TodoWithTask>().ToView(null);
            modelBuilder.Entity<TodoDTOWithTaskDTO>().ToView(null);
            modelBuilder.Entity<TodoProgressAnalysisDTO>().ToView(null);
        }

        public override int SaveChanges()
        {

            _logger.LogInformation("SaveChanges function invoked!>..");

            // Get all tracked entities that are being updated
            var entities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            if (!entities.Any())
            {
                _logger.LogWarning("No entities were modified during SaveChanges.");
            }

            _logger.LogInformation("Get all tracked entities that are being updated {entities}", entities);

            foreach (var entity in entities)
            {
                _logger.LogInformation("Set UpdatedOn field to current timestamp {entity}", entity);


                if (entity.Entity is Goals goal)
                {
                    _logger.LogInformation("Setting UpdateOn for Goal entity with ID: {GoalId}", goal.Id);
                    goal.UpdatedOn = DateTime.Now;  // Use UTC to avoid time zone issues
                }

                if (entity.Entity is Tasks task)
                {
                    _logger.LogInformation("Setting UpdateOn for Task entity with ID: {TaskId}", task.Id);
                    task.UpdatedOn = DateTime.Now;  // Use UTC to avoid time zone issues
                }

                if (entity.Entity is Todo todo)
                {
                    _logger.LogInformation("Setting UpdateOn for Todo entity with ID: {TodoId}", todo.Id);
                    todo.UpdatedOn = DateTime.Now;  // Use UTC to avoid time zone issues
                }

                if (entity.Entity is TodoProgressAnalysis tpa)
                {
                    _logger.LogInformation("Setting UpdateOn for TodoProgressAnalysis entity with ID: {TodoProgressAnalysisId}", tpa.Id);
                    tpa.UpdatedOn = DateTime.Now;  // Use UTC to avoid time zone issues
                }
            }

            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("SaveChangesAsync function invoked!");

            // Get all tracked entities that are being updated
            var entities = ChangeTracker.Entries()
                .Where(e => e.State == EntityState.Modified)
                .ToList();

            _logger.LogInformation("Tracked modified entities: {EntityCount}", entities.Count);

            foreach (var entity in entities)
            {
                if (entity.Entity is Goals goal)
                {
                    _logger.LogInformation("Setting UpdateOn for Goal entity with ID: {GoalId}", goal.Id);
                    goal.UpdatedOn = DateTime.Now;  // Use UTC to avoid time zone issues
                }

                if (entity.Entity is Tasks task)
                {
                    _logger.LogInformation("Setting UpdateOn for Task entity with ID: {TaskId}", task.Id);
                    task.UpdatedOn = DateTime.Now;  // Use UTC to avoid time zone issues
                }

                if (entity.Entity is Todo todo)
                {
                    _logger.LogInformation("Setting UpdateOn for Task entity with ID: {TodoId}", todo.Id);
                    todo.UpdatedOn = DateTime.Now;  // Use UTC to avoid time zone issues
                }

                if (entity.Entity is TodoProgressAnalysis tpa)
                {
                    _logger.LogInformation("Setting UpdateOn for TodoProgressAnalysis entity with ID: {TodoProgressAnalysisId}", tpa.Id);
                    tpa.UpdatedOn = DateTime.Now;  // Use UTC to avoid time zone issues
                }
            }

            // Call the base SaveChangesAsync method to save the changes to the database
            return await base.SaveChangesAsync(cancellationToken);
        }
    }
}
