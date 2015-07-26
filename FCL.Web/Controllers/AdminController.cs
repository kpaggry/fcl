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

namespace FCL.Web.Controllers
{
    public class AdminController : Controller
    {
        //
        // GET: /Admin/
        private FclManager _manager = new FclManager();

        [NonAction]
        public void DropDowns()
        {
            ViewBag.RoleList = _manager.GetRoles().ToList().Select(x => new SelectListItem()
            {
                Text = x.RoleName,
                Value = x.RoleName,
                Selected = false
            });

            ViewBag.BranchList = _manager.GetBranches().ToList().Select(x => new SelectListItem()
            {
                Text = x.Branch,
                Value = x.Id.ToString(),
                Selected = false
            });
        }

        public ActionResult Dashboard()
        {
            var x = _manager.ShowAllClients();
            return View(x);
        }

        public ActionResult CreateAdmin()
        {
            DropDowns();
            return View();
        }

        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult CreateAdmin(AdminModel model)
        {
            if (!ModelState.IsValid) return View(model);
            DropDowns();
            // Attempt to register the user
            try
            {
                if (_manager.ConfirmEmail(model.Email) != 0 || WebSecurity.UserExists(model.UserName) != false)
                {
                    TempData["status"] = "Error";
                    return View(model);
                }

                WebSecurity.CreateUserAndAccount(model.UserName, model.Password);
                model.UserId = WebSecurity.GetUserId(model.UserName);
                _manager.CreateAdmin(model, "create");
                Roles.AddUserToRole(model.UserName, model.Role);
                Helper.Helper.SendAdminMail(model, "AdminCreated");
                TempData["status"] = "Success";
                return View(model);
            }
            catch (MembershipCreateUserException)
            {
                //ModelState.AddModelError("", ErrorCodeToString(e.StatusCode));
            }

            // If we got this far, something failed, redisplay form
            return View(model);
        }

        public ActionResult EditAdmin(int id)
        {
            var x = _manager.GetAdminDetails(id);
            DropDowns();
            x.Role = GetCurrentRole(x.UserName);
            return View(x);
        }

        [HttpPost]
        public ActionResult EditAdmin(EditAdminModel model)
        {
            DropDowns();
            if (_manager.ConfirmEmailForEdit(model.UserId, model.Email) != 0)
            {
                TempData["status"] = "Error";
                return View(model);
            }

            var client = _manager.UpdateAdmin(model);
            var currentRole = GetCurrentRole(model.UserName);

            if (currentRole != model.Role) SetNewRole(currentRole, model.Role, model.UserName);

            TempData["status"] = (client > 0) ? "Success" : "Error";
            return View(model);
        }

        public ActionResult ListBranch()
        {
            var x = _manager.GetBranches();
            return View(x);
        }

        public ActionResult EditBranch(int id)
        {
            var x = _manager.ViewBranchById(id);
            return View(x);
        }

        [HttpPost]
        public ActionResult EditBranch(FCLBranch model)
        {
            if (_manager.BranchExists(model.Branch))
            {
                TempData["status"] = "Exists";
                return View();
            }

            _manager.UpdateBranch(model);
            TempData["status"] = "Success";
            return View(model);
        }

        public ActionResult CreateBranch()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateBranch(FCLBranch model)
        {
            if (_manager.BranchExists(model.Branch))
            {
                TempData["status"] = "Exists";
                return View();
            }

            _manager.CreateBranch(model);
            TempData["status"] = "Success";
            return View();
        }

        public ActionResult ListDse()
        {
            var x = _manager.GetDSE();
            return View(x);
        }

        public ActionResult EditDse(int id)
        {
            var x = _manager.ViewDseById(id);
            return View(x);
        }

        [HttpPost]
        public ActionResult EditDse(DSE model)
        {
            if (_manager.DseExists(model.Name))
            {
                TempData["status"] = "Exists";
                return View();
            }

            _manager.UpdateDse(model);
            TempData["status"] = "Success";
            return View(model);
        }

        public ActionResult CreateDSE()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateDSE(DSE model)
        {
            if (_manager.DseExists(model.Name))
            {
                TempData["status"] = "Exists";
                return View();
            }

            _manager.CreateDse(model);
            TempData["status"] = "Success";
            return View();
        }

        public ActionResult ListCompanies()
        {
            var x = _manager.GetCompanies();
            return View(x);
        }

        public ActionResult EditCompany(int id)
        {
            var x = _manager.ViewCompanyById(id);
            return View(x);
        }

        [HttpPost]
        public ActionResult EditCompany(Company model)
        {
            if (_manager.CompanyExists(model.CompanyName))
            {
                TempData["status"] = "Exists";
                return View();
            }
            
            _manager.UpdateCompany(model);
            TempData["status"] = "Success";
            return View(model);
        }

        public ActionResult CreateCompany()
        {
            return View();
        }
        [HttpPost]
        public ActionResult CreateCompany(Company model)
        {
            if (_manager.CompanyExists(model.CompanyName))
            {
                TempData["status"] = "Exists";
                return View();
            }

            _manager.CreateCompany(model);
            TempData["status"] = "Success";
            return View();
        }

        private void SetNewRole(string oldRole, string newRole, string username)
        {
            Roles.RemoveUserFromRole(username, oldRole);
            Roles.AddUserToRole(username, newRole);
        }

        private string GetCurrentRole(string username)
        {
            var role = Roles.GetRolesForUser(username);
            return role[0];
        }
    }
}
