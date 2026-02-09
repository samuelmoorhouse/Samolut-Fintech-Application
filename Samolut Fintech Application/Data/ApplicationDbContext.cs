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
        
        //all my database tables
        public DbSet<Customer> Customer { get; set; }
        public DbSet<Account> Account { get; set; }
        public DbSet<AccountType> AccountType { get; set; }
        

    }
}
