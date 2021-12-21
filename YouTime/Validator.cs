using System;
using System.Text.RegularExpressions;

namespace ChatClient
{
    public class Validator
    {
        public static bool ValidateIp(string ipstring)
        {
            Regex regex = new(@"[0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}[.][0-9]{1,3}");
            return regex.IsMatch(ipstring);
        }
    }
}
