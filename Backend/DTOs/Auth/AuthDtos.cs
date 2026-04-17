using FluentValidation;

namespace AIStudyPlanner.Api.DTOs.Auth;

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public UserProfileResponse User { get; set; } = new();
}

public class UserProfileResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateTime? LastLoginAtUtc { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class RegisterRequestValidator : FluentValidation.AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FullName).NotEmpty().MaximumLength(120);
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(x => x.PhoneNumber)
            .NotEmpty()
            .Matches("^\\+?[1-9]\\d{7,14}$")
            .WithMessage("Phone number should be in international format.");
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8);
    }
}

public class LoginRequestValidator : FluentValidation.AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Password).NotEmpty();
    }
}
