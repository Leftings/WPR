using System.Text.RegularExpressions;

namespace WPR.Utils
{
    public static class EmailChecker
    {
            public static bool IsValidEmail(string email)
            {
                if (string.IsNullOrEmpty(email))
                    return false;

                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

                return Regex.IsMatch(email, emailPattern);
            }
        }
}