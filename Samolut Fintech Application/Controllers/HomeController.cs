
using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration.UserSecrets;
using MySqlConnector;
using Samolut_Fintech_Application.Data;
using Samolut_Fintech_Application.Models;
using Samolut_Fintech_Application.Models.DatabaseModels;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Pomelo.EntityFrameworkCore.MySql.Query.ExpressionTranslators.Internal;
using Samolut_Fintech_Application.Models.Transfers;

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

        public async Task<IActionResult> ApplicationHome(int accountId)
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

            var customerName = await _context.Customer
                .Where(i => i.CUSTOMER_ID == userId)
                .Select(i => i.FIRST_NAME).FirstOrDefaultAsync();

            //new code for the selected asccount passed in Data, above stuff is just my code for the dropdown again

            var selectedId = accountId;
            var selectedAccount = await _context.Account.FirstOrDefaultAsync(i => i.ACCOUNT_ID == selectedId);
            ViewBag.SelectedTransactions = await _context.Transaction
                .Include(i =>
                    i.SenderAccountIdForeignKey) //include the foreign keys so i know the details of each accoujnt
                .Include(i => i.ReceiverAccountIdForeignKey)
                .Where(i => i.SENDER_ACCOUNT_ID == selectedId || i.RECEIVER_ACCOUNT_ID == selectedId).ToListAsync();

            ViewBag.selectedAccountBalance = selectedAccount?.ACCOUNT_BALANCE;
            ViewBag.accountName = selectedAccount?.CurrencyIdForeignKey.COUNTRY_CURRENCY_NAME;
            ViewBag.CustomerName = customerName;


            return View(accounts);
        }

        public async Task<IActionResult> TransferInternalCurrency()
        {

            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");
            var countryCurrencies = await _context.CurrentCurrency.ToListAsync();


            var accounts = await _context.Account
                .Include(i =>
                    i.CurrencyIdForeignKey) //added a  foreign key in my db, so i can read off trhe currency names as i made it to be 3nf so its in seperate table
                .Where(i => i.CUSTOMER_ID == userId || i.ACCOUNT_TYPE_ID == 1).ToListAsync();

            ViewBag.accounts = accounts;
            //post hasnt happened yet so make it false when page first loads
            
            return View();
        }



        

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CalculateInternalCurrency(InternalCurrencyModel data)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
            }

            int? userId = HttpContext.Session.GetInt32("UserId");

            if (ModelState.IsValid)
            {
                //so im gonna check if the accounts were the same first
                if (data.SENDER_ACCOUNT_ID == data.RECEIVER_ACCOUNT_ID)
                {
                    ViewBag.accounts = await _context.Account
                        .Include(i => i.CurrencyIdForeignKey)
                        .Where(i => i.CUSTOMER_ID == userId || i.ACCOUNT_TYPE_ID == 1).ToListAsync();
                    ViewBag.ErrorMessage = "Cannot send money to the same account! Choose a different account to receive funds.";
                    return View("TransferInternalCurrency", data);
                }
                
                
                
                //calculate exchange rate, so get sender and receiver and calculate it against the uks one
                //ill store all the rates to gbp
                // formula is sender gbp rate / receiver gbp rate
                var SenderCurrencyID = await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.CUSTOMER_ID == data.SENDER_ACCOUNT_ID)
                    .Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefaultAsync();
                
                var ReciverCurrencyID= await _context.Account
                    .Include(i => i.CurrencyIdForeignKey)
                    .Where(i => i.CUSTOMER_ID == data.RECEIVER_ACCOUNT_ID)
                    .Select(i => i.COUNTRY_CURRENCY_ID).FirstOrDefaultAsync();

                double SenderGBPRate = 0;
                double ReceiverGBPRate = 0;
                //though id use switch as its less code for this
                switch(SenderCurrencyID)
                {
                    case 1: //gbp so no convert needed
                        SenderGBPRate = 1;
                        break;
                    case 2: //eur to gbp
                        SenderGBPRate = 0.8624;
                        break;
                    case 3:
                        SenderGBPRate = 0.0046;
                        break;
                }

                switch (ReciverCurrencyID)
                {
                    case 1: //gbp so no convert needed
                        ReceiverGBPRate = 1;
                        break;
                    case 2: //eur to gbp
                        ReceiverGBPRate = 0.8624;
                        break;
                    case 3:
                        ReceiverGBPRate = 0.0046;
                        break;
                }

                var ExchangeRate = SenderGBPRate / ReceiverGBPRate;
                
                double NewCurrencyAmount = data.AMOUNT * ExchangeRate;
                
                
                
                
                RedirectToAction("ConfirmInternalTransfer", new
                {
                    senderID = data.SENDER_ACCOUNT_ID,
                    receiverID = data.RECEIVER_ACCOUNT_ID,
                    beforeAmount = data.AMOUNT,
                    currencyAmount = NewCurrencyAmount, //new amount just made
                    exchangeRate = ExchangeRate,
                    startcurrencyID = SenderCurrencyID,
                    endcurrencyID = ReciverCurrencyID,
                    
                });
                
            }

            //if not need to relaoad page with eveythign it had before
            ViewBag.accounts = await _context.Account
                .Include(i => i.CurrencyIdForeignKey)
                .Where(i => i.CUSTOMER_ID == userId || i.ACCOUNT_TYPE_ID == 1).ToListAsync();
            //becuase my post is named differently to the file i have to tell it where to go back to
            ViewBag.ErrorMessage = "Model Invalid";
            return View("TransferInternalCurrency", data);

        }

        public IActionResult ConfirmInternalTransfer(double beforeAmount, int senderID, int receiverID, double exchangeRate, double currencyAmount, int startcurrencyID, int  endcurrencyID)
        {
            if (HttpContext.Session.GetInt32("UserId") == null)
            {
                return RedirectToAction("Login", "Account");
                
            }
            int? userId = HttpContext.Session.GetInt32("UserId");
            
            var data = new InternalCurrencyModel
            {
                SENDER_ACCOUNT_ID = senderID,
                RECEIVER_ACCOUNT_ID = receiverID,
                AMOUNT = currencyAmount,
                EXCHANGE_RATE = exchangeRate,
                START_CURRENCY = startcurrencyID,
                END_CURRENCY = endcurrencyID
                
            };

            ViewBag.OrginalAmount = beforeAmount;
            
            
            return View(data);
        }
    }
}    
    