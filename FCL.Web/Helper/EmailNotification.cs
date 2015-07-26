using System;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Web;
using FCL.Core;
using FCL.Core.DataAccess;
using Quartz;
using Quartz.Impl;

namespace FCL.Web.Helper
{
    public class EmailNotification : IJob
    {
        private static readonly FclManager _manager = new FclManager();
        private static string _username = ConfigurationManager.AppSettings["username"];
        private static string _password = ConfigurationManager.AppSettings["password"];
        private static string _sender = ConfigurationManager.AppSettings["sender"];

        public void Execute(IJobExecutionContext context)
        {
            var cheques = _manager.GetAllPendingCheques();

            foreach (var x in cheques.Where(x => x.DateDue != null && CheckDate((DateTime)x.DateDue)))
            {
                SendMailToClient(x);
                SendMailToAdmin(x);
            }
        }

        private static void SendMailToAdmin(ChequeTransaction cheque)
        {
            var adminMail = _manager.GetAdminEmail(cheque.LoanId);
            var body = FormatBody(cheque);

            SendMail(adminMail, body);  
        }

        public static bool CheckDate(DateTime dueDate)
        {
            return (dueDate.Date - DateTime.Now.Date).TotalDays <= 3;
        }

        public static void SendMailToClient(ChequeTransaction cheque)
        {
            var custInfo = _manager.GetCustomerInfo(cheque.LoanId);
            var body = FormatBody(cheque, custInfo);
            var sms = FormatBodySMS(cheque);

            SendMail(custInfo.Email, body);
            SendText(custInfo.Phone, sms);
        }

        private static string FormatBodySMS(ChequeTransaction cheque)
        {
            return String.Format(
                            File.ReadAllText(HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["sms-reminder"]))
                            , cheque.ChequeNumber, cheque.Amount, cheque.DateDue);
        }

        private static void SendMail(string email, string body)
        {
            var smtpEmail = ConfigurationManager.AppSettings["SmtpEmail"];
            var reciever = email;

            SmtpClient smtpClient = new SmtpClient();
            MailMessage mail = new MailMessage { From = new MailAddress(smtpEmail, "Deposit Reminder") };

            //Setting From , To and CC
            mail.To.Add(new MailAddress(reciever));
            mail.Subject = "Reminder";
            mail.Body = body;

            smtpClient.Send(mail);
        }

        public static string SendText(string phoneNumber, string textMessage)
        {
            var client = new WebClient();
            var reply =
                client.DownloadString(
                    string.Format("http://account.kudisms.net/api/index.php?username={0}&password={1}&sender={2}&mobiles={3}&message={4}", _username, _password, _sender, phoneNumber, textMessage));

            return "done";
        }

        public static string FormatBody(ChequeTransaction cheque, UserProfile user)
        {
            return String.Format(
                            File.ReadAllText(HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["ClientReminder"]))
                            , user.FirstName, cheque.Amount, cheque.DateDue);
        }

        public static string FormatBody(ChequeTransaction cheque)
        {
            return String.Format(
                            File.ReadAllText(HttpContext.Current.Server.MapPath(ConfigurationManager.AppSettings["AdminReminder"]))
                            , DateTime.Now.ToShortDateString(), cheque.ChequeNumber, cheque.Amount, cheque.DateDue);
        }
    }

    public class JobScheduler
    {
        public static void Start()
        {
            IScheduler scheduler = StdSchedulerFactory.GetDefaultScheduler();
            scheduler.Start();

            IJobDetail job = JobBuilder.Create<EmailNotification>().Build();

            ITrigger trigger = TriggerBuilder.Create()
                .WithDailyTimeIntervalSchedule
                  (s =>
                     s.WithIntervalInHours(24)
                    .OnEveryDay()
                    .StartingDailyAt(TimeOfDay.HourAndMinuteOfDay(10, 0))
                  )
                .Build();

            scheduler.ScheduleJob(job, trigger);
        }
    }
}