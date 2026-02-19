using Microsoft.EntityFrameworkCore;
using Microsoft.Identity.Client;
using Samolut_Fintech_Application.Models.DatabaseModels;
namespace Samolut_Fintech_Application.Data
{

//this will connect the c sharp to the db tables. 
    public class ApplicationDbContext: DbContext
    {
        //takes settings like the db name and passes it to the system
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {

        }
        
        //all my database tables i kept in my models folder, this is to connect to c sharp 
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<AccountType> AccountType { get; set; }
        public DbSet<CurrentCurrency> CurrentCurrency { get; set; }
        public DbSet<Transaction> Transaction { get; set; }
        public DbSet<Staff> Staff { get; set; }
        public DbSet<Role> Role { get; set; } 



    }
}
