using DMF_Services.DTOs.UserDetails;
using FluentValidation;

namespace DMF_Services.Validators
{
    public class CreateUserDetailValidator : AbstractValidator<CreateUserDetailDto>
    {
        public CreateUserDetailValidator()
        {
            // ---------- First Name ----------
            RuleFor(x => x.FirstName)
                .NotEmpty()
                .MaximumLength(100);

            // ---------- Middle Name (Optional) ----------
            RuleFor(x => x.MidleName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.MidleName));

            // ---------- Last Name (Optional) ----------
            RuleFor(x => x.LastName)
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.LastName));

            // ---------- Primary Mobile ----------
            RuleFor(x => x.PrimaryMobile)
                .NotEmpty()
                .MaximumLength(20)
                .Matches(@"^\+?[0-9]{8,15}$")
                .WithMessage("Invalid primary mobile number");

            // ---------- Secondary Mobile (Optional) ----------
            RuleFor(x => x.SecondaryMobile)
                .MaximumLength(20)
                .Matches(@"^\+?[0-9]{8,15}$")
                .When(x => !string.IsNullOrWhiteSpace(x.SecondaryMobile))
                .WithMessage("Invalid secondary mobile number");

            // ---------- Email (Optional) ----------
            RuleFor(x => x.Email)
                .EmailAddress()
                .MaximumLength(100)
                .When(x => !string.IsNullOrWhiteSpace(x.Email));

            // ---------- Company ----------
            RuleFor(x => x.CompanyName)
                .MaximumLength(500);

            // ---------- Address ----------
            RuleFor(x => x.Address1)
                .MaximumLength(100);

            RuleFor(x => x.Address2)
                .MaximumLength(100);

            RuleFor(x => x.City)
                .MaximumLength(100);

            RuleFor(x => x.District)
                .MaximumLength(100);

            RuleFor(x => x.State)
                .MaximumLength(100);

            RuleFor(x => x.Pincode)
                .Matches(@"^[0-9]{5,6}$")
                .When(x => !string.IsNullOrWhiteSpace(x.Pincode))
                .WithMessage("Invalid pincode");

            // ---------- Profile ----------
            RuleFor(x => x.ProfileImage)
                .NotEmpty()
                .MaximumLength(255);

            // ---------- Role ----------
            RuleFor(x => x.IsDealers)
                .NotNull();

            // ---------- Services ----------
            RuleFor(x => x)
            .Must(x =>
                !x.IsDealers ||              // Normal user → OK
                x.LoanService == true ||
                x.RegistrationService == true ||
                x.NocService == true)
            .WithMessage("At least one service must be selected for dealers");
        }
    }
}
