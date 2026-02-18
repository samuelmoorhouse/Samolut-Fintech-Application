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


        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplicationHomeAsync(int? selectedAccountId) //put selected account in here if i change it from a dropdown this knows and wuill display right transactions
         //have to user questionmark as it would error if empty on selectedaccounts
        {

            //checks session on every application file
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            
            int? userId = HttpContext.Session.GetInt32("UserId");
            int? accountId = HttpContext.Session.GetInt32("UserId");


            //list of accounts just for the customer
            var accounts = await _context.Account
                .Where(i => i.CUSTOMER_ID == userId)
                .ToListAsync();



            if (selectedAccountId == null && accounts.Any())
            {
                selectedAccountId = accounts.First().ACCOUNT_ID;
            }


            //list of the selected accounts transactions stuff
            var transactions = await _context.Transaction
                 .Where(i => i.SENDER_ACCOUNT_ID == selectedAccountId || i.RECEIVER_ACCOUNT_ID == selectedAccountId)
                 .ToListAsync();

            ViewBag.Transactions = transactions;
            ViewBag.Accounts = accounts;

            //i call the accounts below using Model and the transactioins using viewbag, so the main stuff goes model and others in viewbag. 
            return View(selectedAccountId);
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
