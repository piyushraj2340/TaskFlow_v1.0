using AutoMapper;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.ViewModel;

namespace TaskMonitoringApp.Mappings
{
    public class GoalProfile : Profile
    {
        public GoalProfile()
        {
            CreateMap<Goals, GoalDTO>().ReverseMap();
            CreateMap<GoalDTO, GoalViewModel>().ReverseMap();
            CreateMap<GoalDTO, GoalDTOWithTaskNameListDTO>().ReverseMap();
            CreateMap<GoalDTO, GoalDTOWithTaskListDTO>().ReverseMap();
            CreateMap<GoalDTO, GoalDTOWithNoteListDTO>().ReverseMap();
            CreateMap<GoalDTO, GoalDTOWithNotesAndTaskNameListDTO>().ReverseMap();
            CreateMap<GoalViewModel, GoalWithNotesListViewModel>().ReverseMap();
            CreateMap<GoalViewModel, GoalWithNotesAndTaskNameListViewModel>().ReverseMap();
            CreateMap<GoalDTOWithNoteListDTO, GoalWithNotesListViewModel>().ReverseMap();
            CreateMap<GoalNameDTO, GoalWithTaskNameListViewModel>().ReverseMap();
            CreateMap<GoalDTOWithTaskNameListDTO, GoalWithTaskNameListViewModel>().ReverseMap();
            CreateMap<GoalDTOWithTaskNameListDTO, GoalWithNotesAndTaskNameListViewModel>().ReverseMap();
        }
    }
}
