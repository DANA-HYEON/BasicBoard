using MailKit.Security;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace BasicBoard.Services
{
    public class MailKitEmailSenderOptions
    {
       public MailKitEmailSenderOptions()
        {
            Host_SecureSocketOptions = SecureSocketOptions.Auto;
        }

        public string Host_Address { get; set; }

        public int Host_Port { get; set; }

        public string Host_UserName { get; set; }

        public string Host_Password { get; set; }

        public SecureSocketOptions Host_SecureSocketOptions { get; set; } //연결에 사용해야하는 SSL 또는 TLS 암호화를 지정하는 방법을 제공

        public string Sender_Email { get; set; }

        public string Sender_Name { get; set; }
    }
}
