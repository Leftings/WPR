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
        responseCookies.Append(cookieName, null, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.Now.AddMinutes(-1)
        });
    }

    // Dispose pattern implementation
    protected virtual void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                // Dispose of managed resources (if any)
            }

            // Dispose of unmanaged resources (if any)

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        // Do not change this code. Put cleanup code in the Dispose(bool disposing) method.
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
