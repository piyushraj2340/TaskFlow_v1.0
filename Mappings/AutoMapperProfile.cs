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
            CreateMap<Goals, GoalDTO>().ReverseMap();
            CreateMap<GoalDTO, GoalViewModel>().ReverseMap();

            CreateMap<GoalDTO, GoalDTOWithTaskNameDTOs>(); 
            CreateMap<GoalDTOWithTaskNameDTOs, GoalDTO>();

            CreateMap<GoalDTO, GoalDTOWithTaskDTOs>();
            CreateMap<GoalDTOWithTaskDTOs, GoalDTO>();

            CreateMap<GoalDTO, NoteDTOWithGoalDTO>();
            CreateMap<NoteDTOWithGoalDTO, GoalDTO>();

            CreateMap<GoalDTO, NoteDTOWithGoalDTO>();

            CreateMap<GoalDTO, GoalDTOWithNoteListDTO>();

            CreateMap<GoalViewModel, GoalWithNotesListViewModel>();

            CreateMap<GoalDTOWithNoteListDTO, GoalWithNotesListViewModel>().ReverseMap();



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

            CreateMap<RegisterViewModel, Users>();
        }
    }
}
