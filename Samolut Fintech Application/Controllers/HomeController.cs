using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Samolut_Fintech_Application.Data;
using Samolut_Fintech_Application.Models;
using System.Diagnostics;

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
}
