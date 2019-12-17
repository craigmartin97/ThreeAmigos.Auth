using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Mail;
using System.Threading.Tasks;
using ThAmCo.Auth.Data.Account;

namespace ThAmCo.Auth
{
    public class EmailSender
    {
        /// <summary>
        /// Send an email to the given email addres for confirmation
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        public void SendEmail(AppUser user, IConfiguration configuration, string msg, string subject)
        {
            try
            {
                string email = configuration["Email"];

                MailMessage message = new MailMessage();
                message.From = new MailAddress(email);
                message.To.Add(user.Email);
                message.Subject = subject;
                message.Body = msg;

                SmtpClient smtp = new SmtpClient("smtp.gmail.com");
                smtp.Credentials = new System.Net.NetworkCredential
                (email,
                 configuration["Pass"]);
                smtp.EnableSsl = true;

                smtp.Send(message);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
            }
        }
    }
}
