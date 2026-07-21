using AutoMapper;
using JiraTrack.Models.DTOs.Auth;
using JiraTrack.Models.DTOs.Projects;
using JiraTrack.Models.DTOs.Users;
using JiraTrack.Models.Entities;

namespace JiraTrack.Models.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        CreateMap<User, UserProfileDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<User, UserListDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<User, UserDetailDto>()
            .ForMember(d => d.Roles, opt => opt.MapFrom(s => s.UserRoles.Select(ur => ur.Role.Name).ToList()));

        CreateMap<Role, RoleDto>();

        CreateMap<Project, ProjectListDto>()
            .ForMember(d => d.LeadName, opt => opt.MapFrom(s => $"{s.LeadUser.FirstName} {s.LeadUser.LastName}"))
            .ForMember(d => d.MemberCount, opt => opt.MapFrom(s => s.Members.Count))
            .ForMember(d => d.TaskCount, opt => opt.MapFrom(s => s.TaskCounter))
            .ForMember(d => d.BugCount, opt => opt.MapFrom(s => s.BugCounter));

        CreateMap<UpdateProfileRequest, User>()
            .ForMember(d => d.Id, opt => opt.Ignore())
            .ForMember(d => d.Email, opt => opt.Ignore())
            .ForMember(d => d.UserName, opt => opt.Ignore())
            .ForMember(d => d.PasswordHash, opt => opt.Ignore());
    }
}
