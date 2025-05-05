using HotelDBLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HotelDBLibrary;

public class GuestManager
{
    private readonly HotelDbContext _context;
    
    public GuestManager(HotelDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Guest>> GetAllGuestsAsync() => await _context.Guests.ToListAsync();

    public async Task<Guest?> GetGuestByTlfAsync(string tlf) => await _context.Guests.FindAsync(tlf);

    public async Task<Guest> AddGuestAsync(Guest guest)
    {
        var guestExists = await _context.Guests.AnyAsync(g => g.Tlf == guest.Tlf);
        if(guestExists) throw new Exception("Guest already exists");
        await _context.Guests.AddAsync(guest);
        await _context.SaveChangesAsync();
        return guest;
    }
    
    public async Task<Guest> UpdateGuestAsync(Guest guest)
    {
        var existingGuest = await GetGuestByTlfAsync(guest.Tlf);
        if (existingGuest == null) throw new Exception("Guest not found");
        
        _context.Entry(existingGuest).CurrentValues.SetValues(guest);
        await _context.SaveChangesAsync();
        return guest;
    }
    
    public async Task<bool> DeleteGuestAsync(string tlf)
    {
        var guest = await GetGuestByTlfAsync(tlf);
        if (guest != null)
        {
            _context.Guests.Remove(guest);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }

    public async Task<List<Reservation>> GetReservationsAsync(string tlf)
    {
        var guest = await GetGuestByTlfAsync(tlf);
        if (guest == null) throw new Exception("Guest not found");
            return await _context.Reservations
                .Where(r => r.GuestId == tlf)
                .ToListAsync();
    }
    
}