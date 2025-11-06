using System.Threading;
using System.Threading.Tasks;

namespace CinemaProject.Services
{
    public interface IAccountService
    {
        Task<(bool Success, string Message)> RegisterAsync(RegisterVM registerVM, string scheme);
        Task<(bool Success, string Message)> ConfirmEmailAsync(string id, string token);
        Task<(bool Success, string Message)> LoginAsync(LoginVM loginVM);
        Task<(bool Success, string Message)> ResendEmailConfirmationAsync(ResendEmailConfirmationVM resendEmailConfirmationVM, string scheme);
        Task<(bool Success, string Message)> ForgetPasswordAsync(ForgetPasswordVM forgetPasswordVM, string scheme, CancellationToken cancellationToken);
        Task<(bool Success, string Message)> ValidateOtpAsync(ValidateOTP validateOTP);
        Task<(bool Success, string Message)> NewPasswordAsync(NewPasswordVM newPasswordVM);
    }
}
