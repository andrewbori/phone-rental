using PhoneRental.Models;
using RazorEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Web;

namespace PhoneRental
{
    public static class SendEmail
    {
        private static SmtpClient smtpClient;
        private static SmtpClient MySmtpClient
        {
            get
            {
                // SMTP settings
                if (smtpClient == null)
                {
                    smtpClient = new SmtpClient();
                    smtpClient.Host = "smtp.gmail.com";
                    smtpClient.Port = 587;
                    smtpClient.Credentials = new System.Net.NetworkCredential("webabc.hu@gmail.com", "WebAbcPass");
                    smtpClient.EnableSsl = true;
                }
                return smtpClient;
            }
            
        }

        public static void FromTemplate(string templatePath, object model, Type modelType, string[] to, string subject)
        {
            
            string template = System.IO.File.ReadAllText(templatePath);
            //Razor.Compile(template, modelType, "myModel");
            string body = Razor.Parse<PreBorrowEmail>(template, (PreBorrowEmail)model);

            MailMessage mail = new MailMessage();
            foreach (var t in to)
            {
                mail.To.Add(t);
            }
            mail.From = new MailAddress("noreply@webabc.hu");
            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            MySmtpClient.Send(mail);
        }
    }
}