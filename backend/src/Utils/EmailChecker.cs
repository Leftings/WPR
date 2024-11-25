using System.Text.RegularExpressions;

namespace WPR.Utils
{
    /// <summary>
    /// Checked het email om te kijken of het volgens het pattern is.
    /// </summary>
    public static class EmailChecker
    {
            public static bool IsValidEmail(string email)
            {
                if (string.IsNullOrEmpty(email))
                    return false;

                // Regular expression pattern voor een standaard correcte email format
                string emailPattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";

                // Gebruik Regex om te checken of de gegeven email matched met het pattern
                return Regex.IsMatch(email, emailPattern);
            }
        }
}