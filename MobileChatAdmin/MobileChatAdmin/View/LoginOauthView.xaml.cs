using SendBirdAPI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileChatAdmin.View
{
    public interface IBaseUrl { string Get(); }

    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginOauthView : ContentPage
    {
        const string AppId = "2515BDA8-9D3A-47CF-9325-330BC37ADA13";
        public WebView oVw { set; get; }
        public string UID { set; get; }
        public string Token { set; get; }
        public Chat oChat { set; get; }
        public bool LoggedIn { set; get; }
        public LoginOauthView(string WebSiteURL)
        {
            InitializeComponent();
            LoggedIn = false;
            oVw = this.FindByName<WebView>("WebviewMain");
            oVw.Source = WebSiteURL;

        }
        private void SetLoader()
        {
            oVw.Source = "file:///android_asset/Loader.html";
        }
        private async void WebviewMain_Navigated(object sender, WebNavigatedEventArgs e)
        {
            if (e.Url == "https://s.reddit.com/api/v1/sendbird/me")
            {
                var sRet = GetJsonFromJSRetu(await oVw.EvaluateJavaScriptAsync("document.body.innerHTML"));
                var jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<SbTokenResponse>(sRet);
                Token = jRet.sb_access_token;
                oVw.Source = "https://www.reddit.com/user/me/about.json";
            }
            else if (e.Url.StartsWith("https://www.reddit.com/user/"))
            {
                var sRet = GetJsonFromJSRetu(await oVw.EvaluateJavaScriptAsync("document.body.innerHTML"));
                var jRet = Newtonsoft.Json.JsonConvert.DeserializeObject<About>(sRet);
                SetLoader();
                UID = jRet.data.id;
                try
                {
                    oChat = new Chat(Token, AppId, UID);
                    await oChat.Connect();
                    LoggedIn = true;
                    var frmMain = new Main(oChat);
                    await Navigation.PushModalAsync(new NavigationPage(frmMain));
                }
                catch (Exception err)
                {
                    await DisplayAlert("Alert", err.Message, "OK");
                    LoggedIn = false;
                    await Navigation.PopAsync();
                }
            }
            else
            {
                await Navigation.PopAsync();
            }
        }

        private void WebviewMain_Navigating(object sender, WebNavigatingEventArgs e)
        {
            if (e.Url == "https://www.reddit.com/")
            {
                oVw.Source = "https://s.reddit.com/api/v1/sendbird/me";
            }
        }
        private string GetJsonFromJSRetu(string sIn)
        {
            int startat = sIn.IndexOf('{');
            return sIn.Substring(startat, sIn.Length - startat - (sIn.Length - sIn.LastIndexOf('}')) + 1).Replace("\\", "");
        }
    }
}