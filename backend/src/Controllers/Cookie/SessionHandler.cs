namespace WPR.Controllers.Cookie;

public class SessionHandler : IDisposable
{
    public void CreateCookie(IResponseCookies responseCookies, string cookieName, string cookieValue)
    {
        responseCookies.Append(cookieName, cookieValue, new CookieOptions
        {
            HttpOnly = true, // Cookies zijn alleen toegankelijk voor de server
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

    public void Dispose()
    {
        throw new NotImplementedException();
    }
}
