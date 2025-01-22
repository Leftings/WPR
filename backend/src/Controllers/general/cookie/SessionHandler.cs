namespace WPR.Controllers.General.Cookie;

/// <summary>
/// SessionHandler zorgt ervoor dat er cookies aangemaakt kunnen worden op een simpele manier
/// </summary>
public class SessionHandler : IDisposable
{
    private bool disposedValue;

    /// <summary>
    /// Cookie kunnen worden aangemaakt doormiddel van een naam, de waarde en er kunnen opties ingezet worden
    /// </summary>
    /// <param name="responseCookies"></param>
    /// <param name="cookieName"></param>
    /// <param name="cookieValue"></param>
    public void CreateCookie(IResponseCookies responseCookies, string cookieName, string cookieValue)
    {
        responseCookies.Append(cookieName, cookieValue, new CookieOptions
        {
            HttpOnly = true,
            Expires = DateTimeOffset.Now.AddMinutes(30)
        });
    }

    /// <summary>
    /// // Als er een ongeldige cookie is gedetecteerd, wordt deze verwijderd
    /// </summary>
    /// <param name="responseCookies"></param>
    /// <param name="cookieName"></param>
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
