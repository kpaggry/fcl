using System;
using System.Collections.Generic;
using System.EnterpriseServices;
using System.Globalization;
using System.Linq;
using System.Web.Mvc;
using System.Web.Security;
using FCL.Core;
using FCL.Core.Models;
using Newtonsoft.Json;
using PagedList;
using WebMatrix.WebData;

namespace FCL.Web.Controllers
{
    public class ChequeController : Controller
    {
        //
        // GET: /Cheque/
        private FclManager _manager = new FclManager();

        public ActionResult Index()
        {
            var x = _manager.GetAllLoans();
            return View(x);
        }

        public ActionResult Dashboard()
        {
            var x = _manager.GenerateDashboard();
            TempData["BankJson"] = JsonConvert.SerializeObject(x.Bank);
            TempData["PaymentTypes"] = JsonConvert.SerializeObject(_manager.GetPaymentTypeValues());
            TempData["Status"] = JsonConvert.SerializeObject(_manager.GetLoanStatusValues());
            //TempData["BankJson"] = Json(x.Bank);
            return View(x);
        }

        public void GenerateCreateVariables()
        {
            var banks = _manager.GetBanks().OrderBy(x => x.BankName).ToList();

            ViewData["Banks"] = banks.Select(b => new SelectListItem()
            {
                Text = b.BankName,
                Value = b.BankId.ToString(),
                Selected = false
            });

            var name = _manager.ShowAllClients().Where(x => Roles.IsUserInRole(x.UserName, "Clients")).OrderBy(x => x.FirstName).ToList();

            ViewData["Clients"] = name.Select(c => new SelectListItem()
            {
                Text = c.FirstName + " " + c.LastName,
                Value = c.UserId.ToString(),
                Selected = false
            });

            var frame = _manager.GetAllFrames().OrderBy(x=>x.Duration1).ToList();

            ViewData["Duration"] = frame.Select(c => new SelectListItem()
            {
                Text = c.Duration1,
                Value = c.DurationId.ToString(),
                Selected = false
            });

            var stat = _manager.GetAllStatus().OrderBy(x=>x.Status).ToList();

            ViewData["Status"] = stat.Select(c => new SelectListItem()
            {
                Text = c.Status,
                Value = c.StatusId.ToString(),
                Selected = false
            });

            var paymentType = _manager.GetPaymentTypes().OrderBy(x=>x.PaymentType).ToList();

            ViewData["PaymentType"] = paymentType.Select(p => new SelectListItem
            {
                Text = p.PaymentType,
                Value = p.PaymentTypeId.ToString(),
                Selected = false
            });

            ViewData["Branches"] = _manager.GetBranches().OrderBy(x => x.Branch).Select(x => new SelectListItem
            {
                Text = x.Branch,
                Value = x.Id.ToString(),
                Selected = false
            }).ToList();
        }

        public ActionResult ViewLoan(int id)
        {
            var x = _manager.GetLoanDetails(id);
            return View(x);
        }

        public ActionResult EditLoan(int id)
        {
            GenerateCreateVariables();
            TempData["canEdit"] = _manager.CanEdit(WebSecurity.GetUserId(User.Identity.Name));
            var x = _manager.GetLoanDetails(id);
            return View(x);
        }

        [HttpPost]
        public JsonResult EditChequeInfo(LoanTransactions loan)
        {
            var x = _manager.EditLoan(loan);
            return Json(x);
        }

        [HttpPost]
        public JsonResult AddChequeInfo(LoanTransactions loan)
        {
            var id = WebSecurity.GetUserId(User.Identity.Name);
            var x = _manager.SaveLoan(loan, id);
            //var details = _manager.GetClientDetails(loan.ClientId);
            //var duration = _manager.DurationValue(loan.DurationId);
            //Helper.Helper.SendLoanCreatedMail(details, "LoanCreated", loan.Cheques[0].Amount, loan.Cheques[0].DateDue, duration);
            return Json(x);
        }

        public PartialViewResult NewChequeRow()
        {
            GenerateCreateVariables();
            return PartialView("../Partials/_Cheque", new ChequeViewModel());
        }

        public ActionResult Create()
        {
            GenerateCreateVariables();
            var x = new LoanTransactions();
            x.Cheques.Add(new ChequeViewModel());
            return View(x);
        }

        public ActionResult CheckNames(string term)
        {
            // Get Tags from database
            string[] tags = { "ASP.NET", "WebForms", 
                    "MVC", "jQuery", "ActionResult", 
                    "MangoDB", "Java", "Windows" };
            return Json(tags.Where(t => t.StartsWith(term)),
                            JsonRequestBehavior.AllowGet);
        }

        public ActionResult GenerateReport()
        {
            GenerateCreateVariables();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult GenerateReport(DateTime? startDate, DateTime? endDate, int? bank, int? paymentType, int? status, int? branch)
        {
            var x = _manager.GetReportObject();
            x = _manager.GetReportQuery(x, startDate, endDate, bank, paymentType, status, branch);

            ViewBag.SearchObj = Helper.Helper.BuildSearchModel(startDate, endDate, bank, paymentType);
            TempData["PostBack"] = "Yes";

            GenerateCreateVariables();

            return View(x);
        }

        public ActionResult ExportAsExl(DateTime? startDate, DateTime? endDate, int? bank, int? paymentType, int? status, int? branch)
        {
            var x = _manager.GetReportObject();
            x = _manager.GetReportQuery(x, startDate, endDate, bank, paymentType, status, branch);

            HttpContext.Response.AddHeader("content-disposition", "attachment; filename=FCLReport_" + DateTime.Now.ToString(CultureInfo.CurrentCulture) + ".xls");

            Response.ContentType = "application/vnd.ms-excel";

            return View(x);
        }
    }
}
