using System.ComponentModel.DataAnnotations;

namespace BasicBoard.ViewModel
{
    public class LoginViewModel
    {

        [Required(ErrorMessage = "사용자 ID를 입력하세요.")]
        public string UserId { get; set; }


        [Required(ErrorMessage = "사용자 비밀번호를 입력하세요.")]
        public string UserPassword { get; set; }


        //비밀번호 암호화
        public void ConvertPassword()
        {
            var sha = new System.Security.Cryptography.HMACSHA512();
            sha.Key = System.Text.Encoding.UTF8.GetBytes(this.UserPassword.Length.ToString());

            var hash = sha.ComputeHash(System.Text.Encoding.UTF8.GetBytes(this.UserPassword));

            this.UserPassword = System.Convert.ToBase64String(hash);


        }
    }
}
