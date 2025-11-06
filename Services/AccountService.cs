using CinemaProject.Repositories.IRepositories;
using CinemaProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace CinemaProject.Services
{
    public class AccountService : IAccountService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;
        private readonly IEmailSender _emailSender;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;

        public AccountService(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager, IEmailSender emailSender, IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }

        public async Task<(bool Success, string Message)> RegisterAsync(RegisterVM registerVM, string scheme)
        {
            var user = new ApplicationUser
            {
                Name = registerVM.Name,
                Email = registerVM.Email,
                UserName = registerVM.UserName
            };

            var result = await _userManager.CreateAsync(user, registerVM.Password);
            if (!result.Succeeded)
            {
                string errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return (false, errors);
            }

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = $"https://localhost:7045/Identity/Account/ConfirmEmail?id={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(user.Email!, "Confirm your email", $"<a href='{link}'>Click Here</a>");

            return (true, "Email Sent Successfully");
        }

        public async Task<(bool Success, string Message)> ConfirmEmailAsync(string id, string token)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user == null) return (false, "User not found");

            var result = await _userManager.ConfirmEmailAsync(user, token);
            return result.Succeeded ? (true, "Email confirmed successfully") : (false, "Invalid token");
        }

        public async Task<(bool Success, string Message)> LoginAsync(LoginVM loginVM)
        {
            var user = await _userManager.FindByNameAsync(loginVM.UserNameOREmail) ??
                       await _userManager.FindByEmailAsync(loginVM.UserNameOREmail);

            if (user == null) return (false, "Invalid username/email or password");

            var result = await _signInManager.PasswordSignInAsync(user, loginVM.Password, loginVM.RememberMe, lockoutOnFailure: true);

            if (result.Succeeded) return (true, "Login successful");
            if (result.IsLockedOut) return (false, "Account locked out, try later");
            if (result.IsNotAllowed) return (false, "Please confirm your email first");

            return (false, "Invalid credentials");
        }

        public async Task<(bool Success, string Message)> ResendEmailConfirmationAsync(ResendEmailConfirmationVM resendEmailConfirmationVM, string scheme)
        {
            var user = await _userManager.FindByNameAsync(resendEmailConfirmationVM.UserNameOREmail) ??
                       await _userManager.FindByEmailAsync(resendEmailConfirmationVM.UserNameOREmail);

            if (user == null)
                return (false, "Invalid User Name or Email");

            if (user.EmailConfirmed)
                return (false, "Already confirmed");

            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var link = $"https://localhost:7045/Identity/Account/ConfirmEmail?id={user.Id}&token={Uri.EscapeDataString(token)}";

            await _emailSender.SendEmailAsync(user.Email!, "Resend Email Confirmation", $"<a href='{link}'>Click Here</a>");
            return (true, "Email sent successfully");
        }

        public async Task<(bool Success, string Message)> ForgetPasswordAsync(ForgetPasswordVM forgetPasswordVM, string scheme, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByNameAsync(forgetPasswordVM.UserNameOREmail) ??
                       await _userManager.FindByEmailAsync(forgetPasswordVM.UserNameOREmail);

            if (user == null)
                return (false, "Invalid username/email");

            var otp = new Random().Next(1000, 9999).ToString();

            var userOTPs = await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId == user.Id);
            var totalCount = userOTPs.Count(e => (DateTime.UtcNow - e.CreateAt).TotalHours < 24);

            if (totalCount > 5)
                return (false, "Too many attempts, try later");

            await _applicationUserOTPRepository.CreateAsync(new ApplicationUserOTP
            {
                ApplicationUserId = user.Id,
                CreateAt = DateTime.UtcNow,
                IsValid = true,
                Id = Guid.NewGuid().ToString(),
                OTP = otp,
                ValidTo = DateTime.UtcNow.AddMinutes(30)
            }, cancellationToken);

            await _applicationUserOTPRepository.CommitAsync(cancellationToken);

            await _emailSender.SendEmailAsync(user.Email!, "Forget Password", $"<h1>Use this OTP: {otp}</h1>");
            return (true, "OTP sent successfully");
        }

        public async Task<(bool Success, string Message)> ValidateOtpAsync(ValidateOTP validateOTP)
        {
            var validOTP = await _applicationUserOTPRepository.GetOneAsync(e =>
                e.ApplicationUserId == validateOTP.UserId && e.IsValid && e.ValidTo > DateTime.UtcNow);

            return validOTP == null ? (false, "Invalid or expired OTP") : (true, "OTP validated successfully");
        }

        public async Task<(bool Success, string Message)> NewPasswordAsync(NewPasswordVM newPasswordVM)
        {
            var user = await _userManager.FindByIdAsync(newPasswordVM.UserId);
            if (user == null)
                return (false, "User not found");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, newPasswordVM.Password);

            return result.Succeeded ? (true, "Password changed successfully") : (false, "Failed to reset password");
        }
    }
}
