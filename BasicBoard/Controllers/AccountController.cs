using BasicBoard.Data;
using BasicBoard.Models;
using BasicBoard.Services;
using BasicBoard.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

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
        public IActionResult Login(LoginViewModel model) //로그인
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
                            //아이디와 패스워드가 db에 있을 때 세션 등록
                            HttpContext.Session.SetInt32("USER_LOGIN_KEY", user.UserNo); //회원번호
                            HttpContext.Session.SetString("USER_LOGIN_NAME", user.UserName); //회원이름
                             
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

        
        [HttpGet]
        public IActionResult MyPage() //마이페이지
        {
            var USER_LOGIN_KEY = HttpContext.Session.GetInt32("USER_LOGIN_KEY"); //세선에 저장된 userNo 불러오기

            if (USER_LOGIN_KEY == null)
            {
                //세션 정보가 없다면, 로그인이 안된 상태 -> 로그인화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var user = db.User.FirstOrDefault(u => u.UserNo.Equals(USER_LOGIN_KEY)); //유저 정보 가져오기

                    if (user != null) //유저정보가 있다면 해당 정보 view에 전달
                    {
                        ViewData["user"] = user;
                        return View();
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}"); //오류 발생 시 error페이지로 이동
            }

            //유저 정보가 없다면 로그인 화면으로 이동
            return RedirectToAction("Login", "Account");
        }


        [HttpGet]
        public IActionResult Logout() //로그아웃
        {
            //세션정보 제거
            HttpContext.Session.Remove("USER_LOGIN_KEY");
            HttpContext.Session.Remove("USER_LOGIN_NAME");
            return RedirectToAction("Index", "Home");
        }


        [HttpGet]
        public IActionResult Register(string msg) //회원가입
        {
            //예외처리 발생 시 오류문구 전달
            ViewData["msg"] = msg;
            return View();
        }


        [HttpPost]
        public IActionResult Register(User model) //회원가입
        {
            try
            {
                //모든 입력값 필수 체크
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

                //회원가입 필수입력 값이 없을 때
                return View(model);
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/account/register?msg={e.Message}");
            }
        }


        [HttpGet]
        public IActionResult FindPassword() //비밀번호 찾기
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> FIndPassword(string userEmail, string verifyCode) //비밀번호 찾기
        {
            if(userEmail != null) //이메일을 입력 받으면
            {
                using(var db = new BasicboardDbContext())
                {
                    var user = db.User.FirstOrDefault(u => u.UserEmail.Equals(userEmail)); //해당 이메일을 가진 유저 정보 가져오기

                    if(user != null) //해당 이메일이 가입정보에 있으면
                    {
                        Random random = new Random();
                        const string strPool = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"; //문자 생성 풀
                        char[] charRandom = new char[6];

                        for (int i = 0; i < 6; i++)
                        {
                            //인증번호 생성
                            charRandom[i] = strPool[random.Next(strPool.Length)]; //문자 생성 풀에서 난수 생성
                        }

                        string body = new string(charRandom); //char to string(인증번호 생성 완료)

                        var subject = "BasicBoard 인증번호"; //이메일 제목
                        await EmailSender.SendEmailAsync(userEmail, subject, body); //인증번호 이메일로 전송

                        TempData["userEmail"] = userEmail; 
                        ViewData["verifyCode"] = body; //인증번호 view로 전달
                        return View();
                    }

                    ViewData["fail"] = "fail"; //해당 이메일이 가입정보에 없으면
                }
            }

            if(verifyCode != null) //인증번호를 입력받으면
            {
                return RedirectToAction("ChangePassword", "Account"); //비밀번호 변경 화면으로 이동
            }

            return View();
        }



        [HttpGet]
        public IActionResult ChangePassword() //비밀번호 변경
        {
            return View();
        }


        [HttpPost]
        public IActionResult ChangePassword(string userPassword, string confirmedPassword) //비밀번호 변경
        {
            if(userPassword != confirmedPassword) //변경 비번과 확인 비번이 틀릴 경우
            {
                return View();
            }

            try
            {
                using(var db = new BasicboardDbContext())
                {
                    var userEmail = TempData["userEmail"]; //FindPassword에서 보낸 TempData["userEmail"](인증번호 받은 이메일) 값 가져오기
                    var user = db.User.FirstOrDefault(u => u.UserEmail.Equals(userEmail)); //해당 이메일로 가입한 회원 정보 가여조기

                    if(user != null) //가입정보가 있으면
                    {
                        user.UserPassword = ConvertPassword(confirmedPassword); //비밀번호 암호화
                        db.Update(user); //해당 비밀번호로 업데이트

                        var result = db.SaveChanges();

                        if(result > 0) //업데이트 성공 시
                        {
                            TempData["changedPwd"] = user.UserEmail; //변경완료 alert를 위한 TempData
                            return RedirectToAction("Index", "Home");
                        }
                        
                    }

                    //가입정보가 없으면
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
            //암호화에 대한 비밀키 생성(sha)
            var sha = new System.Security.Cryptography.HMACSHA512(); //SHA512해시 기능을 이용해서 HMAC(해시 기반 메시지 인증코드) 계산
            sha.Key = System.Text.Encoding.UTF8.GetBytes(userPassword.Length.ToString());

            //지정된 바이트 배열에 대해 해시값을 계산(HMACSHA512 알고리즘에 따라 해시값의 크기는 달라진다)
            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(userPassword));

            return userPassword = System.Convert.ToBase64String(hash); //8비트 부호 없는 정수로 구성된 배열의 값을 base-64 인코딩된 해당하는 문자열 표현으로 변환(인코딩 방식의 하나)
        }





        /////유효성 검사 함수/////

        //아이디 유효성 검사
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyId(string userId) 
        {
            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var userInfo = db.User.FirstOrDefault(u => u.UserId.Equals(userId)); //입력된 아이디가 가입되어 있는지 체크

                    if (userInfo != null) //가입되어 있으면
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

            //입력된 값이 올바르면
            return Json(true);
        }


        //핸드폰번호 유효성 검사
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyPhone([RegularExpression(@"^\d{2,3}-\d{3,4}-\d{4}$")] string userPhone)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //정규식 형식에 올바르지 않으면
                    return Json("전화번호 형식이 올바르지 않습니다.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Account/Register?msg={e.Message}");
            }

            //입력된 값이 올바르면
            return Json(true);
        }


        //이메일 유효성 검사
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyEmail([RegularExpression(@"^[0-9a-zA-Z]([-_.]?[0-9a-zA-Z])*@[0-9a-zA-Z]([-_.]?[0-9a-zA-Z])*.[a-zA-Z]{2,3}$")] string userEmail)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //정규식 형식에 올바르지 않으면
                    return Json("이메일 형식이 올바르지 않습니다. 예시 : right@right.com");
                }

                using(var db = new BasicboardDbContext())
                {
                    var userInfo = db.User.FirstOrDefault(u => u.UserEmail.Equals(userEmail)); //입력된 이메일이 가입되어 있는지 체크

                    if (userInfo != null) //가입되어 있으면
                    {
                        return Json("해당 이메일은 이미 가입되어있습니다.");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Account/Error?msg={e.Message}");
            }

            //입력된 값이 올바르면
            return Json(true);
        }


        //비밀번호 유효성 검사
        [AcceptVerbs("GET", "POST")]
        public IActionResult VerifyPassword([RegularExpression(@"^.*(?=^.{8,15}$)(?=.*\d)(?=.*[a-zA-Z])(?=.*[!@#$%^&+=]).*$")] string userPassword)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    //정규식 형식에 올바르지 않으면
                    return Json("문자/숫자/특수문자를 포함한 형태의 8~15자리 이내의 암호여야 합니다.");
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Account/Error?msg={e.Message}");
            }

            //입력된 값이 올바르면
            return Json(true);
        }
    }
}
