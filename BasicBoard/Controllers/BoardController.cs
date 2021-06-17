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
using System.Threading.Tasks;

namespace BasicBoard.Controllers
{
    public class BoardController : Controller
    {
        public IActionResult Index(Criteria cri, string category, string searchString) //게시판 리스트
        {

            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())
            {

                var list = (from b in db.Board
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
                            }).Skip((cri.pageNum - 1) * cri.amount).Take(cri.amount); //  Take : 컨텐츠 갯수

                if (!String.IsNullOrEmpty(searchString) && !String.IsNullOrEmpty(category))
                {
                    //검색키워드의 앞뒤 공백 제거
                    searchString = searchString.Trim();

                    switch (category)
                    {
                        case "BoardTitle":
                            list = list.Where(s => s.BoardTitle.Contains(searchString));
                            break;

                        case "BoardContent":
                            list = list.Where(s => s.BoardContent.Contains(searchString));
                            break;

                        case "UserName":
                            list = list.Where(s => s.UserName.Contains(searchString));
                            break;
                    }
                }

                int total = db.Board.Count(); //전체 게시물 갯수

                ViewData["pageMaker"] = new PageDTO(cri, total);
                return View(list.ToList());
            }

        }


        public IActionResult Detail(int boardNo) //게시판 상세
        {

            var USER_LOGIN_KEY = HttpContext.Session.GetInt32("USER_LOGIN_KEY"); //세선에 저장된 userNo 불러오기
            string cookieResult = USER_LOGIN_KEY + "" + boardNo; //userNo+boardNo 고유 값 생성 

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
                if (!cookieValue.Contains(cookieResult)) //불러온 쿠키에 고유값 저장되어있는지 체크
                {
                    //쿠키값이 없으면 쿠키 생성 + 조회수 증가
                    CookieOptions options = new CookieOptions();
                    Response.Cookies.Append("visit", cookieResult);

                    board.BoardViews = board.BoardViews + 1; //조회수 count
                    db.Update(board); // 조회수 db 업데이트
                    db.SaveChanges(); //commit

                }

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
        public IActionResult Edit(int boardNo) //게시물 수정
        {

            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())
            {
                var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //게시물 불러오기
                return View(board);
            }
        }

        [HttpPost]
        public IActionResult Edit(int boardNo, Board model) //게시물 수정
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
                        return Redirect("Index");
                    }
                }
                ModelState.AddModelError(string.Empty, "게시물을 저장할 수 없습니다.");
            }
            return View(model);
        }

        [HttpGet]
        public IActionResult Delete(int boardNo) //게시물 삭제
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())
            {
                var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //삭제하려는 게시물 정보 DB에서 가져오기
                return View(board);
            }
        }


        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int boardNo) //게시물 삭제
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태
                return RedirectToAction("Login", "Account");
            }

            using (var db = new BasicboardDbContext())

            {
                var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //삭제하려는 게시물 정보 DB에서 가져오기
                if (board != null)
                {
                    db.Remove(board);
                }

                var result = db.SaveChanges();

                if (result > 0)
                {
                    TempData["success"] = boardNo;
                    return RedirectToAction("Index", "Board");
                }
            }
            return View();
        }

    }
}
