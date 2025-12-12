using AutoMapper;

using DoktarPlanning.Domain.Entities;
using DoktarPlanning.Infrastructure.DTOs;

namespace DoktarPlanning.Application.Mapping
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<TaskItem, TaskDto>()
                .ReverseMap();

            CreateMap<SubTask, SubTaskDto>().ReverseMap();
            CreateMap<RecurrenceRule, RecurrenceDto>().ReverseMap();
            CreateMap<User, UserDto>()
                .ForMember(d => d.Password, opt => opt.Ignore())
                .ReverseMap()
                .ForMember(d => d.PasswordHash, opt => opt.Ignore());

        }
    }
}