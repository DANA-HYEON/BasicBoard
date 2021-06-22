using System;
using System.ComponentModel.DataAnnotations;

namespace BasicBoard.ViewModel
{
    public class ReplyIndex
    {

        //댓글번호 PK
        public int ReplyNo { get; set; }

        //댓글내용
        public string ReplyContent { get; set; }

        //댓글 수정일자
        public DateTime ReplyUptDt { get; set; }

        //댓글 작성자 번호 
        public int UserId { get; set; }

        //댓글 작성자 이름
        public string UserName { get; set; }

    }
}
