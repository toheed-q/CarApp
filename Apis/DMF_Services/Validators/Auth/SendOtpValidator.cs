using DMF_Services.DTOs.Auth;
using FluentValidation;

namespace DMF_Services.Validators.Auth
{
    public class SendOtpValidator : AbstractValidator<SendOtpRequestDto>
    {
        public SendOtpValidator()
        {
            RuleFor(x => x.Mobile)
                .NotEmpty()
                .Matches(@"^\+?[0-9]{8,15}$");
        }
    }
}
