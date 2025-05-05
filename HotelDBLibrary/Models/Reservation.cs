using System;
using System.Collections.Generic;

namespace HotelDBLibrary.Models;

public partial class Reservation
{
    public int Id { get; set; }

    public int? RoomId { get; set; }

    public string? GuestId { get; set; }

    public DateTime Start { get; set; }

    public DateTime End { get; set; }

    public DateTime ReservationDate { get; set; }

    public int NumberOfGuests { get; set; }

    public virtual Guest? Guest { get; set; }

    public virtual Room? Room { get; set; }
}
