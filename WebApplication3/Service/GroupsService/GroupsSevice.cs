using AutoMapper;
using System.Xml.Linq;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.repository.GroupsRepository;
using WebApplication3.repository.SponsorRepository;

namespace WebApplication3.Service.GroupsService
{
    public interface IGroupsService
    {
        Task<List<Group>> GetAllGroups();
        Task AddGroup(GroupsDTOs Group);
        Task DeleteGroup(string name);
        Task<Group> GetGroup(int id);
        Task UpdateGroup(int id, GroupsDTOs Group);
    }
    public class GroupsSevice : IGroupsService
    {
        private readonly IGroupsRepository IGroupsRepository;
        private readonly IMapper _mapper;
        public GroupsSevice(IGroupsRepository IGroupsRepository, IMapper mapper)
        {
            this.IGroupsRepository = IGroupsRepository;
            _mapper = mapper;

        }
        public async Task AddGroup(GroupsDTOs group)
        {
            await IGroupsRepository.AddGroups(group);
        }

        public async Task DeleteGroup(string name)
        {
            await IGroupsRepository.DeleteGroups(name);
        }

        public async Task<List<Group>> GetAllGroups()
        {
            return await IGroupsRepository.GetAllGroups();
        }

        public async Task<Group> GetGroup(int id)
        {
            var group = await IGroupsRepository.GetGroup(id);

            if (group == null)
                throw new KeyNotFoundException("Account not found");

            return group;
        }

        public Task UpdateGroup(int id, GroupsDTOs Group)
        {
            throw new NotImplementedException();
        }
    }
}
