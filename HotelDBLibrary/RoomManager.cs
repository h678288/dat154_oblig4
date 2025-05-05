using HotelDBLibrary.Models;
using Microsoft.EntityFrameworkCore;
using Task = System.Threading.Tasks.Task;

namespace HotelDBLibrary;

public class RoomManager
{
    private readonly HotelDbContext _context;

    public RoomManager(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<List<Room>> GetAllRoomsAsync() => await _context.Rooms.ToListAsync();

    public async Task<Room?> GetRoomByIdAsync(int roomId) => await _context.Rooms.FindAsync(roomId);

    public async Task<List<Room>> FindAvailableRoomsAsync(DateTime startDate, DateTime endDate, int? numberOfBeds,
        string? type)
    {
        var bookedRooms = await _context.Reservations
            .Where(r => r.Start < endDate && r.End > startDate)
            .Select(r => r.RoomId)
            .Distinct()
            .ToListAsync();

        var availableRooms = await _context.Rooms
            .Where(r => r.NumOfBeds >= numberOfBeds && (string.IsNullOrEmpty(type) || r.Type == type) &&
                        !bookedRooms.Contains(r.Id))
            .ToListAsync();

        return availableRooms;
    }

    public async Task<Room> AddRoomAsync(Room room)
    {
        await _context.Rooms.AddAsync(room);
        await _context.SaveChangesAsync();
        return room;
    }
    
    public async Task<Room> UpdateRoomAsync(Room room)
    {
        _context.Rooms.Update(room);
        await _context.SaveChangesAsync();
        return room;
    }
    
    public async Task DeleteRoomAsync(int roomId)
    {
        var room = await GetRoomByIdAsync(roomId);
        if (room != null)
        {
            _context.Rooms.Remove(room);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<List<Room>> GetRoomByStatusAsync(string status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return new List<Room>();
        }

        var rooms = await _context.Rooms
            .Include(r => r.Tasks)
            .Where(r => r.Tasks.Any(t => t.Status == status))
            .ToListAsync();
        
        return rooms;
    }

    public async Task<bool> UpdateRoomStatusAsync(int roomId, string newStatus)
    {
        if (string.IsNullOrEmpty(newStatus)) return false;

        var room = await _context.Rooms
            .Include(r => r.Tasks)
            .FirstOrDefaultAsync(r => r.Id == roomId);
        
        if (room == null) return false;
        if(!room.Tasks.Any()) return false;

        foreach (var task in room.Tasks)
        {
            task.Status = newStatus;
        }
        
        await _context.SaveChangesAsync();
        return true;
    }
    
}