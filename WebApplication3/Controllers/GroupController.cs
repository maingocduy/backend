using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WebApplication3.DTOs.Groups;
using WebApplication3.DTOs.Sponsor;
using WebApplication3.Entities;
using WebApplication3.Service.GroupsService;
using WebApplication3.Service.SponsorService;

namespace WebApplication3.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GroupController : ControllerBase
    {
        private readonly IGroupsService IGroupsService;
        public GroupController(IGroupsService IGroupsService)
        {
            this.IGroupsService = IGroupsService;
        }
        [HttpGet]
        public async Task<ActionResult<List<Group>>> GetAllGroups()
        {
            var spon = await IGroupsService.GetAllGroups();
            return Ok(spon);
        }
        [HttpGet("id")]
        public async Task<ActionResult<sponsor>> GetGroup(int id)
        {
            var group = await IGroupsService.GetGroup(id);
            return Ok(group);
        }
        [HttpPost]
        public async Task<ActionResult> AddGroup(GroupsDTOs create)
        {
            await IGroupsService.AddGroup(create);
            return Ok(new { message = "Group created" });
        }
        [HttpDelete]
        public async Task<ActionResult> DeleteGroup(string name)
        {
            await IGroupsService.DeleteGroup(name);
            return Ok(new { message = "Group deleted" });
        }
   
        

    }
}
