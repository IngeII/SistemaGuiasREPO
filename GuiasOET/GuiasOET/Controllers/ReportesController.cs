using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using GuiasOET.Models;
using System.Collections;
using System.Diagnostics;
using System.Web.UI.HtmlControls;
using System.Windows.Forms;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;
using iTextSharp.text.html.simpleparser;
using System.IO;
using System.Web.Hosting;
using System.Web.UI;

namespace GuiasOET.Controllers
{
    public class ReportesController : Controller
    {

        int LIMITE_PERSONAS = 12;

        /*Método para cargar el combobox de guías*/
        private void CargarGuiasDropDownList(object guiaSeleccionado = null)
        {

            string rol = Session["RolUsuarioLogueado"].ToString();
            List<SelectListItem> items = new List<SelectListItem>();


            if (rol != null)
            {
                if (rol.Contains("Global"))
                {
                    var guias = baseDatos.GUIAS_EMPLEADO.Where(e => e.TIPOEMPLEADO.Contains("Guía"));
                    var list = new SelectList(guias, "CEDULA", "NOMBREEMPLEADO");
                    ViewData["GUIAS"] = list;
                }
                else if (rol.Contains("Interno"))
                {
                    string estacion = Session["EstacionUsuarioLogueado"].ToString();
                    var guias = baseDatos.GUIAS_EMPLEADO.Where(e => e.TIPOEMPLEADO.Contains("Guía") && e.NOMBREESTACION.Contains(estacion));
                    var list = new SelectList(guias, "CEDULA", "NOMBREEMPLEADO");
                    ViewData["GUIAS"] = list;
                }


                // ViewBag.GUIA = items;
            }



        }


        // GET: Reportes
        public ActionResult Index()
        {
            CargarGuiasDropDownList();
            return View();
        }

        private Entities1 baseDatos = new Entities1();

        [HttpGet]
        public ActionResult ReportesReservaciones(string fechaDesde, string fechaHasta, int? page)
        {

            /* Se define tamaño de la pagina */
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            ViewBag.pageNumber = pageNumber;

            //Todas las reservaciones del sistema
            var reservacion = from r in baseDatos.GUIAS_RESERVACION select r;

            return View();
        }

        public ActionResult PersonasAtendidas(string fechaDesde, string fechaHasta)
        {

            ReportesModelo reportes = new ReportesModelo();
            DateTime fechaD;
            DateTime fechaH;
            List<GUIAS_RESERVACION> reservaciones = new List<GUIAS_RESERVACION>();

            //Lista que contiene una copia de las reservaciones
            List<GUIAS_RESERVACION> copiaReservaciones = new List<GUIAS_RESERVACION>();

            string rol = Session["RolUsuarioLogueado"].ToString();
            string estacion = "";

            //Todas las reservaciones con guias
            //LisreservacionAsignada = from p in baseDatos.GUIAS_ASIGNACION select p;


            //Reserva que tiene algún guía asignado
            GUIAS_ASIGNACION reservaConGuias = null;


            //Reserva que tiene algún guía asignado
            List<GUIAS_RESERVACION> reservas = new List<GUIAS_RESERVACION>();


            //Lista que contiene los guias
            List<IEnumerable<GUIAS_EMPLEADO>> guiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();


            //Lista que contiene los guias de todas las reservaciones
            List<IEnumerable<GUIAS_EMPLEADO>> totalGuiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();

            //Lista que contiene el total de guias para cada reservacion
            IEnumerable<GUIAS_EMPLEADO> totalGuias;

            DateTime fechaReservacion;

            List<GUIAS_TURNO> turnoReservasAsignadas = new List<GUIAS_TURNO>();

            List<List<GUIAS_ASIGNACION>> listaReservasAsignadas = new List<List<GUIAS_ASIGNACION>>();

            GUIAS_RESERVACION datosReserva;

            List<List<GUIAS_RESERVACION>> reservasOrdenadasPorFecha = new List<List<GUIAS_RESERVACION>>();


            int contador = 0;

            Debug.WriteLine("fecha desde es: " + fechaDesde);

            if (rol.Contains("Global"))
            {
                //Se verifica que ambas fechas no sean vacías
                if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaD = Convert.ToDateTime(fechaDesde);
                    fechaH = Convert.ToDateTime(fechaHasta);

                    //Todas las reservaciones del sistema en el rango de fechas establecido
                    var reservacionAuxiliar = baseDatos.GUIAS_RESERVACION.Where(e => e.FECHAENTRA >= fechaD && e.FECHAENTRA <= fechaH);

                    //Se ordenan las reservaciones por fecha
                    reservacionAuxiliar = reservacionAuxiliar.OrderBy(r => r.FECHAENTRA);

                    foreach (var row in reservacionAuxiliar)
                    {
                        //Se guardan todas las asignaciones de la reserva
                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        Debug.WriteLine("cantidad en reservaciones asignadas: " + reservacionAsignada.Count());
                        Debug.WriteLine("el num de reservacion es: " + row.NUMERORESERVACION);
                        listaReservasAsignadas.Add(reservacionAsignada.ToList());

                        if (reservacionAsignada != null)
                        {

                            foreach (var reservaAsig in reservacionAsignada)
                            {
                                datosReserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(e => e.NUMERORESERVACION.Equals(reservaAsig.NUMERORESERVACION));
                                reservaciones.Add(datosReserva);

                                if (reservaAsig.GUIAS_TURNO.Equals(null))
                                {
                                    turnoReservasAsignadas.Add(null);
                                }
                                else
                                {

                                    GUIAS_TURNO turno = baseDatos.GUIAS_TURNO.FirstOrDefault(i => i.NOMBRETURNO.Equals(reservaAsig.GUIAS_TURNO.NOMBRETURNO));
                                    turnoReservasAsignadas.Add(turno);
                                }


                                //Se obtienen todos los guías asociados a una determinada reservación                     
                                GUIAS_EMPLEADO guiasReserva = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(x => x.CEDULA.Equals(reservaAsig.CEDULAGUIA));
                                reportes.empleadosReserva.Add(guiasReserva);
                                var guias = baseDatos.GUIAS_EMPLEADO.Where(c => c.CEDULA.Equals(reservaAsig.CEDULAGUIA));
                                guiasAsignados.Add(guias);


                                //Se coloca en una misma posición todos los guías asociados a la reservación
                                totalGuias = guiasAsignados.ElementAt(0);

                                for (int y = 1; y < guiasAsignados.Count(); ++y)
                                {
                                    totalGuias = totalGuias.Concat(guiasAsignados.ElementAt(y));
                                }

                                totalGuiasAsignados.Add(totalGuias);
                                reportes.empleadosReservaciones.Add(totalGuias.ToList());
                                reportes.listaAsignacionReservaciones.Add(reservaAsig);
                            }


                            //Se limpia la lista de los guías asignados
                            guiasAsignados.Clear();


                        }

                    }

                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservaciones)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);
                    }

                    reportes.fechasReservaciones = reportes.fechasReservaciones.Distinct().ToList();
                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.listaReservacionesAsignadas = listaReservasAsignadas;
                    reportes.totalReservaciones = reservaciones.ToList();
                    reportes.turnoReservacion = turnoReservasAsignadas;
                }
            }
            else if (rol.Contains("Interno"))
            {
                estacion = Session["EstacionUsuarioLogueado"].ToString();

                //Se verifica que ambas fechas no sean vacías
                if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaD = Convert.ToDateTime(fechaDesde);
                    fechaH = Convert.ToDateTime(fechaHasta);

                    //Todas las reservaciones del sistema en el rango de fechas establecido
                    var reservacionAuxiliar = baseDatos.GUIAS_RESERVACION.Where(e => e.NOMBREESTACION.Equals(estacion) && e.FECHAENTRA >= fechaD && e.FECHAENTRA <= fechaH);

                    //Se ordenan las reservaciones por fecha
                    reservacionAuxiliar = reservacionAuxiliar.OrderBy(r => r.FECHAENTRA);

                    foreach (var row in reservacionAuxiliar)
                    {
                        //Se guardan todas las asignaciones de la reserva
                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        listaReservasAsignadas.Add(reservacionAsignada.ToList());

                        if (reservacionAsignada != null)
                        {

                            foreach (var reservaAsig in reservacionAsignada)
                            {
                                datosReserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(e => e.NUMERORESERVACION.Equals(reservaAsig.NUMERORESERVACION));
                                reservaciones.Add(datosReserva);

                                if (reservaAsig.GUIAS_TURNO.Equals(null))
                                {
                                    turnoReservasAsignadas.Add(null);
                                }
                                else
                                {

                                    GUIAS_TURNO turno = baseDatos.GUIAS_TURNO.FirstOrDefault(i => i.NOMBRETURNO.Equals(reservaAsig.GUIAS_TURNO.NOMBRETURNO));
                                    turnoReservasAsignadas.Add(turno);
                                }


                                //Se obtienen todos los guías asociados a una determinada reservación                     
                                GUIAS_EMPLEADO guiasReserva = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(x => x.CEDULA.Equals(reservaAsig.CEDULAGUIA));
                                reportes.empleadosReserva.Add(guiasReserva);
                                var guias = baseDatos.GUIAS_EMPLEADO.Where(c => c.CEDULA.Equals(reservaAsig.CEDULAGUIA));
                                guiasAsignados.Add(guias);


                                //Se coloca en una misma posición todos los guías asociados a la reservación
                                totalGuias = guiasAsignados.ElementAt(0);

                                for (int y = 1; y < guiasAsignados.Count(); ++y)
                                {
                                    totalGuias = totalGuias.Concat(guiasAsignados.ElementAt(y));
                                }

                                totalGuiasAsignados.Add(totalGuias);
                                reportes.empleadosReservaciones.Add(totalGuias.ToList());
                                reportes.listaAsignacionReservaciones.Add(reservaAsig);
                            }

                            //Se limpia la lista de los guías asignados
                            guiasAsignados.Clear();


                        }

                    }

                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservaciones)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);
                    }

                    reportes.fechasReservaciones = reportes.fechasReservaciones.Distinct().ToList();
                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.listaReservacionesAsignadas = listaReservasAsignadas;
                    reportes.totalReservaciones = reservaciones.ToList();
                    reportes.turnoReservacion = turnoReservasAsignadas;
                }
            }


                    //Se calcula la cantidad de personas por cada reservacion
                    reportes.subTotales = calcularSubTotales(reportes.totalReservaciones);


                    return View(reportes);

        }


        public ActionResult PDFReporte(string fechaDesde, string fechaHasta)
        {
            ReportesModelo reportes = new ReportesModelo();
            DateTime fechaD;
            DateTime fechaH;
            List<GUIAS_RESERVACION> reservaciones = new List<GUIAS_RESERVACION>();

            //Lista que contiene una copia de las reservaciones
            List<GUIAS_RESERVACION> copiaReservaciones = new List<GUIAS_RESERVACION>();

            string rol = Session["RolUsuarioLogueado"].ToString();
            string estacion = "";

            //Reserva que tiene algún guía asignado
            GUIAS_ASIGNACION reservaConGuias = null;


            //Reserva que tiene algún guía asignado
            GUIAS_RESERVACION reserva;


            //Lista que contiene todas las reservaciones con guias 
            List<GUIAS_ASIGNACION> reservacionesConAsignacion = new List<GUIAS_ASIGNACION>();

            //Lista que contiene los guias
            List<IEnumerable<GUIAS_EMPLEADO>> guiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();


            //Lista que contiene los guias de todas las reservaciones
            List<IEnumerable<GUIAS_EMPLEADO>> totalGuiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();

            //Lista que contiene el total de guias para cada reservacion
            IEnumerable<GUIAS_EMPLEADO> totalGuias;

            DateTime fechaReservacion;

            List<int> indiceReservacionesAsignadas = new List<int>();

            int contador = 0;

            List<GUIAS_TURNO> turnoReservasAsignadas = new List<GUIAS_TURNO>();


            Debug.WriteLine("FECHA DESDE " + fechaDesde);
            Debug.WriteLine("FECHA HASTA " + fechaHasta);


            if (rol.Contains("Global"))
            {
                //Se verifica que ambas fechas no sean vacías
                if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaD = Convert.ToDateTime(fechaDesde);
                    fechaH = Convert.ToDateTime(fechaHasta);

                    //Todas las reservaciones del sistema en el rango de fechas establecido
                    var reservacionAuxiliar = baseDatos.GUIAS_RESERVACION.Where(e => e.FECHAENTRA >= fechaD && e.FECHAENTRA <= fechaH);

                    reservacionAuxiliar = reservacionAuxiliar.Distinct();

                    //Se ordenan las reservaciones por fecha

                    reservacionAuxiliar = reservacionAuxiliar.OrderBy(r => r.FECHAENTRA);


                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservacionAuxiliar)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);

                    }



                    foreach (var row in reservacionAuxiliar)
                    {


                        string a = row.NUMERORESERVACION;
                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        if (reservacionAsignada != null && reservacionAsignada.Count() > 0)
                        {

                            foreach (var asigna in reservacionAsignada)
                            {

                                if (asigna.GUIAS_TURNO == null)
                                {
                                    turnoReservasAsignadas.Add(null);
                                }
                                else
                                {
                                    GUIAS_TURNO turno = baseDatos.GUIAS_TURNO.FirstOrDefault(e => e.NOMBRETURNO.Equals(asigna.GUIAS_TURNO.NOMBRETURNO));
                                    turnoReservasAsignadas.Add(turno);
                                }
                                reservacionesConAsignacion.Add(asigna);

                            }

                            indiceReservacionesAsignadas.Add(0);
                        }
                        else
                        {
                            indiceReservacionesAsignadas.Add(-1);
                        }


                        reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));


                        if (reservaConGuias != null)
                        {
                            reserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(reservaConGuias.NUMERORESERVACION));
                            reservaciones.Add(reserva);



                            //Se obtienen todos los guías asociados a una determinada reservación
                            foreach (var row2 in reservacionAsignada)
                            {
                                var guias = baseDatos.GUIAS_EMPLEADO.Where(x => x.CEDULA.Equals(row2.CEDULAGUIA));
                                guiasAsignados.Add(guias);

                            }

                            //Se coloca en una misma posición todos los guías asociados a la reservación
                            totalGuias = guiasAsignados.ElementAt(0);

                            for (int y = 1; y < guiasAsignados.Count(); ++y)
                            {
                                totalGuias = totalGuias.Concat(guiasAsignados.ElementAt(y));
                            }

                            totalGuiasAsignados.Add(totalGuias);

                        }


                        //Se limpia la lista de los guías asignados
                        guiasAsignados.Clear();

                        ++contador;
                    }
                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList();
                    reportes.totalReservaciones = reservacionAuxiliar.ToList();
                    reportes.listaAsociacion = indiceReservacionesAsignadas;
                    reportes.turnoReservacion = turnoReservasAsignadas;
                    reportes.fechasReservaciones = reportes.fechasReservaciones.Distinct().ToList();



                }
            }
            else if (rol.Contains("Interno"))
            {
                //Se verifica que ambas fechas no sean vacías
                if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaD = Convert.ToDateTime(fechaDesde);
                    fechaH = Convert.ToDateTime(fechaHasta);

                    //Todas las reservaciones del sistema en el rango de fechas establecido
                    var reservacionAuxiliar = baseDatos.GUIAS_RESERVACION.Where(e => e.NOMBREESTACION.Equals(estacion) && e.FECHAENTRA >= fechaD && e.FECHAENTRA <= fechaH);

                    reservacionAuxiliar = reservacionAuxiliar.Distinct();

                    //Se ordenan las reservaciones por fecha

                    reservacionAuxiliar = reservacionAuxiliar.OrderBy(r => r.FECHAENTRA);


                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservacionAuxiliar)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);

                    }



                    foreach (var row in reservacionAuxiliar)
                    {


                        string a = row.NUMERORESERVACION;
                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        if (reservacionAsignada != null && reservacionAsignada.Count() > 0)
                        {

                            foreach (var asigna in reservacionAsignada)
                            {
                                indiceReservacionesAsignadas.Add(0);
                                if (asigna.GUIAS_TURNO == null)
                                {
                                    turnoReservasAsignadas.Add(null);
                                }
                                else
                                {
                                    GUIAS_TURNO turno = baseDatos.GUIAS_TURNO.FirstOrDefault(e => e.NOMBRETURNO.Equals(asigna.GUIAS_TURNO.NOMBRETURNO));
                                    turnoReservasAsignadas.Add(turno);
                                }
                                reservacionesConAsignacion.Add(asigna);

                            }


                        }
                        else
                        {
                            indiceReservacionesAsignadas.Add(-1);
                        }


                        reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));


                        if (reservaConGuias != null)
                        {
                            reserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(reservaConGuias.NUMERORESERVACION));
                            reservaciones.Add(reserva);



                            //Se obtienen todos los guías asociados a una determinada reservación
                            foreach (var row2 in reservacionAsignada)
                            {
                                var guias = baseDatos.GUIAS_EMPLEADO.Where(x => x.CEDULA.Equals(row2.CEDULAGUIA));
                                guiasAsignados.Add(guias);

                            }

                            //Se coloca en una misma posición todos los guías asociados a la reservación
                            totalGuias = guiasAsignados.ElementAt(0);

                            for (int y = 1; y < guiasAsignados.Count(); ++y)
                            {
                                totalGuias = totalGuias.Concat(guiasAsignados.ElementAt(y));
                            }

                            totalGuiasAsignados.Add(totalGuias);

                        }


                        //Se limpia la lista de los guías asignados
                        guiasAsignados.Clear();

                        ++contador;
                    }
                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList();
                    reportes.totalReservaciones = reservacionAuxiliar.ToList();
                    reportes.listaAsociacion = indiceReservacionesAsignadas;
                    reportes.turnoReservacion = turnoReservasAsignadas;
                    reportes.fechasReservaciones = reportes.fechasReservaciones.Distinct().ToList();

                }
            }



            //Se calcula la cantidad de personas por cada reservacion
            reportes.subTotales = calcularSubTotales(reportes.totalReservaciones);
            DownloadPDF("Reservaciones", reportes, "ReporteReservaciones");
            return RedirectToAction("ReportesReservaciones");
        }

        //Requiere: la vista, el modelo al que pertenece la vista, el nombre que se quiere que tenga el archivo
        //Modifica: se encarga de organizar la entrada de una vista, llamar al metodo que lo convierte a un doc itextsharp y que se pueda descargar como PDF
        //Regresa: N/A

        public void DownloadPDF(string viewName, object model, string nombreArchivo)
        {
            string HTMLContent = RenderRazorViewToString(viewName, model);

            Response.Clear();
            Response.ContentType = "application/pdf";
            Response.AddHeader("content-disposition", "attachment;filename=" + nombreArchivo + ".pdf");
            Response.Cache.SetCacheability(HttpCacheability.NoCache);
            Response.BinaryWrite(GetPDF(HTMLContent));
            Response.End();
        }

        //Requiere: la vista, el modelo al que pertenece la vista
        //Modifica: convierte la vista en un string para que pueda ser leido por itextsharp
        //Regresa: un string con la informacion de la vista
        public string RenderRazorViewToString(string viewName, object model)
        {
            // PRESTAMO pRESTAMO = db.PRESTAMOS.Find();
            ViewData.Model = model;
            using (var sw = new StringWriter())
            {
                var viewResult = ViewEngines.Engines.FindPartialView(ControllerContext, viewName);
                var viewContext = new ViewContext(ControllerContext, viewResult.View, ViewData, TempData, sw);
                viewResult.View.Render(viewContext, sw);
                viewResult.ViewEngine.ReleaseView(ControllerContext, viewResult.View);
                return sw.GetStringBuilder().ToString();
            }
        }

        //Requiere: una string parseada de la vista que se quiere convertir en PDF
        //Modifica: se encarga de pasar la vista HTML a un documento de itextsharp
        //Regresa: un byte con el documento 
        public byte[] GetPDF(string pHTML)
        {
            byte[] bPDF = null;

            MemoryStream ms = new MemoryStream();
            TextReader txtReader = new StringReader(pHTML);

            // Se crea un documneto de itextsharp
            Document doc = new Document(PageSize.A4, 25, 25, 25, 25);

            PdfWriter oPdfWriter = PdfWriter.GetInstance(doc, ms);

            // el htmlworker parsea el documento
            HTMLWorker htmlWorker = new HTMLWorker(doc);

            doc.Open();
            htmlWorker.StartDocument();

            // parsea el html en el doc
            htmlWorker.Parse(txtReader);

            htmlWorker.EndDocument();
            htmlWorker.Close();
            doc.Close();

            bPDF = ms.ToArray();

            return bPDF;
        }

        public ActionResult Reservaciones(string fechaDesde, string fechaHasta)
        {
         

            ReportesModelo reportes = new ReportesModelo();
            DateTime fechaD;
            DateTime fechaH;
            List<GUIAS_RESERVACION> reservaciones = new List<GUIAS_RESERVACION>();

            //Lista que contiene una copia de las reservaciones
            List<GUIAS_RESERVACION> copiaReservaciones = new List<GUIAS_RESERVACION>();

            string rol = Session["RolUsuarioLogueado"].ToString();
            string estacion = "";

            //Reserva que tiene algún guía asignado
            GUIAS_ASIGNACION reservaConGuias = null;


            //Reserva que tiene algún guía asignado
            GUIAS_RESERVACION reserva;


            //Lista que contiene todas las reservaciones con guias 
            List<GUIAS_ASIGNACION> reservacionesConAsignacion = new List<GUIAS_ASIGNACION>();

            //Lista que contiene los guias
            List<IEnumerable<GUIAS_EMPLEADO>> guiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();


            //Lista que contiene los guias de todas las reservaciones
            List<IEnumerable<GUIAS_EMPLEADO>> totalGuiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();

            //Lista que contiene el total de guias para cada reservacion
            IEnumerable<GUIAS_EMPLEADO> totalGuias;

            DateTime fechaReservacion;

            List<int> indiceReservacionesAsignadas = new List<int>();

            int contador = 0;

            List<GUIAS_TURNO> turnoReservasAsignadas = new List<GUIAS_TURNO>();



            if (rol.Contains("Global"))
            {
                //Se verifica que ambas fechas no sean vacías
                if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaD = Convert.ToDateTime(fechaDesde);
                    fechaH = Convert.ToDateTime(fechaHasta);

                    //Todas las reservaciones del sistema en el rango de fechas establecido
                    var reservacionAuxiliar = baseDatos.GUIAS_RESERVACION.Where(e => e.FECHAENTRA >= fechaD && e.FECHAENTRA <= fechaH);

                    reservacionAuxiliar = reservacionAuxiliar.Distinct();

                    //Se ordenan las reservaciones por fecha

                    reservacionAuxiliar = reservacionAuxiliar.OrderBy(r => r.FECHAENTRA);


                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservacionAuxiliar)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);

                    }



                    foreach (var row in reservacionAuxiliar)
                    {


                        string a = row.NUMERORESERVACION;
                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        if (reservacionAsignada != null && reservacionAsignada.Count() > 0)
                        {

                            foreach (var asigna in reservacionAsignada)
                            {
                                
                                if (asigna.GUIAS_TURNO == null)
                                {
                                    turnoReservasAsignadas.Add(null);
                                }
                                else
                                {
                                    GUIAS_TURNO turno = baseDatos.GUIAS_TURNO.FirstOrDefault(e => e.NOMBRETURNO.Equals(asigna.GUIAS_TURNO.NOMBRETURNO));
                                    turnoReservasAsignadas.Add(turno);
                                }
                                reservacionesConAsignacion.Add(asigna);

                            }

                            indiceReservacionesAsignadas.Add(0);
                        }
                        else
                        {
                            indiceReservacionesAsignadas.Add(-1);
                        }


                        reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));


                        if (reservaConGuias != null)
                        {
                            reserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(reservaConGuias.NUMERORESERVACION));
                            reservaciones.Add(reserva);



                            //Se obtienen todos los guías asociados a una determinada reservación
                            foreach (var row2 in reservacionAsignada)
                            {
                                var guias = baseDatos.GUIAS_EMPLEADO.Where(x => x.CEDULA.Equals(row2.CEDULAGUIA));
                                guiasAsignados.Add(guias);

                            }

                            //Se coloca en una misma posición todos los guías asociados a la reservación
                            totalGuias = guiasAsignados.ElementAt(0);

                            for (int y = 1; y < guiasAsignados.Count(); ++y)
                            {
                                totalGuias = totalGuias.Concat(guiasAsignados.ElementAt(y));
                            }

                            totalGuiasAsignados.Add(totalGuias);

                        }
                       

                        //Se limpia la lista de los guías asignados
                        guiasAsignados.Clear();

                        ++contador;
                    }
                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList();
                    reportes.totalReservaciones = reservacionAuxiliar.ToList();
                    reportes.listaAsociacion = indiceReservacionesAsignadas;
                    reportes.turnoReservacion = turnoReservasAsignadas;
                    reportes.fechasReservaciones = reportes.fechasReservaciones.Distinct().ToList();



                }
            }
            else if (rol.Contains("Interno"))
            {
                //Se verifica que ambas fechas no sean vacías
                if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaD = Convert.ToDateTime(fechaDesde);
                    fechaH = Convert.ToDateTime(fechaHasta);

                    //Todas las reservaciones del sistema en el rango de fechas establecido
                    var reservacionAuxiliar = baseDatos.GUIAS_RESERVACION.Where(e => e.NOMBREESTACION.Equals(estacion) && e.FECHAENTRA >= fechaD && e.FECHAENTRA <= fechaH);

                    reservacionAuxiliar = reservacionAuxiliar.Distinct();

                    //Se ordenan las reservaciones por fecha

                    reservacionAuxiliar = reservacionAuxiliar.OrderBy(r => r.FECHAENTRA);


                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservacionAuxiliar)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);

                    }



                    foreach (var row in reservacionAuxiliar)
                    {


                        string a = row.NUMERORESERVACION;
                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        if (reservacionAsignada != null && reservacionAsignada.Count() > 0)
                        {

                            foreach (var asigna in reservacionAsignada)
                            {
                                indiceReservacionesAsignadas.Add(0);
                                if (asigna.GUIAS_TURNO == null)
                                {
                                    turnoReservasAsignadas.Add(null);
                                }
                                else
                                {
                                    GUIAS_TURNO turno = baseDatos.GUIAS_TURNO.FirstOrDefault(e => e.NOMBRETURNO.Equals(asigna.GUIAS_TURNO.NOMBRETURNO));
                                    turnoReservasAsignadas.Add(turno);
                                }
                                reservacionesConAsignacion.Add(asigna);

                            }


                        }
                        else
                        {
                            indiceReservacionesAsignadas.Add(-1);
                        }


                        reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));


                        if (reservaConGuias != null)
                        {
                            reserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(reservaConGuias.NUMERORESERVACION));
                            reservaciones.Add(reserva);



                            //Se obtienen todos los guías asociados a una determinada reservación
                            foreach (var row2 in reservacionAsignada)
                            {
                                var guias = baseDatos.GUIAS_EMPLEADO.Where(x => x.CEDULA.Equals(row2.CEDULAGUIA));
                                guiasAsignados.Add(guias);

                            }

                            //Se coloca en una misma posición todos los guías asociados a la reservación
                            totalGuias = guiasAsignados.ElementAt(0);

                            for (int y = 1; y < guiasAsignados.Count(); ++y)
                            {
                                totalGuias = totalGuias.Concat(guiasAsignados.ElementAt(y));
                            }

                            totalGuiasAsignados.Add(totalGuias);

                        }
                        

                        //Se limpia la lista de los guías asignados
                        guiasAsignados.Clear();

                        ++contador;
                    }
                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList();
                    reportes.totalReservaciones = reservacionAuxiliar.ToList();
                    reportes.listaAsociacion = indiceReservacionesAsignadas;
                    reportes.turnoReservacion = turnoReservasAsignadas;
                    reportes.fechasReservaciones = reportes.fechasReservaciones.Distinct().ToList();

                }
            }



                //Se calcula la cantidad de personas por cada reservacion
            reportes.subTotales = calcularSubTotales(reportes.totalReservaciones);

            return View(reportes);
        }





        public List<string> calcularSubTotales(List<GUIAS_RESERVACION> reservaciones)
        {

            List<string> listaSubTotales = new List<string>();


            decimal cantidadPersonas = 0;
            decimal cantidadGuias = 0;
            decimal cantidadAsignada = 0;
            string distribucion = "";
            bool limite = false;


            for (int i = 0; i < reservaciones.Count(); ++i)
            {
                cantidadPersonas = (decimal)reservaciones.ElementAt(i).NUMEROPERSONAS;
                // cantidadGuias = totalGuiasAsignados.ElementAt(i).Count();

                while (cantidadPersonas > LIMITE_PERSONAS)
                {
                    limite = true;
                    cantidadAsignada = cantidadPersonas - LIMITE_PERSONAS;
                    distribucion = distribucion + 12 + "-";
                    cantidadPersonas = cantidadPersonas - LIMITE_PERSONAS;
                }

                if (limite)
                {
                    distribucion = distribucion + cantidadAsignada;
                }
                else
                {
                    cantidadAsignada = (decimal)reservaciones.ElementAt(i).NUMEROPERSONAS;
                    distribucion = Convert.ToString(cantidadAsignada);
                }
                  
                listaSubTotales.Add(distribucion);
                distribucion = "";
                cantidadAsignada = 0;
                limite = false;

            }

            return listaSubTotales;
        }


        [HttpGet]
        public ActionResult DiasLibres()
        {
            Debug.WriteLine("ENTRE AL GET DE DIAS LIBRES ");
            ReportesModelo reportes = new ReportesModelo();
            var semana = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.fechaLunes = DateTime.Now.DayOfWeek.ToString();
            string rol = Session["RolUsuarioLogueado"].ToString();
            var estacion = "";

            //Lista que asocia cada empleado que tiene un dia libre 
            List<GUIAS_EMPLEADO> empleadosConDiasLibres = new List<GUIAS_EMPLEADO>();

            //Lista de rol dias libres
            List<GUIAS_ROLDIASLIBRES> diasLibresEmpleados = new List<GUIAS_ROLDIASLIBRES>();

            //Lista de listas de dias libres de empleados
            List<List<GUIAS_ROLDIASLIBRES>> totalDiasLibresEmpleados = new List<List<GUIAS_ROLDIASLIBRES>>();

            DateTime semanaDiasLibres = DateTime.Now;

            DateTime fechaLimiteHasta = DateTime.Now;

            DateTime fecha;

            switch (DateTime.Now.DayOfWeek)
            {

                case System.DayOfWeek.Monday:

                    ViewBag.fechaLunes = DateTime.Now.ToString("dd/MM/yyyy");
                    semanaDiasLibres = DateTime.Now;
                    ViewBag.semanaABuscar = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();
                    ViewBag.fechaMartes = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.AddDays(4).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(5).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(6).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Tuesday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    semanaDiasLibres = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0));
                    fecha = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0));
                    ViewBag.semanaABuscar = String.Format("{0:yyyy-MM-dd}", fecha).Trim();
                    ViewBag.fechaMartes = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(4).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(5).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Wednesday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    fecha = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0));
                    ViewBag.semanaABuscar = String.Format("{0:yyyy-MM-dd}", fecha).Trim();
                    semanaDiasLibres = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0));
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(4).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Thursday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd/MM/yyyy");
                    fecha = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0));
                    ViewBag.semanaABuscar = String.Format("{0:yyyy-MM-dd}", fecha).Trim();
                    semanaDiasLibres = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0));
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Friday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0)).ToString("dd/MM/yyyy");
                    fecha = DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0));
                    ViewBag.semanaABuscar = String.Format("{0:yyyy-MM-dd}", fecha).Trim();
                    semanaDiasLibres = DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0));
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");

                    break;

                case System.DayOfWeek.Saturday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)).ToString("dd/MM/yyyy");
                    fecha = DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0));
                    ViewBag.semanaABuscar = String.Format("{0:yyyy-MM-dd}", fecha).Trim();
                    semanaDiasLibres = DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0));
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Sunday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(6, 0, 0, 0)).ToString("dd/MM/yyyy");
                    fecha = DateTime.Now.Subtract(new TimeSpan(6, 0, 0, 0));
                    ViewBag.semanaABuscar = String.Format("{0:yyyy-MM-dd}", fecha).Trim();
                    semanaDiasLibres = DateTime.Now.Subtract(new TimeSpan(6, 0, 0, 0));
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.ToString("dd/MM/yyyy");
                    break;

            }

            if (rol.Contains("Global"))
            {
                //Se obtienen todos los guías ordenados por nombre
                var empleados = baseDatos.GUIAS_EMPLEADO.Where(c => c.TIPOEMPLEADO.Contains("Interno") || c.TIPOEMPLEADO.Contains("Externo")).OrderBy(c => c.NOMBREEMPLEADO);
                fechaLimiteHasta = semanaDiasLibres.AddDays(7);

                foreach (var row in empleados)
                {
                    //Se obtienen los dias libres que corresponden a la semana seleccionada
                    var diasLibres = baseDatos.GUIAS_ROLDIASLIBRES.Where(e => e.FECHA >= semanaDiasLibres && e.FECHA <= fechaLimiteHasta);
                    diasLibres = diasLibres.OrderBy(e => e.FECHA);

                    //Se obtienen los dias libres del empleado
                    var numDiasLibre = diasLibres.Where(e => e.CEDULAINTERNO.Equals(row.CEDULA));

                    //Se verifica si el empleado tiene dia libre
                    if (numDiasLibre != null)
                    {
                        //En caso de tener días libres, se guardan estos días
                        totalDiasLibresEmpleados.Add(numDiasLibre.ToList());
                    }
                    else
                    {

                        //En caso de que no, se guarda un null
                        totalDiasLibresEmpleados.Add(null);

                    }


                }

                //Se guardan todos los empleados
                reportes.empleadosDiasLibres = empleados.ToList();

                //Se guardan todos los días libres
                reportes.diasLibres = totalDiasLibresEmpleados.ToList();


                for (int i = 0; i < reportes.diasLibres.Count(); ++i)
                {
                    if (reportes.diasLibres.ElementAt(i).Equals(null))
                    {
                        Debug.WriteLine("era vacio");
                    }
                    else
                    {
                        for (int j = 0; j < reportes.diasLibres.ElementAt(i).Count(); ++j)
                        {
                            Debug.WriteLine("dia libre: " + reportes.diasLibres.ElementAt(i).ElementAt(j).CEDULAINTERNO + " tipo de incapacidad " + reportes.diasLibres.ElementAt(i).ElementAt(j).TIPODIALIBRE);
                        }
                    }



                }
            }

            else if (rol.Contains("Local"))
            {
                estacion = Session["EstacionUsuarioLogueado"].ToString();

                //Se obtienen todos los guías ordenados por nombre de acuerdo a la estación del administrador logueado
                var empleados = baseDatos.GUIAS_EMPLEADO.Where(e => e.NOMBREESTACION.Equals(estacion) && (e.TIPOEMPLEADO.Contains("Interno") || e.TIPOEMPLEADO.Contains("Externo"))).OrderBy(c => c.NOMBREEMPLEADO);
                fechaLimiteHasta = semanaDiasLibres.AddDays(7);

                foreach (var row in empleados)
                {
                    //Se obtienen los dias libres que corresponden a la semana seleccionada
                    var diasLibres = baseDatos.GUIAS_ROLDIASLIBRES.Where(e => e.FECHA >= semanaDiasLibres && e.FECHA <= fechaLimiteHasta);
                    diasLibres = diasLibres.OrderBy(e => e.FECHA);

                    //Se obtienen los dias libres del empleado
                    var numDiasLibre = diasLibres.Where(e => e.CEDULAINTERNO.Equals(row.CEDULA));

                    //Se verifica si el empleado tiene dia libre
                    if (numDiasLibre != null)
                    {
                        //En caso de tener días libres, se guardan estos días
                        totalDiasLibresEmpleados.Add(numDiasLibre.ToList());
                    }
                    else
                    {
                        //En caso de que no, se guarda un null
                        totalDiasLibresEmpleados.Add(null);

                    }
                }

                //Se guardan todos los empleados
                reportes.empleadosDiasLibres = empleados.ToList();

                //Se guardan todos los días libres
                reportes.diasLibres = totalDiasLibresEmpleados.ToList();

            }

            return View(reportes);
        }

        [HttpPost]
        public ActionResult DiasLibres(DateTime semanaABuscar)
        {
            ViewBag.fechaLunes = semanaABuscar.ToString("dd/MM/yyyy");
            ViewBag.semanaABuscar = String.Format("{0:yyyy-MM-dd}", semanaABuscar).Trim();
            ViewBag.fechaMartes = semanaABuscar.AddDays(1).ToString("dd/MM/yyyy");
            ViewBag.fechaMiercoles = semanaABuscar.AddDays(2).ToString("dd/MM/yyyy");
            ViewBag.fechaJueves = semanaABuscar.AddDays(3).ToString("dd/MM/yyyy");
            ViewBag.fechaViernes = semanaABuscar.AddDays(4).ToString("dd/MM/yyyy");
            ViewBag.fechaSabado = semanaABuscar.AddDays(5).ToString("dd/MM/yyyy");
            ViewBag.fechaDomingo = semanaABuscar.AddDays(6).ToString("dd/MM/yyyy");

            string rol = Session["RolUsuarioLogueado"].ToString();
            var estacion = "";
            ReportesModelo reportes = new ReportesModelo();

            //Lista que asocia cada empleado que tiene un dia libre 
            List<GUIAS_EMPLEADO> empleadosConDiasLibres = new List<GUIAS_EMPLEADO>();

            //Lista de rol dias libres
            List<GUIAS_ROLDIASLIBRES> diasLibresEmpleados = new List<GUIAS_ROLDIASLIBRES>();

            //Lista de listas de dias libres de empleados
            List<List<GUIAS_ROLDIASLIBRES>> totalDiasLibresEmpleados = new List<List<GUIAS_ROLDIASLIBRES>>();

            DateTime fechaLimiteHasta = DateTime.Now;

            if (rol.Contains("Global"))
            {
                //Se obtienen todos los guías ordenados por nombre
                var empleados = baseDatos.GUIAS_EMPLEADO.Where(c => c.TIPOEMPLEADO.Contains("Interno") || c.TIPOEMPLEADO.Contains("Externo")).OrderBy(c => c.NOMBREEMPLEADO);
                fechaLimiteHasta = semanaABuscar.AddDays(7);

                foreach (var row in empleados)
                {
                    //Se obtienen los dias libres que corresponden a la semana seleccionada
                    var diasLibres = baseDatos.GUIAS_ROLDIASLIBRES.Where(e => e.FECHA >= semanaABuscar && e.FECHA <= fechaLimiteHasta);
                    diasLibres = diasLibres.OrderBy(e => e.FECHA);

                    //Se obtienen los dias libres del empleado
                    var numDiasLibre = diasLibres.Where(e => e.CEDULAINTERNO.Equals(row.CEDULA));


                    //Se verifica si el empleado tiene dia libre
                    if (numDiasLibre != null)
                    {
                        //En caso de tener días libres, se guardan estos días
                        totalDiasLibresEmpleados.Add(numDiasLibre.ToList());

                    }
                    else
                    {
                        //En caso de que no, se guarda un null
                        totalDiasLibresEmpleados.Add(null);
                    }
                }

                //Se guardan todos los empleados
                reportes.empleadosDiasLibres = empleados.ToList();

                //Se guardan todos los días libres
                reportes.diasLibres = totalDiasLibresEmpleados.ToList();

            }

            else if (rol.Contains("Local"))
            {
                estacion = Session["EstacionUsuarioLogueado"].ToString();

                //Se obtienen todos los guías ordenados por nombre de acuerdo a la estación del administrador logueado
                var empleados = baseDatos.GUIAS_EMPLEADO.Where(e => e.NOMBREESTACION.Equals(estacion) && (e.TIPOEMPLEADO.Contains("Interno") || e.TIPOEMPLEADO.Contains("Externo"))).OrderBy(c => c.NOMBREEMPLEADO);
                fechaLimiteHasta = semanaABuscar.AddDays(7);

                foreach (var row in empleados)
                {
                    //Se obtienen los dias libres que corresponden a la semana seleccionada
                    var diasLibres = baseDatos.GUIAS_ROLDIASLIBRES.Where(e => e.FECHA >= semanaABuscar && e.FECHA <= fechaLimiteHasta);
                    diasLibres = diasLibres.OrderBy(e => e.FECHA);

                    //Se obtienen los dias libres del empleado
                    var numDiasLibre = diasLibres.Where(e => e.CEDULAINTERNO.Equals(row.CEDULA));

                    //Se verifica si el empleado tiene dia libre
                    if (numDiasLibre != null)
                    {
                        //En caso de tener días libres, se guardan estos días
                        totalDiasLibresEmpleados.Add(numDiasLibre.ToList());

                    }
                    else
                    {
                        //En caso de que no, se guarda un null
                        totalDiasLibresEmpleados.Add(null);
                    }
                }

                //Se guardan todos los empleados
                reportes.empleadosDiasLibres = empleados.ToList();

                //Se guardan todos los días libres
                reportes.diasLibres = totalDiasLibresEmpleados.ToList();

            }



            return View(reportes);
        }



    }
}