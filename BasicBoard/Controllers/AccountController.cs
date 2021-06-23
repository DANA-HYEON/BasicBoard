using BasicBoard.Data;
using BasicBoard.Models;
using BasicBoard.ViewModel;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
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
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            try
            {
                //ID,비밀번호 - 필수
                if (ModelState.IsValid)
                {
                    using(var db = new BasicboardDbContext())
                    {

                        model.ConvertPassword();
                        
                        //단순 메모리 위치상의 비교, 데이터 값도 비교한다. 메모리 누수를 방지 ==로 표시하면 새로운 string객체로 비교하기 때문에 메모리 누수 발생
                        var user = db.User.FirstOrDefault(u => u.UserId.Equals(model.UserId) && u.UserPassword.Equals(model.UserPassword));

                        if(user != null)
                        {

                            var identity = new ClaimsIdentity(CookieAuthenticationDefaults.AuthenticationScheme, ClaimTypes.Name, ClaimTypes.Role);
                            identity.AddClaim(new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()));
                            identity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
                            identity.AddClaim(new Claim(ClaimTypes.Email, user.UserEmail));
                            identity.AddClaim(new Claim("LastCheckDateTime", DateTime.UtcNow.ToString("yyyyMMddHHmmss")));

                            var princiapl = new ClaimsPrincipal(identity);

                            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, princiapl, new AuthenticationProperties
                            {

                                IsPersistent = false, //브라우저 종료시 쿠키 삭제
                                ExpiresUtc = DateTime.UtcNow.AddHours(4), //4시간이 지나면 쿠키 만료
                                AllowRefresh = true //다시 접속시 쿠키 생성

                            });

                            //로그인에 성공했을 때
                            //HttpContext.Session.SetInt32("USER_LOGIN_KEY", user.UserNo);
                            return RedirectToAction("LoginSuccess", "Home"); //로그인 성공 페이지로 이동
                        }
                 
                    //로그인에 실패했을 때
                    ModelState.AddModelError(string.Empty, "사용자 ID 혹은 비밀번호가 올바르지 않습니다.");
                    }

                }
                 return View(model);

            }
            catch(Exception e)
            {
                return Redirect($"/account/login?msg={HttpUtility.UrlEncode(e.Message)}");
            }
        }

        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync();
            //HttpContext.Session.Remove("USER_LOGIN_KEY");
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
                    model.ConvertPassword();
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
                return Redirect($"/account/register?msg={HttpUtility.UrlEncode(e.Message)}");

            }

        }


}
    }
