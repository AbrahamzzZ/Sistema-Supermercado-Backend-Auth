namespace Domain.Model.Dto
{
    public class UsuarioCredencial
    {
        public int Id_Usuario { get; set; }
        public string? Codigo { get; set; }
        public string? Nombre_Completo { get; set; }
        public string? Correo_Electronico { get; set; }
        public int? Id_Rol { get; set; }
        public string? Nombre_Rol { get; set; }
        public bool? Estado { get; set; }
        public DateTime? Fecha_Creacion { get; set; }
        public string? Clave { get; set; }

        public UsuarioRol ToUsuarioRol() => new()
        {
            Id_Usuario = Id_Usuario,
            Codigo = Codigo,
            Nombre_Completo = Nombre_Completo,
            Correo_Electronico = Correo_Electronico,
            Id_Rol = Id_Rol,
            Nombre_Rol = Nombre_Rol,
            Estado = Estado,
            Fecha_Creacion = Fecha_Creacion
        };
    }
}
