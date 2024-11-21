namespace WPR.Cookie;

public class SessionHandler
{
    public void CreateCookie(IResponseCookies responseCookies, string cookieName, string cookieValue)
    {
        responseCookies.Append(cookieName, cookieValue, new CookieOptions
        {
            Expires = DateTimeOffset.Now.AddMinutes(30) 
        });
    }
}
