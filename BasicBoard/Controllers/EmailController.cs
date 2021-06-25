using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBoard.Controllers
{
    public class EmailController : Controller
    {

        public static IEmailSender EmailSender { get; set; }

        public EmailController(IEmailSender emailSender)
        {
            EmailSender = emailSender;
        }

        public async Task<IActionResult> Send(string toAddress)
        {
            const string strPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; //문자 생성 풀
            char[] charRandom = new char[6];

            for(int i=0; i<6; i++)
            {
                
            }


            var subject = "BasicBoard 인증번호";
            var body = "인증번호입니다!";
            await EmailSender.SendEmailAsync(toAddress, subject, body);
            return RedirectToAction("FindPassword", "Account");
        }
    }
}
