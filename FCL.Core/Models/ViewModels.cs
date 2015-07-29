using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FCL.Core.DataAccess;
using Newtonsoft.Json;


namespace FCL.Core.Models
{
    public class ViewModels
    {
        public int UserId { get; set; }
        public int DseId { get; set; }
        public int CompanyId { get; set; }
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName{ get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        public string ClientId { get; set; }
        public int? AdminId { get; set; }
    }

    public class AdminModel
    {
        public int UserId { get; set; }
        [Required]
        [Display(Name = "User name")]
        public string UserName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "The {0} must be at least {2} characters long.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string ConfirmPassword { get; set; }

        [Required]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Display(Name = "Email")]
        public string Email { get; set; }

        [Required]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }
        public bool CreatePriviledge { get; set; }
        public bool EditPriviledge { get; set; }
        public int BranchId { get; set; }
        public string Role { get; set; }
    }

    public class EditAdminModel
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public bool CreatePriviledge { get; set; }
        public bool EditPriviledge { get; set; }
        public int BranchId { get; set; }
        public string Role { get; set; }
    }

    public class LoanTransactions
    {
        public LoanTransactions()
        {
            Cheques = Cheques ?? new List<ChequeViewModel>();
        }
        
        public int LoanId { get; set; }
        public int ClientId { get; set; }
        [Required]
        [Display(Name = "Client Name")]
        public string ClientName { get; set; }
        public double? TotalAmount { get; set; }
        [Required]
        [Display(Name = "Duration")]
        public string Duration { get; set; }
        public int DurationId { get; set; }
        public List<ChequeViewModel> Cheques { get; set; }

        public Guarantor Guarantee { get; set; }
        public int? LoanIdForCore { get; set; }

    }

    public class ChequeViewModel
    {
        public string PaymentType { get; set; }
        public int ChequeId { get; set; }
        public string ChequeNumber { get; set; }
        public string BankName { get; set; }
        public string Status { get; set; }
        public DateTime? DateDue { get; set; }
        public bool IsDelete { get; set; }
        public bool PaidCash { get; set; }
        public int BankId { get; set; }
        public decimal? Amount { get; set; }
        public int StatusId { get; set; }
        public int PaymentTypeId { get; set; }
        public DateTime? ValueDate { get; set; }
        public int RecievingBank { get; set; }

    }

    public class ReportView
    {
        public int? LoanIdFromCore { get; set; }
        public int? Dse { get; set; }
        public int? Company { get; set; }
        public string Phone { get; set; }
        public string ClientIdFromCore { get; set; }
        public int? AdminId { get; set; }
        public int? BranchId { get; set; }
        public string ClientName { get; set; }
        public string PaymentType { get; set; }
        public int? ChequeId { get; set; }
        public string ChequeNumber { get; set; }
        public string BankName { get; set; }
        public string Status { get; set; }
        public DateTime? DateDue { get; set; }
        public int? BankId { get; set; }
        public double? Amount { get; set; }
        public double? LoanAmount { get; set; }
        public int? StatusId { get; set; }
        public int? PaymentTypeId { get; set; }
        public DateTime? ValueDate { get; set; }
        public int? RecievingBank { get; set; }
        public DateTime? DateCreated { get; set; }
    }

    public class PaymentDueToday
    {
        public int NumberOfPaymentsDueToday { get; set; }
        public decimal ValueOfChequesDueToday { get; set; }
    }

    public class BankValue
    {
        public string BankName { get; set; }
        public decimal Value { get; set; }
        public decimal ValueMonth { get; set; }
        public int Count { get; set; }
    }

    public class MonthlyStanding
    {
        public int Count { get; set; }
        public decimal Value { get; set; }
    }

    public class Total
    {
        public int CountTotal { get; set; }
        public decimal GrandTotal { get; set; }
    }

    public class DashboardModels
    {
        public DashboardModels()
        {
            Bank = Bank ?? new List<BankValue>();
            TypeChequeToday = TypeChequeToday ?? new PaymentDueToday();
            TypeStandingToday = TypeStandingToday ?? new PaymentDueToday();
            TypeDirectToday = TypeDirectToday ?? new PaymentDueToday();
            TypeChequeMonth = TypeChequeMonth ?? new PaymentDueToday();
            TypeStandingMonth = TypeStandingMonth ?? new PaymentDueToday();
            TypeDirectMonth = TypeDirectMonth ?? new PaymentDueToday();
            Expected = Expected ?? new MonthlyStanding();
            Cleared = Cleared ?? new MonthlyStanding();
            Pending = Pending ?? new MonthlyStanding();
            PaidCash = PaidCash ?? new MonthlyStanding();
            Presented = Presented ?? new MonthlyStanding();
            Returned = Returned ?? new MonthlyStanding();
            TodayTypeTotal = TodayTypeTotal ?? new Total();
            MonthTypeTotal = MonthTypeTotal ?? new Total();
            StatusTotal = StatusTotal ?? new Total();
            BankTotal = BankTotal ?? new Total();
        }
        public PaymentDueToday TypeChequeToday { get; set; }
        public PaymentDueToday TypeStandingToday { get; set; }
        public PaymentDueToday TypeDirectToday { get; set; }
        public Total TodayTypeTotal { get; set; }

        public PaymentDueToday TypeChequeMonth { get; set; }
        public PaymentDueToday TypeStandingMonth { get; set; }
        public PaymentDueToday TypeDirectMonth { get; set; }
        public Total MonthTypeTotal { get; set; }

        public List<BankValue> Bank { get; set; }
        public Total BankTotal { get; set; }
        public MonthlyStanding Expected { get; set; }
        public MonthlyStanding Cleared { get; set; }
        public MonthlyStanding Pending { get; set; }
        public MonthlyStanding PaidCash { get; set; }
        public MonthlyStanding Presented { get; set; }
        public MonthlyStanding Returned { get; set; }
        public Total StatusTotal { get; set; }
    }

    public class TypeValue
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int Count { get; set; }
        public decimal ValueDay { get; set; }
        public decimal ValueMonth { get; set; }
    }

    public class SearchValues
    {
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set; }
        public int? BankId { get; set; }
        public int? PaymentType { get; set; }
        public int? StatusType { get; set; }
        public int? BranchId { get; set; }

    }

}
