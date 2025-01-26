namespace WPR.Utils;

public class StrongPasswordMaker
{
    // Definieer verschillende tekenreeksen voor het genereren van een sterk wachtwoord
    private static string lower = "abcdefghijklmnopqrstuvwxyz"; // Kleine letters
    private static string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ"; // Hoofdletters
    private static string digits = "1234567890"; // Cijfers
    private static string special = "!@#$%^&*()<>?/.,-=+_{}[]|`~"; // Speciale tekens
    private static readonly string allChars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890!@#$%^&*()<>?/.,-=+_{}[]|~"; // Alle mogelijke tekens

    // Methode voor het genereren van een sterk wachtwoord
    public static string CreatePassword()
    {
        Random random = new Random(); // Random object voor willekeurige selectie van tekens

        char[] password = new char[12]; // Array voor het wachtwoord van 12 tekens
        password[0] = lower[random.Next(lower.Length)]; // Voeg een kleine letter toe
        password[1] = upper[random.Next(upper.Length)]; // Voeg een hoofdletter toe
        password[2] = digits[random.Next(digits.Length)]; // Voeg een cijfer toe
        password[3] = special[random.Next(special.Length)]; // Voeg een speciaal teken toe

        // Vul de resterende tekens in met willekeurige karakters uit alle mogelijke tekens
        for (int i = 4; i < 12; i++)
        {
            password[i] = allChars[random.Next(allChars.Length)];
        }

        // Shuffle het wachtwoord zodat de tekens in willekeurige volgorde staan en geef het wachtwoord terug
        return new string(password.OrderBy(x => random.Next()).ToArray());
    }
}