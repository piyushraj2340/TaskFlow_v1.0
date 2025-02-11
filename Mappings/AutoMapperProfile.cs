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

            CreateMap<Notes, NoteDTO>();
            CreateMap<NoteDTO, Notes>();
            CreateMap<NoteDTO, NoteDTOWithGoalDTO>().ReverseMap();

            CreateMap<RegisterViewModel, Users>();
        }
    }
}
