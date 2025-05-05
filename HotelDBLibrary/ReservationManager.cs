using HotelDBLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelDBLibrary;

public class ReservationManager
{
    private readonly HotelDbContext _context;
    
    public ReservationManager(HotelDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<Reservation>> GetAllReservationsAsync() => await _context.Reservations.ToListAsync();
    
    public async Task<Reservation?> GetReservationByIdAsync(int reservationId) => await _context.Reservations.FindAsync(reservationId);
    
    public async Task<List<Reservation>> GetReservationsByGuestIdAsync(string guestId)
    {
        return await _context.Reservations
            .Where(r => r.GuestId == guestId)
            .ToListAsync();
    }
    
    public async Task<List<Reservation>> GetReservationsByRoomIdAsync(int roomId)
    {
        return await _context.Reservations
            .Where(r => r.RoomId == roomId)
            .ToListAsync();
    }
    
    public async Task<Reservation> AddReservationAsync(Reservation reservation)
    {
        var room = await _context.Rooms.FindAsync(reservation.RoomId);
        if (room == null) throw new Exception("Room not found");
        var guest = await _context.Guests.FindAsync(reservation.GuestId);
        if (guest == null) throw new Exception("Guest not found");
        await _context.Reservations.AddAsync(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }
    
    public async Task<Reservation> UpdateReservationAsync(Reservation reservation)
    {
        var existingReservation = await GetReservationByIdAsync(reservation.Id);
        if (existingReservation == null) throw new Exception("Reservation not found");
        
        _context.Entry(existingReservation).CurrentValues.SetValues(reservation);
        await _context.SaveChangesAsync();
        return reservation;
    }
    
    public async Task<bool> DeleteReservationAsync(int reservationId)
    {
        var reservation = await GetReservationByIdAsync(reservationId);
        if (reservation != null)
        {
            _context.Reservations.Remove(reservation);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
    
    public async Task<List<Reservation>> GetReservationsByDateRangeAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Reservations
            .Where(r => r.Start >= startDate && r.End <= endDate)
            .ToListAsync();
    }
}