using System;
namespace FacElec.model
{
    public class Error
    {
        public string NumFacturaInterno;
        public string CodigoError;
        public string DescripcionError;

        public Error() { }

        public Error(string numFacturaInterno, string codigoError, string descripcionError)
        {
            NumFacturaInterno = numFacturaInterno;
            CodigoError = codigoError;
            DescripcionError = descripcionError;
 
        }
    }
}
