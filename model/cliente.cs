namespace FacElec.model
{
    public class cliente
    {
        public int id_Cliente;
        public string identificacion;
        public int tipoIdentificacion;
        public string nombre;
        public string telefono;
        public string email;
        public string direccion;
        public int provincia;
        public int canton;
        public int distrito;
        public string idautomercado;
        public int precio;

        public cliente(int id_Cliente, string identificacion, int tipoIdentificacion, string nombre, string telefono, string email, string direccion,
                       int provincia, int canton, int distrito, string idautomercado, int precio)
        {
            this.id_Cliente = id_Cliente;
            this.identificacion = identificacion;
            if (tipoIdentificacion == 5) tipoIdentificacion = 10;
            this.tipoIdentificacion = tipoIdentificacion;
            this.nombre = nombre;
            this.telefono = telefono.Trim().Replace("-","");
            this.email = email;
            this.direccion = direccion;
            this.provincia = provincia;
            this.canton = canton;
            this.distrito = distrito;
            this.idautomercado = idautomercado;
            this.precio = precio;
        }
    }
}