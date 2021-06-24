using BasicBoard.Data;
using BasicBoard.Models;
using BasicBoard.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace BasicBoard.Controllers
{
    public class BoardController : Controller
    {
             
        public IActionResult Index(Criteria cri) //게시판 리스트
        {
            var USER_LOGIN_KEY = HttpContext.Session.GetInt32("USER_LOGIN_KEY"); //세선에 저장된 userNo 불러오기

            if (USER_LOGIN_KEY == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())
            {
                //전체 게시물 가져오기
                var totalList = from b in db.Board
                                join u in db.User on b.UserNo equals u.UserNo
                                orderby b.BoardNo descending
                                select new BoardIndex
                                {
                                    BoardNo = b.BoardNo,
                                    BoardTitle = b.BoardTitle,
                                    BoardContent = b.BoardContent,
                                    BoardUpdateDate = b.BoardUpdateDate,
                                    BoardViews = b.BoardViews,
                                    UserName = u.UserName
                                };

                int total = totalList.Count();//전체 게시물 갯수

                if (!String.IsNullOrEmpty(cri.searchString) && !String.IsNullOrEmpty(cri.category))
                {
                    //검색키워드의 앞뒤 공백 제거
                    string searchString = cri.searchString.Trim();

                    switch (cri.category)
                    {
                        case "BoardTitle":
                            totalList = totalList.Where(s => s.BoardTitle.Contains(searchString)); //해당 키워드가 포함된 게시물 리스트
                            total = totalList.Count(); //검색된 리스트 갯수
                            break;

                        case "BoardContent":
                            totalList = totalList.Where(s => s.BoardContent.Contains(searchString)); //해당 키워드가 포함된 게시물 리스트
                            total = totalList.Count();
                            break;

                        case "UserName":
                            totalList = totalList.Where(s => s.UserName.Contains(searchString));
                            total = totalList.Count();
                            break;
                    }
                }

                ViewData["pageMaker"] = new PageDTO(cri, total);

                var list = totalList.Skip((cri.pageNum - 1) * cri.amount).Take(cri.amount).ToList(); //페이징

                return View(list);
            }

        }


        public IActionResult Detail(Criteria cri, int boardNo) //게시판 상세
        {
            var USER_LOGIN_KEY = HttpContext.Session.GetInt32("USER_LOGIN_KEY"); //세선에 저장된 userNo 불러오기
            string cookieResult = USER_LOGIN_KEY + "_" + boardNo; //userNo+boardNo 고유 값 생성 


            if (USER_LOGIN_KEY == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())
            {
                var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //게시물 불러오기

                
                //쿠키읽기
                string cookieValue = Request.Cookies["visit"];

                if(string.IsNullOrEmpty(cookieValue)) //쿠키값이 없으면
                {

                    //쿠키 생성 + 조회수 증가
                    CookieOptions options = new CookieOptions();
                    Response.Cookies.Append("visit", cookieResult);

                    board.BoardViews = board.BoardViews + 1; //조회수 count
                    db.Update(board); // 조회수 db 업데이트
                    db.SaveChanges(); //commit
                }
                else //쿠키값이 있으면
                {
                    if (!cookieValue.Contains(cookieResult)) //불러온 쿠키에 고유값 있는지 체크
                    {
                        //쿠키 생성 + 조회수 증가
                        CookieOptions options = new CookieOptions();
                        Response.Cookies.Append("visit", cookieResult);

                        board.BoardViews = board.BoardViews + 1; //조회수 count
                        db.Update(board); // 조회수 db 업데이트
                        db.SaveChanges(); //commit
                    }
                }

                ViewData["cri"] = cri;
                return View(board);
            }
        }

        public IActionResult Add() //게시물 추가
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            return View();
        }

        [HttpPost]
        public IActionResult Add(Board model) //게시물 추가
        {

            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            model.UserNo = int.Parse(HttpContext.Session.GetInt32("USER_LOGIN_KEY").ToString()); //세션에 저장된 UserNo가져오기(로그인 정보)

            if (ModelState.IsValid)
            {
                using (var db = new BasicboardDbContext())
                {
                    db.Board.Add(model);
                    var result = db.SaveChanges(); //Commit //성공한 갯수를 받는다.

                    if (result > 0)
                    {
                        var insertedRow = (from b in db.Board
                                           orderby b.BoardNo descending
                                           select b).FirstOrDefault();

                        TempData["success"] = insertedRow.BoardNo; //다른 요청에서 읽혀질때까지만 데이터를 저장(쿠키 또는 세선 상태를 사용해서 구현)
                        return Redirect("Index"); //RedirectToAction은 컨트롤러의 입력 유무(Index가 입력된 컨트롤러 호출)
                    }
                }

                ModelState.AddModelError(string.Empty, "게시물을 저장할 수 없습니다.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(Criteria cri, int boardNo) //게시물 수정
        {

            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())
            {
                var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //게시물 불러오기
                ViewData["cri"] = cri;
                return View(board);
            }
        }

        [HttpPost]
        public IActionResult Edit(Criteria cri, int boardNo, Board model) //게시물 수정
        {

            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            if (ModelState.IsValid)
            {

                using (var db = new BasicboardDbContext())
                {
                    var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //수정하려는 게시물 정보 DB에서 가져오기

                    board.BoardTitle = model.BoardTitle; //제목 수정
                    board.BoardContent = model.BoardContent; //내용 수정

                    db.Update(board);
                    var result = db.SaveChanges();

                    if (result > 0)
                    {
                        TempData["success"] = boardNo;
                        return RedirectToAction("Index", cri);
                    }
                }
                ModelState.AddModelError(string.Empty, "게시물을 저장할 수 없습니다.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Delete(Criteria cri, int boardNo) //게시물 삭제
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())
            {
                var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //삭제하려는 게시물 정보 DB에서 가져오기
                ViewData["cri"] = cri;
                return View(board);
            }
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(Criteria cri, int boardNo) //게시물 삭제
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var transaction = db.Database.BeginTransaction();

                    var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //삭제하려는 게시물 정보 DB에서 가져오기
                    var replyList = db.Reply.Where(r => r.BoardNo.Equals(boardNo)).ToList(); //삭제하려는 게시물의 댓글 리스트

                    if (board != null)
                    {
                        //댓글 삭제
                        foreach(var r in replyList)
                        {
                            db.Remove(r);
                        }

                        //게시물 삭제
                        db.Remove(board);
                    }

                    var result = db.SaveChanges();

                    if (result > 0)
                    {
                        transaction.Commit(); //트랜잭션

                        TempData["success"] = boardNo;
                        ViewData["cri"] = cri;
                        return RedirectToAction("Index", cri);
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

    }
}
