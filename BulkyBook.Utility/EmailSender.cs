using MailKit.Net.Smtp;
using Microsoft.AspNetCore.Identity.UI.Services;
using MimeKit;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
//using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.Utility
{
	public class EmailSender : IEmailSender
	{
		public Task SendEmailAsync(string email, string subject, string htmlMessage)
		{
			var emailToSend = new MimeMessage();
			emailToSend.From.Add(MailboxAddress.Parse("hello@dotnetmastery.com"));
			emailToSend.To.Add(MailboxAddress.Parse(email));
			emailToSend.Subject = subject;
			emailToSend.Body = new TextPart(MimeKit.Text.TextFormat.Html){ Text = htmlMessage};


			// send email
			var emailClient = new SmtpClient();
			emailClient.Connect("smtp.gmail.com", 587, MailKit.Security.SecureSocketOptions.StartTls);
			emailClient.Authenticate("tik.dikawahyu123@gmail.com", "kiogyochnzhrgvpt");
			emailClient.SendAsync(emailToSend);

			//var smtpClient = new SmtpClient("smtp.gmail.com")
			//{
			//	Port = 587,
			//	Credentials = new NetworkCredential("tik.dikawahyu123@gmail.com", "kiogyochnzhrgvpt"),
			//	EnableSsl = true
			//};
			//smtpClient.Send("hello@dotnetmastery.com", "tik.dikawahyu123@gmail.com", "subject", "body");
			return Task.CompletedTask;
		}
	}
}
