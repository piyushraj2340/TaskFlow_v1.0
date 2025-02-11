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
