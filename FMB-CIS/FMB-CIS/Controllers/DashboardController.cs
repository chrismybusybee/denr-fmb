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
        public IActionResult Index()
        {
            ViewModel mymodel = new ViewModel();

            var ChainsawList = _context.tbl_chainsaw.ToList();
            int user_id = 2;
            var ChainsawOwnedList = ChainsawList.Where(m => m.user_id == user_id && m.status == "Seller").ToList();

            var applicationlist = from a in _context.tbl_application
                                  where a.tbl_user_id == user_id
                                  select a;


            var applicationtypelist = _context.tbl_application_type;

            var applicationMod = from a in applicationlist
                                 join appt in applicationtypelist on a.tbl_application_type_id equals appt.id
                                 join pT in _context.tbl_permit_type on a.tbl_application_type_id equals pT.id
                                 join pS in _context.tbl_permit_status on appt.id equals pS.application_type
                                 where a.tbl_user_id == user_id
                                 select new ApplicationModel{id = a.id, application_type = appt.name, permit_type = pT.name, permit_status = pS.status};
            mymodel.tbl_Chainsaws = ChainsawOwnedList;
            mymodel.applicationModels = applicationMod;

            return View(mymodel);
        }

        public IActionResult UserHistory()
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
