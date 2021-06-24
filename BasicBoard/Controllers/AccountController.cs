using BasicBoard.Data;
using BasicBoard.Models;
using BasicBoard.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace BasicBoard.Controllers
{
    public class AccountController : Controller
    {

        [HttpGet]
        public IActionResult Login(string msg) //로그인
        {
            ViewData["msg"] = msg;
            return View();
        }


        [HttpPost]
        public IActionResult Login(LoginViewModel model)
        {
            try
            {
                //ID,비밀번호 - 필수
                if (ModelState.IsValid)
                {
                    using(var db = new BasicboardDbContext())
                    {
                        //비밀번호 암호화
                        model.UserPassword = ConvertPassword(model.UserPassword);
                        
                        //단순 메모리 위치상의 비교, 데이터 값도 비교한다. 메모리 누수를 방지 ==로 표시하면 새로운 string객체로 비교하기 때문에 메모리 누수 발생
                        var user = db.User.FirstOrDefault(u => u.UserId.Equals(model.UserId) && u.UserPassword.Equals(model.UserPassword));

                        if(user != null)
                        {
                            //로그인에 성공했을 때
                            HttpContext.Session.SetInt32("USER_LOGIN_KEY", user.UserNo);
                            HttpContext.Session.SetString("USER_LOGIN_NAME", user.UserName);

                            return RedirectToAction("Index", "Home"); //로그인 성공 페이지로 이동
                        }
                 
                        //로그인에 실패했을 때
                        ModelState.AddModelError(string.Empty, "사용자 ID 혹은 비밀번호가 올바르지 않습니다.");
                    }

                }

                //로그인 필수 입력값이 없을 때 
                return View(model);
                               
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/account/login?msg=로그인 시도 중 오류가 발생하였습니다. 다시 시도해 주십시오.");
            }
        }

        public IActionResult MyPage()
        {
            var USER_LOGIN_KEY = HttpContext.Session.GetInt32("USER_LOGIN_KEY"); //세선에 저장된 userNo 불러오기


            if (USER_LOGIN_KEY == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using(var db = new BasicboardDbContext())
                {
                    var user = db.User.FirstOrDefault(u => u.UserNo.Equals(USER_LOGIN_KEY));

                    if(user != null)
                    {
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


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("USER_LOGIN_KEY");
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Register(string msg) //회원가입
        {
            ViewData["msg"] = msg;
            return View();
        }


        [HttpPost]
        public IActionResult Register(User model)
        {
            try
            {
                //중복된 값이 있는지 유효성 검사 로직 추가(이메일, 아이디 등..)

                if (ModelState.IsValid) //필수 입력사항이 입력 받았는지 체크(Required어노테이션으로 체크)
                {
                    //비밀번호 암호화
                    model.UserPassword = ConvertPassword(model.UserPassword);

                    using(var db = new BasicboardDbContext())
                    {
                        db.User.Add(model); //메모리에 올리기(실제 DB에는 저장안됨)
                        db.SaveChanges(); //실제 SQL로 저장
                    }
                    return RedirectToAction("Index", "Home"); //다른 View로 전달(HomeControlle의 Index뷰로 전달)
                }
                return View(model);

            }
            catch(Exception e)
            {
                return Redirect($"/account/register?msg={e.Message}");

            }

        }


        //비밀번호 암호화
        public string ConvertPassword(string userPassword)
        {
            var sha = new System.Security.Cryptography.HMACSHA512();
            sha.Key = System.Text.Encoding.UTF8.GetBytes(userPassword.Length.ToString());

            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userPassword));

            return userPassword = System.Convert.ToBase64String(hash);
        }

    }
}
