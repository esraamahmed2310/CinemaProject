using CinemaProject.Services;
using CinemaProject.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CinemaProject.Areas.Identity.Controllers
{
    [Area("Identity")]
    public class AccountController : Controller
    {
        private readonly IAccountService _accountService;

        public AccountController(IAccountService accountService)
        {
            _accountService = accountService;
        }

        // ================= REGISTER ==================
        [HttpGet]
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            var (success, message) = await _accountService.RegisterAsync(registerVM, Request.Scheme);

            if (success)
            {
                TempData["success-notification"] = message;
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", message);
            return View(registerVM);
        }

        // ================= CONFIRM EMAIL ==================
        public async Task<IActionResult> ConfirmEmail(string id, string token)
        {
            var (success, message) = await _accountService.ConfirmEmailAsync(id, token);

            TempData[success ? "success-notification" : "error-notification"] = message;
            return RedirectToAction("Index", "Home", new { area = "Customer" });
        }

        // ================= LOGIN ==================
        [HttpGet]
        public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

            var (success, message) = await _accountService.LoginAsync(loginVM);

            if (success)
            {
                TempData["success-notification"] = message;
                return RedirectToAction("Index", "Home", new { area = "Customer" });
            }

            ModelState.AddModelError("", message);
            return View(loginVM);
        }

        public async Task<IActionResult> Logout()
        {
            await _accountService.LogoutAsync();   // استدعاء السيرفيس بدل SignInManager

            TempData["success-notification"] = "Logout Successfully";

            return RedirectToAction("Login");
        }



        // ================= RESEND EMAIL CONFIRMATION ==================
        [HttpGet]
        public IActionResult ResendEmailConfirmation() => View();

        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
                return View(resendEmailConfirmationVM);

            var (success, message) = await _accountService.ResendEmailConfirmationAsync(resendEmailConfirmationVM, Request.Scheme);

            if (success)
            {
                TempData["success-notification"] = message;
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", message);
            return View(resendEmailConfirmationVM);
        }

        // ================= FORGET PASSWORD ==================
        [HttpGet]
        public IActionResult ForgetPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM, CancellationToken cancellationToken)
        {
            if (!ModelState.IsValid)
                return View(forgetPasswordVM);

            var (success, message) = await _accountService.ForgetPasswordAsync(forgetPasswordVM, Request.Scheme, cancellationToken);

            if (success)
            {
                TempData["success-notification"] = message;
                TempData["From-ForgetPassword"] = Guid.NewGuid().ToString();
                return RedirectToAction("ValidateOTP", new { userId = forgetPasswordVM.UserNameOREmail });
            }

            ModelState.AddModelError("", message);
            return View(forgetPasswordVM);
        }

        // ================= VALIDATE OTP ==================
        [HttpGet]
        public IActionResult ValidateOTP(string userId)
        {
            if (TempData["From-ForgetPassword"] is null)
                return NotFound();

            return View(new ValidateOTP { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTP validateOTP)
        {
            if (!ModelState.IsValid)
                return View(validateOTP);

            var (success, message) = await _accountService.ValidateOtpAsync(validateOTP);

            if (success)
            {
                TempData["From-ValidateOTP"] = Guid.NewGuid().ToString();
                TempData["success-notification"] = message;
                return RedirectToAction("NewPassword", new { userId = validateOTP.UserId });
            }

            TempData["error-notification"] = message;
            return RedirectToAction(nameof(ValidateOTP), new { userId = validateOTP.UserId });
        }

        // ================= NEW PASSWORD ==================
        [HttpGet]
        public IActionResult NewPassword(string userId)
        {
            if (TempData["From-ValidateOTP"] is null)
                return NotFound();

            return View(new NewPasswordVM { UserId = userId });
        }

        [HttpPost]
        public async Task<IActionResult> NewPassword(NewPasswordVM newPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(newPasswordVM);

            var (success, message) = await _accountService.NewPasswordAsync(newPasswordVM);

            if (success)
            {
                TempData["success-notification"] = message;
                return RedirectToAction("Login");
            }

            ModelState.AddModelError("", message);
            return View(newPasswordVM);
        }
    }
}
