using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BasicBoard.Models
{
    public class Reply
    {

        [Key] //댓글번호 PK
        public int ReplyNo { get; set; }
        
        [Required(ErrorMessage ="댓글을 입력해주세요.")] //댓글내용
        public string ReplyContent { get; set; }
        
        [Timestamp] //댓글작성일자
        public DateTime ReplyRegDt { get; set; }

        [Timestamp] //댓글 수정일자
        public DateTime ReplyUptDt { get; set; }

        public int BoardNo { get; set; } //게시물 번호
        public int UserId { get; set; } //사용자 아이디

        [ForeignKey("BoardNo")] //외래키 게시물 번호
        public virtual Board Board { get; set; }

        [ForeignKey("UserId")] //외래키 사용자 아이디
        public virtual User User { get; set; }
    }
}
