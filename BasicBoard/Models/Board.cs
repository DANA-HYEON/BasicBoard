using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BasicBoard.Models
{
    public class Board
    {

        [Key] //PK 설정
        public int BoardNo { get; set; } //게시물 번호

        [Required(ErrorMessage ="제목을 입력해주세요.")]
        public string BoardTitle { get; set; }  //게시물 제목

        [Required(ErrorMessage ="내용을 입력해주세요.")]
        public string BoardContent { get; set; } //게시물 내용

        [Timestamp]
        public DateTime BoardUpdateDate { get; set; } //게시물 등록날짜

        public int BoardViews { get; set; } //게시물 조회수

        [Required]
        public int UserNo { get; set; } //작성자 번호

        [ForeignKey("UserNo")] //외래키
        public virtual User User { get; set; } //Join, lazy loading을 위한 virtual, 
    }
}
