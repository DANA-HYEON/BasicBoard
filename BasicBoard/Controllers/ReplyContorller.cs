using BasicBoard.Data;
using BasicBoard.Models;
using BasicBoard.ViewModel;
using Microsoft.AspNetCore.Mvc;
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
            using (var db = new BasicboardDbContext())
            {
                //var reply = db.Reply.ToList().Where(r => r.BoardNo.Equals(bno));
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


        //댓글 추가
        [HttpPost, Route("new")]
        public IActionResult Add([FromBody] Reply model)
        {

            if (ModelState.IsValid) 
            {
                using (var db = new BasicboardDbContext())
                {
                    db.Add(model);
                    int result = db.SaveChanges();

                    if(result > 0) 
                    {
                        return Ok("success");
                    }
                }
            }

            return NotFound();
        }


        //댓글 읽기
        [HttpGet, Route("{rno:int}")]
        public IActionResult Get(int rno)
        {
            using (var db = new BasicboardDbContext())
            {
                var reply = db.Reply.FirstOrDefault(r => r.ReplyNo.Equals(rno));
                return Ok(reply);
            }
        }



        //댓글 수정
        [HttpPut, Route("{rno:int}")]
        public IActionResult Modify(int rno, [FromBody]Reply model)
        {

            if (ModelState.IsValid)
            {
                using (var db = new BasicboardDbContext())
                {
                    var reply = db.Reply.FirstOrDefault(r => r.ReplyNo.Equals(rno));

                    if (reply != null)
                    {
                        reply.ReplyContent = model.ReplyContent;

                        db.Update(reply);
                        var result = db.SaveChanges();

                        if (result > 0)
                        {
                            return Ok("success");
                        }
                    }
                }
                ModelState.AddModelError(string.Empty, "댓글을 수정할 수 없습니다.");
            }
            return NotFound();
        }



        //댓글 삭제
        [HttpDelete, Route("{rno:int}")]
        public IActionResult Delete(int rno)
        {
            using (var db = new BasicboardDbContext())
            {
                var reply = db.Reply.FirstOrDefault(r => r.ReplyNo.Equals(rno));

                if (reply != null)
                {
                    db.Remove(reply);

                }

                var result = db.SaveChanges();

                if (result > 0)
                {
                    return Ok("success");
                }
            }

            return NotFound();
        }

    }
}
