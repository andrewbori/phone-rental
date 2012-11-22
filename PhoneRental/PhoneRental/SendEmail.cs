using PhoneRental.Models;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;
using System.Web.UI.WebControls;

namespace PhoneRental
{
    public static class SendEmail
    {
        public static void FromTemplate(string templatePath, object model, Type modelType, string[] to, string subject)
        {
             string template = System.IO.File.ReadAllText(templatePath);
             string body = Razor.Parse<PreBorrowEmail>(template, (PreBorrowEmail)model);

             MailMessage mail = new MailMessage();
             foreach (var t in to)
             {
                 mail.To.Add(t);
             }
             mail.From = new MailAddress("amorg.phonerental@gmail.com");
             mail.Subject = subject;
             mail.Body = body;
             mail.IsBodyHtml = true;

             SmtpClient smtp = new SmtpClient();
             smtp.EnableSsl = true;
             smtp.Send(mail);
        }

        public static void FromTemplate2(string templatePath, object model, Type modelType, string[] to, string subject)
        {
            string template = System.IO.File.ReadAllText(templatePath);
            string body = Razor.Parse<NewUserEmail>(template, (NewUserEmail)model);

            MailMessage mail = new MailMessage();
            foreach (var t in to)
            {
                mail.To.Add(t);
            }
            mail.From = new MailAddress("amorg.phonerental@gmail.com");
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            SmtpClient smtp = new SmtpClient();
            smtp.EnableSsl = true;
            smtp.Send(mail);
        }
    }
}