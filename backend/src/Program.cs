namespace WPR;
public class Program
{
    public static void Main(string[] args)
    {
        var app = AppConfigure.ConfigureApplication(args);

        using (var scope = app.Services.CreateScope())
        {
            AppConfigure.InitDatabase(scope.ServiceProvider);
        }
        app.Run();
    }
}