using BasicBoard.Data;
using BasicBoard.Models;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace BasicBoard.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login() //로그인
        {
            return View();
        }

         [HttpPost]
         public IActionResult Login(User model)
         {
            //ID,비밀번호 - 필수
            if (ModelState.IsValid)
            {
                using(var db = new BasicboardDbContext())
                {
                    //Linq - 메서드 체이닝
                    //단순 메모리 위치상의 비교, 데이터 값도 비교한다. 메모리 누수를 방지 ==로 표시하면 새로운 string객체로 비교하기 때문에 메모리 누수 발생
                    var user = db.User.FirstOrDefault(u => u.UserId.Equals(model.UserId) && u.UserPassword.Equals(model.UserPassword));

                    if(user == null)
                    {
                        //로그인에 실패했을 때
                        ModelState.AddModelError(string.Empty, "사용자 ID 혹은 비밀번호가 올바르지 않습니다.");
                    }
                    else
                    {
                        //로그인에 성공했을 때
                        return RedirectToAction("LoginSuccess", "Home");
                    }
                }
            }
             return View(model);
         }

        [HttpGet]
        public IActionResult Register() //회원가입
        {
            return View();
        }

        [HttpPost]
        public IActionResult Register(User model)
        {
            if (ModelState.IsValid) //필수 입력사항이 입력 받았는지 체크(Required어노테이션으로 체크)
            {
                using(var db = new BasicboardDbContext())
                {
                    db.User.Add(model); //메모리에 올리기(실제 DB에는 저장안됨)
                    db.SaveChanges(); //실제 SQL로 저장
                }
                return RedirectToAction("Index", "Home"); //다른 View로 전달(HomeControlle의 Index뷰로 전달)
            }
            return View(model);
        }
    }
}
