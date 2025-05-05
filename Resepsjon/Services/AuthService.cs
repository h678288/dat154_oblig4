using HotelDBLibrary.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System;

namespace Resepsjon.Services;

public class AuthService
{
    private readonly Func<HotelDBLibrary.Models.HotelDbContext> _contextFactory;

    public AuthService(Func<HotelDBLibrary.Models.HotelDbContext> contextFactory) {
        _contextFactory = contextFactory;
    }

    public async Task<Employee> LoginResepsjonistAsync(string username, string password) {
        if (string.IsNullOrEmpty(username) || string.IsNullOrEmpty(password)) {
            throw new ArgumentException("Username and password cannot be empty");
        }

        using var context = _contextFactory();

        var user = await context.Employees
            .AsNoTracking()
            .FirstOrDefaultAsync(e => e.Username == username);

        if (user == null) {
            throw new Exception($"No employee with username {username}");
        }

        if (user.Password == password)
        {

            return user; }
        else {
            throw new Exception("Wrong password");
        }
    }
}