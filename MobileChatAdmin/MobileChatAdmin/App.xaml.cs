using MobileChatAdmin.View;
using System;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileChatAdmin
{
    public partial class App : Application
    {
        public App()
        {
            InitializeComponent();

            MainPage = new NavigationPage(new LoginPage());
        }
    }
}
