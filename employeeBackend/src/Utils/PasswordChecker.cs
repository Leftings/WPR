using System.Text.RegularExpressions;

namespace Employee.Utils;

/// <summary>
/// Password checker that makes use of Regex(Regular Expression checks)
/// </summary>

public static class PasswordChecker
{
    public static (bool, string) IsValidPassword(string password)
    {
        if (string.IsNullOrEmpty(password))
        {
            return (false, "Password is empty");
        }

        if (password.Length < 8 )
        {
            return (false, "Password must be at least 8 characters.");
        }

        if (!Regex.IsMatch(password, @"[0-9]+"))
        {
            return (false, "Password must contain at least one number.");
        }

        if (!Regex.IsMatch(password, @"[A-Z]+"))
        {
            return (false, "Password must contain at least one upper case letter.");
        }
        
        if (!Regex.IsMatch(password, @"[a-z]+"))
        {
            return (false, "Password must contain at least one lower case letter.");
        }

        if (!Regex.IsMatch(password, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
        {
            return (false, "Password must contain at least one symbol");
        }

        return (true, null);
    }
}