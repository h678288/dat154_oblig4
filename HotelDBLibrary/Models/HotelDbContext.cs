using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace HotelDBLibrary.Models;

public partial class HotelDbContext : DbContext
{
    public HotelDbContext()
    {
    }

    public HotelDbContext(DbContextOptions<HotelDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Employee> Employees { get; set; }

    public virtual DbSet<Guest> Guests { get; set; }

    public virtual DbSet<Reservation> Reservations { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Task> Tasks { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("Server=tcp:dat154-hoteldb.database.windows.net,1433;Initial Catalog=HotelDB;Persist Security Info=False;User ID=dat154gr18;Password=tX4%YsKz6@DQhktM5TGf;Encrypt=false;");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Employee>(entity =>
        {
            entity.HasKey(e => e.Username).HasName("employe_pk");

            entity.ToTable("employee", "hotel");

            entity.Property(e => e.Username)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("username");
            entity.Property(e => e.Password)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("password");
        });

        modelBuilder.Entity<Guest>(entity =>
        {
            entity.HasKey(e => e.Tlf).HasName("gjest_pk");

            entity.ToTable("guest", "hotel");

            entity.Property(e => e.Tlf)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("tlf");
            entity.Property(e => e.Navn)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("navn");
            entity.Property(e => e.Passord)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("passord");
        });

        modelBuilder.Entity<Reservation>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("reservation_pk");

            entity.ToTable("reservation", "hotel");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.End)
                .HasColumnType("datetime")
                .HasColumnName("end");
            entity.Property(e => e.GuestId)
                .HasMaxLength(9)
                .IsUnicode(false)
                .IsFixedLength()
                .HasColumnName("guestId");
            entity.Property(e => e.NumberOfGuests).HasColumnName("numberOfGuests");
            entity.Property(e => e.ReservationDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("reservationDate");
            entity.Property(e => e.RoomId).HasColumnName("roomId");
            entity.Property(e => e.Start)
                .HasColumnType("datetime")
                .HasColumnName("start");

            entity.HasOne(d => d.Guest).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.GuestId)
                .HasConstraintName("reservation_guest_tlf_fk");

            entity.HasOne(d => d.Room).WithMany(p => p.Reservations)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("reservation_room_id_fk");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("room_pk");

            entity.ToTable("room", "hotel");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Type)
                .HasMaxLength(1)
                .IsUnicode(false)
                .HasColumnName("type");
        });

        modelBuilder.Entity<Task>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("task_pk");

            entity.ToTable("task", "hotel");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Notes)
                .IsUnicode(false)
                .HasColumnName("notes");
            entity.Property(e => e.RoomId).HasColumnName("roomId");
            entity.Property(e => e.Status)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("status");
            entity.Property(e => e.Task1)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("task");

            entity.HasOne(d => d.Room).WithMany(p => p.Tasks)
                .HasForeignKey(d => d.RoomId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("task_room_id_fk");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
