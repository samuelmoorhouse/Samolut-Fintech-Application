using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using Samolut_Fintech_Application.Data;
using Samolut_Fintech_Application.Models;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Samolut_Fintech_Application.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }
        
        
        
        public IActionResult About()
        {
            return View();
        }

        //login stuff

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

    //so im making a new controller basically new folder for all my admin files. using the same code above but from my admin folder
    public class AdminController : Controller
    {
        //this needs to be at the top of every file
        private readonly ApplicationDbContext _context;

        //setting up the contellers stuff
        public AdminController(ApplicationDbContext context)
        {
            _context = context;
        }

        //to view the customers
        public async Task<IActionResult> ViewCustomers()
        {
            var customers = await _context.Customer.ToListAsync();
            return View(customers);
        }

    }

    public class Application : Controller
    {

        private readonly ApplicationDbContext _context;
        public Application(ApplicationDbContext context)
        {
            _context = context;
        }


        
        public async Task<IActionResult> ApplicationHome() 
        {
            //checks session on every application file
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            //so my user id from the session
            int? userId = HttpContext.Session.GetInt32("UserId");
            //get list of accounts for dropdown
            var accounts = await _context.Account
                .Where(i => i.CUSTOMER_ID == userId)
                .ToListAsync();

            //because of the way i made my db to be 3rd normal form ill have to look up the ids for each currency accont against the table. 
            var currencies = await _context.CurrentCurrency.ToListAsync();
            ViewBag.Currencies = currencies;

            return View(accounts);


            
            //int? accountId = HttpContext.Session.GetInt32("UserId");


            ////list of accounts just for the customer
            //var accounts = await _context.Account
            //    .Where(i => i.CUSTOMER_ID == userId)
            //    .ToListAsync();



            //if (selectedAccountId == null && accounts.Any())
            //{
            //    selectedAccountId = accounts.First().ACCOUNT_ID;
            //}
            //var selectedAccount = accounts.FirstOrDefault(a => a.ACCOUNT_ID == selectedAccountId); //get the selected account from its id


            ////list of the selected accounts transactions stuff
            //var transactions = await _context.Transaction
            //     .Where(i => i.SENDER_ACCOUNT_ID == selectedAccountId || i.RECEIVER_ACCOUNT_ID == selectedAccountId)
            //     .ToListAsync();

            //ViewBag.Transactions = transactions;
            //ViewBag.Accounts = accounts;

            ////i call the accounts below using Model and the transactioins using viewbag, so the main stuff goes model and others in viewbag. 
            //return View(selectedAccountId);
        }

        public async Task<IActionResult> Account()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            return View();

        }

        public IActionResult Payments()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }
        public IActionResult Add()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }


    }
}
