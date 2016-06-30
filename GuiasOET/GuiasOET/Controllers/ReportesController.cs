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
            Debug.WriteLine("EEEEEENTREEEEEE");

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

            List<GUIAS_TURNO> turnoReservasAsignadas = new List<GUIAS_TURNO>();

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

                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));

                        if (reservacionAsignada != null)
                        {

                            foreach(var reservaAsig in reservacionAsignada)
                            {
                                GUIAS_RESERVACION datosReserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(e => e.NUMERORESERVACION.Equals(reservaAsig.NUMERORESERVACION));
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

                                reservacionesConAsignacion.Add(reservaAsig);
                            }
                      
                            reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        }



                        if (reservaConGuias != null)
                        {
                        //   reservacionesConAsignacion.Add(reservaConGuias);

                           
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


                    }

                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservaciones)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);
                    }

                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList();
                    reportes.totalReservaciones = reservaciones.ToList();
                    reportes.turnoReservacion = turnoReservasAsignadas;

                    for (int i = 0; i < reportes.totalReservaciones.Count(); ++i)
                    {
                        Debug.WriteLine("esto tiene total reservaciones: " + reportes.totalReservaciones.ElementAt(i).FECHAENTRA + " num reserv: " + reportes.totalReservaciones.ElementAt(i).NUMERORESERVACION);

                    }

                    for (int i = 0; i < reportes.fechasReservaciones.Count(); ++i)
                    {
                        Debug.WriteLine("fecha tien esto: " + reportes.fechasReservaciones.ElementAt(i));
                    }

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


                    foreach (var row in reservacionAuxiliar)
                    {

                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));

                        if (reservacionAsignada != null)
                        {

                            //   var listaReservas = baseDatos.GUIAS_RESERVACION.Where(i => i.NUMERORESERVACION.Equals(reservaConGuias.NUMERORESERVACION));


                            foreach (var reservaAsig in reservacionAsignada)
                            {
                                GUIAS_RESERVACION datosReserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(e => e.NUMERORESERVACION.Equals(reservaAsig.NUMERORESERVACION));
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


                                reservacionesConAsignacion.Add(reservaAsig);
                            }

                            reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        }


                        if (reservaConGuias != null)
                        {
                         //   reservacionesConAsignacion.Add(reservaConGuias);
                          


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

                    }


                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservaciones)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);
                    }

                    for (int i = 0; i < reportes.fechasReservaciones.Count(); ++i)
                    {
                        Debug.WriteLine("fecha es: " + reportes.fechasReservaciones.ElementAt(i));
                    }



                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList(); 
                    reportes.totalReservaciones = reservaciones.ToList();
                    reportes.turnoReservacion = turnoReservasAsignadas;

                    for (int i = 0; i < reportes.totalReservaciones.Count(); ++i )
                    {
                        Debug.WriteLine("esto tiene total reservaciones: " + reportes.totalReservaciones.ElementAt(i).FECHAENTRA + " num reserv: " + reportes.totalReservaciones.ElementAt(i).NUMERORESERVACION);

                    }

                    for(int i = 0; i < reportes.fechasReservaciones.Count(); ++i)
                    {
                        Debug.WriteLine("fecha tien esto: " + reportes.fechasReservaciones.ElementAt(i));
                    }


                }
            }


            //Se calcula la cantidad de personas por cada reservacion
            reportes.subTotales = calcularSubTotales(reportes.totalReservaciones);

            return View(reportes);
        }

        public ActionResult generarReportePDF(Array datosTabla)
        {
            string x;
            Debug.WriteLine("tamano de la tabla " + datosTabla.Length);
            List<string> informacion = new List<string>();


            int contador = 1;
            String[] substrings = null;


               foreach (var item in datosTabla)
               {

                 substrings = item.ToString().Split('$');
                //Debug.WriteLine("item es: " + item.ToString() );
                }

           


            for (int i = 0; i < substrings.Length; ++i )
            {
              /*  if(substrings.ElementAt(i).Equals(""))
                {
                    Debug.WriteLine("era espacio en blanco");
                 //   Debug.WriteLine("contenido del substring es: " + substrings.ElementAt(i));
                } */
             /*   else
                { */
                    informacion.Add(substrings.ElementAt(i));
              //  }
               
            }

            for(int i = 0; i < informacion.Count(); ++i)
            {
                Debug.WriteLine("el contenido de info es: " + informacion.ElementAt(i));
            }



            return View();
        }


        public ActionResult Reservaciones(string fechaDesde, string fechaHasta)
        {
            Debug.WriteLine("EEEEEENTREEEEEE");

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
            int[] asociacionReservaciones = null;

            //Lista que contiene los guias de todas las reservaciones
            List<IEnumerable<GUIAS_EMPLEADO>> totalGuiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();

            //Lista que contiene el total de guias para cada reservacion
            IEnumerable<GUIAS_EMPLEADO> totalGuias;

            DateTime fechaReservacion;

            List<int> indiceReservacionesAsignadas = new List<int>();

            int contador = 0;

            List<GUIAS_TURNO> turnoReservasAsignadas = new List<GUIAS_TURNO>();            

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
                                    }else
                                    {
                                         GUIAS_TURNO turno = baseDatos.GUIAS_TURNO.FirstOrDefault(e => e.NOMBRETURNO.Equals(asigna.GUIAS_TURNO.NOMBRETURNO));
                                         turnoReservasAsignadas.Add(turno);
                                     }
                                    reservacionesConAsignacion.Add(asigna);

                            }

                                                                                                        
                        }else
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
                        else
                        {
                           // indiceReservacionesAsignadas.Add(-1);
                            totalGuiasAsignados.Add(null);
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


                    //Se guardan las fechas de las reservaciones
                    foreach (var row in reservacionAuxiliar)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);
                    }

                    /*   if (reservacionAuxiliar != null)
                       {
                           asociacionReservaciones = new int[reservacionAuxiliar.Count()];
                       } */

                    foreach (var row in reservacionAuxiliar)
                    {

                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));

                        if (reservacionAsignada != null)
                        {
                            //   asociacionReservaciones[indice] = indiceAuxiliar;
                            //   ++indiceAuxiliar;
                            indiceReservacionesAsignadas.Add(0);

                            //	574839201
                            //	 PITA0424092015.10493619719      
                            string a = row.NUMERORESERVACION;
                            reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        }
                        else
                        {
                            // para indicar que la reservacion no esta asignada
                            //  asociacionReservaciones[indice] = -1;
                            indiceReservacionesAsignadas.Add(-1);
                        }

                        //  ++indice;

                        if (reservaConGuias != null)
                        {
                            reservacionesConAsignacion.Add(reservaConGuias);
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


                    }



                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList();
                    reportes.totalReservaciones = reservacionAuxiliar.ToList();
                    //     reportes.listaAsociacion = asociacionReservaciones;
                    reportes.listaAsociacion = indiceReservacionesAsignadas;
                    reportes.turnoReservacion = turnoReservasAsignadas;

                    for (int i = 0; i < reportes.fechasReservaciones.Count(); ++i)
                    {
                        Debug.WriteLine("fecha es: " + reportes.fechasReservaciones.ElementAt(i));
                    }


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