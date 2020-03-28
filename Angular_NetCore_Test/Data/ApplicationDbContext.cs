
using Angular_NetCore_Test.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Angular_NetCore_Test.Data
{
    public class ApplicationDbContext : IdentityDbContext<IdentityUser>
    {
        public ApplicationDbContext( DbContextOptions<ApplicationDbContext> options) : base (options)
        {

        }


        //Craete role for Application

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<IdentityRole>().HasData
            (
                new { Id = "1", Name="Admin", NormalizedName ="ADMIN" },
                new { Id = "2", Name = "Customer", NormalizedName = "CUSTOMER" },
                new { Id = "3", Name = "Moderator", NormalizedName = "MODERATE" }
            ); 
        }

        public DbSet<ProductModel> Products { get; set; }

    }
}
