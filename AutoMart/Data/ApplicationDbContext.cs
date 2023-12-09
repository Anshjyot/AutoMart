using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using AutoMart.Models;

namespace AutoMart.Data
{
    public class ApplicationDbContext : IdentityDbContext<WebUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<WebUser> WebUsers { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Category> VehicleCategory { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<Cart> Carts { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            
            modelBuilder.Entity<Cart>().HasKey(ab => new
            {
                ab.Id,
                ab.VehicleId,
                ab.UserId
            }) ;
           
            modelBuilder.Entity<Cart>()
            .HasOne(ab => ab.Vehicle)
            .WithMany(ab => ab.Carts)
            .HasForeignKey(ab => ab.VehicleId);
            modelBuilder.Entity<Cart>()
            .HasOne(ab => ab.User)
            .WithMany(ab => ab.Carts)
            .HasForeignKey(ab => ab.UserId);
        }
    }

}