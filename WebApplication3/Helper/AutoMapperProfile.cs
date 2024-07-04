using AutoMapper;
using Microsoft.OpenApi.Writers;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Auth;
using WebApplication3.DTOs.Blog;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.Member;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;

namespace WebApplication3.Helper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<account, AccountDTO>();
            CreateMap<AccountDTO, account>()
            .ForMember(dest => dest.Account_id, opt => opt.Ignore()); // Không ánh xạ trường Account_id vì nó có thể được sinh tự động hoặc không cần thiết.
            CreateMap<sponsor, SponsorDTO>();
            CreateMap<SponsorDTO, sponsor>();
            CreateMap<CreateAccountRequestDTO, AccountDTO>()
                 .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.username))
          .ForMember(dest => dest.Password, opt => opt.MapFrom(src => src.password));
            CreateMap<UpdatePasswordRequestDTO, AccountDTO>();
            CreateMap<CreateRequestSponsorDTO, SponsorDTO>();
            CreateMap<UpdateRequestSponsorDTO, SponsorDTO>()

            .ForAllMembers(x => x.Condition(
                (src, dest, prop) =>
                {
                    // ignore both null & empty string properties
                    if (prop == null) return false;
                    if (prop.GetType() == typeof(string) && string.IsNullOrEmpty((string)prop)) return false;
                    return true;
                }
            ));
            CreateMap<CreateRequestMemberDTO, MemberDTO>()
     .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
     .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.Email))
     .ForMember(dest => dest.phone, opt => opt.MapFrom(src => src.Phone))
     .ForMember(dest => dest.groups, opt => opt.MapFrom(src => new Group { group_name = src.Group_name }));

            CreateMap<CreateAccountRequestDTO, MemberDTO>()
           .ForMember(dest => dest.name, opt => opt.MapFrom(src => src.Name))
          .ForMember(dest => dest.email, opt => opt.MapFrom(src => src.Email))
              .ForMember(dest => dest.phone, opt => opt.MapFrom(src => src.Phone))
          .ForMember(dest => dest.groups, opt => opt.MapFrom(src => new Group { group_name = src.group_name }));
            CreateMap<blog, BlogDTO>()
                .ForMember(dest => dest.Blog_id, opt => opt.MapFrom(src => src.Blog_id));
            CreateMap<CreateRequestBLogDTO, BlogDTO>();

        }
    }
}
