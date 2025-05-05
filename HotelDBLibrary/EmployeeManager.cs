using HotelDBLibrary.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelDBLibrary;

public class EmployeeManager
{
    private readonly HotelDbContext _context;

    public EmployeeManager(HotelDbContext context)
    {
        _context = context;
    }

    public async Task<Employee?> GetEmployeeByUsernameAsync(string username) => await _context.Employees.FindAsync(username);

    public async Task<Employee> AddEmployeeAsync(Employee employee)
    {
        var employeeExists = await _context.Employees.AnyAsync(e => e.Username == employee.Username);
        if (employeeExists) throw new Exception("Employee already exists");
        await _context.Employees.AddAsync(employee);
        await _context.SaveChangesAsync();
        return employee;
    }

    public async Task<bool> DeleteEmployeeAsync(Employee employee)
    {
        var exsitingEmployee = await GetEmployeeByUsernameAsync(employee.Username);
        if (exsitingEmployee == null) throw new Exception("Employee not found");
        _context.Employees.Remove(exsitingEmployee);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<Employee?> AuthenticateEmployee(string username, string password)
    {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) throw new Exception("Invalid Input");
        var employee = await GetEmployeeByUsernameAsync(username);
        if (employee == null) throw new Exception($"No employee with username {username}");
        if (employee.Password == password) return employee;
        throw new Exception("Invalid Password");
    }

    public async Task<List<Employee>> GetEmployeesAsync() => await _context.Employees.ToListAsync();
    
    public async Task<Employee> UpdateEmployeeAsync(Employee employee)
    {
        var existingEmployee = await GetEmployeeByUsernameAsync(employee.Username);
        if (existingEmployee == null) throw new Exception("Employee not found");
        
        _context.Entry(existingEmployee).CurrentValues.SetValues(employee);
        await _context.SaveChangesAsync();
        return employee;
    }
}