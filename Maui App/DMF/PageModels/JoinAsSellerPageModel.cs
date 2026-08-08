using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DMF.Constants;
using DMF.DTOs.Dealer;
using DMF.Services.Interfaces;
using DMF.Utilities;

namespace DMF.PageModels
{
    public partial class JoinAsSellerPageModel : ObservableObject
    {
        private readonly IUserDetailService _userDetailService;
        private readonly IDealerService _dealerService;
        private readonly ISecureStorageService _storage;

        private int _userId;

        [ObservableProperty] private string fullName = string.Empty;
        [ObservableProperty] private string primaryMobile = string.Empty;
        [ObservableProperty] private string? email;
        [ObservableProperty] private string? companyName;
        [ObservableProperty] private string? address1;
        [ObservableProperty] private string? state;
        [ObservableProperty] private string? city;
        [ObservableProperty] private string? pincode;
        [ObservableProperty] private bool isBusy;

        public JoinAsSellerPageModel(
            IUserDetailService userDetailService,
            IDealerService dealerService,
            ISecureStorageService storage)
        {
            _userDetailService = userDetailService;
            _dealerService = dealerService;
            _storage = storage;
        }

        // Pre-fill the form with the signed-in user's existing account details so
        // they only have to confirm/edit before requesting dealer access.
        public async Task InitializeAsync()
        {
            var idStr = await _storage.GetAsync(AppConstants.UserId);
            int.TryParse(idStr, out _userId);
            if (_userId <= 0) return;

            var result = await _userDetailService.GetByIdAsync(_userId);
            if (result?.Data == null) return;

            var u = result.Data;
            FullName      = string.Join(" ", new[] { u.FirstName, u.LastName }.Where(s => !string.IsNullOrEmpty(s)));
            PrimaryMobile = u.PrimaryMobile;
            Email         = u.Email;
            CompanyName   = u.CompanyName;
            Address1      = u.Address1;
            State         = u.State;
            City          = u.City;
            Pincode       = u.Pincode;
        }

        [RelayCommand]
        private async Task Submit()
        {
            if (IsBusy) return;

            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(PrimaryMobile))
            {
                await Shell.Current.CurrentPage.DisplayAlert(
                    "Missing details", "Please enter at least your name and mobile number.", "OK");
                return;
            }

            IsBusy = true;
            try
            {
                var dto = new CreateDealerRequestDto
                {
                    UserDetailId  = _userId,
                    FullName      = FullName.Trim(),
                    PrimaryMobile = PrimaryMobile.Trim(),
                    Email         = Email,
                    CompanyName   = CompanyName,
                    Address1      = Address1,
                    City          = City,
                    State         = State,
                    Pincode       = Pincode,
                };

                var response = await _dealerService.SubmitRequestAsync(dto);

                if (response.Success)
                {
                    await Shell.Current.CurrentPage.DisplayAlert(
                        "Request submitted",
                        "Thanks! Your request to become a seller has been submitted for review. Our team will enable dealer access on your account soon.",
                        "OK");
                    await Shell.Current.GoToAsync("..");
                }
                else
                {
                    await Shell.Current.CurrentPage.DisplayAlert(
                        "Something went wrong",
                        response.Message ?? "Could not submit your request. Please try again.", "OK");
                }
            }
            catch
            {
                await Shell.Current.CurrentPage.DisplayAlert(
                    "Connection error",
                    "Could not reach the server. Please check your connection and try again.", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        [RelayCommand]
        private Task Back() => Shell.Current.GoToAsync("..");
    }
}
