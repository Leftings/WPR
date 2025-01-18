namespace WPR.Utils;

public class StrongPasswordMaker()
{
    const string lower = "abcdefghijklmnopqrstuvwxyz";
    const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    const string digits = "1234567890";
    const string special = "!@#$%^&*()<>?/.,-=+_{}[]|`~";

    public static string CreatePassword()
    {
        Random random = new Random();

        char[] password = new char[12];
        password[0] = lower[random.Next(lower.Length)];
        password[1] = upper[random.Next(upper.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        string allChars = lower + upper + digits + special;

        for (int i = 4; i < 12; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        return new string(password.OrderBy(x => random.Next()).ToArray());
    }
}