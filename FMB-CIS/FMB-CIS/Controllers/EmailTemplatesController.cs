using FMB_CIS.Data;
using FMB_CIS.Models;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FMB_CIS.Controllers
{
    public class EmailTemplatesController : Controller
    {
        private readonly IConfiguration _configuration;
        private readonly LocalContext _context;
        private IEmailSender EmailSender { get; set; }

        public EmailTemplatesController(IConfiguration configuration, LocalContext context, IEmailSender emailSender)
        {
            this._configuration = configuration;
            _context = context;
            EmailSender = emailSender;

        }
        public IActionResult Index() //List of email templates
        {
            if (((ClaimsIdentity)User.Identity).FindFirst("userRole").Value.Contains("Chainsaw") == true)
            {
                return RedirectToAction("EditAccount", "AccountManagement");
            }
            else
            {
                string host = $"{Request.Scheme}://{Request.Host}{Request.PathBase}/";
                ViewData["BaseUrl"] = host;

                ViewModel model = new ViewModel();
                //var userinfoList                    

                var tblEmailTempList = _context.tbl_email_template.ToList();
                model.tbl_Email_Templates_List = tblEmailTempList;

                
                return View(model);
            }
            //return View();
        }

        [HttpPost]
        public IActionResult Index(ViewModel model)
        {
            return View(model);
        }

        [HttpPost]
        public IActionResult CreateEditTemplate(ViewModel model, string typeOfAction)
        {            
            //USED FOR BOTH CREATE AND EDIT
            int loggedUserID = Convert.ToInt32(((ClaimsIdentity)User.Identity).FindFirst("userID").Value);
            if (typeOfAction == "create")
            {
                var emailTemplate = new tbl_email_template();
                //emailTemplate.id
                emailTemplate.template_name = model.tbl_Email_Template.template_name;
                emailTemplate.template_description = model.tbl_Email_Template.template_description;
                emailTemplate.email_subject = model.tbl_Email_Template.email_subject;
                emailTemplate.email_content = model.tbl_Email_Template.email_content;
                emailTemplate.is_active = true;
                emailTemplate.created_by = loggedUserID;
                emailTemplate.modified_by = loggedUserID;
                emailTemplate.date_created = DateTime.Now;
                emailTemplate.date_modified = DateTime.Now;
                _context.Add(emailTemplate);
                _context.SaveChanges();
            }
            else if (typeOfAction == "edit")
            {
                var emailTemplateDB = _context.tbl_email_template.Where(e => e.id == model.tbl_Email_Template.id).FirstOrDefault();
                //emailTemplate.id
                emailTemplateDB.template_name = model.tbl_Email_Template.template_name;
                emailTemplateDB.template_description = model.tbl_Email_Template.template_description;
                emailTemplateDB.email_subject = model.tbl_Email_Template.email_subject;
                emailTemplateDB.email_content = model.tbl_Email_Template.email_content;
                //emailTemplateDB.is_active = true;
                //emailTemplateDB.created_by = loggedUserID;
                emailTemplateDB.modified_by = loggedUserID;
                //emailTemplateDB.date_created = DateTime.Now;
                emailTemplateDB.date_modified = DateTime.Now;
                _context.Update(emailTemplateDB);
                _context.SaveChanges();
            }
            return RedirectToAction("Index", "EmailTemplates");
        }

        [HttpPost, ActionName("GetEmailTemplateDetails")]
        public JsonResult GetEmailTemplateDetails(int id)
        {
            var emailTemplateDetais = _context.tbl_email_template.Where(e => e.id == id).FirstOrDefault();
            return Json(emailTemplateDetais);

        }

        [HttpPost]
        public IActionResult SendEmailTemplate(ViewModel model, string sendToEmailAddress)
        {
            //Email
            var subject = model.tbl_Email_Template.email_subject;
            var body = model.tbl_Email_Template.email_content;
            EmailSender.SendEmailAsync(sendToEmailAddress, subject, body);
            return RedirectToAction("Index", "EmailTemplates");
        }

    }
}
