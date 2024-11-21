namespace WPR.Utils;

public static class TelChecker
{
    public static bool IsValidPhoneNumber(int phoneNumber)
    {
        string phoneString = phoneNumber.ToString();

        if (phoneString.Length == 9 && phoneString.StartsWith("06") && phoneString.All(char.IsDigit))
        {
            return true;
        }
        return false;
    }
}