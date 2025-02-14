using AutoMapper;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Mappings
{
    public class TodoProfile : Profile
    {
        public TodoProfile()
        {
            CreateMap<Todo, TodoDTO>().ReverseMap(); 
            CreateMap<TodoDTO, TodoStatusDTO>().ReverseMap(); ;
            CreateMap<TodoDTO, TodoDTOWithTaskDTO>().ReverseMap();
            CreateMap<TodoDTO, TodoDTOWithTaskDTO>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Task.Id)).ReverseMap();
        }
    }
}
