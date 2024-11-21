namespace WPR.Utils;

public static class TelChecker
{
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        if (phoneNumber.Length == 10 && phoneNumber.StartsWith("06") && phoneNumber.All(char.IsDigit))
        {
            return true;
        }
        return false;
    }
}