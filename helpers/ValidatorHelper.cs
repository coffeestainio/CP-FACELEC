using System;
using System.Net.Mail;
using System.Text.RegularExpressions;
using FacElec.model;
using log4net;

namespace FacElec.helpers
{
    public static class ValidatorHelper
    {
        internal static ILog log;

        public static Error validarFacturas(Factura factura)
        {

            if (factura.clienteTributa)
            {
                var cliente = factura.cliente[0];
                try
                {
                    //validate email
                    log.Info($"Correo del cliente: {cliente.email}");
                    MailAddress mail = new MailAddress(cliente.email);

                    Regex regex = new Regex(@"^[0-9]{8}$");

                    log.Info($"Telefono del cliente: {cliente.telefono}");
                    //validate phone
                    if (regex.Match(cliente.telefono.Trim().Replace("-", "")) == Match.Empty)
                    {
                        return (new Error(factura.id_documento, "Error:002", "Formato del numero incorrecto"));
                    }

                    log.Info($"Identificacion del cliente: {cliente.identificacion} del tipo: {cliente.tipoIdentificacion}");
                    regex = new Regex(@"^[0-9]{12}$");
                    //validate identificacion
                    if (cliente.tipoIdentificacion == 3 &&
                        regex.Match(cliente.identificacion.Trim().Replace("-", "")) == Match.Empty)
                    {
                        return (new Error(factura.id_documento, "Error:003", "El formato de la cedula no corresponde al tipo de identificacion"));
                    }

                    regex = new Regex(@"^[0-9]{10}$");
                    //validate identificacion
                    if (cliente.tipoIdentificacion == 2 &&
                        regex.Match(cliente.identificacion.Trim().Replace("-", "")) == Match.Empty)
                    {
                        return (new Error(factura.id_documento, "Error:003", "El formato de la cedula no corresponde al tipo de identificacion"));
                    }

                    regex = new Regex(@"^[0-9]{9}$");
                    //validate identificacion
                    if (cliente.tipoIdentificacion == 1 &&
                        regex.Match(cliente.identificacion.Trim().Replace("-", "")) == Match.Empty)
                    {
                        return (new Error(factura.id_documento, "Error:003", "El formato de la cedula no corresponde al tipo de identificacion"));
                    }


                    return null;
                }
                catch (FormatException ex)
                {
                    log.Error(ex.Message);
                    return new Error(factura.id_documento, "Error:001", "Formato del correo incorrecto");
                }
                catch (NullReferenceException ex)
                {
                    log.Error(ex.Message);
                    return new Error(factura.id_documento, "Error:004", "Unhaldled exception");
                }
            }
                
            log.Info("La factura esta marcada sin receptor, no necesita validacion");
            return null;
        }
    }
}
