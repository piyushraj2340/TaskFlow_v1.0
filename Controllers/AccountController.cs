using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using TaskMonitoringApp.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using TaskMonitoringApp.Models.Entities;
using TaskMonitoringApp.Models.ViewModel;

namespace TaskMonitoringApp.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserManager<Users> _userManager;
        private readonly SignInManager<Users> _signInManager;
        private readonly IMapper _mapper;

        public AccountController(UserManager<Users> userManager, SignInManager<Users> signInManager, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _mapper = mapper;
        }

        [Authorize]
        [HttpGet]
        public async  Task<IActionResult> Index()
        {
            // Get the logged-in user's ID
            var userId = _userManager.GetUserId(User);

            if (userId == null)
            {
                return RedirectToAction("Login", "Account");
            }

            // Get user info (e.g., Email, UserName, etc.)
            var user = await _userManager.FindByIdAsync(userId);
            return View(user);
        }


        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");

            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            
            if (ModelState.IsValid)
            {
                var user = _mapper.Map<Users>(model);
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login()
        {
            if (User.Identity.IsAuthenticated) return RedirectToAction("Index", "Home");
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {

            if (ModelState.IsValid)
            {
                var userName = model.UserName;

                if (userName.Contains("@"))
                {
                    var user = await _userManager.FindByEmailAsync(userName);

                    if (user != null)
                    {
                        userName = user.UserName;
                    }
                }

                if(userName == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid Email.");
                    return View(model);
                }
            

                var result = await _signInManager.PasswordSignInAsync(userName, model.Password, model.RememberMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home");
                }

                ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            }
            return View(model);
        }

        // POST: /Account/GuestLogin
        [HttpPost]
        public async Task<IActionResult> GuestLogin([FromServices] IGuestSeederService guestSeeder)
        {
            // Ensure they have the seed data immediately on login (this also creates the user if they don't exist)
            await guestSeeder.EnsureGuestDataExistsAsync();

            var guestEmail = "guest@taskmonitorapp.com";
            var guestUser = await _userManager.FindByEmailAsync(guestEmail);

            if (guestUser == null)
            {
                ModelState.AddModelError(string.Empty, "Could not initialize Guest account.");
                return RedirectToAction("Login");
            }

            // Sign in
            await _signInManager.SignInAsync(guestUser, isPersistent: false);

            // Set TempData flag to trigger tour on dashboard
            TempData["TriggerTour"] = "true";

            return RedirectToAction("Index", "Home");
        }

        // POST: /Account/Logout
        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }
    }
}
