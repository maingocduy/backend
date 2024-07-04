using AutoMapper;
using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Web.Http.ModelBinding;
using WebApplication3.DTOs;
using WebApplication3.DTOs.Account;
using WebApplication3.DTOs.Member;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.repository.AccountRepository;
using WebApplication3.repository.MemberRepository;
using WebApplication3.repository.ProjectReposiotry;
using WebApplication3.repository.SponsorRepository;


namespace WebApplication3.Service.SponsorService
{
    public interface ISponsorService
    {
        Task<PagedResult<SponsorDTO>> GetAllSponsor(int pageNumber, int? ProjectId = null);
        Task AddSponsor(CreateRequestSponsorDTO sponsor);
        Task DeleteSponsor(string name);
        Task<SponsorDTO> GetSponsor(string name);
        Task UpdateSponsors(string name, UpdateRequestSponsorDTO sponsor);
    }
    public class SponsorService : ISponsorService
    {
        private readonly ISponsorRepository ISponsorRepository;
        private readonly IMapper _mapper;
        private readonly IProjectRepository projectRepository;
        public SponsorService(ISponsorRepository ISponsorRepository,IProjectRepository projectRepository, IMapper mapper)
        {
            this.ISponsorRepository = ISponsorRepository;
            _mapper = mapper;
            this.projectRepository = projectRepository;
        }

        public async Task AddSponsor(CreateRequestSponsorDTO createRequestSponsorDTO)
        {
            var project = await projectRepository.GetProjectID(createRequestSponsorDTO.nameProject);
            if (project == null)
            {
                throw new KeyNotFoundException("Project not found");
            }
           
                var sponsorDTO = _mapper.Map<SponsorDTO>(createRequestSponsorDTO);

                await ISponsorRepository.AddSponsor(project.Project_id, sponsorDTO);
           
        }

        public async Task DeleteSponsor(string name)
        {
            await ISponsorRepository.DeleteSponsor(name);
        }
        
        public async Task<PagedResult<SponsorDTO>> GetAllSponsor(int pageNumber ,int? ProjectId = null)
        {
            return await ISponsorRepository.GetAllSponsor(pageNumber,ProjectId);
        }

        public async Task<SponsorDTO> GetSponsor(string name)
        {
            var spon = await ISponsorRepository.GetSponsor(name);

            if (spon == null)
                throw new KeyNotFoundException("Account not found");

            return spon;
        }

        public async Task UpdateSponsors(string name,UpdateRequestSponsorDTO sponsor)
        {
            var sposo = await ISponsorRepository.GetSponsor(name);

            if (sposo == null)
                throw new KeyNotFoundException("User not found");

            // copy model props to user
            _mapper.Map(sponsor, sposo);

            // save user
            await ISponsorRepository.UpdateSponsors(sposo);
        }
    }
}
