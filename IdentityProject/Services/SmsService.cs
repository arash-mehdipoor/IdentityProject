using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace IdentityProject.Services
{
    public static class SmsService
    {
        public static void Send(string PhoneNumber, string Code)
        {
            var client = new WebClient();
            string url = $"http://panel.kavenegar.com/v1/apikey/verify/lookup.json?receptor={PhoneNumber}&token={Code}&template=VerifyAccount";
            var content = client.DownloadString(url);
        }
    }
}
