namespace WPR.Utils;

public class StrongPasswordMaker()
{
    private static string lower = "abcdefghijklmnopqrstuvwxyz";
    private static string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
    private static string digits = "1234567890";
    private static string special = "!@#$%^&*()<>?/.,-=+_{}[]|`~";
    private static readonly string allChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()<>?/.,-=+_{}[]|~";

    public static string CreatePassword()
    {
        Random random = new Random();

        char[] password = new char[12];
        password[0] = lower[random.Next(lower.Length)];
        password[1] = upper[random.Next(upper.Length)];
        password[2] = digits[random.Next(digits.Length)];
        password[3] = special[random.Next(special.Length)];

        for (int i = 4; i < 12; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        return new string(password.OrderBy(x => random.Next()).ToArray());
    }
}