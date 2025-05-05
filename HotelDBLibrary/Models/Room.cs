using System;
using System.Collections.Generic;

namespace HotelDBLibrary.Models;

public partial class Room
{
    public int Id { get; set; }

    public string? Type { get; set; }

    public int? NumOfBeds { get; set; }

    public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();

    public virtual ICollection<Task> Tasks { get; set; } = new List<Task>();
}
