namespace WPR.Utils;

/// <summary>
/// Checked om te kijken of het een correct telefoon nummer is
/// </summary>
public static class TelChecker
{
    
    public static bool IsValidPhoneNumber(string phoneNumber)
    {
        // Kijkt of het nummer 10 cijfers is, begint met 06 en alles in het nummer een cijfer is.
        if (phoneNumber.Length == 10 && phoneNumber.StartsWith("06") && phoneNumber.All(char.IsDigit))
        {
            return true;
        }
        return false;
    }
}