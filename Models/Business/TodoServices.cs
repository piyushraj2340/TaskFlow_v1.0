using AutoMapper;using TaskMonitoringApp.Models.DTOs;using TaskMonitoringApp.Models.Entities;using TaskMonitoringApp.Models.Repositories;using TaskMonitoringApp.Models.Services;using TodoMonitoringApp.Models.Entities;namespace TaskMonitoringApp.Models.Business{    public class TodoServices(ITodoRepository repository, IMapper mapper) : ITodoServices    {        private readonly ITodoRepository _repository = repository;        private readonly IMapper _mapper = mapper;        public async Task AddNewTodo(string UserId, TodoDTO todo)        {            if (todo.Task == null)            {                throw new ArgumentNullException("Missing {TaskID} while creating the new Todo.");            }            await _repository.AddTodoAsync(UserId, _mapper.Map<Todo>(todo));        }        public async Task DeleteTodoById(string UserId, int id)        {            var getTodo = await _repository.GetTodoByIdAsync<Todo>(UserId, id, ResponseDataMode.Model);            getTodo.IsDeleted = true;            getTodo.DeletedOn = DateTime.Now;            await _repository.UpdateTodoAsync(UserId, getTodo);        }        public async Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId, Status status)        {            return await _repository.GetAllTodoAsync<TodoDTOWithTaskDTO>(UserId, status, ResponseDataMode.ModelDTO);        }

        public async Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId, Status status, DateTime selectDate)        {            if (selectDate.Date == DateTime.Today.Date)
            {
                return await _repository.GetAllTodoAsync<TodoDTOWithTaskDTO>(UserId, status, ResponseDataMode.ModelDTO);            }            return await _repository.GetAllTodoAsync<TodoDTOWithTaskDTO>(UserId, status, selectDate, ResponseDataMode.ModelDTO);        }

        public async Task<IEnumerable<TodoDTOWithTaskDTO>> GetAllTodo(string UserId)
        {
            return await _repository.GetAllTodoAsync<TodoDTOWithTaskDTO>(UserId, Status.All, ResponseDataMode.ModelDTO);
        }        public async Task<TodoDTOWithTaskDTO> GetTodoById(string UserId, int id)        {            return await _repository.GetTodoByIdAsync<TodoDTOWithTaskDTO>(UserId, id, ResponseDataMode.ModelDTO);        }        public async Task UpdateTodo(string userId, TodoDTO todo)        {            var todoToUpdate = await _repository.GetTodoByIdAsync<Todo>(userId, todo.Id, ResponseDataMode.Model);            _mapper.Map(todo, todoToUpdate);            await _repository.UpdateTodoAsync(userId, todoToUpdate);        }        public async Task<TodoProgressAnalysisDTO> GetTodoProgressAnalyses(string userId, DateTime forDate)
        {
            if(forDate.Date == DateTime.Now.Date)
            {
                return await _repository.GetTodoProgressAnalysesAsync<TodoProgressAnalysisDTO>(userId, ResponseDataMode.ModelDTO);
            }

            return await _repository.GetTodoProgressAnalysesAsync<TodoProgressAnalysisDTO>(userId, forDate, ResponseDataMode.ModelDTO);
        }

        public async Task UpdateTodoStatus(string userId, int todoId, Status statusToChange)
        {
            await _repository.UpdateTodoStatusAsync(userId, todoId, statusToChange);
        }
    }}