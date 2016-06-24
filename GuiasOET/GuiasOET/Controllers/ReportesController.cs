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

        // GET: Reportes
        public ActionResult Index()
        {
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
            string rol = Session["RolUsuarioLogueado"].ToString();
            string estacion = "";


            //Todas las reservaciones del sistema
            var reservacion = from r in baseDatos.GUIAS_RESERVACION select r;

            //Todas las reservaciones con guias
            var reservacionAsignada = from p in baseDatos.GUIAS_ASIGNACION select p;


            //Reserva que tiene algún guía asignado
            GUIAS_ASIGNACION reservaConGuias;


            //Reserva que tiene algún guía asignado
            GUIAS_RESERVACION reserva;



            //Todos los guias del sistema
            var guias = from p in baseDatos.GUIAS_EMPLEADO select p;

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

                    reservacion = reservacion.Where(e => e.FECHAENTRA < fechaH && e.FECHAENTRA > fechaD);
                    reservacion = reservacion.OrderBy(r => r.FECHAENTRA);
                    var copiaReservaciones = reservacion;
                    copiaReservaciones = copiaReservaciones.Distinct();

                    foreach (var row in copiaReservaciones)
                    {
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);
                    }

                    
                    
                }
            }
            else if (rol.Contains("Interno"))
            {
                estacion = Session["EstacionUsuarioLogueado"].ToString();
                reservacion = reservacion.Where(e => e.NOMBREESTACION.Equals(estacion));

                //Se verifica que ambas fechas no sean vacías
                if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaD = Convert.ToDateTime(fechaDesde);
                    fechaH = Convert.ToDateTime(fechaHasta);

                    reservacion = reservacion.Where(e => e.FECHAENTRA < fechaH && e.FECHAENTRA > fechaD);
                    reservacion = reservacion.OrderBy(r => r.FECHAENTRA);
                    var copiaReservaciones = reservacion;
                    copiaReservaciones = copiaReservaciones.Distinct();
             
                    foreach (var row in copiaReservaciones)
                    {
                        fechaReservacion = Convert.ToDateTime(row.FECHAENTRA);
                        reportes.fechasReservaciones.Add(fechaReservacion);
                    }

                  
                  
                }
            }

            foreach (var row in reservacion)
            {

                reservacionAsignada = reservacionAsignada.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                reservaConGuias = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));


                if (reservaConGuias != null)
                {
                    reservacionesConAsignacion.Add(reservaConGuias);
                    reserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(reservaConGuias.NUMERORESERVACION));
                    reservaciones.Add(reserva);



                    foreach (var row2 in reservacionAsignada)
                    {
                        guias = guias.Where(x => x.CEDULA.Equals(row2.CEDULAGUIA));
                        guiasAsignados.Add(guias);
                        guias = from p in baseDatos.GUIAS_EMPLEADO select p;

                    }

                    totalGuias = guiasAsignados.ElementAt(0);

                    for (int y = 1; y < guiasAsignados.Count(); ++y)
                    {
                        totalGuias = totalGuias.Concat(guiasAsignados.ElementAt(y));
                    }

                    totalGuiasAsignados.Add(totalGuias);

                }

                guiasAsignados.Clear();
                reservacionAsignada = from p in baseDatos.GUIAS_ASIGNACION select p;
                guias = from p in baseDatos.GUIAS_EMPLEADO select p;

            }

            reportes.empleados = totalGuiasAsignados.ToList();
            reportes.reservacionesAsignadas = reservacionesConAsignacion.ToList();
            reportes.totalReservaciones = reservacion.ToList();

            //Se calcula la cantidad de personas por cada reservacion
            reportes.subTotales = calcularSubTotales(reportes.totalReservaciones, reportes.empleados);

            return View(reportes);
        }

        public ActionResult Reservaciones()
        {




            return View("ReportesReservaciones");
        }


        public List<string> calcularSubTotales(List<GUIAS_RESERVACION> reservaciones, List<IEnumerable<GUIAS_EMPLEADO>> totalGuiasAsignados)
        {

            List<string> listaSubTotales = new List<string>();


            decimal cantidadPersonas = 0;
            decimal cantidadGuias = 0;
            decimal cantidadAsignada = 0;
            string distribucion = "";



            for (int i = 0; i < totalGuiasAsignados.Count; ++i)
            {
                cantidadPersonas = (decimal)reservaciones.ElementAt(i).NUMEROPERSONAS;
                cantidadGuias = totalGuiasAsignados.ElementAt(i).Count();
                while (cantidadPersonas > LIMITE_PERSONAS)
                {
                    cantidadAsignada = cantidadPersonas - LIMITE_PERSONAS;
                    distribucion = distribucion + cantidadAsignada.ToString() + "-";
                    cantidadPersonas = cantidadPersonas - LIMITE_PERSONAS;
                }

                if (cantidadPersonas > 0)
                {
                    distribucion = distribucion + cantidadAsignada.ToString() + "-";
                    cantidadAsignada = 0;
                    listaSubTotales.Add(distribucion);
                    distribucion = "";
                }
                else
                {
                    cantidadAsignada = 0;
                    distribucion = "";
                }


            }

            return listaSubTotales;
        }
    }
}