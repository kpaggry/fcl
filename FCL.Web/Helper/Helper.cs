using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web.WebPages.Html;
using FCL.Core;
using FCL.Core.DataAccess;
using FCL.Core.Models;
using Postal;

namespace FCL.Web.Helper
{
    public static class Helper
    {
        public static FclManager _manager = new FclManager();
        public static void SendMail(ViewModels model, string mailType)
        {
            dynamic email = new Email(mailType);
            email.To = model.Email;
            email.UserName = model.UserName;
            email.Sender = ConfigurationManager.AppSettings["SmtpEmail"];
            email.Send();
        }

        public static void SendAdminMail(AdminModel model, string mailType)
        {
            dynamic email = new Email(mailType);
            email.To = model.Email;
            email.UserName = model.UserName;
            email.Password = model.Password;
            email.Name = model.FirstName + model.LastName;
            email.Sender = ConfigurationManager.AppSettings["SmtpEmail"];
            email.Send();
        }

        public static void SendLoanCreatedMail(UserProfile model, string mailType, decimal? amount, DateTime? due, string duration)
        {
            dynamic email = new Email(mailType);
            email.To = model.Email;
            email.Name = model.FirstName;
            email.ChequeAmount = amount;
            email.Due = due;
            email.Duration = duration;
            email.Sender = ConfigurationManager.AppSettings["SmtpEmail"];
            email.Send();
        }

        public static string GetFullName(int? id)
        {
            return id == 0 ? null : _manager.GetFullNameFromDb(id);
        }

        public static string GetDurationValue(int? id)
        {
            return id == 0 ? null : _manager.DurationValue(id);
        }

        public static string GetStatusValue(int? id)
        {
            return id == 0 ? null : _manager.StatusValue(id);
        }

        public static string GetBankValue(int? id)
        {
            return id == 0 ? null : _manager.BankValue(id);
        }

        public static string GetBranchName(int? id)
        {
            return id == 0 ? null : _manager.GetBranchName(id);
        }
        public static string GetDSEName(int? id)
        {
            return id == 0 ? null : _manager.GetDSEName(id);
        }
        public static string GetCompanyName(int? id)
        {
            return id == 0 ? null : _manager.GetCompanyName(id);
        }

        public static string GetPaymentTypeValue(int? id)
        {
            return id == 0 ? null : _manager.PaymentTypeValue(id);
        }

        public static SearchValues BuildSearchModel(DateTime? startDate, DateTime? endDate, int? bank, int? paymentType)
        {
            return new SearchValues
            {
                StartDate = startDate,
                EndDate = endDate,
                BankId = bank,
                PaymentType = paymentType
            };
        }
    }

    public static class SelectListExtensions
    {
        public static List<SelectListItem> ToSelectListItems<T>(this IEnumerable<T> items, Func<T, object> valueField, Func<T, object> textField)
        {
            return items.ToSelectListItems(valueField, textField, null).ToList();
        }


        public static IEnumerable<SelectListItem> ToSelectListItems<T>(this IEnumerable<T> items, Func<T, object> valueField, Func<T, object> textField, Func<T, bool> isSelected)
        {
            List<SelectListItem> listItems = new List<SelectListItem>();
            foreach (T item in items)
            {
                string value = valueField != null ? valueField(item).ToString() : item.ToString();
                string text = textField != null ? textField(item).ToString() : item.ToString();
                bool selected = isSelected != null && isSelected(item);
                SelectListItem listItem = new SelectListItem() { Value = value, Text = text, Selected = selected };
                listItems.Add(listItem);
            }
            return listItems;
        }
    }
}