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
            CreateMap<Goals, GoalDTO>();
            CreateMap<GoalDTO, Goals>();
            CreateMap<Goals, GoalViewModel>();
            CreateMap<GoalViewModel, Goals>();
            CreateMap<GoalStatusDTO, Goals>();
            CreateMap<Goals, GoalStatusDTO>();
            CreateMap<Goals, GoalNameDTO>();
            CreateMap<GoalDTO, GoalViewModel>();
            CreateMap<GoalViewModel, GoalDTO>();
            CreateMap<GoalDTO, GoalDTOWithTaskNameDTOs>();
            CreateMap<GoalDTOWithTaskNameDTOs, GoalDTO>();
            CreateMap<GoalDTO, GoalDTOWithTaskDTOs>();
            CreateMap<GoalDTOWithTaskDTOs, GoalDTO>();
            CreateMap<GoalNameDTO, GoalWithTaskNameListViewModel>().ReverseMap();
            CreateMap<GoalDTOWithTaskNameDTOs, GoalWithTaskNameListViewModel>().ReverseMap();



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

            CreateMap<RegisterViewModel, Users>();
        }
    }
}
