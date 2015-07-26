using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using FCL.Core;
using FCL.Core.DataAccess;
using FCL.Core.Models;
using WebMatrix.WebData;
using FCL.Web.Helper;

namespace FCL.Web.Controllers
{
    public class ClientController : Controller
    {
        //
        // GET: /Client/
        private FclManager _manager = new FclManager();
        public ActionResult Index()
        {
            List<UserProfile> x;

            x = _manager.ShowAllClients();
            return View(x);
        }

        [NonAction]
        public void Generate(){
            ViewBag.CompanyList = _manager.GetCompanies().ToList().Select(x => new SelectListItem()
            {
                Text = x.CompanyName,
                Value = x.Id.ToString(),
                Selected = false
            });

            ViewBag.DseList = _manager.GetDSE().ToList().Select(x => new SelectListItem()
            {
                Text = x.Name,
                Value = x.Id.ToString(),
                Selected = false
            });
        }

        public ActionResult Create()
        {
            Generate();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult Create(ViewModels model)
        {
            Generate();
            if (!ModelState.IsValid) return View(model);
            // Attempt to register the user
            try
            {
                if (_manager.ConfirmEmail(model.Email) != 0 || WebSecurity.UserExists(model.UserName))
                {
                    TempData["status"] = "Error";
                    return View(model);
                }


                WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                model.UserId = WebSecurity.GetUserId(model.UserName);
                model.AdminId = WebSecurity.GetUserId(User.Identity.Name);
                _manager.CreateClient(model, "create");
                Roles.AddUserToRole(model.UserName, "Clients");
                Helper.Helper.SendMail(model, "ClientCreated");
                TempData["status"] = "Success";
                return View(model);
            }
            catch (MembershipCreateUserException e)
            {
                //ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult EditClient(int id)
        {
            var client = _manager.GetClientDetails(id);
            Generate();
            return View(client);
        }

        [HttpPost]
        public ActionResult EditClient(UserProfile model)
        {
            Generate();
            if (_manager.ConfirmEmailForEdit(model.UserId, model.Email) != 0)
            {
                TempData["status"] = "Error";
                return View(model);
            }

            var client = _manager.UpdateClient(model);
            TempData["status"] = (client > 0) ? "Success" : "Error";
            return View(model);
        }
    }
}
