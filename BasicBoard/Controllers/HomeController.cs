using BasicBoard.Data;
using BasicBoard.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBoard.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
        {
            try
            {
                var USER_LOGIN_KEY = HttpContext.Session.GetInt32("USER_LOGIN_KEY"); //세선에 저장된 userNo 불러오기

                using(var db = new BasicboardDbContext())
                {
                    var user = db.User.FirstOrDefault(u => u.UserNo.Equals(USER_LOGIN_KEY)); //현재 로그인 된 유저 정보 가져오기

                    if(user != null)
                    {
                        //로그인 되어 있으면
                        ViewData["user"] = user;
                        return View();
                    }
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }

            return View();
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error(string msg)
        {
            ViewData["msg"] = msg;
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
