using BasicBoard.Data;
using BasicBoard.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBoard.Controllers
{
    public class ReplyContorller : Controller
    {

        // reply/new POST
        [Route("reply/new")]
        public IActionResult Index([FromBody] Reply reply)
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())
            {
                db.Add(reply);
                int result = db.SaveChanges();

                if(result > 0)
                {
                    
                }
            }
                return View();
        }
    }
}
