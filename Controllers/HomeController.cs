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
                    var principal = await claimsPrincipalFactory.CreateAsync(user);
                    await HttpContext.SignInAsync("Identity.Application", principal);
                    return RedirectToAction("Index");
                }

                ModelState.AddModelError("", "Invalid username and password");

            }
            return View();
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
    }
}