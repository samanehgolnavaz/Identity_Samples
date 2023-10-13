using Identity_Samples.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Security.Claims;

namespace Identity_Samples.Controllers
{
    public class HomeController : Controller
    {
        private readonly UserManager<User> userManager;
        private readonly IUserClaimsPrincipalFactory<User> claimsPrincipalFactory;
        private readonly SignInManager<User> signInManager;   
        public HomeController(UserManager<User> userManager,
            IUserClaimsPrincipalFactory<User> claimsPrincipalFactory,
            SignInManager<User> signInManager)
        {
            this.userManager = userManager;
            this.claimsPrincipalFactory = claimsPrincipalFactory;
            this.signInManager = signInManager;
        }

        public IActionResult Index()
        {
            return View();
        }
        [HttpGet]
        [Authorize]
        public IActionResult About()
        {
            ViewData["Message"] = "Your application description page.";

            return View();
        } 
        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.UserName);
                if (user == null)
                {
                    user = new User
                    {
                        Id = Guid.NewGuid().ToString(),
                        UserName = model.UserName
                    };
                    var result=await userManager.CreateAsync(user,model.Password);
                }
                return View("Success");

            }
            return View();
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View() ;
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByNameAsync(model.UserName);
                if (user != null && await userManager.CheckPasswordAsync(user, model.Password))
                {
                    if (!await userManager.IsEmailConfirmedAsync(user))
                    {
                        ModelState.AddModelError("","Email is not confirmed");
                        return View();
                    }
                    if (await userManager.GetTwoFactorEnabledAsync(user))
                    {
                        var ValidProviders =
                            await userManager.GetValidTwoFactorProvidersAsync(user);
                        if (ValidProviders.Contains("Email"))
                        {
                            var token = await userManager.GenerateTwoFactorTokenAsync(user, "Email");
                            System.IO.File.WriteAllText("email2sv.txt", token);
                            await HttpContext.SignInAsync(IdentityConstants.TwoFactorUserIdScheme,
                                Store2FA(user.Id, "Email"));

                            return RedirectToAction("TwoFactor");
                        }
                    }
                  
                    var principal = await claimsPrincipalFactory.CreateAsync(user);
                    await HttpContext.SignInAsync("Identity.Application", principal);
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Invalid username and password");

            }
            return View();
        }

        private ClaimsPrincipal Store2FA(string userId, string provider)
        {
            var identity = new ClaimsIdentity(new List<Claim>
            { 

                new Claim("sub",userId),
                new Claim("amr",provider)
            },IdentityConstants.TwoFactorUserIdScheme);
            return new ClaimsPrincipal(identity);
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user!= null)
                {
                    var token = await userManager.GeneratePasswordResetTokenAsync(user);
                    var resetUrl = Url.Action("ResetPassword","Home",
                      new {token=token,email=user.Email},Request.Scheme);
                    System.IO.File.WriteAllText("resetLink.txt",resetUrl);
                }
                else
                {
                    // email user and inform them that they do not have an account
                }
                return View("Success");
            }
            return View();
        }
        [HttpGet]
        public IActionResult ResetPassword(string token,string email)
        {
            return View(new ResetPasswordModel { Token=token,Email=email});
        }
        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordModel model)
        {
            if (!ModelState.IsValid)
            {
                var user = await userManager.FindByEmailAsync(model.Email);
                if (user!= null) 
                {
                    var result = await userManager.ResetPasswordAsync(user, model.Token, model.Password);
                    if (!result.Succeeded)
                    {
                        foreach (var error in result.Errors)
                        {
                            ModelState.AddModelError("", error.Description);
                        }
                        return View();
                    }
                    return View("Success");
                }
                ModelState.AddModelError("","Invalid Request");
            }
            return View();
        }

        [HttpGet]
        public IActionResult TwoFactor()
        {
            return View();
        }
        [HttpPost]
        public async Task<IActionResult> TwoFactor(TwoFactorModel model)
        {
            var result = await HttpContext.AuthenticateAsync(IdentityConstants.TwoFactorUserIdScheme);
            if (!result.Succeeded)
            {
                ModelState.AddModelError("","You login request has expired,please start over");
                return View();
            }
            //sun =subject=name identify
            if (ModelState.IsValid)
            {
                var user = await userManager.FindByIdAsync(result.Principal.FindFirstValue("sub"));
                if (user!= null) 
                {
                    var isValid = await userManager.VerifyTwoFactorTokenAsync(user,
                    result.Principal.FindFirstValue("amr"), model.Token);

                    if (isValid)
                    {
                        await HttpContext.SignOutAsync(IdentityConstants.TwoFactorUserIdScheme);
                        var claimsPrincipal=await claimsPrincipalFactory.CreateAsync(user);
                        await HttpContext.SignInAsync(IdentityConstants.ApplicationScheme, claimsPrincipal);
                        return RedirectToAction("Index");
                    }
                    ModelState.AddModelError("", "Invalid token");
                    return View();
                }
                ModelState.AddModelError("","Invalid Request");
            }
            return View();
        }
    }
}