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
            
        public HomeController(UserManager<User> userManager,
            IUserClaimsPrincipalFactory<User> claimsPrincipalFactory)
        {
            this.userManager = userManager;
            this.claimsPrincipalFactory= claimsPrincipalFactory;
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
                if (user!=null && await userManager.CheckPasswordAsync(user,model.Password))
                {
                    var principal=await claimsPrincipalFactory.CreateAsync(user);
                    await HttpContext.SignInAsync("Identity.Application",principal);
                    return RedirectToAction("Index");
                }
            }
            ModelState.AddModelError("","Invalid username and password");
            return View();
        }
    }
}