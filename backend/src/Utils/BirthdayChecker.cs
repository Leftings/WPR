namespace WPR.Utils;

public class BirthdayChecker
{
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