using AutoMapper;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.ViewModel;

namespace TaskMonitoringApp.Mappings
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Tasks, TaskDTO>();
            CreateMap<TaskDTO, Tasks>();
            CreateMap<Tasks, TaskViewModel>();
            CreateMap<TaskViewModel, Tasks>();
            CreateMap<TaskStatusDTO, Tasks>();
            CreateMap<Tasks, TaskStatusDTO>();
            CreateMap<TaskDTO, TaskViewModel>();
            CreateMap<TaskViewModel, TaskDTO>();
            CreateMap<TaskDTOWithGoalNameDTOs, TaskViewModel>();
            CreateMap<TaskViewModel, TaskDTOWithGoalNameDTOs>();
            CreateMap<TaskDTO, TaskDTOWithGoalNameDTOs>();
            CreateMap<TaskDTOWithGoalNameDTOs, TaskDTO>();
            CreateMap<TaskDTO, TaskDTOWithGoalDTOs>();
            CreateMap<TaskDTOWithGoalDTOs, TaskDTO>();


            CreateMap<Todo, TodoDTO>();
            CreateMap<TodoDTO, Todo>();
            CreateMap<TodoStatusDTO, Todo>();
            CreateMap<Todo, TodoStatusDTO>();
            CreateMap<TodoDTO, TodoDTOWithTaskDTO>().ReverseMap();

            CreateMap<Notes, NoteDTO>();
            CreateMap<NoteDTO, Notes>();
            CreateMap<NoteDTO, NoteDTOWithGoalDTO>().ReverseMap();

            CreateMap<RegisterViewModel, Users>();
        }
    }
}
