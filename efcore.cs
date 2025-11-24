// Archivos sugeridos:
// Models/User.cs, Models/Person.cs, Models/Role.cs, Models/Garden.cs, Models/Room.cs,
// Models/UserRoomRole.cs, Models/Child.cs, Models/ChildGuardian.cs, Models/ChildRoom.cs
// Data/ApplicationDbContext.cs
// Data/Configurations/*.cs

using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace NurseryApp.Models
{
    // ---------------------------
    // Core domain entities
    // ---------------------------

    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid(); // PK
        public string Email { get; set; } = null!;
        public string? Phone { get; set; }
        public string PasswordHash { get; set; } = null!;
        public bool IsActive { get; set; } = true;
        public DateTime? LastLoginUtc { get; set; }

        // Navigation
        public Person? Person { get; set; }    // 1:1
        public ICollection<UserRoomRole> UserRoomRoles { get; set; } = new List<UserRoomRole>();
        public ICollection<ChildGuardian> ChildGuardians { get; set; } = new List<ChildGuardian>();
    }

    /// <summary>
    /// Datos personales, separados del user (autenticación).
    /// Mapeado a la misma PK que User (UserId).
    /// </summary>
    public class Person
    {
        public Guid Id { get; set; } // PK = UserId
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public string? DocumentNumber { get; set; } // DNI
        public string? PhotoUrl { get; set; }

        // Navigation
        public User User { get; set; } = null!;
    }

    public class Role
    {
        public int Id { get; set; } // e.g. 1 = Familiar, 2 = Educador, 3 = Director
        public string Name { get; set; } = null!;

        // Navigation
        public ICollection<UserRoomRole> UserRoomRoles { get; set; } = new List<UserRoomRole>();
    }

    public class Garden
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Address { get; set; }

        // Navigation
        public ICollection<Room> Rooms { get; set; } = new List<Room>();
    }

    public class Room
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid GardenId { get; set; }
        public string Name { get; set; } = null!;

        // Navigation
        public Garden Garden { get; set; } = null!;
        public ICollection<UserRoomRole> UserRoomRoles { get; set; } = new List<UserRoomRole>();
        public ICollection<ChildRoom> ChildRooms { get; set; } = new List<ChildRoom>();
    }

    /// <summary>
    /// Relaciona un usuario con una sala y un rol contextual (p. ej. "educador en Sala Verde").
    /// Llave compuesta [UserId, RoomId, RoleId] para evitar duplicados exactos.
    /// </summary>
    public class UserRoomRole
    {
        public Guid UserId { get; set; }
        public Guid RoomId { get; set; }
        public int RoleId { get; set; }

        // Navigation
        public User User { get; set; } = null!;
        public Room Room { get; set; } = null!;
        public Role Role { get; set; } = null!;
    }

    // ---------------------------
    // Children (Niños)
    // ---------------------------

    public class Child
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string FirstName { get; set; } = null!;
        public string LastName { get; set; } = null!;
        public DateTime Birthdate { get; set; }
        public string? DocumentNumber { get; set; }
        public string? PhotoUrl { get; set; }
        public string Status { get; set; } = "Activo"; // Activo / Egresado

        // Navigation
        public ICollection<ChildGuardian> ChildGuardians { get; set; } = new List<ChildGuardian>();
        public ICollection<ChildRoom> ChildRooms { get; set; } = new List<ChildRoom>();
    }

    /// <summary>
    /// Puente Many-to-Many entre Child y User (familiar),
    /// composite PK (ChildId, UserId) para evitar duplicados.
    /// </summary>
    public class ChildGuardian
    {
        public Guid ChildId { get; set; }
        public Guid UserId { get; set; }

        public string RelationshipType { get; set; } = null!; // Madre/Padre/Tutor
        public bool IsPrimaryGuardian { get; set; } = false;
        public DateTime AssignedAtUtc { get; set; } = DateTime.UtcNow;

        // Navigation
        public Child Child { get; set; } = null!;
        public User User { get; set; } = null!;
    }

    /// <summary>
    /// Historial de la sala del niño. Tiene PK propio para facilitar inserts/updates.
    /// FromDate..ToDate (nullable = vigente).
    /// </summary>
    public class ChildRoom
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid ChildId { get; set; }
        public Guid RoomId { get; set; }

        public DateTime FromDate { get; set; }
        public DateTime? ToDate { get; set; }

        // Navigation
        public Child Child { get; set; } = null!;
        public Room Room { get; set; } = null!;
    }
}

// ---------------------------
// EF Core configurations
// ---------------------------

namespace NurseryApp.Data.Configurations
{
    using NurseryApp.Models;

    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.ToTable("Users");
            builder.HasKey(u => u.Id);

            builder.Property(u => u.Email)
                   .IsRequired()
                   .HasMaxLength(256);

            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.PasswordHash)
                   .IsRequired();

            builder.Property(u => u.Phone)
                   .HasMaxLength(32);
        }
    }

    public class PersonConfiguration : IEntityTypeConfiguration<Person>
    {
        public void Configure(EntityTypeBuilder<Person> builder)
        {
            builder.ToTable("Persons");
            builder.HasKey(p => p.Id);

            // 1:1 with User, shared PK
            builder.HasOne(p => p.User)
                   .WithOne(u => u.Person)
                   .HasForeignKey<Person>(p => p.Id)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
            builder.Property(p => p.DocumentNumber).HasMaxLength(50);
            builder.Property(p => p.PhotoUrl).HasMaxLength(512);
        }
    }

    public class RoleConfiguration : IEntityTypeConfiguration<Role>
    {
        public void Configure(EntityTypeBuilder<Role> builder)
        {
            builder.ToTable("Roles");
            builder.HasKey(r => r.Id);
            builder.Property(r => r.Name).IsRequired().HasMaxLength(50);
            builder.HasIndex(r => r.Name).IsUnique();
        }
    }

    public class GardenConfiguration : IEntityTypeConfiguration<Garden>
    {
        public void Configure(EntityTypeBuilder<Garden> builder)
        {
            builder.ToTable("Gardens");
            builder.HasKey(g => g.Id);
            builder.Property(g => g.Name).IsRequired().HasMaxLength(200);
            builder.Property(g => g.Address).HasMaxLength(500);
        }
    }

    public class RoomConfiguration : IEntityTypeConfiguration<Room>
    {
        public void Configure(EntityTypeBuilder<Room> builder)
        {
            builder.ToTable("Rooms");
            builder.HasKey(r => r.Id);

            builder.Property(r => r.Name).IsRequired().HasMaxLength(200);

            builder.HasOne(r => r.Garden)
                   .WithMany(g => g.Rooms)
                   .HasForeignKey(r => r.GardenId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class UserRoomRoleConfiguration : IEntityTypeConfiguration<UserRoomRole>
    {
        public void Configure(EntityTypeBuilder<UserRoomRole> builder)
        {
            builder.ToTable("UserRoomRoles");

            // Composite PK to prevent duplicate role entries for same user/room/role
            builder.HasKey(urr => new { urr.UserId, urr.RoomId, urr.RoleId });

            builder.HasOne(urr => urr.User)
                   .WithMany(u => u.UserRoomRoles)
                   .HasForeignKey(urr => urr.UserId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(urr => urr.Room)
                   .WithMany(r => r.UserRoomRoles)
                   .HasForeignKey(urr => urr.RoomId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(urr => urr.Role)
                   .WithMany(ro => ro.UserRoomRoles)
                   .HasForeignKey(urr => urr.RoleId)
                   .OnDelete(DeleteBehavior.Restrict);
        }
    }

    public class ChildConfiguration : IEntityTypeConfiguration<Child>
    {
        public void Configure(EntityTypeBuilder<Child> builder)
        {
            builder.ToTable("Children");
            builder.HasKey(c => c.Id);

            builder.Property(c => c.FirstName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.LastName).IsRequired().HasMaxLength(100);
            builder.Property(c => c.DocumentNumber).HasMaxLength(50);
            builder.Property(c => c.PhotoUrl).HasMaxLength(512);
            builder.Property(c => c.Status).HasMaxLength(50).IsRequired();
        }
    }

    public class ChildGuardianConfiguration : IEntityTypeConfiguration<ChildGuardian>
    {
        public void Configure(EntityTypeBuilder<ChildGuardian> builder)
        {
            builder.ToTable("ChildGuardians");

            // Composite PK
            builder.HasKey(cg => new { cg.ChildId, cg.UserId });

            builder.Property(cg => cg.RelationshipType).IsRequired().HasMaxLength(50);
            builder.Property(cg => cg.IsPrimaryGuardian).IsRequired();

            builder.HasOne(cg => cg.Child)
                   .WithMany(c => c.ChildGuardians)
                   .HasForeignKey(cg => cg.ChildId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cg => cg.User)
                   .WithMany(u => u.ChildGuardians)
                   .HasForeignKey(cg => cg.UserId)
                   .OnDelete(DeleteBehavior.Cascade);
        }
    }

    public class ChildRoomConfiguration : IEntityTypeConfiguration<ChildRoom>
    {
        public void Configure(EntityTypeBuilder<ChildRoom> builder)
        {
            builder.ToTable("ChildRooms");
            builder.HasKey(cr => cr.Id);

            builder.Property(cr => cr.FromDate).IsRequired();
            builder.Property(cr => cr.ToDate).IsRequired(false);

            builder.HasOne(cr => cr.Child)
                   .WithMany(c => c.ChildRooms)
                   .HasForeignKey(cr => cr.ChildId)
                   .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(cr => cr.Room)
                   .WithMany(r => r.ChildRooms)
                   .HasForeignKey(cr => cr.RoomId)
                   .OnDelete(DeleteBehavior.Restrict);

            // Index to quickly query current room
            builder.HasIndex(cr => new { cr.ChildId, cr.ToDate });
        }
    }
}

// ---------------------------
// ApplicationDbContext
// ---------------------------

namespace NurseryApp.Data
{
    using Microsoft.EntityFrameworkCore;
    using NurseryApp.Models;
    using NurseryApp.Data.Configurations;

    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) { }

        public DbSet<User> Users => Set<User>();
        public DbSet<Person> Persons => Set<Person>();
        public DbSet<Role> Roles => Set<Role>();
        public DbSet<Garden> Gardens => Set<Garden>();
        public DbSet<Room> Rooms => Set<Room>();
        public DbSet<UserRoomRole> UserRoomRoles => Set<UserRoomRole>();
        public DbSet<Child> Children => Set<Child>();
        public DbSet<ChildGuardian> ChildGuardians => Set<ChildGuardian>();
        public DbSet<ChildRoom> ChildRooms => Set<ChildRoom>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Register configurations
            modelBuilder.ApplyConfiguration(new UserConfiguration());
            modelBuilder.ApplyConfiguration(new PersonConfiguration());
            modelBuilder.ApplyConfiguration(new RoleConfiguration());
            modelBuilder.ApplyConfiguration(new GardenConfiguration());
            modelBuilder.ApplyConfiguration(new RoomConfiguration());
            modelBuilder.ApplyConfiguration(new UserRoomRoleConfiguration());
            modelBuilder.ApplyConfiguration(new ChildConfiguration());
            modelBuilder.ApplyConfiguration(new ChildGuardianConfiguration());
            modelBuilder.ApplyConfiguration(new ChildRoomConfiguration());

            // Seed default roles (optional)
            modelBuilder.Entity<Role>().HasData(
                new Role { Id = 1, Name = "Familiar" },
                new Role { Id = 2, Name = "Educador" },
                new Role { Id = 3, Name = "Director" }
            );
        }
    }
}
