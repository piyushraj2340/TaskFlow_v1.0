using TaskMonitoringApp.Models.DTOs;using TaskMonitoringApp.Models.Entities;using TaskMonitoringApp.Models.Repositories;using TaskMonitoringApp.Models.Services;using TodoMonitoringApp.Models.Entities;namespace TaskMonitoringApp.Models.Business{    public class TodoServices : ITodoServices    {        private readonly ITodoRepository _repository;        public TodoServices(ITodoRepository repository)        {            _repository = repository;        }        public async Task AddNewTodo(string UserId, Todo todo)        {            if (todo.Task == null)            {                throw new ArgumentNullException("Missing {TaskID} while creating the new Todo.");            }            await _repository.AddTodoAsync(UserId, todo);        }        public async Task DeleteTodoById(string UserId, int id)        {            var getTodo = await GetTodoById(UserId, id);            getTodo.IsDeleted = true;            getTodo.DeletedOn = DateTime.Now;            await UpdateTodo(UserId, getTodo);        }        public async Task<IEnumerable<Todo>> GetAllTodo(string UserId, Status status)        {            return await _repository.GetAllTodoAsync<Todo>(UserId, status, ResponseDataMode.Model);        }        public async Task<Todo> GetTodoById(string UserId, int id)        {            return await _repository.GetTodoByIdAsync<Todo>(UserId, id, ResponseDataMode.Model);        }        public async Task UpdateTodo(string UserId, Todo todo)        {            await _repository.UpdateTodoAsync(UserId, todo);        }

        public async Task MarkAsComplete(string UserId, Todo todo)
        {
            if (todo.DeletedOn != null)
            {
                // Mark as deleted.... 
                todo.IsDeleted = true;
                await _repository.UpdateTodoAsync(UserId, todo);

                throw new ArgumentException("Deleted todo can't be mark as complete!");
            }

            if (DateTime.Now > todo.EndDate)
            {
                // Mark as Ended.... 
                todo.Status = Status.Ended;
                await _repository.UpdateTodoAsync(UserId, todo);

                throw new ArgumentException("Ended todo can't be mark as complete!");
            }

            todo.Status = Status.Completed;
            await _repository.UpdateTodoAsync(UserId, todo);
        }

        public async Task MoveToRunning(string UserId, Todo todo)
        {
            if (DateTime.Now > todo.EndDate)
            {
                throw new ArgumentException("The end date must be in the future.");
            }

            todo.DeletedOn = null; // if goal is deleted and want to move to the running...
            todo.Status = Status.Running;
            await _repository.UpdateTodoAsync(UserId, todo);
        }        Task<TodoProductivityDTO> ITodoServices.GetProgressForTodays(string UserId)
        {
            throw new NotImplementedException();
            //var startOfDay = DateTime.Today;
            //var endOfDay = DateTime.Today.AddDays(1); // This will give you the start of the next day, i.e., 00:00:00 tomorrow

            //int runningTodo = await _repository.GetTodoCountByTodoStatusAndDateTimeRange(UserId, Status.Running, startOfDay, endOfDay);

            //int completedTodo = await _repository.GetTodoCountByTodoStatusAndDateTimeRange(UserId, Status.Completed, startOfDay, endOfDay);

            //double productivity = 0;

            //if(runningTodo + completedTodo > 0)
            //{
            //    productivity =  (((double)completedTodo / (runningTodo + completedTodo)) * 100);
            //}
            //productivity = Math.Round(productivity, 2);

            //TodoProductivityDTO todoProductivity = new TodoProductivityDTO(productivity, runningTodo, completedTodo, runningTodo + completedTodo);

            //// Round to 2 decimal places

            //return todoProductivity;
        }

        Task<IEnumerable<Todo>> ITodoServices.GetAllTodo(string UserId)
        {
            throw new NotImplementedException();
        }
    }}