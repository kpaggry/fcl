using System;
using System.Collections.Generic;
using System.Linq;
using FCL.Core.DataAccess;
using FCL.Core.Models;

namespace FCL.Core
{
    public class FclManager
    {
        private FCLDataEntities _db;
        private int _loanId;
        private static DateTime _myDate = DateTime.Now;
        private static DateTime _startOfMonth = new DateTime(_myDate.Year, _myDate.Month, 1);
        private static readonly DateTime EndOfMonth = _startOfMonth.AddMonths(1).AddDays(-1);
        private static decimal _bankGrandTotal;
        private static decimal _todayPaymentTypeGrandTotal;
        private static decimal _monthPaymentTypeGrandTotal;
        private static decimal _statusGrandTotal;
        private static decimal _statusDayGrandTotal;
        private static int _bankGrandTotalCount;
        private static int _todayPaymentTypeGrandTotalCount;
        private static int _monthPaymentTypeGrandTotalCount;
        private static int _statusGrandTotalCount;
        public decimal BankGrandTotalMonth;

        public FclManager(int loanId)
        {
            _loanId = loanId;
            _db = new FCLDataEntities();
        }

        public List<UserProfile> ShowAllClients()
        {
            return _db.UserProfiles.ToList();
        }

        public List<UserProfile> ShowAllClientsByAdmin(int adminId)
        {
            return _db.UserProfiles.Where(user => user.AdminId == adminId).ToList();
        }

        public int CreateClient(ViewModels user, string type)
        {
            var x = _db.UserProfiles.Find(user.UserId);
            x.FirstName = user.FirstName;
            x.LastName = user.LastName;
            x.Email = user.Email;
            x.Phone = user.Phone;
            x.DateCreated = type == "create" ? DateTime.Now : x.DateCreated;
            x.DateModified = type == "edit" ? DateTime.Now : x.DateModified;
            x.ClientId = user.ClientId;
            x.DseId = user.DseId;
            x.CompanyId = user.CompanyId;
            x.AdminId = user.AdminId;

            _db.SaveChanges();
            return 0;
        }

        public void CreateAdmin(AdminModel user, string type)
        {
            var x = _db.UserProfiles.Find(user.UserId);
            x.FirstName = user.FirstName;
            x.LastName = user.LastName;
            x.Email = user.Email;
            x.Phone = user.Phone;
            x.DateCreated = type == "create" ? DateTime.Now : x.DateCreated;
            x.DateModified = type == "edit" ? DateTime.Now : x.DateModified;
            x.CanEdit = user.EditPriviledge;
            x.BranchId = user.BranchId;

            _db.SaveChanges();
        }

        public void DeleteClient(int id)
        {
            var x = _db.UserProfiles.Find(id);
            _db.UserProfiles.Remove(x);
            _db.SaveChanges();
        }

        public int ConfirmEmail(string mail)
        {
            var x = (_db.UserProfiles.Where(u => u.Email == mail)).Count();
            return x;
        }

        public object GetAllLoans()
        {
            var x = new List<LoanTransaction>();
            foreach (var loan in _db.LoanTransactions.Select(c => c))
                x.Add(loan);
            return x;
        }

        public string GetFullNameFromDb(int? id)
        {
            var x = (_db.UserProfiles.Where(u => u.UserId == id).Select(u => u.FirstName + " " + u.LastName)).SingleOrDefault();
            return x;
        }

        public List<Bank> GetBanks()
        {
            var x = new List<Bank>();
            foreach (var bank in _db.Banks.Select(b => b))
                x.Add(bank);
            return x;
        }

        public object EditLoan(LoanTransactions loan)
        {
            UpdateLoan(loan);

            if (IsNullOrEmpty(loan.Guarantee))
            {
                loan.Guarantee.GuarantorId = UpdateGurantor(loan.Guarantee);
            }

            foreach (var cheque in loan.Cheques)
            {
                if (cheque.IsDelete && cheque.ChequeId != 0)
                {
                    RemoveCheque(cheque.ChequeId);
                }

                if (!cheque.IsDelete && cheque.ChequeId != 0)
                {
                    EditCheque(cheque);
                }

                if (cheque.IsDelete || cheque.ChequeId != 0) continue;
                cheque.ChequeId = SaveCheque(cheque, loan.LoanId);
            }

            return loan;
        }

        private static bool IsNullOrEmpty(Guarantor guarantor)
        {
            if (guarantor.ChequeNumber != "" || guarantor.NameOnCheque != "" || guarantor.Name != "") return false;
            return true;
        }

        private void RemoveCheque(int p)
        {
            var x = _db.ChequeTransactions.Find(p);
            _db.ChequeTransactions.Remove(x);
            _db.SaveChanges();
        }

        private void EditCheque(ChequeViewModel c)
        {
            var x = _db.ChequeTransactions.Find(c.ChequeId);
            x.PaymentTypeId = c.PaymentTypeId;
            x.ChequeNumber = c.ChequeNumber;
            x.BankId = c.BankId;
            x.Amount = c.Amount;
            x.StatusId = c.StatusId;
            x.DateDue = c.DateDue;
            x.PaidCash = c.PaidCash;
            x.ValueDate = c.ValueDate;
            x.RecievingBank = c.RecievingBank;

            _db.SaveChanges();
        }

        private int UpdateGurantor(Guarantor p)
        {
            var x = _db.Guarantors.Find(p.GuarantorId);
            x.Name = p.Name;
            x.PhoneNumber = p.PhoneNumber;
            x.NameOnCheque = p.NameOnCheque;
            x.ChequeNumber = p.ChequeNumber;
            x.Bank = p.Bank;
            x.Status = p.Status;

            _db.SaveChanges();
            return p.GuarantorId;
        }

        private void UpdateLoan(LoanTransactions loan)
        {
            var x = _db.LoanTransactions.Find(loan.LoanId);
            x.Duration = loan.Duration;
            x.TotalAmount = loan.TotalAmount;
            x.LoanIdForCore = loan.LoanIdForCore;

            _db.SaveChanges();
        }

        public LoanTransactions SaveLoan(LoanTransactions loan, int adminId)
        {
            loan.LoanId = CraeteLoan(loan, adminId);

            //create a function to check if this is null
            if (loan.Guarantee != null)
            {
                loan.Guarantee.GuarantorId = CreateGurantor(loan.LoanId, loan.Guarantee);
            }

            foreach (var cheque in loan.Cheques.Where(cheque => !cheque.IsDelete))
            {
                cheque.ChequeId = SaveCheque(cheque, loan.LoanId);
            }

            return loan;
        }

        private int CreateGurantor(int p, Guarantor guarantor)
        {
            guarantor.LoanId = p;
            _db.Guarantors.Add(guarantor);
            _db.SaveChanges();

            return guarantor.GuarantorId;
        }

        private int CraeteLoan(LoanTransactions loan, int adminId)
        {
            var x = new LoanTransaction()
            {
                DateCreated = DateTime.Now,
                ClientId = loan.ClientId,
                DurationId = loan.DurationId,
                TotalAmount = loan.TotalAmount,
                AdminId = adminId,
                LoanIdForCore = loan.LoanIdForCore
            };

            _db.LoanTransactions.Add(x);
            _db.SaveChanges();
            return x.LoanId;
        }

        private int SaveCheque(ChequeViewModel cheque, int id)
        {
            var x = new ChequeTransaction()
            {
                ChequeNumber = cheque.ChequeNumber,
                LoanId = id,
                BankId = cheque.BankId,
                StatusId = cheque.StatusId,
                DateDue = cheque.DateDue,
                PaidCash = cheque.PaidCash,
                Amount = Convert.ToDecimal(cheque.Amount),
                PaymentTypeId = cheque.PaymentTypeId,
                DateCreated = DateTime.Now.Date
            };

            _db.ChequeTransactions.Add(x);
            _db.SaveChanges();

            return x.ChequeId;
        }

        public List<ChequeTransaction> GetAllPendingCheques()
        {
            var x = new List<ChequeTransaction>();
            foreach (var transaction in (_db.ChequeTransactions.Where(c => c.StatusId == 1)))
                x.Add(transaction);

            return x;
        }

        public UserProfile GetCustomerInfo(int? nullable)
        {
            var x = _db.LoanTransactions.Where(l => l.LoanId == nullable).Select(l => l.ClientId).SingleOrDefault();
            return _db.UserProfiles.SingleOrDefault(c => c.UserId == x);
        }

        public string GetAdminEmail(int? nullable)
        {
            var x = _db.LoanTransactions.Where(l => l.LoanId == nullable).Select(l => l.AdminId).SingleOrDefault();
            return _db.UserProfiles.Where(c => c.UserId == x).Select(c => c.Email).SingleOrDefault();
        }

        public UserProfile GetClientDetails(int id)
        {
            return _db.UserProfiles.SingleOrDefault(u => u.UserId == id);
        }

        public EditAdminModel GetAdminDetails(int id)
        {
            return _db.UserProfiles.Where(x => x.UserId == id).Select(x => new EditAdminModel
            {
                UserId = x.UserId,
                FirstName = x.FirstName,
                LastName = x.LastName,
                Email = x.Email,
                UserName = x.UserName,
                Phone = x.Phone,
                CreatePriviledge = x.CanCreate ?? false,
                EditPriviledge = x.CanEdit ?? false,
                BranchId = (int)x.BranchId
            }).FirstOrDefault();
        }

        public int UpdateClient(UserProfile model)
        {
            var x = _db.UserProfiles.Find(model.UserId);
            x.EmployerName = model.EmployerName;
            x.FirstName = model.FirstName;
            x.LastName = model.LastName;
            x.Email = model.Email;
            x.Phone = model.Phone;
            x.DateModified = DateTime.Today;
            x.ClientId = model.ClientId;
            x.DseId = model.DseId;
            x.CompanyId = model.CompanyId;

            return _db.SaveChanges();
        }


        public int UpdateAdmin(EditAdminModel model)
        {
            var x = _db.UserProfiles.Find(model.UserId);
            x.FirstName = model.FirstName;
            x.LastName = model.LastName;
            x.Phone = model.Phone;
            x.CanEdit = model.EditPriviledge;
            x.BranchId = model.BranchId;
            x.DateModified = DateTime.Today;

            _db.SaveChanges();
            return 1;
        }

        //public AdminModel GetAdminDetails(int id)
        //{
        //    return _db.UserProfiles.Where(x=>x.UserId == id).Select(x => new AdminModel
        //    {
                
        //    })
        //}

        public int ConfirmEmailForEdit(int id, string email)
        {
            var x = _db.UserProfiles.Find(id);
            if (email == x.Email) return 0;

            var mail = ConfirmEmail(email);
            return mail;
        }

        public List<TypeValue> GetPaymentTypeValues()
        {
            var val = _db.LoanPaymentTypes.Select(x => new TypeValue
            {
                Type = x.PaymentType,
                Id = x.PaymentTypeId
            }).ToList();

            return ConvertPaymentTypeModel(val);

        }

        public List<TypeValue> GetLoanStatusValues()
        {
            var val = _db.LoanStatus.Select(x => new TypeValue
            {
                Type = x.Status,
                Id = x.StatusId
            }).ToList();

            return ConvertStatusModel(val);

        }

        private List<TypeValue> ConvertPaymentTypeModel(List<TypeValue> val)
        {
            foreach (var type in val)
            {
                type.ValueDay = GetTotalValue(type.Id, "day");
                type.ValueMonth = GetTotalValue(type.Id, "month");
                type.Count = NumberOfPaymentsDueToday(type.Id);
            }

            return val;
        }

        private List<TypeValue> ConvertStatusModel(List<TypeValue> val)
        {
            foreach (var type in val)
            {
                type.ValueDay = GetDailyValueStatus(type.Id);
                type.ValueMonth = GetMonthlyValueStanding(type.Id);
                type.Count = NumberOfPaymentsDueToday(type.Id);
            }

            return val;
        }

        public DashboardModels GenerateDashboard()
        {

            var x = new DashboardModels
            {
                TypeChequeToday =
                {
                    NumberOfPaymentsDueToday = NumberOfPaymentsDueToday(1),
                    ValueOfChequesDueToday = GetTotalValue(1, "day")
                },
                TypeStandingToday =
                {
                    NumberOfPaymentsDueToday = NumberOfPaymentsDueToday(2),
                    ValueOfChequesDueToday = GetTotalValue(2, "day")
                },
                TypeDirectToday =
                {
                    NumberOfPaymentsDueToday = NumberOfPaymentsDueToday(3),
                    ValueOfChequesDueToday = GetTotalValue(3, "day")
                },
                TodayTypeTotal =
                {
                    CountTotal = _todayPaymentTypeGrandTotalCount,
                    GrandTotal = _todayPaymentTypeGrandTotal
                },
                TypeChequeMonth = 
                {
                    NumberOfPaymentsDueToday = NumberOfPaymentsDueThisMonth(1),
                    ValueOfChequesDueToday = GetTotalValue(1, "month")
                },
                TypeStandingMonth =
                {
                    NumberOfPaymentsDueToday = NumberOfPaymentsDueThisMonth(2),
                    ValueOfChequesDueToday = GetTotalValue(2, "month")
                },
                TypeDirectMonth =
                {
                    NumberOfPaymentsDueToday = NumberOfPaymentsDueThisMonth(3),
                    ValueOfChequesDueToday = GetTotalValue(3, "month")
                },
                MonthTypeTotal =
                {
                  CountTotal  = _monthPaymentTypeGrandTotalCount,
                  GrandTotal = _monthPaymentTypeGrandTotal
                },
                Bank = GetBankValues(),
                BankTotal =
                {
                    CountTotal = _bankGrandTotalCount,
                    GrandTotal = _bankGrandTotal
                },
                Cleared =
                {
                    Count = GetMonthlyCountStanding(2),
                    Value = GetMonthlyValueStanding(2)
                },
                Presented =
                {
                    Count = GetMonthlyCountStanding(3),
                    Value = GetMonthlyValueStanding(3)
                },
                Pending =
                {
                    Count = GetMonthlyCountStanding(1),
                    Value = GetMonthlyValueStanding(1)
                },
                PaidCash =
                {
                    Count = GetMonthlyCountStanding(4),
                    Value = GetMonthlyValueStanding(4)
                },
                Returned =
                {
                    Count = GetMonthlyCountStanding(5),
                    Value = GetMonthlyValueStanding(5)
                },
                StatusTotal =
                {
                    CountTotal = _statusGrandTotalCount,
                    GrandTotal = _statusGrandTotal
                }   
            };

            return x;
        }

        private int NumberOfPaymentsDueThisMonth(int p)
        {
            var x = _db.ChequeTransactions.Count(c => c.DateCreated >= _startOfMonth && c.DateCreated <= EndOfMonth && c.PaymentTypeId == p);
            _monthPaymentTypeGrandTotalCount += x;
            return x;
        }

        private int NumberOfPaymentsDueToday(int typeId)
        {
            var x = _db.ChequeTransactions.Count(c => c.DateDue == DateTime.Now && c.PaymentTypeId == typeId);
            _todayPaymentTypeGrandTotalCount += x;
            return x;
        }

        private int GetMonthlyCountStanding(int status)
        {
            var x = _db.ChequeTransactions.Count(c => c.DateCreated >= _startOfMonth && c.DateCreated <= EndOfMonth && c.StatusId == status);
            _statusGrandTotalCount += x;
            return x;
        }

        private decimal GetMonthlyValueStanding(int status)
        {
            var x = _db.ChequeTransactions.Where(c => c.DateCreated >= _startOfMonth && c.DateCreated <= EndOfMonth && c.StatusId == status).ToList();
            var total = TotalValue(x);
            _statusGrandTotal += total;
            return total;
        }

        private decimal GetDailyValueStatus(int status)
        {
            var x = _db.ChequeTransactions.Where(c => c.DateDue == DateTime.Now && c.StatusId == status).ToList();
            var total = TotalValue(x);
            _statusDayGrandTotal += total;
            return total;
        }

        private List<BankValue> GetBankValues()
        {
            var x = _db.Banks.ToList();

            return x.Select(bank => new BankValue
            {
                BankName = bank.BankName, 
                Value = GetTotalValueByBank(bank.BankId),
                ValueMonth = GetTotalValueByBankMonth(bank.BankId),
                Count = GetCountDueTodayByBank(bank.BankId)
            }).ToList();
        }

        private int GetCountDueTodayByBank(int p)
        {
            var x = _db.ChequeTransactions.Count(c => c.BankId == p && c.DateDue == DateTime.Now);
            _bankGrandTotalCount += x;
            return x;
        }

        private decimal GetTotalValueByBank(int p)
        {
            var x = _db.ChequeTransactions.Where(c => c.BankId == p && c.DateDue == DateTime.Now).ToList();
            var total = TotalValue(x);
            _bankGrandTotal += total;
            return total;
        }

        private decimal GetTotalValueByBankMonth(int p)
        {
            var x = _db.ChequeTransactions.Where(c => c.BankId == p && c.DateCreated >= _startOfMonth && c.DateCreated <= EndOfMonth).ToList();
            var total = TotalValue(x);
            BankGrandTotalMonth += total;
            return total;
        }

        private decimal GetTotalValue(int p, string type)
        {
            List<ChequeTransaction> x;
            decimal total;

            switch (type)
            {
                case "day":
                    x = _db.ChequeTransactions.Where(c => c.PaymentTypeId == p && c.DateDue == DateTime.Now).ToList();
                    total = TotalValue(x);
                    _todayPaymentTypeGrandTotal += total;
                    break;
                default:
                    x = _db.ChequeTransactions.Where(c => c.DateCreated >= _startOfMonth && c.DateCreated <= EndOfMonth && c.PaymentTypeId == p).ToList();
                    total = TotalValue(x);
                    _monthPaymentTypeGrandTotal += total;
                    break;
            }

            return total;
        }

        private static decimal TotalValue(IEnumerable<ChequeTransaction> x)
        {
            if (!x.Any()) return 0;
            decimal total = 0;
            total = x.Aggregate(total, (current, cheque) => current + cheque.Amount ?? 0);
            return total;
        }

        public LoanTransactions GetLoanDetails(int loanId)
        {
            var x = (_db.LoanTransactions.Where(l => l.LoanId == loanId).Select(l => new LoanTransactions
            {
                ClientId = (int)l.ClientId,
                ClientName = (_db.UserProfiles.Where(user => user.UserId == l.ClientId)
                    .Select(user => user.FirstName + " " + user.LastName)).FirstOrDefault(),
                TotalAmount = (double)l.TotalAmount,
                Duration = l.Duration,
                DurationId = (int)l.DurationId,
                Cheques =
                    (_db.ChequeTransactions.Where(cheque => cheque.LoanId == l.LoanId)
                        .Select(cheque => new ChequeViewModel
                        {
                            ChequeId = cheque.ChequeId,
                            ChequeNumber = cheque.ChequeNumber,
                            BankName = (_db.Banks.Where(b => b.BankId == cheque.BankId).Select(b => b.BankName).FirstOrDefault()),
                            Status = _db.LoanStatus.Where(s => s.StatusId == cheque.StatusId).Select(s => s.Status).FirstOrDefault(),
                            StatusId = (int)cheque.StatusId,
                            DateDue = (DateTime)cheque.DateDue,
                            PaidCash = (bool)cheque.PaidCash,
                            BankId = (int)cheque.BankId,
                            Amount = (decimal)cheque.Amount,
                            PaymentType = _db.LoanPaymentTypes.Where(p => p.PaymentTypeId == cheque.PaymentTypeId).Select(p => p.PaymentType).FirstOrDefault(),
                            PaymentTypeId = (int)cheque.PaymentTypeId
                        })).ToList(),
                Guarantee = _db.Guarantors.FirstOrDefault(g => g.LoanId == l.LoanId)
            })).SingleOrDefault();

            return x;
        }

        public List<Duration> GetAllFrames()
        {
            return _db.Durations.ToList();
        }

        public List<LoanStatu> GetAllStatus()
        {
            return _db.LoanStatus.ToList();
        }

        public List<LoanPaymentType> GetPaymentTypes()
        {
            return _db.LoanPaymentTypes.ToList();
        }

        public string DurationValue(int? id)
        {
            return _db.Durations.Where(x => x.DurationId == id).Select(x => x.Duration1).FirstOrDefault();
        }

        public string BankValue(int? id)
        {
            return _db.Banks.Where(x => x.BankId == id).Select(x => x.BankName).FirstOrDefault();
        }

        public string StatusValue(int? id)
        {
            return _db.LoanStatus.Where(x => x.StatusId == id).Select(x => x.Status).FirstOrDefault();
        }

        public string PaymentTypeValue(int? id)
        {
            return _db.LoanPaymentTypes.Where(x => x.PaymentTypeId == id).Select(x => x.PaymentType).FirstOrDefault();
        }

        public IEnumerable<ReportView> GetReportObject()
        {
            var x = (_db.ChequeTransactions.Join(_db.LoanTransactions, c => c.LoanId, loan => loan.LoanId,
                            (c, loan) => new { c, loan })
                            .Join(_db.UserProfiles, @t => @t.loan.ClientId, client => client.UserId, (@t, client) => new ReportView
                            {
                                LoanIdFromCore = @t.loan.LoanIdForCore,
                                ClientIdFromCore = client.ClientId,
                                ClientName = client.FirstName + " " + client.LastName,
                                //PaymentType = PaymentTypeValue(@t.c.PaymentTypeId),
                                ChequeId = @t.c.ChequeId,
                                ChequeNumber = @t.c.ChequeNumber,
                                //BankName = BankValue(@t.c.BankId),
                                //Status = StatusValue(@t.c.StatusId),
                                DateDue = @t.c.DateDue,
                                Amount = (double)@t.c.Amount,
                                LoanAmount = @t.loan.TotalAmount,
                                ValueDate = @t.c.ValueDate,
                                RecievingBank = @t.c.RecievingBank,
                                PaymentTypeId = @t.c.PaymentTypeId,
                                BankId = @t.c.BankId,
                                StatusId = @t.c.StatusId,
                                AdminId = @t.loan.AdminId,
                                DateCreated = @t.loan.DateCreated,
                                Phone = client.Phone,
                                Dse = client.DseId,
                                Company = client.CompanyId
                            })).ToList();

            return AddBranchIdtoModel(x);
        }

        private IEnumerable<ReportView> AddBranchIdtoModel(List<ReportView> x)
        {
            foreach (var r in x)
            {
                r.BranchId = GetBranchId(r.AdminId);
            }
            return x;
        }

        public IEnumerable<ReportView> GetReportQuery(IEnumerable<ReportView> report, DateTime? startDate, DateTime? endDate,
            int? bank, int? paymentType, int? status, int? branch)
        {
            if (startDate.HasValue && startDate.Value > DateTime.MinValue)
                report = report.Where(r => r.DateDue != null && r.DateDue.Value.Date >= startDate.Value.Date).ToList();

            if (endDate.HasValue && endDate.Value > DateTime.MinValue)
                report = report.Where(r => r.DateDue != null && r.DateDue.Value.Date <= endDate.Value.Date).ToList();

            if (paymentType.HasValue && paymentType > 0)
                report = report.Where(r => r.PaymentTypeId == paymentType).ToList();

            if (bank.HasValue && bank > 0)
                report = report.Where(r => r.BankId == bank).ToList();

            if (status.HasValue && status > 0)
                report = report.Where(r => r.StatusId == status).ToList();

            if (branch.HasValue && branch > 0)
                report = report.Where(r => r.BranchId == branch).ToList();

            return report;
        }

        public IEnumerable<ReportView> GetReportPagingQuery(IEnumerable<ReportView> postsQuery, int? startRow, int? rowCount)
         {
             if ((startRow.HasValue) && (rowCount.HasValue && rowCount.Value > 0))
                postsQuery = postsQuery.Skip((int)startRow).Take((int)rowCount);
     
           return postsQuery;
         }

        public List<webpages_Roles> GetRoles()
        {
            return _db.webpages_Roles.ToList();
        }

        public List<FCLBranch> GetBranches()
        {
            return _db.FCLBranches.ToList();
        }

        public List<DSE> GetDSE()
        {
            return _db.DSEs.ToList();
        }

        public List<Company> GetCompanies()
        {
            return _db.Companies.ToList();
        }

        public string BranchName(int? id)
        {
            return (_db.UserProfiles.Where(user => user.UserId == id)
                .Join(_db.FCLBranches, user => user.BranchId, branch => branch.Id, (user, branch) => branch.Branch)).FirstOrDefault();
        }

        public string GetBranchName(int? id)
        {
            return _db.FCLBranches.Where(x => x.Id == id).Select(x => x.Branch).FirstOrDefault();
        }
        public string GetDSEName(int? id)
        {
            return _db.DSEs.Where(x => x.Id == id).Select(x => x.Name).FirstOrDefault();
        }

        public string GetCompanyName(int? id)
        {
            return _db.Companies.Where(x => x.Id == id).Select(x => x.CompanyName).FirstOrDefault();
        }

        public int? GetBranchId(int? id)
        {
            return id == 0 ? null : _db.UserProfiles.Where(user => user.UserId == id).Select(x => x.BranchId).FirstOrDefault();
        }

        public int CreateBranch(FCLBranch x)
        {
            _db.FCLBranches.Add(x);
            return _db.SaveChanges();
        }

        public FCLBranch ViewBranchById(int id)
        {
            var x = _db.FCLBranches.Find(id);
            return x;
        }

        public List<FCLBranch> ViewBranch()
        {
            var x = _db.FCLBranches.ToList();
            return x;
        }

        public int DeleteBranch(int id)
        {
            var x = _db.FCLBranches.Find(id);
            _db.FCLBranches.Remove(x);
            return _db.SaveChanges();
        }

        public int UpdateBranch(FCLBranch branch)
        {
            var x = _db.FCLBranches.Find(branch.Id);
            if (branch.Branch == x.Branch) return 1;

            x.Branch = branch.Branch;
            return _db.SaveChanges();
        }

        public int CreateDse(DSE x)
        {
            _db.DSEs.Add(x);
            return _db.SaveChanges();
        }

        public DSE ViewDseById(int id)
        {
            var x = _db.DSEs.Find(id);
            return x;
        }

        public List<DSE> ViewDse()
        {
            var x = _db.DSEs.ToList();
            return x;
        }

        public int DeleteDse(int id)
        {
            var x = _db.DSEs.Find(id);
            _db.DSEs.Remove(x);
            return _db.SaveChanges();
        }

        public int UpdateDse(DSE dse)
        {
            var x = _db.DSEs.Find(dse.Id);
            if (dse.Name == x.Name) return 1;

            x.Name = dse.Name;
            return _db.SaveChanges();
        }

        public int CreateCompany(Company x)
        {
            _db.Companies.Add(x);
            return _db.SaveChanges();
        }

        public Company ViewCompanyById(int id)
        {
            var x = _db.Companies.Find(id);
            return x;
        }

        public List<Company> ViewCompany()
        {
            var x = _db.Companies.ToList();
            return x;
        }

        public int DeleteCompany(int id)
        {
            var x = _db.Companies.Find(id);
            _db.Companies.Remove(x);
            return _db.SaveChanges();
        }

        public int UpdateCompany(Company company)
        {
            var x = _db.Companies.Find(company.Id);
            if (company.CompanyName == x.CompanyName) return 1;

            x.CompanyName = company.CompanyName;
            return _db.SaveChanges();
        }

        public bool CanEdit(int p)
        {
            return _db.UserProfiles.Where(x => x.UserId == p).Select(x => x.CanEdit).FirstOrDefault() ?? false;
        }

        public bool BranchExists(string p)
        {
            return _db.FCLBranches.Count(x => x.Branch == p) > 0;
        }

        public bool CompanyExists(string p)
        {
            return _db.Companies.Count(x => x.CompanyName == p) > 0;
        }

        public bool DseExists(string p)
        {
            return _db.DSEs.Count(x => x.Name == p) > 0;
        }
    }
}
