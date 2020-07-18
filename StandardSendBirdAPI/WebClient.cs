using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace SendBirdAPI
{
    public class CookieAwareWebClient : WebClient
    {
        public void PassCookies(WebRequest request)
        {
            foreach (Cookie cookie in ResponseCookies)
            {
                TryAddCookie(request, cookie);
            }
        }
        public static bool TryAddCookie(WebRequest webRequest, Cookie cookie)
        {
            HttpWebRequest httpRequest = webRequest as HttpWebRequest;
            if (httpRequest == null)
            {
                return false;
            }

            if (httpRequest.CookieContainer == null)
            {
                httpRequest.CookieContainer = new CookieContainer();
            }
            httpRequest.CookieContainer.SetCookies(webRequest.RequestUri, cookie.ToString());
            return true;
        }
        public void UpdateToken(string application_id, string session_key)
        {
            if (Headers["Session-Key"] == null)
                Headers.Add("Session-Key", session_key);
        }
        public CookieAwareWebClient()
        {
            CookieContainer = new CookieContainer();
            this.ResponseCookies = new CookieCollection();
        }

        public CookieContainer CookieContainer { get; private set; }
        public CookieCollection ResponseCookies { get; set; }

        protected override WebRequest GetWebRequest(Uri address)
        {

            HttpWebRequest request = base.GetWebRequest(address) as HttpWebRequest;
            request.AutomaticDecompression = DecompressionMethods.Deflate | DecompressionMethods.GZip;
            request.CookieContainer = CookieContainer;
            request.Headers.Add("DNT", "1");
            PassCookies(request);
            return request;
        }

        protected override WebResponse GetWebResponse(WebRequest request)
        {
            try
            {
                var response = (HttpWebResponse)base.GetWebResponse(request);
                this.ResponseCookies = response.Cookies;
                foreach (Cookie c in ResponseCookies)
                {
                    CookieContainer.Add(c);
                }
                return response;
            }
            catch (WebException err)
            {
                Console.WriteLine(err);
                //? new System.IO.StreamReader(err.Response.GetResponseStream(), Encoding.UTF8).ReadToEnd() debug with this
            }
            return null;
        }
    }
}
