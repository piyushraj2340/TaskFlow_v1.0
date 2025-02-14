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

            CreateMap<GoalNameDTO, GoalWithTaskNameListViewModel>().ReverseMap();
            CreateMap<GoalDTOWithTaskNameDTOs, GoalWithTaskNameListViewModel>().ReverseMap();

            CreateMap<TodoDTO, TodoDTOWithTaskDTO>()
                .ForMember(dest => dest.TaskId, opt => opt.MapFrom(src => src.Task.Id)).ReverseMap();

            CreateMap<RegisterViewModel, Users>();
        }
    }
}
