using System;
using System.Collections.Generic;
using FacElec.model;
using FacElec.helpers;
using System.Xml.Linq;
using log4net;
using System.Net;
using System.Text;

namespace FacElec.sync
{
    public static class Sincronizador
    {

        public static ILog log;

        public static void SincronizarDocumentos(Boolean notaCredito)
        {

            var facturas = new List<Factura>();

            var hayConexion = verificarInternet();

            facturas = SqlHelper.GetFacturas(notaCredito);

            if (facturas != null && facturas.Count > 0)
            {
                foreach (Factura fac in facturas)
                {
                    if (fac.claveNumerica == null)
                        log.Warn($"Documento: {fac.id_documento} no registra a una factura en tributacion");
                    else{
                    log.Info($"** Procesando la {(notaCredito ? "Nota de Credito" : "Factura")}: {fac.id_documento} **");
                    var error = ValidatorHelper.validarFacturas(fac);

                    if (error != null)
                    {
                        SqlHelper.updateWithError(error, notaCredito);
                    }
                    else
                    {
                        //si no hay internet se cambia la situacion de envio a 3
                        if (!hayConexion)
                        {
                            fac.claveNumerica = newClaveNumerica(fac.claveNumerica);
                            SqlHelper.updateNoInternet(fac, notaCredito);

                        }

                        //se genera el xml el pdf se llama el proceso y se guarda la factura
                        XmlHelper.GenerateXML(fac, notaCredito);
                        PdfHelper.GeneratePDF(fac, notaCredito);
                        AdemarHelper.CallBatchProcess(fac.claveNumerica);
                        SqlHelper.UpdateSuccessful(fac.id_documento, notaCredito);
                    }

                    }
                }
            }
            else{
                log.Info($"No se encontraron {(notaCredito ? "Notas de Credito" : "Facturas")} pendientes");
            }

        }

        private static string newClaveNumerica(string oldClaveNumerica){
            var sb = new StringBuilder(oldClaveNumerica);
            sb[41] = '3';
            return sb.ToString();
        }

        private static bool verificarInternet()
        {
            log.Info("Verificando conexion a intenet");

            try
            {
                using (var client = new WebClient())
                using (client.OpenRead("http://www.google.com"))
                {
                    log.Info("Conexion exitosa");
                    return true;
                }
            }
            catch
            {
                log.Warn("No se pudo conectar a internet");
                return false;
            }
        }

    }
}
