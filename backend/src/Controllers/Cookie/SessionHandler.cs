namespace WPR.Controllers.Cookie;

public class SessionHandler : IDisposable
{
    private bool disposedValue;

    public void CreateCookie(IResponseCookies responseCookies, string cookieName, string cookieValue)
    {
        responseCookies.Append(cookieName, cookieValue, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.Now.AddMinutes(30)
        });
    }

    public void CreateInvalidCookie(IResponseCookies responseCookies, string cookieName)
    {
        responseCookies.Append(cookieName, "", new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.Now.AddMinutes(-1),
            MaxAge = TimeSpan.Zero,
            Path = "/"
        });

        Console.WriteLine($"Cookie invalidated: {cookieName}");
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
