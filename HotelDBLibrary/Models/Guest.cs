using System;
using System.Collections.Generic;

namespace HotelDBLibrary.Models;

public partial class Guest
{
    public string Tlf { get; set; } = null!;

    public string? Navn { get; set; }

    public string? Passord { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
}
