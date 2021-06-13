﻿using System.ComponentModel.DataAnnotations;

namespace BasicBoard.Models
{
    public class User
    {

        [Key] //PK설정
        public int UserNo { get;set; } //사용자 번호

        [Required(ErrorMessage ="사용자 이름을 입력하세요.")] //Not Null설정
        public string UserName { get; set; } //사용자 이름

        [Required(ErrorMessage = "사용자 ID를 입력하세요.")]
        public string UserId { get; set; } //사용자 아이디

        [Required(ErrorMessage = "사용자 비밀번호를 입력하세요.")]
        public string UserPassword { get; set; } //사용자 비밀번호

        [DataType(DataType.EmailAddress)]
        public string UserEmail { get; set; } //사용자 이메일

        [DataType(DataType.PhoneNumber)]
        [Required(ErrorMessage = "사용자 전화번호를 입력하세요.")]
        public string UserPhone { get; set; } //사용자 전화번호
    }
}