using CinemaSystem.ViewModels;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages;
using Microsoft.VisualStudio.Web.CodeGenerators.Mvc.Templates.BlazorIdentity.Pages.Manage;
using System.Threading.Tasks;

namespace CinemaSystem.Areas.Identity.Controllers
{
    [Area(SD.IDENTITY_AREA)]
    public class AccountController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManger;
        private readonly SignInManager<ApplicationUser> _signInManger;
        private readonly IRepository<ApplicationUserOTP> _applicationUserOTPRepository;
        private readonly IEmailSender _emailSender;

        public AccountController(UserManager<ApplicationUser> userManger,
            SignInManager<ApplicationUser> signInManger,
            IEmailSender emailSender,
            IRepository<ApplicationUserOTP> applicationUserOTPRepository)
        {
            _userManger = userManger;
            _signInManger = signInManger;
            _emailSender = emailSender;
            _applicationUserOTPRepository = applicationUserOTPRepository;
        }

        public IActionResult AccessDenied()
        {
            return View();
        }

        public async Task<IActionResult> Logout()
        {
            await _signInManger.SignOutAsync();

            TempData["success-notification"] = "Sign Out Successfully.";

            return RedirectToAction(nameof(Login));
        }


        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> Register(RegisterVM registerVM)
        {
            if (!ModelState.IsValid)
                return View(registerVM);

            var user = new ApplicationUser()
            {
                FirstName = registerVM.FirstName,
                LastName = registerVM.LastName,
                Email = registerVM.Email,
                UserName = registerVM.UserName,
                Address = registerVM.Address
            };


            var result = await _userManger.CreateAsync(user, registerVM.Password);

            if (!result.Succeeded)
            {
                foreach (var item in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, item.Description);
                }
                return View(registerVM);
            }

            //Send Email

            await SendConfirmationMailAsync(user);


            TempData["success-notification"] = "Create Account Successfully , please check your email to verfiy";

            await _userManger.AddToRoleAsync(user, SD.CUSTOMER_ROLE);

            return RedirectToAction(nameof(Login));
        }



        public async Task<IActionResult> Confirm(string token, string userId)
        {
            var user = await _userManger.FindByIdAsync(userId);
            if (user is null) return NotFound();

            var result = await _userManger.ConfirmEmailAsync(user, token);
            if (!result.Succeeded)
            {
                TempData["error-notification"] = string.Join(",",
                    result.Errors.Select(e => e.Description));

                return RedirectToAction(nameof(Login));
            }

            TempData["success-notification"] = "Active Account Successfully";

            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> Login(LoginVM loginVM)
        {
            if (!ModelState.IsValid)
                return View(loginVM);

          var user = await _userManger.FindByEmailAsync(loginVM.UserNameOrEmail) ??
              await  _userManger.FindByNameAsync(loginVM.UserNameOrEmail);

            if (user is null)
            {
                ModelState.AddModelError("UserNameOrEmail", "Email or UserName Incorrect");
                ModelState.AddModelError("Password", "Password Incorrect");

                return View(loginVM);
            }

            var result = await _signInManger.PasswordSignInAsync(user, loginVM.Password ,
                loginVM.RememberMe, true);

            if (!result.Succeeded)
            {
                ModelState.AddModelError("UserNameOrEmail", "Email or UserName Incorrect");
                ModelState.AddModelError("Password", "Password Incorrect");

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Too many attempts , please try again later.");
                }
                return View(loginVM);
            }

            TempData["success-notification"] = $"Welcome Back {user.FirstName} {user.LastName}";


            return RedirectToAction("Index", "Home", new { area = "Customer" });

        }

        [HttpGet]
        public IActionResult ResendEmailConfirmation()
        {
            return View();
        }


        [HttpPost]
        public async Task<IActionResult> ResendEmailConfirmation(ResendEmailConfirmationVM resendEmailConfirmationVM)
        {
            if (!ModelState.IsValid)
                return View(resendEmailConfirmationVM);

            var user = await _userManger.FindByEmailAsync(resendEmailConfirmationVM.EmailOrUserName) ??
            await _userManger.FindByNameAsync(resendEmailConfirmationVM.EmailOrUserName);

            if (user is not null)
            {
                await SendConfirmationMailAsync(user);
            }


            TempData["success-notification"] = "Send Confirmation Mail ,Check Your Mail";
            return RedirectToAction(nameof(Login));
        }

        [HttpGet]
        public IActionResult ForgetPassword()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ForgetPassword(ForgetPasswordVM forgetPasswordVM)
        {
            if (!ModelState.IsValid)
                return View(forgetPasswordVM);

            var user = await _userManger.FindByEmailAsync(forgetPasswordVM.EmailOrUserName) ??
                     await _userManger.FindByNameAsync(forgetPasswordVM.EmailOrUserName);

            if (user is not null)
            {
                await SendOTPMailAsync(user);
            }

            TempData["success-notification"] = "Send OTP Number To Your Mail ,Check Your Mail";

            TempData["userId"] = user.Id;
            return RedirectToAction(nameof(ValidateOTP));
        }

        [HttpGet]
        public IActionResult ValidateOTP()
        {           
            if (TempData.Peek("userId") is null)
                return NotFound();

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ValidateOTP(ValidateOTPVM validateOTPVM)
        {
            if (!ModelState.IsValid)
                return View(validateOTPVM);

            var userId = TempData.Peek("userId");

            var otpInDB = (await _applicationUserOTPRepository.GetAsync(e => e.ApplicationUserId ==
            userId.ToString() && !e.IsUsed))
            .OrderByDescending(e => e.CreateAt).FirstOrDefault();

            if (otpInDB.OTP != validateOTPVM.OTP)
            {
                TempData["error-notification"] = "In Valid or Expire OTP.";
                return View();
            }
            return RedirectToAction(nameof(ChangePassword));

        }

        [HttpGet]
        public IActionResult ChangePassword()
        {
            if (TempData.Peek("userId") is null)
                return NotFound();

            return View();

        }
        [HttpPost]
        public async Task<IActionResult> ChangePassword(ChangePasswordVM changePasswordVM)
        {
            if (!ModelState.IsValid)
                return View(changePasswordVM);

            var user = await _userManger.FindByIdAsync(TempData["userId"].ToString());
            if (user == null) return NotFound();

            var token = await _userManger.GeneratePasswordResetTokenAsync(user);
            var result = await _userManger.ResetPasswordAsync(user, token, changePasswordVM.Password);

            if (!result.Succeeded)
            {
                TempData["error-notification"] = string.Join(",",
                  result.Errors.Select(e => e.Description));

                TempData["userId"] = user.Id;
                return View();
            }
            TempData["success-notification"] = "Change Password Successfully";
            return RedirectToAction(nameof(Login));

        }


        private async Task<bool> SendOTPMailAsync(ApplicationUser user)
        {
            try
            {
                //send confirm

                var otp = new Random().Next(1000, 9999);

                await _emailSender.SendEmailAsync(user.Email, "Reset Password Yor Account",
                    $"<h1>OTP: <b>{otp}</b> .Don't Shar it.</h1>");

                await _applicationUserOTPRepository.CreateAsync(new ApplicationUserOTP()
                {
                    OTP = otp.ToString(),
                    ApplicationUserId = user.Id
                });
                await _applicationUserOTPRepository.CommitAsync();
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }
        private async Task<bool> SendConfirmationMailAsync(ApplicationUser user)
        {
            try
            {
                //send confirm

                var token = await _userManger.GenerateEmailConfirmationTokenAsync(user); //by default  => token valid to 24h

                var link = Url.Action("Confirm", "Account", new
                {
                    area = "Identity",
                    token = token,
                    userId = user.Id
                }, Request.Scheme);


                await _emailSender.SendEmailAsync(user.Email, "Confirmation Yor Account",
                    $"<h1>Confirm Your Account By Clicking</h1><a href='{link}'> here</a> ");

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }


    }
}
