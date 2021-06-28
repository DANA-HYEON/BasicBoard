using BasicBoard.Data;
using BasicBoard.Models;
using BasicBoard.ViewModel;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace BasicBoard.Controllers
{
    [Route("reply")]
    public class ReplyContorller : Controller
    {

        //특정 게시물의 댓글 리스트 가져오기
        [HttpGet, Route("list/{bno:int}")]
        public IActionResult GetList(int bno)
        {
            try
            {
                using (var db = new BasicboardDbContext())
                {
                    //클릭한 게시물의 댓글 리스트 가져오기
                    var reply = from r in db.Reply
                                join u in db.User on r.UserId equals u.UserNo
                                where r.BoardNo == bno
                                orderby r.ReplyRegDt descending
                                select new ReplyIndex
                                {
                                    ReplyNo = r.ReplyNo,
                                    ReplyContent = r.ReplyContent,
                                    ReplyUptDt = r.ReplyUptDt,
                                    UserName = u.UserName,
                                    UserId = r.UserId
                                };

                    return Ok(reply.ToList());
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }
        }


        //댓글 추가
        [HttpPost, Route("new")]
        public IActionResult Add([FromBody] Reply model)
        {
            try
            {
                //필수 입력값이 모두 있다면
                if (ModelState.IsValid)
                {
                    using (var db = new BasicboardDbContext())
                    {
                        db.Add(model); //댓글 insert
                        int result = db.SaveChanges(); //commit

                        if (result > 0) //성공적으로 insert 된다면
                        {
                            return Ok("success"); //200
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }

            //insert 실패시
            return Content("댓글 등록을 실패하였습니다."); //200
        }



        //댓글 수정
        [HttpPut, Route("{rno:int}")]
        public IActionResult Modify(int rno, [FromBody] Reply model)
        {
            try
            {
                //필수 입력값이 모두 있다면
                if (ModelState.IsValid)
                {
                    using (var db = new BasicboardDbContext())
                    {
                        var reply = db.Reply.FirstOrDefault(r => r.ReplyNo.Equals(rno)); //수정하려는 댓글 정보 가져오기

                        //댓글 정보가 있다면
                        if (reply != null)
                        {
                            //댓글 내용 수정
                            reply.ReplyContent = model.ReplyContent;

                            db.Update(reply); //update
                            var result = db.SaveChanges(); //commit

                            //성공적으로 업데이트되었다면
                            if (result > 0)
                            {
                                return Ok("success"); //200
                            }

                            //댓글 업데이트 실패 시
                            ModelState.AddModelError(string.Empty, "댓글을 수정할 수 없습니다.");
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }

            return Content("댓글 수정에 실패하였습니다."); //200
        }



        //댓글 삭제
        [HttpDelete, Route("{rno:int}")]
        public IActionResult Delete(int rno)
        {
            try
            {
                using (var db = new BasicboardDbContext())
                {
                    var reply = db.Reply.FirstOrDefault(r => r.ReplyNo.Equals(rno)); //댓글 정보 가져오기

                    //댓글 정보가 있다면
                    if (reply != null)
                    {
                        //댓글 삭제
                        db.Remove(reply);

                    }

                    var result = db.SaveChanges(); //commit

                    //성공적으로 댓글이 삭제되었다면
                    if (result > 0)
                    {
                        return Ok("success");
                    }
                }

            }
            catch (Exception e)
            {
                Console.WriteLine(e.StackTrace);
                return Redirect($"/Home/Error?msg={e.Message}");
            }

            return Content("댓글 삭제에 실패하였습니다."); //200
        }
    }
}
