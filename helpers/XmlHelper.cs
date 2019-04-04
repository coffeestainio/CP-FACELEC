using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using FacElec.model;
using log4net;

namespace FacElec.helpers
{
    public static class XmlHelper
    {
        internal static ILog log;

        public static bool GenerateXML(Factura factura, bool notaDeCredito)
        {
            log.Info($"Generando el XML");

            try
            {
                var cliente = factura.cliente[0];

                XNamespace tribunet = notaDeCredito 
                    ? "https://tribunet.hacienda.go.cr/docs/esquemas/2017/v4.2/notaCreditoElectronica"
                    : "https://tribunet.hacienda.go.cr/docs/esquemas/2017/v4.2/facturaElectronica";
                XNamespace xsi = "http://www.w3.org/2001/XMLSchema-instance";
                XNamespace xsd = "http://www.w3.org/2001/XMLSchema";

                var xmlDoc = new XDocument(
                        new XDeclaration("1.0", "utf-8", ""),
                    new XElement(tribunet + (notaDeCredito ? "NotaCreditoElectronica" : "FacturaElectronica"),                                               
                                              new XAttribute(XNamespace.Xmlns + "xsi", xsi),
                                              new XAttribute(XNamespace.Xmlns + "xsd", xsd),

                                              new XElement(tribunet + "Clave", factura.claveNumerica),
                                              new XElement(tribunet + "NumeroConsecutivo", factura.numConsecutivo),
                                              new XElement(tribunet + "FechaEmision", factura.fecha),

                                              new XElement(tribunet + "Emisor",
                                                                 new XElement(tribunet + "Nombre", "Comercial Pozos S.A"),
                                                                 new XElement(tribunet + "Identificacion",
                                                                              new XElement(tribunet + "Tipo", "02"),
                                                                              new XElement(tribunet + "Numero", "3101159911")
                                                                             ),
                                                           new XElement(tribunet + "NombreComercial", "Comercial Pozos S.A"),
                                                           new XElement(tribunet + "Ubicacion",
                                                                        new XElement(tribunet + "Provincia", 01),
                                                                        new XElement(tribunet + "Canton", "09"),
                                                                        new XElement(tribunet + "Distrito", "03"),
                                                                        new XElement(tribunet + "Barrio", "01"),
                                                                        new XElement(tribunet + "OtrasSenas", "100 Sur Iglesia Pozos, Santa Ana")
                                                                       ),
                                                           new XElement(tribunet + "Telefono",
                                                                        new XElement(tribunet + "CodigoPais", "506"),
                                                                        new XElement(tribunet + "NumTelefono", "22826030")
                                                                       ),
                                                           new XElement(tribunet + "CorreoElectronico", "comercialpozos2@hotmail.com")
                                                          ),                                           
                                                           GenerateReceptor(factura.clienteTributa, cliente, tribunet),
                                                           new XElement(tribunet + "CondicionVenta", factura.condicionVenta.ToString("00")),
                                                           new XElement(tribunet + "PlazoCredito", factura.plazo),
                                                           new XElement(tribunet + "MedioPago", "01"),
                                                           GenerateDetailsXml(factura.factura_Detalle, tribunet),

                                                           new XElement(tribunet + "ResumenFactura",
                                                                        new XElement(tribunet + "CodigoMoneda", "CRC"),
                                                                        new XElement(tribunet + "TipoCambio", 1),
                                                                         new XElement(tribunet + "TotalServGravados", 0),
                                                                         new XElement(tribunet + "TotalServExentos", 0),
                                                                        new XElement(tribunet + "TotalMercanciasGravadas", factura.totalGravado),
                                                                        new XElement(tribunet + "TotalMercanciasExentas", factura.totalExento),
                                                                        new XElement(tribunet + "TotalGravado", factura.totalGravado),
                                                                        new XElement(tribunet + "TotalExento", factura.totalExento),
                                                                        new XElement(tribunet + "TotalVenta", factura.total),
                                                                        new XElement(tribunet + "TotalDescuentos", factura.totalDescuentos),
                                                                        new XElement(tribunet + "TotalVentaNeta", factura.totalVentaNeta),
                                                                        new XElement(tribunet + "TotalImpuesto", factura.totalImpuestos),
                                                                        new XElement(tribunet + "TotalComprobante", factura.totalComprobante)
                                                                         ),
                                                           notaDeCredito ? generateRefencia(factura, tribunet) : null,
                                                           new XElement(tribunet + "Normativa",
                                                                        new XElement(tribunet + "NumeroResolucion", "DGT-R-48-2016"),
                                                                        new XElement(tribunet + "FechaResolucion", "2016-10-07 10:22:22"))
                                                           )
                );
                storeXml(xmlDoc, factura.claveNumerica);
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }
            return true;
        }

        static XElement GenerateReceptor (bool clienteTributa, cliente cliente, XNamespace tribunet) {

            log.Info($"Generando datos del cliente receptor: {clienteTributa}");

            return clienteTributa
                ? new XElement(tribunet + "Receptor",
                                                             new XElement(tribunet + "Nombre", cliente.nombre),
                                                             new XElement(tribunet + "Identificacion",
                                                                          new XElement(tribunet + "Tipo", cliente.tipoIdentificacion.ToString("00")),
                                                                          new XElement(tribunet + "Numero", cliente.identificacion.Trim().Replace("-",""))
                                                                         ),
                                                       new XElement(tribunet + "NombreComercial", cliente.nombre),
                                                       new XElement(tribunet + "Ubicacion",
                                                                    new XElement(tribunet + "Provincia", cliente.provincia),
                                                                    new XElement(tribunet + "Canton", cliente.canton.ToString("00")),
                                                                    new XElement(tribunet + "Distrito", cliente.distrito.ToString("00")),
                                                                    new XElement(tribunet + "Barrio", "01"),
                                                                    new XElement(tribunet + "OtrasSenas", cliente.direccion)
                                                                   ),
                                                       new XElement(tribunet + "Telefono",
                                                                    new XElement(tribunet + "CodigoPais", "506"),
                                                                    new XElement(tribunet + "NumTelefono", cliente.telefono.Trim().Replace("-",""))
                                                                   ),
                                                       new XElement(tribunet + "CorreoElectronico", cliente.email)
                            )
                : null;
        }

        static XElement generateRefencia(Factura factura, XNamespace tribunet)
        {
            return new XElement(tribunet + "InformacionReferencia",
                                             new XElement(tribunet + "TipoDoc", "01"),
                                             new XElement(tribunet + "Numero", factura.claveNumericaFactura),
                                             new XElement(tribunet + "FechaEmision", factura.fechaEmisionFactura),
                                             new XElement(tribunet + "Codigo", "03"),
                                             new XElement(tribunet + "Razon", "Devolucion")
                               );
        }

        private static XElement GenerateDetailsXml(List<factura_Detalle> detalles, XNamespace tribunet){
                var xml = new XElement(tribunet + "DetalleServicio");
            var i = 1;

            foreach (factura_Detalle detalle in detalles)
            {
                xml.Add(new XElement(tribunet + "LineaDetalle",
                                     new XElement(tribunet + "NumeroLinea", i),
                                     new XElement(tribunet + "Codigo",
                                                  new XElement(tribunet + "Tipo", "01"),
                                                  new XElement(tribunet + "Codigo", detalle.producto[0].id_producto)
                                                 ),

                                     new XElement(tribunet + "Cantidad", $"{detalle.cantidad.ToString("00")}.00"),
                                     new XElement(tribunet + "UnidadMedida", "Unid"),
                                     new XElement(tribunet + "UnidadMedidaComercial", detalle.unidadString),
                                     new XElement(tribunet + "Detalle", detalle.producto[0].nombre),
                                     new XElement(tribunet + "PrecioUnitario", detalle.precio),
                                     new XElement(tribunet + "MontoTotal", detalle.montoTotal),
                                     (detalle.descuento > 0) ? new XElement(tribunet + "MontoDescuento", detalle.montoDescuento) : null,
                                     (detalle.descuento > 0) ? new XElement(tribunet + "NaturalezaDescuento", "Pronto pago") : null,
                                     new XElement(tribunet + "SubTotal", detalle.subtotal),
                                     new XElement(tribunet + "Impuesto",
                                                           new XElement(tribunet + "Codigo","01"),
                                                           new XElement(tribunet + "Tarifa", (detalle.IV == true) ? "13.00" : "00.00"),
                                                            new XElement(tribunet + "Monto", detalle.montoImpuesto)
                                                 ),
                                     new XElement(tribunet + "MontoTotalLinea", detalle.montoTotalLinea)
                              
                                )
                       );
                i++;
            }

            return xml;

        }

        public static void storeXml(XDocument xmlDoc, string claveNumerica){
            var fileName = $"C://DSign//Temp//{claveNumerica}.xml";
            System.IO.FileInfo file = new System.IO.FileInfo(fileName);
            file.Directory.Create();
            xmlDoc.Save(fileName);
        }

        public static string formatedDireccion (cliente cliente){
            var Xdoc = XElement.Load("provincias.xml");

            var provincia = "";
            var canton = "";

            var query = from d in Xdoc.Descendants("provincia")
                        where d.Element("codigo").Value.Equals(cliente.provincia.ToString())
                        select d;

            var subquery = from c in query.Descendants("canton")
                           where c.Element("codigo").Value.Equals(cliente.canton.ToString())
                           select c;

            provincia = query.First().Element("nombre").Value;
            canton = subquery.First().Element("nombre").Value;

            return $"{provincia}, {canton}. {cliente.direccion}";
        }

    }
}
