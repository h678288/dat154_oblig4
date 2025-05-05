using HotelDBLibrary;
using HotelDBLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.ViewModels;

public class AccountController : Controller
{
    private readonly GuestManager _guestManager;

    public AccountController(GuestManager guestManager)
    {
        _guestManager = guestManager;
    }
    
    // GET: /Account/Register
    [HttpGet]
    public IActionResult Register()
    {
        return View();
    }
    
    [HttpPost]
    public async Task<IActionResult> Register(RegisterGuestViewModel model)
    {
        if (ModelState.IsValid)
        {
            var guest = new Guest
            {
                Tlf = model.Tlf,
                Navn = model.Navn,
                Passord = model.Passord 
            };

            try
            {
                await _guestManager.AddGuestAsync(guest);
                HttpContext.Session.SetString("GuestTlf", guest.Tlf);
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", ex.Message);
            }
        }
        return View(model);
    }

    [HttpGet]
    public IActionResult Login(string returnUrl = null)
    {
        var model = new LoginViewModel
        {
            ReturnUrl = returnUrl
        };
        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> Login(string tlf, string password)
    {
        var guest = await _guestManager.GetGuestByTlfAsync(tlf);

        if (guest != null && guest.Passord == password)
        {
            // Set guest in session
            HttpContext.Session.SetString("GuestTlf", guest.Tlf);
            return RedirectToAction("Index", "Home"); 
        }
        else
        {
            ModelState.AddModelError("", "Invalid login attempt.");
            return View(); 
        }
    }

    
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();  
        return RedirectToAction("Index", "Home");
    }
}