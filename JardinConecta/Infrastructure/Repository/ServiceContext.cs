using JardinConecta.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace JardinConecta.Infrastructure.Repository
{
    public class ServiceContext : DbContext
    {
        public ServiceContext(DbContextOptions<ServiceContext> options) : base(options)
        {
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);
        }
        protected override void OnModelCreating(ModelBuilder builder)
        {
            builder.Entity<Usuario>().ToTable("Usuarios");
            builder.Entity<Usuario>().Property(u => u.Email).HasColumnName("Email").HasMaxLength(254);
            builder.Entity<Usuario>().Property(u => u.PasswordHash).HasColumnName("PasswordHash").HasMaxLength(500);
            builder.Entity<Usuario>().ComplexProperty(u => u.Telefono, t => {
                t.Property(t => t.CaracteristicaPais).HasColumnName("CaracteristicaPais").HasMaxLength(3);
                t.Property(t => t.CodigoArea).HasColumnName("CodigoArea").HasMaxLength(6);
                t.Property(t => t.Numero).HasColumnName("Numero").HasMaxLength(20);
            });
            builder.Entity<Usuario>().HasOne(u => u.Persona).WithOne(p => p.Usuario).HasForeignKey<Persona>(p => p.IdUsuario);
            builder.Entity<Usuario>().HasOne(u => u.TipoUsuario).WithMany().HasForeignKey(u => u.IdTipoUsuario);
            builder.Entity<Usuario>().HasOne(u => u.Jardin).WithMany().HasForeignKey(u => u.IdJardin);
            builder.Entity<Usuario>().HasMany(u => u.Tutelas).WithOne(t => t.Usuario).HasForeignKey(t => t.IdUsuario);

            builder.Entity<Persona>().ToTable("Personas");
            builder.Entity<Persona>().HasKey(p => p.IdUsuario);
            builder.Entity<Persona>().Property(p => p.Nombre).HasColumnName("Nombre").HasMaxLength(100);
            builder.Entity<Persona>().Property(p => p.Apellido).HasColumnName("Apellido").HasMaxLength(100);
            builder.Entity<Persona>().Property(p => p.Documento).HasColumnName("Documento").HasMaxLength(50);
            builder.Entity<Persona>().Property(p => p.PhotoUrl).HasColumnName("PhotoUrl");
            

            builder.Entity<TipoUsuario>().ToTable("TiposUsuarios");
            builder.Entity<TipoUsuario>().Property(r => r.Descripcion).HasColumnName("Descripcion").HasMaxLength(200);
            builder.Entity<TipoUsuario>().HasData(
                new TipoUsuario() { Id = (int)TipoUsuarioId.Usuario, Descripcion = "Usuario" },
                new TipoUsuario() { Id = (int)TipoUsuarioId.AdminJardin, Descripcion = "Admin Jardin" },
                new TipoUsuario() { Id = (int)TipoUsuarioId.AdminSistema, Descripcion = "Admin Sistema" }
            );

            builder.Entity<Rol>().ToTable("Roles");
            builder.Entity<Rol>().Property(r => r.Descripcion).HasColumnName("Descripcion").HasMaxLength(200);
            builder.Entity<Rol>().HasData(
                new Rol() { Id = (int)RolId.Familia, Descripcion = "Familia"},
                new Rol() { Id = (int)RolId.Educador, Descripcion = "Educador"}
            );

            builder.Entity<Jardin>().ToTable("Jardines");
            builder.Entity<Jardin>().Property(j => j.Nombre).HasColumnName("Nombre").HasMaxLength(100);
            builder.Entity<Jardin>().HasMany(j => j.Salas).WithOne(s => s.Jardin).HasForeignKey(s => s.IdJardin);
            builder.Entity<Jardin>().HasMany(j => j.Infantes).WithOne(i => i.Jardin).HasForeignKey(i => i.IdJardin);

            builder.Entity<Sala>().ToTable("Salas");
            builder.Entity<Sala>().Property(s => s.Nombre).HasColumnName("Nombre").HasMaxLength(100);

            builder.Entity<Infante>().ToTable("Infantes");
            builder.Entity<Infante>().Property(i => i.Nombre).HasColumnName("Nombre").HasMaxLength(100);
            builder.Entity<Infante>().Property(i => i.Apellido).HasColumnName("Apellido").HasMaxLength(100);
            builder.Entity<Infante>().Property(i => i.Documento).HasColumnName("Documento").HasMaxLength(50);
            builder.Entity<Infante>().Property(i => i.FechaNacimiento).HasColumnName("FechaNacimiento");
            builder.Entity<Infante>().Property(i => i.PhotoUrl).HasColumnName("PhotoUrl");
            builder.Entity<Infante>().HasMany(i => i.Tutelas).WithOne(t => t.Infante).HasForeignKey(t => t.IdInfante);

            builder.Entity<TipoTutela>().ToTable("TiposTutelas");
            builder.Entity<TipoTutela>().Property(t => t.Descripcion).HasColumnName("Descripcion").HasMaxLength(50);
            builder.Entity<TipoTutela>().HasData(
                new TipoTutela() { Id = (int)TipoTutelaId.Madre , Descripcion = "Madre" },
                new TipoTutela() { Id = (int)TipoTutelaId.Padre , Descripcion = "Padre" },
                new TipoTutela() { Id = (int)TipoTutelaId.Tutor , Descripcion = "Tutor" }
            );

            builder.Entity<Tutela>().ToTable("Tutelas");
            builder.Entity<Tutela>().HasKey(t => new { t.IdInfante, t.IdUsuario });
            builder.Entity<Tutela>().HasOne(t => t.TipoTutela).WithMany().HasForeignKey(t => t.IdTipoTutela);

            builder.Entity<UsuarioSalaRol>().ToTable("Usuarios_Salas_Roles");
            builder.Entity<UsuarioSalaRol>().HasKey(x => new { x.IdUsuario, x.IdSala, x.IdRol });
            builder.Entity<UsuarioSalaRol>().HasOne(x => x.Rol).WithMany().HasForeignKey(x => x.IdRol);
            builder.Entity<UsuarioSalaRol>().HasOne(x => x.Usuario).WithMany(u => u.UsuariosSalasRoles).HasForeignKey(x => x.IdUsuario);
            builder.Entity<UsuarioSalaRol>().HasOne(x => x.Sala).WithMany(s => s.UsuariosSalasRoles).HasForeignKey(x => x.IdSala);

            builder.Entity<Comunicado>().ToTable("Comunicados");
            builder.Entity<Comunicado>().Property(c => c.Titulo).HasColumnName("Titulo").HasMaxLength(200);
            builder.Entity<Comunicado>().Property(c => c.Contenido).HasColumnName("Contenido").HasColumnType("text");
            builder.Entity<Comunicado>().HasOne(c => c.Sala).WithMany().HasForeignKey(c => c.IdSala);
            builder.Entity<Comunicado>().HasOne(c => c.Usuario).WithMany().HasForeignKey(c => c.IdUsuario);

            builder.Entity<ComunicadoView>().ToTable("ComunicadosViews");
            builder.Entity<ComunicadoView>().HasKey(x => new { x.IdComunicado, x.IdUsuario });
            builder.Entity<ComunicadoView>().HasOne(x => x.Comunicado).WithMany(c => c.ComunicadoViews).HasForeignKey(x => x.IdComunicado);
            builder.Entity<ComunicadoView>().HasOne(x => x.Usuario).WithMany().HasForeignKey(x => x.IdUsuario);

            base.OnModelCreating(builder);
        }
    }
}
