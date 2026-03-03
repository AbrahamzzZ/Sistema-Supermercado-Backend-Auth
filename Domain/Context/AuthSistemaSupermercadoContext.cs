using Domain.Model;
using Domain.Model.Dto;
using Microsoft.EntityFrameworkCore;

namespace Domain.Context
{
    public class AuthSistemaSupermercadoContext : DbContext
    {
        public DbSet<Usuario> Usuario { get; set; }
        public DbSet<Rol> Rol { get; set; }
        public DbSet<Menu> Menu { get; set; }
        public DbSet<Permiso> Permiso { get; set; }
        public DbSet<UsuarioRol> UsuariosDto { get; set; }
        public AuthSistemaSupermercadoContext(DbContextOptions<AuthSistemaSupermercadoContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //Clases serializadas
            modelBuilder.Entity<UsuarioRol>().HasNoKey().ToView(null);

            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AuthSistemaSupermercadoContext).Assembly);
        }
    }
}
