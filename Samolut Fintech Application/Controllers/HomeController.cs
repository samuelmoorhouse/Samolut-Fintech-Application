using AspNetCoreGeneratedDocument;
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using MySqlConnector;
using Samolut_Fintech_Application.Data;
using Samolut_Fintech_Application.Models;
using Samolut_Fintech_Application.Models.DatabaseModels;
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

       
        public IActionResult Add()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }


            return View();
        }

        
        //all payments page stuff -------------------------------------------------
        public async Task<IActionResult> Payments()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        
        public async Task<IActionResult> transferInternalCurrency()
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            var countryCurrencies = await _context.CurrentCurrency.ToListAsync();

            var accounts = await _context.Account
                .Include(i=>i.CurrencyIdForeignKey) //added a  foreign key in my db, so i can read off trhe currency names as i made it to be 3nf so its in seperate table
                .Where(i => i.CUSTOMER_ID == userId || i.ACCOUNT_TYPE_ID == 1).ToListAsync();


            return View(accounts);
        }

        

        public async Task<IActionResult> ApplicationHome(int ACCOUNT_ID)
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            var countryCurrencies = await _context.CurrentCurrency.ToListAsync();

            var accounts = await _context.Account
                .Include(i => i.CurrencyIdForeignKey) 
                .Where(i => i.CUSTOMER_ID == userId || i.ACCOUNT_TYPE_ID == 1).ToListAsync();


            //new code for the selected asccount passed in Data, above stuff is just my code for the dropdown again

            var SelectedID = ACCOUNT_ID;
            var selectedAccount = await _context.Account.FirstOrDefaultAsync(i => i.ACCOUNT_ID == SelectedID);
            ViewBag.SelectedTransactions = await _context.Transaction
                .Include(i => i.SenderAccountIdForeignKey) //include the foreign keys so i know the details of each accoujnt
                .Include(i => i.ReceiverAccountIdForeignKey)
                .Where(i => i.SENDER_ACCOUNT_ID == SelectedID || i.RECEIVER_ACCOUNT_ID == SelectedID).ToListAsync();

            ViewBag.selectedAccountBalance = selectedAccount?.ACCOUNT_BALANCE;
            ViewBag.accountName =  selectedAccount?.CurrencyIdForeignKey.COUNTRY_CURRENCY_NAME;



            return View(accounts);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> externalTransfer()
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }
            int? userId = HttpContext.Session.GetInt32("UserId");






            return View();
        }
        // -------------------------------------------------------------------------------

    }
}
