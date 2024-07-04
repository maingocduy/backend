using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace WebApplication3.Helper.Data
{
    public class Datacontext(DbContextOptions<Datacontext> options) : IdentityDbContext<ApplicationUser>(options)
    {

    }
}
