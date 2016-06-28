using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Data.Entity;
using GuiasOET.Models;
using System.Collections;
using System.Diagnostics;

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


            if (rol!=null)
            {
                if (rol.Contains("Global"))
                {
                    var guias = baseDatos.GUIAS_EMPLEADO.Where(e => e.TIPOEMPLEADO.Contains("Guía"));
                    var list = new SelectList(guias,"CEDULA","NOMBREEMPLEADO");
                    ViewData["GUIAS"] = list;
                }
                else if (rol.Contains("Interno"))
                {
                    string estacion = Session["EstacionUsuarioLogueado"].ToString();
                    var guias = baseDatos.GUIAS_EMPLEADO.Where(e=>e.TIPOEMPLEADO.Contains("Guía") && e.NOMBREESTACION.Contains(estacion));
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

            ReportesModelo reportes =  new ReportesModelo();
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

                        var reservacionAsignada = baseDatos.GUIAS_ASIGNACION.Where(e=> e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));

                        if (reservacionAsignada != null)
                        {
                            reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        }



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

                    //Se guardan las fechas de las reservaciones
                    foreach(var row in reservaciones)
                    {
                        //Se guarda la fecha de la reservación
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);
                    }
                       
                    reportes.empleados = totalGuiasAsignados.ToList();
                    reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList();
                    reportes.totalReservaciones = reservaciones.ToList();


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
                            reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                        }



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
                    reportes.totalReservaciones = reservacionAuxiliar.ToList();


                }
            }


            //Se calcula la cantidad de personas por cada reservacion
            reportes.subTotales = calcularSubTotales(reportes.totalReservaciones, totalGuiasAsignados);

            return View(reportes);
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

            int indice = 0;

            int indiceAuxiliar = 0;

            List<int> indiceReservacionesAsignadas = new List<int>();



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

                    for (int i = 0; i < reportes.fechasReservaciones.Count(); ++i)
                    {
                        Debug.WriteLine("fecha es: " + reportes.fechasReservaciones.ElementAt(i));
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

                    for (int i = 0; i < reportes.fechasReservaciones.Count(); ++i)
                    {
                        Debug.WriteLine("fecha es: " + reportes.fechasReservaciones.ElementAt(i));
                    }


                }
            }


            //Se calcula la cantidad de personas por cada reservacion
            reportes.subTotales = calcularSubTotales(reportes.totalReservaciones, totalGuiasAsignados);

            return View(reportes);
        }


        public List<string> calcularSubTotales(List<GUIAS_RESERVACION> reservaciones, List<IEnumerable<GUIAS_EMPLEADO>> totalGuiasAsignados)
        {

            List<string> listaSubTotales = new List<string>();


            decimal cantidadPersonas = 0;
            decimal cantidadGuias = 0;
            decimal cantidadAsignada = 0;
            string distribucion = "";

         
                for (int i = 0; i < reservaciones.Count(); ++i)
                {
                    cantidadPersonas = (decimal)reservaciones.ElementAt(i).NUMEROPERSONAS;
                   // cantidadGuias = totalGuiasAsignados.ElementAt(i).Count();

                    while (cantidadPersonas > LIMITE_PERSONAS)
                    {
                        cantidadAsignada = cantidadPersonas - LIMITE_PERSONAS;
                        distribucion = distribucion + 12 + "-";
                        cantidadPersonas = cantidadPersonas - LIMITE_PERSONAS;
                    }

                    distribucion = distribucion + cantidadAsignada;

                    //Se elimina el ultimo caracter
                    //distribucion = distribucion.Remove(distribucion.Length - 1, 1);
                    listaSubTotales.Add(distribucion);
                    distribucion = "";
                    cantidadAsignada = 0;

                }
            
            return listaSubTotales;
        }
    }
}