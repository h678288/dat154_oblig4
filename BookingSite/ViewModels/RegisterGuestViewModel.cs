using System.ComponentModel.DataAnnotations;

namespace WebApplication1.ViewModels;


public class RegisterGuestViewModel
{
    public string Tlf { get; set; } = null!;
    public string? Navn { get; set; }
    public string? Passord { get; set; }
}