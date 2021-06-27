using BasicBoard.Data;
using BasicBoard.Models;
using BasicBoard.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Web;

namespace BasicBoard.Controllers
{
    public class AccountController : Controller
    {

        public IEmailSender EmailSender { get; set; }

        public AccountController(IEmailSender emailSender)
        {
            EmailSender = emailSender;
        }



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
                    using (var db = new BasicboardDbContext())
                    {
                        //비밀번호 암호화
                        model.UserPassword = ConvertPassword(model.UserPassword);

                        //단순 메모리 위치상의 비교, 데이터 값도 비교한다. 메모리 누수를 방지 ==로 표시하면 새로운 string객체로 비교하기 때문에 메모리 누수 발생
                        var user = db.User.FirstOrDefault(u => u.UserId.Equals(model.UserId) && u.UserPassword.Equals(model.UserPassword));

                        if (user != null)
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
            catch (Exception e)
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
                using (var db = new BasicboardDbContext())
                {
                    var user = db.User.FirstOrDefault(u => u.UserNo.Equals(USER_LOGIN_KEY));

                    if (user != null)
                    {
                        ViewData["user"] = user;
                        return View();
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }

            return View();
        }


        public IActionResult Logout()
        {
            HttpContext.Session.Remove("USER_LOGIN_KEY");
            HttpContext.Session.Remove("USER_LOGIN_NAME");
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
                if (ModelState.IsValid)
                {
                    //userName 공백값 제거
                    model.UserName = model.UserName.Trim();

                    //비밀번호 암호화
                    model.UserPassword = ConvertPassword(model.UserPassword);

                    using (var db = new BasicboardDbContext())
                    {
                        db.User.Add(model); //메모리에 올리기(실제 DB에는 저장안됨)
                        db.SaveChanges(); //실제 SQL로 저장
                    }
                    return RedirectToAction("Index", "Home"); //다른 View로 전달(HomeControlle의 Index뷰로 전달)
                }
                return View(model);

            }
            catch (Exception e)
            {
                return Redirect($"/account/register?msg={e.Message}");

            }

        }


        [HttpGet]
        public IActionResult FindPassword()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> FIndPassword(string userEmail, string verifyCode)
        {
            if(userEmail != null) //이메일을 입력 받으면
            {
                using(var db = new BasicboardDbContext())
                {
                    var user = db.User.FirstOrDefault(u => u.UserEmail.Equals(userEmail));

                    if(user != null) //해당 이메일정보가 가입정보에 있으면
                    {
                        Random random = new Random();
                        const string strPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; //문자 생성 풀
                        char[] charRandom = new char[6];

                        for (int i = 0; i < 6; i++)
                        {
                            //인증번호 생성 var number = 123;
                            charRandom[i] = strPool[random.Next(strPool.Length)];
                        }

                        string strRet = new string(charRandom); //char to string

                        var subject = "BasicBoard 인증번호";
                        string body = strRet;
                        await EmailSender.SendEmailAsync(userEmail, subject, body); //인증번호 이메일로 전송

                        TempData["userEmail"] = userEmail;
                        ViewData["verifyCode"] = body;
                        return View();
                    }
                }
            }

            if(verifyCode != null) //인증번호를 입력받으면
            {
                return RedirectToAction("ChangePassword", "Account");
            }

            ViewData["fail"] = "fail";
            return View(); //이메일이 가입정보에 없으면
        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            return View();
        }

        [HttpPost]
        public IActionResult ChangePassword(string userPassword, string confirmedPassword)
        {
            if(userPassword != confirmedPassword)
            {
                return View();
            }

            try
            {
                using(var db = new BasicboardDbContext())
                {
                    var userEmail = TempData["userEmail"];
                    var user = db.User.FirstOrDefault(u => u.UserEmail.Equals(userEmail));

                    if(user != null)
                    {
                        user.UserPassword = ConvertPassword(confirmedPassword);
                        db.Update(user);

                        var result = db.SaveChanges();

                        if(result > 0)
                        {
                            TempData["changedPwd"] = user.UserEmail;
                            return RedirectToAction("Index", "Home");
                        }
                        
                    }
                    return View();
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }
        }


        //비밀번호 암호화
        public string ConvertPassword(string userPassword)
        {
            var sha = new System.Security.Cryptography.HMACSHA512();
            sha.Key = System.Text.Encoding.UTF8.GetBytes(userPassword.Length.ToString());

            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userPassword));

            return userPassword = System.Convert.ToBase64String(hash); //33%정도 길어진다https://docs.microsoft.com/ko-kr/dotnet/api/system.convert.tobase64string?view=net-5.0
        }


        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyId(string userId)
        {
            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var userInfo = db.User.FirstOrDefault(u => u.UserId.Equals(userId));

                    if (userInfo != null)
                    {
                        return Json("해당 아이디는 이미 존재하는 아이디입니다.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Account/Register?msg={e.Message}");
            }

            return Json(true);
        }


        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyPhone([RegularExpression(@"^\d{3}-\d{4}-\d{4}$")] string userPhone)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json("전화번호 형식이 올바르지 않습니다. 예시 : ###-####-####");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Account/Register?msg={e.Message}");
            }

            return Json(true);
        }


        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyEmail([RegularExpression(@"^[0-9a-zA-Z]([-_.]?[0-9a-zA-Z])*@[0-9a-zA-Z]([-_.]?[0-9a-zA-Z])*.[a-zA-Z]{2,3}$")] string userEmail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json("이메일 형식이 올바르지 않습니다. 예시 : right@right.com");
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Account/Error?msg={e.Message}");
            }

            return Json(true);
        }


        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyPassword([RegularExpression(@"^.*(?=^.{8,15}$)(?=.*\d)(?=.*[a-zA-Z])(?=.*[!@#$%^&+=]).*$")] string userPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return Json("문자/숫자/특수문자를 포함한 형태의 8~15자리 이내의 암호여야 합니다.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Account/Error?msg={e.Message}");
            }

            return Json(true);
        }
    }
}
