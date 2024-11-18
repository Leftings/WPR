namespace WPR;

using WPR.Data;
using WPR.Database;

public class Program
{
    public static void Main(string[] args)
    {
        AppConfigure.InitDatabase();

        var app = AppConfigure.ConfigureApplication(args);
        app.Run();
    }
}