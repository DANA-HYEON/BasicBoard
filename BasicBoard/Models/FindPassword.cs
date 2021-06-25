using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBoard.Models
{
    public class FindPassword
    {
        [Remote("VerifyEmail", "Account")]
        [Required(ErrorMessage ="이메일을 입력해주세요.")]
        public string UserEmail { get; set; } //인증번호 받을 이메일

        [Required(ErrorMessage ="인증번호를 입력해주세요.")]
        public string VerifyCode { get; set; } //인증번호

        [Remote("VerifyPassword", "Account")]
        [Required(ErrorMessage = "사용자 비밀번호를 입력하세요.")]
        public string UserPassword { get; set; } //사용자 비밀번호

        [Compare(nameof(UserPassword),ErrorMessage ="비밀번호가 서로 일치하지 않습니다.")]
        [Required(ErrorMessage ="비밀번호를 한번 더 입력해주세요.")]
        public string ConfirmedPassword { get; set; } //새로운 비밀번호 확인
    }
}
