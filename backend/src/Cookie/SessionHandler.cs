using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace WPR.Cookie;

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
