using System;
using System.Diagnostics;
using log4net;

namespace FacElec.helpers
{
    public static class AdemarHelper
    {
        internal static ILog log;

        static public void CallBatchProcess (string claveFactura){
            log.Info($"Ejecutando proceso de sincronizacion y envio de factura");

            try
            {
                var batCommand = $"DFD {claveFactura} -Q -M D";
                //var batCommand = "C:\\facelecCP\\bat.bat";

                Console.WriteLine($"Ejecutando comando: {batCommand}");

                var proc = new Process();
                proc.StartInfo.FileName = "C:\\DSign\\ExeFirmaFactura\\FacturaElectronicaCR_CS.exe";
                proc.StartInfo.Arguments = batCommand;
                proc.Start();

            }
            catch (Exception ex){
                log.Error(ex.Message);
            }
        }
    }
}
