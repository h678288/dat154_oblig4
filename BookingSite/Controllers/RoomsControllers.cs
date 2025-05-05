using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Threading.Tasks;
using HotelDBLibrary;
using HotelDBLibrary.Models;

public class RoomsController : Controller

{
    private readonly HotelDbContext _context;
    private readonly GuestManager _guestManager;  

    public RoomsController(HotelDbContext context, GuestManager guestManager)
    {
        _context = context;
        _guestManager = guestManager;
    }

    // GET: Rooms
    public async Task<IActionResult> Index()
    {
        var rooms = await _context.Rooms.ToListAsync();
        return View(rooms);
    }

    // GET: Rooms/Book/
    public async Task<IActionResult> Book(int? id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var room = await _context.Rooms.FindAsync(id);
        if (room == null)
        {
            return NotFound();
        }

        
        var guestTlf = HttpContext.Session.GetString("GuestTlf");
        if (string.IsNullOrEmpty(guestTlf))
        {
            return RedirectToAction("Login", "Account");  
        }

        
        var reservation = new Reservation
        {
            RoomId = room.Id,
           
        };

        return View(reservation);
    }

    // POST: Rooms/Book/
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Book(int id, [Bind("GuestId, Start, End, NumberOfGuests")] Reservation reservation)
    {
        if (ModelState.IsValid)
        {
            var room = await _context.Rooms.FindAsync(id);
            if (room == null)
            {
                return NotFound();
            }

            var guestTlf = HttpContext.Session.GetString("GuestTlf");
            var guest = await _guestManager.GetGuestByTlfAsync(guestTlf);
        
            if (guest == null)
            {
                return NotFound();
            }

            reservation.RoomId = room.Id;
            reservation.GuestId = guest.Tlf;  

            _context.Add(reservation);
            await _context.SaveChangesAsync();

            return RedirectToAction("ReservationConfirmation", new { id = reservation.Id });
        }

        return View(reservation);
    }

    // GET: ReservationConfirmation/
    public async Task<IActionResult> ReservationConfirmation(int id)
    {
        var reservation = await _context.Reservations.FindAsync(id);
        if (reservation == null)
        {
            return NotFound();
        }

        return View(reservation);
    }
    public IActionResult ConfirmAndRedirect()
    {
        return RedirectToAction("Index", "Home"); 
    }
}

