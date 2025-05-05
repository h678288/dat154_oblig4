using HotelDBLibrary.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Resepsjon.Services;

public class ReservationService {
    private readonly Func<HotelDBLibrary.Models.HotelDbContext> _contextFactory;

    public ReservationService(Func<HotelDBLibrary.Models.HotelDbContext> contextFactory) {
        _contextFactory = contextFactory;
    }

    public async Task<List<Reservation>> GetAllReservationsAsync() {
        await using var context = _contextFactory();
        return await context.Reservations
                            .Include(r => r.Room)
                            .AsNoTracking()
                            .ToListAsync();
    }

    public async Task<Reservation?> GetReservationByIdAsync(int reservationId) {
        await using var context = _contextFactory();
        return await context.Reservations
                            .Include(r => r.Room)
                            .FirstOrDefaultAsync(r => r.Id == reservationId);
    }

    public async Task<List<Reservation>> GetReservationsByGuestTlfAsync(string guestTlf) {
        if (string.IsNullOrEmpty(guestTlf)) return new List<Reservation>();
        await using var context = _contextFactory();
        return await context.Reservations
                            .Where(r => r.GuestId == guestTlf)
                            .AsNoTracking()
                            .ToListAsync();
    }

    public async Task<List<Reservation>> GetReservationsByRoomIdAsync(int roomId) {
        using var context = _contextFactory();
        return await context.Reservations
                            .Where(r => r.RoomId == roomId)
                            .AsNoTracking()
                            .ToListAsync();
    }


    // Legge til en ny reservasjon
    public async Task<Reservation> AddReservationAsync(Reservation reservation) {
        if (reservation == null || string.IsNullOrEmpty(reservation.GuestId) || !reservation.RoomId.HasValue || reservation.RoomId <= 0) {
            throw new ArgumentException("Ugyldig reservasjonsdata.");
        }

        using var context = _contextFactory();

        var roomExists = await context.Rooms.AnyAsync(r => r.Id == reservation.RoomId.Value);
        if (!roomExists) { throw new KeyNotFoundException($"Rom med ID {reservation.RoomId} finnes ikke.");
        }

        try {
            context.Reservations.Add(reservation);
            await context.SaveChangesAsync();
            return reservation;
        }
        catch (DbUpdateException ex) {
            Console.WriteLine($"Database Error adding reservation: {ex.InnerException?.Message ?? ex.Message}");
            throw;
        }
    }

    // Oppdatere en reservasjon
    public async Task<Reservation> UpdateReservationAsync(Reservation reservation) {
        if (reservation == null || reservation.Id <= 0) {
            throw new ArgumentException("Ugyldig reservasjon for oppdatering.");
        }

        await using var context = _contextFactory();
        try {
            var existingReservation = await context.Reservations.FindAsync(reservation.Id);
            if (existingReservation == null) {
                throw new KeyNotFoundException($"Reservasjon med ID {reservation.Id} finnes ikke.");
            }
            context.Entry(existingReservation).CurrentValues.SetValues(reservation);

            await context.SaveChangesAsync();
            return existingReservation;
        }
        catch (DbUpdateException ex) {
            Console.WriteLine($"Database Error updating reservation: {ex.InnerException?.Message ?? ex.Message}");
            throw;
        }
    }

    // Slette en reservasjon
    public async Task<bool> DeleteReservationAsync(int reservationId) {
        if (reservationId <= 0) return false;

        await using var context = _contextFactory();
        try {
            var reservation = await context.Reservations.FindAsync(reservationId);
            if (reservation != null) {
                context.Reservations.Remove(reservation);
                await context.SaveChangesAsync();
                return true;
            }
            return false;
        }
        catch (DbUpdateException ex) {
            Console.WriteLine($"Database Error deleting reservation: {ex.InnerException?.Message ?? ex.Message}");
            throw;
        }
    }
}