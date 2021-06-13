using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BasicBoard.Models
{
    public class Board
    {

        [Key] //PK 설정
        public int BoardNo { get; set; } //게시물 번호

        [Required]
        public string BoardTitle { get; set; }  //게시물 제목

        [Required]
        public string BoardContent { get; set; } //게시물 내용

        [DataType(DataType.Date)]
        [Required]
        public DateTime BoardUpdateDate { get; set; } //게시물 등록날짜

        [Required]
        public int BoardViews { get; set; } //게시물 조회수

        [Required]
        public int UserNo { get; set; } //작성자 번호

        [ForeignKey("UserNo")] //외래키
        public virtual User User { get; set; } //Join, lazy loading을 위한 virtual, 
    }
}
