using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Samolut_Fintech_Application.Data;
using Samolut_Fintech_Application.Models;

namespace Samolut_Fintech_Application.Controllers
{
    public class AdminPagesController : Controller
    {
        private readonly ApplicationDbContext _context;
        public AdminPagesController(ApplicationDbContext context)
        {
            _context = context;
        }

        
        public IActionResult ViewCustomers()
        {
            return View();
        }

    }
}
