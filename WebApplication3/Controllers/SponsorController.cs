using K4os.Compression.LZ4.Internal;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.Net.Mail;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.repository.AccountRepository;
using WebApplication3.Service.AccountService;
using WebApplication3.Service.SponsorService;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SponsorController : Controller
    {
        private readonly ISponsorService sponsorService;
        public SponsorController(ISponsorService sponsorService)
        {
            this.sponsorService = sponsorService;
        }
        [HttpPost("get_all_sponsor")]
        public async Task<ActionResult<List<sponsor>>> GetAllSponsor(GetAllSponsorRequest request)
        {
            var spon = await sponsorService.GetAllSponsor(request.pageNumber,request.projectId);
            return Ok(new { spons = spon.Data, totalPages = spon.TotalPages });
        }
        [HttpGet("{name}")]
        public async Task<ActionResult<sponsor>> GetSponsor(string name)
        {
            var spon = await sponsorService.GetSponsor(name);
            return Ok(spon);
        }
        [HttpPost("add_sponsor")]
        public async Task<ActionResult> AddSponsor(CreateRequestSponsorDTO create)
        {
            try
            {
                await sponsorService.AddSponsor(create);
                return Ok(new { message = "sponsor created" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ex.Message);
            }    

        }
        [HttpDelete]
        public async Task<ActionResult> DeleteSponsor(string name)
        {
            await sponsorService.DeleteSponsor(name);
            return Ok(new { message = "Sponsor deleted" });
        }
        [HttpPut]
        public async Task<ActionResult> UpdateSponsor(string name,UpdateRequestSponsorDTO up)
        {
            await sponsorService.UpdateSponsors(name,up);
            return Ok(new { message = "Sponsor Updated" });
        }


    }
}
