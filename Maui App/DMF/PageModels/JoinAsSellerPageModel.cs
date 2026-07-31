using CommunityToolkit.Maui.Alerts;
using CommunityToolkit.Maui.Core;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel.Communication;

namespace DMF.PageModels
{
    public partial class JoinAsSellerPageModel : ObservableObject
    {
        // Dealer-request inbox shown on screen and used for the email.
        // TODO: replace with the real address once finalised.
        public string DealerEmail => "dealer@dmfmotors.com";

        [RelayCommand]
        private async Task CopyEmail()
        {
            await Clipboard.Default.SetTextAsync(DealerEmail);
            await Toast.Make("Email copied to clipboard", ToastDuration.Short).Show();
        }

        [RelayCommand]
        private async Task RequestDealer()
        {
            var message = new EmailMessage
            {
                Subject = "Request to become a Dealer — DMF Motors",
                Body = "Hello DMF Motors team,\n\n" +
                       "I would like to request dealer access on my account so I can list cars.\n\n" +
                       "Name: \n" +
                       "Registered mobile: \n\n" +
                       "Thank you.",
                To = new List<string> { DealerEmail }
            };

            try
            {
                if (Email.Default.IsComposeSupported)
                    await Email.Default.ComposeAsync(message);
                else
                    await Launcher.Default.OpenAsync(
                        new Uri($"mailto:{DealerEmail}?subject={Uri.EscapeDataString(message.Subject)}"));
            }
            catch
            {
                // No email app configured — copy the address so the user can still use it.
                await Clipboard.Default.SetTextAsync(DealerEmail);
                await Toast.Make("No email app found. Address copied instead.", ToastDuration.Long).Show();
            }
        }

        [RelayCommand]
        private Task Back() => Shell.Current.GoToAsync("..");
    }
}
