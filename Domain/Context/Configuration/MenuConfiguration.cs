using Domain.Model;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Domain.Contexts.Configuration
{
    public class MenuConfiguration : IEntityTypeConfiguration<Menu>
    {
        public void Configure(EntityTypeBuilder<Menu> builder)
        {
            builder.ToTable("MENU");
            builder.HasKey(e => e.IdMenu);

            builder.Property(e => e.IdMenu).HasColumnName("ID_MENU");
            builder.Property(e => e.FechaCreacion)
                .HasDefaultValueSql("(sysutcdatetime())")
                .HasColumnType("datetime2")
                .HasColumnName("FECHA_CREACION");
            builder.Property(e => e.NombreIcono)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NOMBRE_ICONO");
            builder.Property(e => e.NombreMenu)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("NOMBRE_MENU");
            builder.Property(e => e.CodigoMenu)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("CODIGO_MENU");
        }
    }
}
