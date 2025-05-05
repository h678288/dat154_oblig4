using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using HotelDBLibrary.Models;
using System.Linq;
using System.Threading.Tasks;

public class ReservationsController : Controller
{
    private readonly HotelDbContext _context;

    public ReservationsController(HotelDbContext context)
    {
        _context = context;
    }

    // GET: Reservations
    public async Task<IActionResult> MyReservations()
    {
        // Får gjest sin tlf nr.
        var guestTlf = HttpContext.Session.GetString("GuestTlf");

        if (string.IsNullOrEmpty(guestTlf))
        {
            return RedirectToAction("Login", "Account");  
        }

        // Få reservasjoner
        var reservations = await _context.Reservations
            .Where(r => r.Guest.Tlf == guestTlf)  
            .Include(r => r.Room)  
            .ToListAsync();

        return View(reservations);  
    }
}