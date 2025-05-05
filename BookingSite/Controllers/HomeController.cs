using HotelDBLibrary;
using HotelDBLibrary.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Http;

public class HomeController : Controller
{
    private readonly GuestManager _guestManager;
    private readonly HotelDbContext _context;

    public HomeController(GuestManager guestManager, HotelDbContext context)
    {
        _guestManager = guestManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var guestTlf = HttpContext.Session.GetString("GuestTlf");

        if (!string.IsNullOrEmpty(guestTlf))
        {
            var guest = await _guestManager.GetGuestByTlfAsync(guestTlf);
            ViewBag.Guest = guest;
        }

        var rooms = await _context.Rooms.ToListAsync();
        return View(rooms);
    }
}