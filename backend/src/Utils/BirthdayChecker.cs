namespace WPR.Utils;

public class BirthdayChecker
{
    /// <summary>
    /// Check om te kijken of het een correcte verjaardag is
    /// Leeftijd mag niet onder de 18 zijn en niet boven de 120
    /// </summary>
    /// <param name="birthdate">Birthday van user</param>
    /// <returns>True als birthdate correct is; anders False</returns>
    public static bool IsValidBirthday(DateTime? birthdate)
    {
        if (birthdate == null)
        {
            return false;
        }

        var date = birthdate.Value;

        if (date > DateTime.Now)
        {
            return false;
        }
        
        var age = DateTime.Now.Year - date.Year;

        if (date > DateTime.Now.AddYears(-age))
        {
            age--;
        }

        if (age < 18 || age > 120)
        {
            return false;
        }
        
        return true;
    }
}