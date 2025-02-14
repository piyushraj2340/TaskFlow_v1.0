using AutoMapper;
using TaskMonitoringApp.Models.DTOs;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.ViewModel;

namespace TaskMonitoringApp.Mappings
{
    public class TaskProfile : Profile
    {
        public TaskProfile()
        {
            CreateMap<Tasks, TaskDTO>().ReverseMap();
            CreateMap<TaskDTO, TaskViewModel>().ReverseMap();
            CreateMap<TaskDTO, TaskStatusDTO>().ReverseMap();
            CreateMap<TaskDTO, TaskDTOWithGoalNameListDTO>().ReverseMap(); 
            CreateMap<TaskDTO, TaskDTOWithGoalListDTO>().ReverseMap();
        }
    }
}
