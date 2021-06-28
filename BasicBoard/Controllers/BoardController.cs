using BasicBoard.Data;
using BasicBoard.Models;
using BasicBoard.ViewModel;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BasicBoard.Controllers
{
    public class BoardController : Controller
    {
             
        public IActionResult Index(Criteria cri) //게시판 리스트
        {
            var USER_LOGIN_KEY = HttpContext.Session.GetInt32("USER_LOGIN_KEY"); //세선에 저장된 userNo 불러오기

            if (USER_LOGIN_KEY == null)
            {
                //로그인이 안된 상태면 로그인 화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            try
            {
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

                    if (!String.IsNullOrEmpty(cri.searchString) && !String.IsNullOrEmpty(cri.category)) //검색카테고리와 검색어가 있다면
                    {
                        //검색키워드의 앞뒤 공백 제거
                        string searchString = cri.searchString.Trim();

                        switch (cri.category)
                        {
                            case "BoardTitle":
                                totalList = totalList.Where(s => s.BoardTitle.Contains(searchString)); //해당 키워드가 게시물 제목에 포함된 게시물 리스트
                                total = totalList.Count(); //검색된 리스트 갯수
                                break;

                            case "BoardContent":
                                totalList = totalList.Where(s => s.BoardContent.Contains(searchString)); //해당 키워드가 게시물 내용에 포함된 게시물 리스트
                                total = totalList.Count(); //검색된 리스트 갯수
                                break;

                            case "UserName":
                                totalList = totalList.Where(s => s.UserName.Contains(searchString)); //해당 키워드가 작성자에 포함된 게시물 리스트
                                total = totalList.Count();//검색된 리스트 갯수
                                break;
                        }
                    }

                    //현재페이지번호, 한페이지에 보여줄 컨텐츠 갯수, 검색 카테고리, 검색 키워드 , 리스트 갯수 데이터 전달
                    ViewData["pageMaker"] = new PageDTO(cri, total); 

                    var list = totalList.Skip((cri.pageNum - 1) * cri.amount).Take(cri.amount).ToList(); //페이징

                    return View(list);

                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }

        }


        public IActionResult Detail(Criteria cri, int boardNo) //게시판 상세
        {
            var USER_LOGIN_KEY = HttpContext.Session.GetInt32("USER_LOGIN_KEY"); //세선에 저장된 userNo 불러오기
            string cookieResult = USER_LOGIN_KEY + "_" + boardNo; //userNo+boardNo 고유 값 생성 


            if (USER_LOGIN_KEY == null)
            {
                //로그인이 안된 상태면 로그인 화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //게시물 불러오기

                
                    //쿠키 값 가져오기
                    string cookieValue = Request.Cookies["visit"];

                    if(string.IsNullOrEmpty(cookieValue)) //visit 쿠키값이 없으면
                    {

                        //쿠키 생성 + 조회수 증가
                        CookieOptions options = new CookieOptions();
                        Response.Cookies.Append("visit", cookieResult);

                        board.BoardViews = board.BoardViews + 1; //조회수 count
                        db.Update(board); // 조회수 db 업데이트
                        db.SaveChanges(); //commit
                    }
                    else //visit 쿠키값이 있으면
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

                    //현재페이지번호, 한페이지에 보여줄 컨텐츠 갯수, 검색 카테고리, 검색 키워드 , 리스트 갯수 데이터 전달
                    ViewData["cri"] = cri; 
                    return View(board);
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }

        }

        public IActionResult Add() //게시물 추가
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태면 로그인 화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            return View();
        }


        [HttpPost]
        public IActionResult Add(Board model) //게시물 추가
        {

            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태면 로그인 화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            model.UserNo = int.Parse(HttpContext.Session.GetInt32("USER_LOGIN_KEY").ToString()); //세션에 저장된 UserNo가져오기(로그인 정보)


            try
            {   //필수 입력값이 모두 있다면
                if (ModelState.IsValid)
                {
                    using (var db = new BasicboardDbContext())
                    {
                        db.Board.Add(model); //게시물 insert
                        var result = db.SaveChanges(); //Commit 후 성공한 갯수를 받는다.

                        if (result > 0) //정상적으로 insert가 되었다면
                        {

                            //db에서 가장 최근에 저장된 게시물 정보를 가져오기
                            var insertedRow = (from b in db.Board
                                               orderby b.BoardNo descending
                                               select b).FirstOrDefault();

                            //가장 최근에 저장된 게시물의 게시물 번호 전달(alert를 위해)
                            TempData["success"] = insertedRow.BoardNo; //다른 요청에서 읽혀질때까지만 데이터를 저장(쿠키 또는 세선 상태를 사용해서 구현)
                            return Redirect("Index"); //RedirectToAction은 컨트롤러의 입력 유무(Index가 입력된 컨트롤러 호출)
                        }
                    }

                    //게시물 저장 실패 시
                    ModelState.AddModelError(string.Empty, "게시물을 저장할 수 없습니다.");
                }

                return View(model);

            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }

        }

        [HttpGet]
        public IActionResult Edit(Criteria cri, int boardNo) //게시물 수정
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태면 로그인 화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //게시물 불러오기

                    //현재페이지번호, 한페이지에 보여줄 컨텐츠 갯수, 검색 카테고리, 검색 키워드 , 리스트 갯수 데이터 전달
                    ViewData["cri"] = cri;
                    return View(board);
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }
        }



        [HttpPost]
        public IActionResult Edit(Criteria cri, int boardNo, Board model) //게시물 수정
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태면 로그인 화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            try
            {
                //필수 입력값이 모두 있다면
                if (ModelState.IsValid)
                {
                    using (var db = new BasicboardDbContext())
                    {
                        var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //수정하려는 게시물 정보 DB에서 가져오기

                        board.BoardTitle = model.BoardTitle; //제목 수정
                        board.BoardContent = model.BoardContent; //내용 수정

                        db.Update(board);  //db update
                        var result = db.SaveChanges(); //commit

                        if (result > 0) //성공적으로 update되었다면
                        {
                            TempData["success"] = boardNo; //alert를 위한 boardno 전달
                            return RedirectToAction("Index", cri);
                        }
                    }

                    //update가 실패하면
                    ModelState.AddModelError(string.Empty, "게시물을 저장할 수 없습니다.");
                }

                return View(model);

            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }
        }



        [HttpGet]
        public IActionResult Delete(Criteria cri, int boardNo) //게시물 삭제
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태면 로그인화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //삭제하려는 게시물 정보 DB에서 가져오기

                    //현재페이지번호, 한페이지에 보여줄 컨텐츠 갯수, 검색 카테고리, 검색 키워드 , 리스트 갯수 데이터 전달
                    ViewData["cri"] = cri;
                    return View(board);
                }

            }
            catch(Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }
        }


        [HttpPost, ActionName("Delete")]
        public IActionResult DeleteConfirmed(Criteria cri, int boardNo) //게시물 삭제
        {
            if (HttpContext.Session.GetInt32("USER_LOGIN_KEY") == null)
            {
                //로그인이 안된 상태면 로그인 화면으로 이동
                return RedirectToAction("Login", "Account");
            }

            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var transaction = db.Database.BeginTransaction(); //트랜잭션 시작

                    var board = db.Board.FirstOrDefault(b => b.BoardNo.Equals(boardNo)); //삭제하려는 게시물 정보 DB에서 가져오기
                    var replyList = db.Reply.Where(r => r.BoardNo.Equals(boardNo)).ToList(); //삭제하려는 게시물의 댓글 리스트 가져오기

                    if (board != null) //게시물 정보가 DB에 있으면
                    {
                        //댓글 모두 삭제
                        foreach(var r in replyList)
                        {
                            db.Remove(r);
                        }

                        //게시물 삭제
                        db.Remove(board);
                    }

                    var result = db.SaveChanges(); //commit

                    if (result > 0) //성공적으로 삭제되었다면
                    {
                        transaction.Commit(); //트랜잭션

                        TempData["success"] = boardNo; //alert를 위한 boardNo 전달
                        return RedirectToAction("Index", cri);
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

    }
}
