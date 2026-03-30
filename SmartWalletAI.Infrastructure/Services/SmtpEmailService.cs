using Microsoft.AspNetCore.Mvc;
using SmartWalletAI.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Infrastructure.Services
{
    public class SmtpEmailService : IEmailService
    {
        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                DeliveryMethod = SmtpDeliveryMethod.Network, 
                UseDefaultCredentials = false,             
                Credentials = new NetworkCredential("ozalposman005@gmail.com", "bunrivacfaphaljo"), 
            };
            var mailMessage = new MailMessage
            {
                From = new MailAddress("ozalposman005@gmail.com", "SmartWallet AI Güvenlik"),
                Subject = subject,
                Body = body,
                IsBodyHtml = true,
            };
            mailMessage.To.Add(to);

            await smtpClient.SendMailAsync(mailMessage);
        }
    }
}
