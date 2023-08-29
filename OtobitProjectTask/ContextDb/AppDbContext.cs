
using Microsoft.EntityFrameworkCore;
using Otobit.Models.Domain;
using OtobitProjectTask.Models;

namespace OtobitProjectTask.ContextDb
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions options) : base(options)
        {
        }

        public DbSet<Customers> Cutomers { get; set; }
       public DbSet<Books> Books { get; set; }
       public DbSet<Sellers> Sellers { get; set; }
       public DbSet<CustomerRequest> PurchesedBooks { get; set; }
       public DbSet<SellerResponce> OfferAccepted { get; set; }

    }
}
