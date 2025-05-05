using System;
using System.Collections.Generic;

namespace HotelDBLibrary.Models;

public partial class Task
{
    public int Id { get; set; }

    public int RoomId { get; set; }

    public string Task1 { get; set; } = null!;

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public virtual Room Room { get; set; } = null!;
}
