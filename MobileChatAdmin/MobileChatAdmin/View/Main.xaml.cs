using SendBirdAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

using Xamarin.Forms;
using Xamarin.Forms.Xaml;

namespace MobileChatAdmin.View
{
    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class Main : ContentPage
    {
        public Chat oChat { set; get; }
        public Main(Chat mChat)
        {
            InitializeComponent();
            oChat = mChat;
            cmbChannel.ItemsSource = oChat.Channels.channels.Where(x => x.custom_type.StartsWith("subreddit") && x.my_role == "operator").Select(x => new PickerItem(x)).ToList();
            if (cmbChannel.Items.Any())
            {
                cmbChannel.SelectedIndex = 0;
            }
            else
            {
                DisplayAlert("Alert", "No sos moderador de ningun chat!", "OK");
                Navigation.PopAsync();
            }
        }

        private async void cmdBan_Clicked(object sender, EventArgs e)
        {
            await VerifyBanMute(true);
        }
        private async Task VerifyBanMute(bool Ban)
        {
            var Time = await VerifyBanMuteFields();
            if (Time.HasValue && await Verifyuser(null))
            {
                await DisplayAlert("Alert", oChat.BanMute(Ban, Time.Value, ""), "OK");
                ClearFields();
            }
        }

        private async void cmdMute_Clicked(object sender, EventArgs e)
        {
            await VerifyBanMute(false);
        }
        private async Task<bool> Verifyuser(bool? Ban)
        {
            if (string.IsNullOrWhiteSpace(txtUser.Text))
            {
                await DisplayAlert("Alert", "Selecciona un usuario", "OK");
                txtUser.Focus();
                return false;
            }
            else
            {
                List<string> lstResults;
                switch (Ban)
                {
                    case true:
                        lstResults = oChat.FindBannedUser(txtUser.Text);
                        break;
                    case false:
                        lstResults = oChat.FindMutedUser(txtUser.Text);
                        break;
                    default:
                        lstResults = new List<string>();
                        lstResults.Add(oChat.GetUser(txtUser.Text));
                        break;
                }
                return lstResults.Any();
            }
        }
        private async void cmdUnBan_Clicked(object sender, EventArgs e)
        {
            await UnbanMute(true);
        }

        private async void cmdUnMute_Clicked(object sender, EventArgs e)
        {
            await UnbanMute(false);
        }
        private async Task UnbanMute(bool Ban)
        {
            if (await Verifyuser(Ban))
            {
                await DisplayAlert("Alert", oChat.UnBanMute(Ban), "OK");
                ClearFields();
            }
        }
        private async Task<long?> VerifyBanMuteFields()
        {
            if (long.TryParse(txtTime.Text, out long lTime) || cmbTime.SelectedIndex == 7)
            {
                switch (cmbTime.SelectedIndex)
                {
                    case 0://<x:String>Segundos</x:String>
                        return lTime;
                    case 1://<x:String>Minutos</x:String>
                        return lTime * 60;
                    case 2://<x:String>Horas</x:String>
                        return lTime * 3600;
                    case 3://<x:String>Dias</x:String>
                        return lTime * 86400;
                    case 4://<x:String>Semanas</x:String>
                        return lTime * 604800;
                    case 5://<x:String>Meses</x:String>
                        if (int.TryParse(lTime.ToString(), out int iTime))
                            return ((long)(DateTime.Now.AddMonths(iTime) - DateTime.Now).TotalSeconds);
                        else
                            await DisplayAlert("Alert", "Valor incorrecto para meses, eleji un numero mas chico", "OK");
                        break;
                    case 6://<x:String>Años</x:String>
                        if (int.TryParse(lTime.ToString(), out int iTimey))
                            return ((long)(DateTime.Now.AddYears(iTimey) - DateTime.Now).TotalSeconds);
                        else
                            await DisplayAlert("Alert", "Valor incorrecto para años, eleji un numero mas chico", "OK");
                        break;
                    case 7://<x:String>Permanente</x:String>
                        return -1;
                    default:
                        cmbTime.Focus();
                        await DisplayAlert("Alert", "Selecciona un valor en el combo", "OK");
                        break;
                }
            }
            else
            {
                txtTime.Focus();
                await DisplayAlert("Alert", "Ingresa un numero en el campo de texto", "OK");
            }
            return null;
        }

        private void cmbChannel_SelectedIndexChanged(object sender, EventArgs e)
        {
            oChat.channel_url = ((PickerItem)cmbChannel.SelectedItem).URL;
        }
        public void ClearFields()
        {
            txtTime.Text = "";
            txtUser.Text = "";
            cmbTime.SelectedIndex = -1;
        }
        private void cmdLogout_Clicked(object sender, EventArgs e)
        {
            var LoginAuth = new LoginOauthView(@"https://old.reddit.com/", Mode.Logout);
            Navigation.PushModalAsync(new NavigationPage(LoginAuth));
        }
    }
    public class PickerItem
    {
        public PickerItem(ChannelInfo.MainChannel oRaw)
        {
            Name = oRaw.name;
            URL = oRaw.channel_url;
        }
        public string Name { set; get; }
        public string URL { set; get; }
        public override string ToString()
        {
            return Name;
        }
    }
}