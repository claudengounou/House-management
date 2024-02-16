using House_management_Api.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace House_management_Api.Data
{
    public class Context_identity : IdentityDbContext<User>
    {
        public Context_identity(DbContextOptions<Context_identity> options) : base(options)
        {
            
        }
    }
}
