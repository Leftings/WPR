namespace WPR.Controllers.Cookie;

public class SessionHandler
{
    public void CreateCookie(IResponseCookies responseCookies, string cookieName, string cookieValue)
    {
        responseCookies.Append(cookieName, cookieValue, new CookieOptions
        {
            HttpOnly = true, // Cookies zijn alleen toegankelijk voor de server
            Expires = DateTimeOffset.Now.AddMinutes(30) 
        });
    }
}
