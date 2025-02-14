using AutoMapper;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;

namespace TaskMonitoringApp.Mappings
{
    public class NotesProfile : Profile
    {
        public NotesProfile() {
            CreateMap<Notes, NoteDTO>().ReverseMap();
            CreateMap<NoteDTO, NoteDTOWithGoalDTO>().ReverseMap();
        }
    }
}
