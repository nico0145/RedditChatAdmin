using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MobileChatAdmin.Model;
using SendBirdAPI;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileChatAdmin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class LoginPage : ContentPage
    {
        const string ConfigFileName = "jsconfig.json";
        const string EncryptionKey = "wUu0Hnuzuj6BnMYIOOW5XuVL9H5WO9SHGWtH0W5A6TdjvFtHFnhzWjvaLce6s3VRKw5jIllRM5c0pJowpGxsf0H54cF8SLgyHSXCiIMGDTzsqMEDikufiSZtHKF1c4DawuK1dd1aW47mX8uCLSh6nzX3japXKH6fwrV1OlQImZiryduiU03QI4FgmJVAuWqvAwVJos2h"; //lul i'm so random!
        public Chat oChat { set; get; }
        public string Code { set; get; }
        public LoginPage()
        {
            InitializeComponent();
        }

        private void Button_Clicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(Code))
            {
                var LoginAuth = new LoginOauthView(@"https://www.reddit.com/login/");
                Navigation.PushModalAsync(new NavigationPage(LoginAuth));
            }
        }
    }
}