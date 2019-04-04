using System;
using System.IO;
using System.Text;
using DinkToPdf;
using FacElec.model;
using Humanizer;
using log4net;

namespace FacElec.helpers
{
    public static class PdfHelper
    {
        internal static ILog log;

        public static bool GeneratePDF(Factura factura, bool notadeCredito)
        {
            log.Info($"Generando el PDF");

            var cliente = factura.cliente[0];

            string formatedId = formatIndentificacion(cliente);
            string humanizedTotal = humanizeTotal(factura);
            string formatedDireccion = XmlHelper.formatedDireccion(cliente);

            string consecutivoFactura = notadeCredito
                         ? factura.claveNumericaFactura.Substring(21, 20)
                         : "";

            try
            {

                string cssPath = Path.Combine(Directory.GetCurrentDirectory(), "dist/template.css");

                var sb = new StringBuilder();
                sb.AppendFormat(@"
                           <html>
                             <head>
                             </head>
                             <body>
                                <div class='header row'>
                                    <h1>{0}</h1>
                                    <h1>Clave Numérica {1}</h1>
                                    {10}
                                    <h3 class='date'><strong>Fecha de emisión:</strong> {2}</h3>
                                </div>
                                <div class='header row'>
                                    <div class='col-50'>
                                        <h1><strong>Comercial Pozos de Santa Ana S.A</strong></h1>
                                        <h2><strong>Comercial Pozos de Santa Ana S.A</strong></h2>
                                        <h2><strong>Cédula Jurídica: </strong> 3-101-159911</h2>
                                    </div>
                                    <div class='col-50'>
                                       <h2><strong>Tel.: </strong>2282-6030 / 2282-4516 / 2282-1420 / 2282-1889 / 2282-4571 </h2> 
                                       <h2><strong>Correo: </strong>comercialpozos2@hotmail.com</h2>
                                       <h2><strong>Direccion: </strong>San José, Santa Ana, Pozos. 100 metros sur de la iglesia de pozos.</h2>  
                                    </div>
                                </div>
                                
                                 <div class='header row'>
                                    <div class='col-50'>
                                       <h2><strong>Receptor: </strong>{3}</h2> 
                                       <h2>{4}</h2>
                                       <h2>{5}</h2>
                                       <h2>{6}</h2>
                                       <h2>{7}</h2>
                                    </div>
                                    <div class='col-50'>
                                       <h2><strong>Número de referencia: </strong>{8}</h2> 
                                       <h2><strong>Condición de venta: </strong>{9}</h2>  
                                    </div>
                                </div>

                                <div class='row'>
                                    <h1 align='center'> Lineas de Detalle</h1>
                                </div>

                                <table align='center'>
                                    <tr class='table-header'>
                                        <th>Código</th>
                                        <th>Cantidad</th>
                                        <th>Unidad Medida</th>
                                        <th>Descipción del Producto</th>
                                        <th>Consumidor</th>
                                        <th>Precio Unitario</th>
                                        <th>Descuento</th>
                                        <th>SubTotal</th>
                                    </tr>", 
                                $"{(notadeCredito ? "Nota de Crédito" : "Factura")} No {factura.numConsecutivo}", 
                                factura.claveNumerica, factura.fecha,
                                factura.clienteTributa ? cliente.nombre : "Cliente Contado",
                                factura.clienteTributa ? $"<strong>Identificación: </strong>{formatedId}" :"",
                                factura.clienteTributa ? $"<strong>Teléfono: </strong>{cliente.telefono}" : "", 
                                factura.clienteTributa ? $"<strong>Correo: </strong>{cliente.email }" : "", 
                                factura.clienteTributa ? $"<strong>Dirección: </strong>{formatedDireccion}" : "",
                                factura.id_documento,
                                $"{(factura.plazo == 0 ? "Contado" : $"Crédito a {factura.plazo} dias plazo")}",
                                notadeCredito ? $"<h1>Factura de Referencia: {consecutivoFactura}" : "");

                foreach (factura_Detalle detalle in factura.factura_Detalle)
                {
                    sb.AppendFormat(@"<tr class='table-row' align='center'>
                                    <td>{0}</td>
                                    <td>{1}</td>
                                    <td>{2}</td>
                                    <td align='left'>{3}</td>
                                    <td>₡{4}</td>
                                    <td>₡{5}</td>   
                                    <td>{6}</td>
                                    <td>₡{7}</td>
                                  </tr>", detalle.Id_Producto,
                                    detalle.cantidad,
                                    detalle.unidadString,
                                    detalle.producto[0].nombre,
                                    detalle.consumidor.ToString("N2"),
                                    detalle.precio.ToString("N2"),
                                    $"{Convert.ToInt32(detalle.descuento)}%",
                                    $"{detalle.montoTotal.ToString("n2")}{((!detalle.IV) ? "*" : "")}");
                }

                sb.AppendFormat(@"
                                </table>
                                <div>
                                    <h3>{6}</d3>
                                </div>
                                <div class='totales'>
                                       <table class='tabla-totales' cellpadding='10'>
                                            <tr class='row-totales'>
                                                <td class='toti'><strong>Gravado:</strong></td>
                                                <td> ₡</td>
                                                <td class='toti'>{0}</td>
                                            </tr>
                                            <tr class='row-totales'>
                                                <td class='toti'><strong>Exento:</strong></td>
                                                <td> ₡</td>
                                                <td class='toti'>{1}</td>
                                            </tr>
                                            <tr class='row-totales'>
                                                <td class ='toti'><strong>Descuento:</strong></td>
                                                <td> ₡</td>
                                                <td class='toti'>{2}</td>
                                            </tr>
                                            <tr class='row-totales'>
                                                <td class ='toti'><strong>Impuestos:</strong></td>
                                                <td> ₡</td>
                                                <td class='toti'>{3}</td>
                                            </tr>
                                            <tr class='row-totales'>
                                                <td class ='toti'><strong>Total {7}:</strong></td>
                                                <td> ₡</td>
                                                <td class='toti'>{4}</td>
                                            </tr>                                       
                                       </table>  
                                       <p>{5}</p>
                                </div>                              
                                <div class='header foota'>
                                    <h1>Emitida confirme lo establecido en la resolución de Facturación Electrónica, No DGT-R-48-2016 siete de octubre
                                        de dos mil dieciseis de la Direccion General de Tributación</h1>                     
                                </div>
                            </body>
                        </html>", factura.totalGravado.ToString("N2"),
                                factura.totalExento.ToString("N2"),
                                factura.totalDescuentos.ToString("N2"),
                                factura.totalImpuestos.ToString("N2"),
                                factura.totalComprobante.ToString("N2"),
                                humanizedTotal,
                                (factura.totalDescuentos > 0) ? "<strong>Motivo del descuento: </strong>Pronto Pago" : "",
                                notadeCredito ? "Nota de Crédito" : "Factura"
                               );

                var fileName = $"C://DSign//Temp//{factura.claveNumerica}.pdf";
                FileInfo file = new FileInfo(fileName);
                file.Directory.Create();

                var converter = new BasicConverter(new PdfTools());
                var doc = new HtmlToPdfDocument()
                {
                    GlobalSettings = {
                    ColorMode = ColorMode.Color,
                    Orientation = Orientation.Portrait,
                    PaperSize = PaperKind.Letter,
                    Out = fileName
            },
                    Objects = {
                    new ObjectSettings() {
                        PagesCount = true,
                        HtmlContent = sb.ToString(),
                            WebSettings = { DefaultEncoding = "utf-8", UserStyleSheet = cssPath},
                            FooterSettings = {
                                FontSize = 9,
                                Right = "Pagina [page] de [toPage]",
                                Line = true,
                                Spacing = 2.812
                        }
                }
            }
                };

                converter.Convert(doc);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
                return false;
            }
            return true;

        }

        static string formatIndentificacion(cliente cliente)
        {
            var formatedId = "";

            try
            {
                switch (cliente.tipoIdentificacion)
                {
                    case 0:
                        {
                            formatedId = $"{cliente.identificacion.Substring(0, 1)}-{cliente.identificacion.Substring(1, 4)}-{cliente.identificacion.Substring(5)}";
                            break;
                        }
                    case 1:
                        {
                            formatedId = $"{cliente.identificacion.Substring(0, 1)}-{cliente.identificacion.Substring(1, 3)}-{cliente.identificacion.Substring(4)}";
                            break;
                        }
                    default:
                        {
                            formatedId = cliente.identificacion;
                            break;
                        }
                }
            }
            catch
            {
                formatedId = cliente.identificacion;
            }

            return formatedId;
        }

        static string humanizeTotal(Factura factura)
        {

            var decimales = (factura.totalComprobante - Math.Truncate(factura.totalComprobante)).ToString().Remove(0, 2);
            try
            {
                decimales = decimales.Remove(2);
                return int.Parse(factura.totalComprobante.ToString().Remove(factura.totalComprobante.ToString().IndexOf('.'))).ToWords(new System.Globalization.CultureInfo("es")) +
                                       $" con {int.Parse(decimales).ToWords(new System.Globalization.CultureInfo("es"))} decimos";
            }
            catch {
                
            }
            return "";
        }
    }
}
