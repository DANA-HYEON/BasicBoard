using System;
using System.ComponentModel.DataAnnotations;

namespace BasicBoard.ViewModel
{
    public class BoardIndex
    {

        
        public int BoardNo { get; set; } //게시물 번호
        
        public string BoardTitle { get; set; }  //게시물 제목
        
        public string BoardContent { get; set; } //게시물 내용

        public DateTime BoardUpdateDate { get; set; } //게시물 등록날짜

        public int BoardViews { get; set; } //게시물 조회수

        public string UserName { get; set; } //등록자
    }
}
