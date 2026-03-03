using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Contexts.Configuration
{
    public class PermisoConfiguration : IEntityTypeConfiguration<Permiso>
    {
        public void Configure(EntityTypeBuilder<Permiso> builder)
        {
            builder.ToTable("PERMISO");
            builder.HasKey(e => e.IdPermiso);

            builder.Property(e => e.IdPermiso).HasColumnName("ID_PERMISO");
            builder.Property(e => e.IdMenu).HasColumnName("ID_MENU");
            builder.Property(e => e.IdRol).HasColumnName("ID_ROL");

            builder.HasOne(d => d.IdMenuNavigation)
                .WithMany(p => p.Permisos)
                .HasForeignKey(d => d.IdMenu)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(d => d.IdRolNavigation)
                .WithMany(p => p.Permisos)
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.Restrict);

        }
    }
}
