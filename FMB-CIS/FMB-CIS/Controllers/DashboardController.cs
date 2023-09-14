using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using FMB_CIS.Data;
using FMB_CIS.Models;


namespace FMB_CIS.Controllers
{
   
    public class DashboardController : Controller
    {
        private readonly LocalContext _context;
        public DashboardController(LocalContext context)
        {
            _context = context;
        }

        // GET: Movies
        public async Task<IActionResult> Index()
        {
            var ChainsawList = _context.tbl_chainsaw.ToList();
            int user_id = 1;
            var ChainsawOwnedList = ChainsawList.Where(m => m.user_id == user_id).ToList();

            return _context.tbl_chainsaw != null ?
                        View(ChainsawOwnedList) :
                        Problem("Entity set 'LocalContext.tbl_chainsaw'  is null.");
        }


    }
}
