using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Text;

namespace CRMFunction.Services
{
    public class EmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendCustomerAssignedEmail(
            string toEmail,
            string salespersonName,
            string customerName,
            string customerPhone,
            string customerAddress)
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse("crm@system.local"));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = "New customer assignment";
            email.Body = new TextPart("plain")
            {
                Text =
                $"""
                Hello {salespersonName}

                You have been assigned as responsible seller for a new customer.

                Customer information:
                - Name: {customerName}
                - Phone: {customerPhone}
                - Address: {customerAddress}

                Please follow up with the customer.

                Regards,
                CRM System
                """
            };

            using var smtp = new SmtpClient();

            await smtp.ConnectAsync(
                _configuration["MailHost"],
                int.Parse(_configuration["MailPort"])!,
                SecureSocketOptions.StartTls);

            await smtp.AuthenticateAsync(
                _configuration["MailUsername"],
                _configuration["MailPassword"]);

            await smtp.SendAsync(email);

            await smtp.DisconnectAsync(true);
        }
    }
}
