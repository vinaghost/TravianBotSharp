namespace MainCore.DTO
{
    public record CookieDto(string Name, string Value, string Domain, string Path, DateTime? Expiry)
    {
        public static CookieDto FromCookie(OpenQA.Selenium.Cookie cookie)
        {
            return new CookieDto(cookie.Name, cookie.Value, cookie.Domain ?? string.Empty, cookie.Path ?? string.Empty, cookie.Expiry);
        }

        public OpenQA.Selenium.Cookie ToCookie()
        {
            return new OpenQA.Selenium.Cookie(Name, Value, Domain, Path, Expiry);
        }
    }
}
