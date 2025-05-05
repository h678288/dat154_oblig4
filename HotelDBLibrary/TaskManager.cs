using HotelDBLibrary.Models;
using Microsoft.EntityFrameworkCore;
using DbTask = HotelDBLibrary.Models.Task;

namespace HotelDBLibrary;

public class TaskManager
{
    private readonly HotelDbContext _context;
    
    public const string StatusPending = "Pending";
    public const string StatusInProgress = "InProgress";
    public const string StatusCompleted = "Completed";
    
    public const string TypeCleaning = "Cleaning";
    public const string TypeMaintenance = "Maintenance";
    public const string TypeService = "Service";
    
    public TaskManager(HotelDbContext context)
    {
        _context = context;
    }
    
    public async Task<List<DbTask>> GetAllTasksAsync() => await _context.Tasks.ToListAsync();
    
    public async Task<DbTask?> GetTaskByIdAsync(int taskId) => await _context.Tasks.FindAsync(taskId);
    
    public async Task<List<DbTask>> GetTasksByRoomIdAsync(int roomId)
    {
        return await _context.Tasks
            .Where(t => t.RoomId == roomId)
            .ToListAsync();
    }
    
    public async Task<List<DbTask>> GetTasksByTypeAsync(string type)
    {
        if (string.IsNullOrEmpty(type)) throw new Exception("Type cannot be null or empty");
        
        return await _context.Tasks
            .Where(t => t.Task1 == type)
            .ToListAsync();
    }
    
    public async Task<List<DbTask>> GetTasksByStatusAsync(string status)
    {
        if (string.IsNullOrEmpty(status))
        {
            return new List<DbTask>();
        }
        
        return await _context.Tasks
            .Where(t => t.Status == status)
            .ToListAsync();
    }
    
    public async Task<DbTask> AddTaskAsync(DbTask task)
    {
        var room = await _context.Rooms.FindAsync(task.RoomId);
        if (room == null) throw new Exception("Room not found");
        await _context.Tasks.AddAsync(task);
        await _context.SaveChangesAsync();
        return task;
    }
    
    public async Task<DbTask> UpdateTaskAsync(DbTask task)
    {
        var existingTask = await GetTaskByIdAsync(task.Id);
        if (existingTask == null) throw new Exception("Task not found");
        
        _context.Entry(existingTask).CurrentValues.SetValues(task);
        await _context.SaveChangesAsync();
        return task;
    }
    
    public async Task<bool> UpdateTaskStatusAndNotesAsync(int taskId, string newStatus, string? notes)
    {
        if (string.IsNullOrWhiteSpace(newStatus)) return false;

        var task = await GetTaskByIdAsync(taskId);
        if (task == null) return false;

        task.Status = newStatus;
        task.Notes = notes;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTaskAsync(DbTask task)
    {
        var existingTask = await GetTaskByIdAsync(task.Id);
        if (existingTask != null)
        {
            _context.Tasks.Remove(existingTask);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
    
}