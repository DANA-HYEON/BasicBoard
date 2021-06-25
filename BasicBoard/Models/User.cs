using BasicBoard.Data;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;

namespace BasicBoard.Models
{
    public class User
    {

        [Key] //PK설정
        public int UserNo { get;set; } //사용자 번호


        [Required(ErrorMessage ="사용자 이름을 입력하세요.")] //Not Null설정
        public string UserName { get; set; } //사용자 이름

        [Remote("VerifyId","Account")]
        [StringLength(8, ErrorMessage = "아이디는 최소 {2}자리 이상 {1}자리 사이어야합니다.", MinimumLength = 4)]
        [Required(ErrorMessage = "사용자 ID를 입력하세요.")]
        public string UserId { get; set; } //사용자 아이디


        [Remote("VerifyPassword", "Account")]
        [Required(ErrorMessage = "사용자 비밀번호를 입력하세요.")]
        public string UserPassword { get; set; } //사용자 비밀번호


        [Remote("VerifyEmail", "Account")]
        [Required(ErrorMessage = "사용자 이메일을 입력하세요.")]
        public string UserEmail { get; set; } //사용자 이메일


        [Remote("VerifyPhone", "Account")]
        [Required(ErrorMessage = "사용자 전화번호를 입력하세요.")]
        public string UserPhone { get; set; } //사용자 전화번호

    }
}
