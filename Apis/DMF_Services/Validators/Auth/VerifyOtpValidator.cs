using DMF_Services.DTOs.Auth;
using FluentValidation;

namespace DMF_Services.Validators.Auth
{
    public class VerifyOtpValidator : AbstractValidator<VerifyOtpRequestDto>
    {
        public VerifyOtpValidator()
        {
            RuleFor(x => x.Mobile)
                .NotEmpty()
                .Matches(@"^\+?[0-9]{8,15}$");

            RuleFor(x => x.Otp)
                .Length(4)
                .Matches(@"^[0-9]{4}$");
        }
    }
}
