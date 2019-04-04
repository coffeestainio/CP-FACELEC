namespace FacElec.model
{
    public class producto
    {
        public string id_producto;
        public string nombre;
        public decimal costo;
        public int empaque;
        public int sub_empaque;
        public decimal detalle;
        public decimal utilidad_1;
        public decimal utilidad_2;
        public decimal utilidad_3;
        public decimal utilidad_4;

        public producto(string id_producto, string nombre, decimal costo, int empaque, int sub_empaque, decimal detalle
                        , decimal utilidad_1,decimal utilidad_2,decimal utilidad_3,decimal utilidad_4)
        {
            this.id_producto = id_producto;
            this.nombre = nombre;
            this.costo = costo;
            this.empaque = empaque;
            this.sub_empaque = sub_empaque;
            this.detalle = detalle;
            this.utilidad_1 = utilidad_1;
            this.utilidad_2 = utilidad_2;
            this.utilidad_3 = utilidad_3;
            this.utilidad_4 = utilidad_4;
        }
    }
}