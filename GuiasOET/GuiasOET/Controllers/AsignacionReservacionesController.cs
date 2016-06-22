using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GuiasOET.Models;
using System.Diagnostics;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Web.UI.HtmlControls;
using System.Data.Entity;
using PagedList;
using MvcFlashMessages;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections;

namespace GuiasOET.Controllers
{
    public class AsignacionReservacionesController : Controller
    {

        private Entities1 baseDatos = new Entities1();

        [HttpGet]
        public ActionResult AsignarReservacion(string sortOrder, string currentFilter1, string currentFilter2, string fechaDesde, string fechaHasta, int? page)
        {
            string rol = Session["RolUsuarioLogueado"].ToString();
            string estacion = Session["EstacionUsuarioLogueado"].ToString();

            List<V_GUIAS_RESERVADOS> reservaciones = null;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.ReservacionSortParm = String.IsNullOrEmpty(sortOrder) ? "Reservacion" : "";
            ViewBag.NombreSortParm = String.IsNullOrEmpty(sortOrder) ? "Nombre" : "";
            ViewBag.EstacionSortParm = String.IsNullOrEmpty(sortOrder) ? "Estacion" : "";
            ViewBag.PersonasSortParm = String.IsNullOrEmpty(sortOrder) ? "Personas" : "";
            ViewBag.FechaSortParm = String.IsNullOrEmpty(sortOrder) ? "Fecha" : "";


            AsignacionModelos table = new AsignacionModelos();

            /* Se define tamaño de la pagina */
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            ViewBag.pageNumber = pageNumber;


            //Todas las reservaciones del sistema
            var reservacion = from r in baseDatos.GUIAS_RESERVACION select r;

            //Lista reservaciones guia externo
            List<IQueryable<GUIAS_RESERVACION>> reserv = new List<IQueryable<GUIAS_RESERVACION>>();


            //Todas las reservaciones con guias
            var reservacionAsignada = from p in baseDatos.GUIAS_ASIGNACION select p;

            //Todos los guias del sistema
            var guias = from p in baseDatos.GUIAS_EMPLEADO select p;

            //Lista que contiene todas las reservaciones sin guias 
            List<GUIAS_ASIGNACION> reservacionesConAsignacion = new List<GUIAS_ASIGNACION>();

            //Lista que contiene los guias
            List<IEnumerable<GUIAS_EMPLEADO>> guiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();

            //Lista que contiene los guias de todas las reservaciones
            List<IEnumerable<GUIAS_EMPLEADO>> totalGuiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();

            //Lista que contiene el total de guias para cada reservacion
            IEnumerable<GUIAS_EMPLEADO> totalGuias;

            List<GUIAS_RESERVACION> listaReservaciones = new List<GUIAS_RESERVACION>();

            DateTime fechaInicio;
            DateTime fechaFin;

            //Lista que contiene los guias de todas las reservaciones
            List<IEnumerable<GUIAS_RESERVACION>> totalReservas = new List<IEnumerable<GUIAS_RESERVACION>>();

            //Reserva que tiene algún guía asignado
            GUIAS_ASIGNACION reserva;

            rol = Session["RolUsuarioLogueado"].ToString();

            //Ninguna fecha es vacía
            if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
            {

                ViewBag.CurrentFilter1 = fechaDesde;
                ViewBag.CurrentFilter2 = fechaHasta;
            }

            //Solo la fecha inicial es vacía
            else if (String.IsNullOrEmpty(fechaDesde) && !(String.IsNullOrEmpty(fechaHasta)))
            {

                ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();
                ViewBag.CurrentFilter2 = fechaHasta;

                fechaDesde = Convert.ToString(DateTime.Now);
                fechaHasta = currentFilter2;

            }
            //Solo la fecha final es vacía
            else if (!(String.IsNullOrEmpty(fechaDesde)) && String.IsNullOrEmpty(fechaHasta))
            {

                ViewBag.CurrentFilter1 = fechaDesde;
                ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(7)).Trim();

                fechaDesde = currentFilter1;
                fechaHasta = Convert.ToString(DateTime.Now.AddDays(7));


            }
            //Las fechas son vacias
            else if ((String.IsNullOrEmpty(fechaDesde)) && String.IsNullOrEmpty(fechaHasta))
            {
                string v1 = ViewBag.CurrentFilter1;
                string v2 = ViewBag.CurrentFilter2;

                if ((String.IsNullOrEmpty(currentFilter1) && (String.IsNullOrEmpty(currentFilter2))))
                {
                    ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();
                    ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(7)).Trim();

                    fechaDesde = Convert.ToString(DateTime.Now);
                    fechaHasta = Convert.ToString(DateTime.Now.AddDays(7));
                }
                else if ((String.IsNullOrEmpty(currentFilter1)))
                {
                    ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
                    ViewBag.CurrentFilter2 = fechaHasta;

                    fechaDesde = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
                    fechaHasta = currentFilter2;
                }
                else if ((String.IsNullOrEmpty(currentFilter2)))
                {

                    ViewBag.CurrentFilter1 = fechaDesde;
                    ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(7)).Trim();

                    fechaDesde = currentFilter1;
                    fechaHasta = String.Format("{0:dd/MM/yyyy}", DateTime.Now.AddDays(7)).Trim();

                }
                else
                {
                    fechaDesde = currentFilter1;
                    fechaHasta = currentFilter2;

                    ViewBag.CurrentFilter1 = fechaDesde;
                    ViewBag.CurrentFilter2 = fechaHasta;

                }


            }

            if (rol.Contains("Local") || rol.Contains("Interno") || rol.Contains("Global"))
            {
                if (rol.Contains("Local") || rol.Contains("Interno"))
                {
                    estacion = Session["EstacionUsuarioLogueado"].ToString();
                    reservacion = reservacion.Where(e => e.NOMBREESTACION.Equals(estacion));
                }
                //Ninguna fecha es vacía
                if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
                {

                    fechaInicio = Convert.ToDateTime(fechaDesde);
                    fechaFin = Convert.ToDateTime(fechaHasta);

                    //Se obtienen todas las reservaciones de la vista sql que cumplan con las distintas condiciones de acuerdo al usuario
                    if (rol.Contains("Local") || rol.Contains("Interno"))
                    {
                        reservaciones = baseDatos.V_GUIAS_RESERVADOS.Where(p => p.ESTACION.Equals(estacion) && p.ENTRA >= fechaInicio && p.ENTRA <= fechaFin).Distinct().ToList();

                    }
                    else if (rol.Contains("Global"))
                    {
                        reservaciones = baseDatos.V_GUIAS_RESERVADOS.Where(p=> p.ENTRA >= fechaInicio && p.ENTRA <= fechaFin).Distinct().ToList();
                    }

                    GUIAS_RESERVACION contador = new GUIAS_RESERVACION();

                    for (int i = 0; i < reservaciones.Count; i++)
                    {
                        contador = null;
                        contador = baseDatos.GUIAS_RESERVACION.Find(reservaciones[i].ID);

                        //Si la reservacion de la vista no se encuentra guardada en la base de datos se agrega a la tabla Reservaciones
                        if (contador == null)
                        {
                            GUIAS_RESERVACION nuevo = new GUIAS_RESERVACION();
                            nuevo.NUMERORESERVACION = reservaciones[i].ID;
                            nuevo.APELLIDOSSOLICITANTE = reservaciones[i].APELLIDOS;
                            nuevo.FECHAENTRA = reservaciones[i].ENTRA;
                            nuevo.FECHASALE = reservaciones[i].SALE;
                            nuevo.HORA = "8:00am";
                            nuevo.NOMBREESTACION = reservaciones[i].ESTACION;
                            nuevo.NOMBRESOLICITANTE = reservaciones[i].NOMBRE;
                            //nuevo.NOTAS = reservaciones[i].
                            nuevo.NUMEROPERSONAS = reservaciones[i].PAX;
                            nuevo.ULTIMAMODIFICACION = reservaciones[i].ULTIMA_MODIFICACION;
                            baseDatos.GUIAS_RESERVACION.Add(nuevo);
                            baseDatos.SaveChanges();
                        }
                        //Si la reservacion ya se encuentra en la base de datos se verifica que no tenga guías asociados para actualizarse sin lanzar alertas
                        else {
                            if (!contador.ULTIMAMODIFICACION.Equals(reservaciones[i].ULTIMA_MODIFICACION))
                            {
                                List<GUIAS_ASIGNACION> guiasGuardados = baseDatos.GUIAS_ASIGNACION.Where(p => p.NUMERORESERVACION.Equals(contador.NUMERORESERVACION)).ToList();

                                //Si esta reservacion no tiene guias asignados debe actualizarse sin lanzar alertas
                                if (guiasGuardados.Count == 0)
                                {
                                    GUIAS_RESERVACION actualizado = new GUIAS_RESERVACION();
                                    actualizado.NUMERORESERVACION = reservaciones[i].ID;
                                    actualizado.APELLIDOSSOLICITANTE = reservaciones[i].APELLIDOS;
                                    actualizado.FECHAENTRA = reservaciones[i].ENTRA;
                                    actualizado.FECHASALE = reservaciones[i].SALE;
                                    actualizado.HORA = "8:00am";
                                    actualizado.NOMBREESTACION = reservaciones[i].ESTACION;
                                    actualizado.NOMBRESOLICITANTE = reservaciones[i].NOMBRE;
                                    //nuevo.NOTAS = reservaciones[i].
                                    actualizado.NUMEROPERSONAS = reservaciones[i].PAX;
                                    actualizado.ULTIMAMODIFICACION = reservaciones[i].ULTIMA_MODIFICACION;

                                    baseDatos.Entry(contador).CurrentValues.SetValues(actualizado);
                                    baseDatos.SaveChanges();
                                }
                            }
                        }
                    }

                    if (rol.Contains("Local") || rol.Contains("Interno"))
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA >= fechaInicio && e.FECHAENTRA <= fechaFin && e.NOMBREESTACION.Equals(estacion)));
                    }
                    else
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA >= fechaInicio && e.FECHAENTRA <= fechaFin));
                    }

                    page = 1;
                }

                //Solo la fecha inicial es vacía
                else if (String.IsNullOrEmpty(fechaDesde) && !(String.IsNullOrEmpty(fechaHasta)))
                {

                    fechaFin = Convert.ToDateTime(fechaHasta);

                    if (rol.Contains("Local") || rol.Contains("Interno"))
                    {
                        reservaciones = baseDatos.V_GUIAS_RESERVADOS.Where(p => p.ESTACION.Equals(estacion) && p.ENTRA <= fechaFin).ToList();

                    }
                    else if (rol.Contains("Global"))
                    {
                        reservaciones = baseDatos.V_GUIAS_RESERVADOS.Where(p => p.ENTRA <= fechaFin).ToList();
                    }

                    
                    GUIAS_RESERVACION contador = new GUIAS_RESERVACION();

                    for (int i = 0; i < reservaciones.Count; i++)
                    {

                        contador = null;
                        contador = baseDatos.GUIAS_RESERVACION.Find(reservaciones[i].ID);
                        if (contador == null)
                        {
                            GUIAS_RESERVACION nuevo = new GUIAS_RESERVACION();
                            nuevo.APELLIDOSSOLICITANTE = reservaciones[i].APELLIDOS;
                            nuevo.FECHAENTRA = reservaciones[i].ENTRA;
                            nuevo.FECHASALE = reservaciones[i].SALE;
                            nuevo.HORA = "8:00am";
                            nuevo.NOMBREESTACION = reservaciones[i].ESTACION;
                            nuevo.NOMBRESOLICITANTE = reservaciones[i].NOMBRE;
                            //nuevo.NOTAS = reservaciones[i].
                            nuevo.NUMEROPERSONAS = reservaciones[i].PAX;
                            nuevo.NUMERORESERVACION = reservaciones[i].ID;
                            nuevo.ULTIMAMODIFICACION = reservaciones[i].ULTIMA_MODIFICACION;

                            baseDatos.GUIAS_RESERVACION.Add(nuevo);
                            baseDatos.SaveChanges();
                        }
                        //Si la reservacion ya se encuentra en la base de datos se verifica que no tenga guías asociados para actualizarse sin lanzar alertas
                        else
                        {
                            if (!contador.ULTIMAMODIFICACION.Equals(reservaciones[i].ULTIMA_MODIFICACION))
                            {
                                List<GUIAS_ASIGNACION> guiasGuardados = baseDatos.GUIAS_ASIGNACION.Where(p => p.NUMERORESERVACION.Equals(contador.NUMERORESERVACION)).ToList();

                                //Si esta reservacion no tiene guias asignados debe actualizarse sin lanzar alertas
                                if (guiasGuardados.Count == 0)
                                {
                                    GUIAS_RESERVACION actualizado = new GUIAS_RESERVACION();
                                    actualizado.NUMERORESERVACION = reservaciones[i].ID;
                                    actualizado.APELLIDOSSOLICITANTE = reservaciones[i].APELLIDOS;
                                    actualizado.FECHAENTRA = reservaciones[i].ENTRA;
                                    actualizado.FECHASALE = reservaciones[i].SALE;
                                    actualizado.HORA = "8:00am";
                                    actualizado.NOMBREESTACION = reservaciones[i].ESTACION;
                                    actualizado.NOMBRESOLICITANTE = reservaciones[i].NOMBRE;
                                    //nuevo.NOTAS = reservaciones[i].
                                    actualizado.NUMEROPERSONAS = reservaciones[i].PAX;
                                    actualizado.ULTIMAMODIFICACION = reservaciones[i].ULTIMA_MODIFICACION;

                                    baseDatos.Entry(contador).CurrentValues.SetValues(actualizado);
                                    baseDatos.SaveChanges();
                                }
                            }
                        }
                    }

                    if (rol.Contains("Local") || rol.Contains("Interno"))
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA <= fechaFin && e.NOMBREESTACION.Equals(estacion)));
                    }
                    else
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA <= fechaFin));
                    }

                    page = 1;

                }

                //Solo la fecha final es vacía
                else if (!(String.IsNullOrEmpty(fechaDesde)) && String.IsNullOrEmpty(fechaHasta))
                {
                    fechaInicio = Convert.ToDateTime(fechaDesde);

                    if (rol.Contains("Local") || rol.Contains("Interno"))
                    {
                        reservaciones = baseDatos.V_GUIAS_RESERVADOS.Where(p => p.ESTACION.Equals(estacion) && p.ENTRA >= fechaInicio).ToList();

                    }
                    else if (rol.Contains("Global"))
                    {
                        reservaciones = baseDatos.V_GUIAS_RESERVADOS.Where(p => p.ENTRA >= fechaInicio).ToList();
                    }

                    GUIAS_RESERVACION contador = new GUIAS_RESERVACION();

                    for (int i = 0; i < reservaciones.Count; i++)
                    {

                        contador = null;
                        contador = baseDatos.GUIAS_RESERVACION.Find(reservaciones[i].ID);
                        if (contador == null)
                        {
                            GUIAS_RESERVACION nuevo = new GUIAS_RESERVACION();
                            nuevo.APELLIDOSSOLICITANTE = reservaciones[i].APELLIDOS;
                            nuevo.FECHAENTRA = reservaciones[i].ENTRA;
                            nuevo.FECHASALE = reservaciones[i].SALE;
                            nuevo.HORA = "8:00am";
                            nuevo.NOMBREESTACION = reservaciones[i].ESTACION;
                            nuevo.NOMBRESOLICITANTE = reservaciones[i].NOMBRE;
                            //nuevo.NOTAS = reservaciones[i].
                            nuevo.NUMEROPERSONAS = reservaciones[i].PAX;
                            nuevo.NUMERORESERVACION = reservaciones[i].ID;
                            nuevo.ULTIMAMODIFICACION = reservaciones[i].ULTIMA_MODIFICACION;

                            baseDatos.GUIAS_RESERVACION.Add(nuevo);
                            baseDatos.SaveChanges();
                        }
                        //Si la reservacion ya se encuentra en la base de datos se verifica que no tenga guías asociados para actualizarse sin lanzar alertas
                        else
                        {
                            if (!contador.ULTIMAMODIFICACION.Equals(reservaciones[i].ULTIMA_MODIFICACION))
                            {
                                List<GUIAS_ASIGNACION> guiasGuardados = baseDatos.GUIAS_ASIGNACION.Where(p => p.NUMERORESERVACION.Equals(contador.NUMERORESERVACION)).ToList();

                                //Si esta reservacion no tiene guias asignados debe actualizarse sin lanzar alertas
                                if (guiasGuardados.Count == 0)
                                {
                                    GUIAS_RESERVACION actualizado = new GUIAS_RESERVACION();
                                    actualizado.NUMERORESERVACION = reservaciones[i].ID;
                                    actualizado.APELLIDOSSOLICITANTE = reservaciones[i].APELLIDOS;
                                    actualizado.FECHAENTRA = reservaciones[i].ENTRA;
                                    actualizado.FECHASALE = reservaciones[i].SALE;
                                    actualizado.HORA = "8:00am";
                                    actualizado.NOMBREESTACION = reservaciones[i].ESTACION;
                                    actualizado.NOMBRESOLICITANTE = reservaciones[i].NOMBRE;
                                    //nuevo.NOTAS = reservaciones[i].
                                    actualizado.NUMEROPERSONAS = reservaciones[i].PAX;
                                    actualizado.ULTIMAMODIFICACION = reservaciones[i].ULTIMA_MODIFICACION;

                                    baseDatos.Entry(contador).CurrentValues.SetValues(actualizado);
                                    baseDatos.SaveChanges();
                                }
                            }
                        }
                    }

                    if (rol.Contains("Local") || rol.Contains("Interno"))
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA >= fechaInicio && e.NOMBREESTACION.Equals(estacion)));
                    }
                    else
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA >= fechaInicio));
                    }

                    page = 1;
                }


                foreach (var row in reservacion)
                {

                    reservacionAsignada = reservacionAsignada.Where(e => e.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                    reserva = baseDatos.GUIAS_ASIGNACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));

                    if (reserva != null)
                    {
                        reservacionesConAsignacion.Add(reserva);

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

                //Se asigna la cantidad de paginas
                if (reservacion.Count() == 0)
                {
                    ViewBag.TotalPages = 1;
                }
                else
                {
                    var count = reservacion.Count();
                    decimal totalPages = count / (decimal)pageSize;
                    ViewBag.TotalPages = Math.Ceiling(totalPages);
                }


                //Todos los guias asociados a las reservaciones
                table.empleados = totalGuiasAsignados.ToPagedList(pageNumber, pageSize);

                //Todas las reservaciones que tienen guias asignados
                table.reservacionesAsignadas = reservacionesConAsignacion.ToPagedList(pageNumber, pageSize);



            }
            else if (Session["RolUsuarioLogueado"].ToString().Contains("Externo"))
            {
                string cedula = Session["IdUsuarioLogueado"].ToString();
                var reservacionesAsignadas = baseDatos.GUIAS_ASIGNACION.Where(x => x.CEDULAGUIA.Equals(cedula));


                IQueryable<GUIAS_RESERVACION> todasReservaciones = baseDatos.GUIAS_RESERVACION;


                //Ninguna fecha es vacía
                if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
                {

                    fechaInicio = Convert.ToDateTime(fechaDesde);
                    fechaFin = Convert.ToDateTime(fechaHasta);
                    todasReservaciones = (baseDatos.GUIAS_RESERVACION.Where(e => e.FECHAENTRA >= fechaInicio && e.FECHAENTRA <= fechaFin));
                    page = 1;


                }

                //Solo la fecha inicial es vacía
                else if (String.IsNullOrEmpty(fechaDesde) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaFin = Convert.ToDateTime(fechaHasta);
                    todasReservaciones = (baseDatos.GUIAS_RESERVACION.Where(e => e.FECHAENTRA <= fechaFin));
                    page = 1;

                }

                //Solo la fecha final es vacía
                else if (!(String.IsNullOrEmpty(fechaDesde)) && String.IsNullOrEmpty(fechaHasta))
                {
                    fechaInicio = Convert.ToDateTime(fechaDesde);
                    todasReservaciones = (baseDatos.GUIAS_RESERVACION.Where(e => e.FECHAENTRA >= fechaInicio));
                    page = 1;
                }


                IEnumerable<GUIAS_EMPLEADO> guia = baseDatos.GUIAS_EMPLEADO.Where(x => x.CEDULA.Equals(cedula));

                GUIAS_RESERVACION datosReserva;

                IEnumerable<GUIAS_ASIGNACION> reservasConAsignacion = baseDatos.GUIAS_ASIGNACION;


                foreach (var row in reservacionesAsignadas)
                {

                    datosReserva = todasReservaciones.FirstOrDefault(e => e.NUMERORESERVACION == row.NUMERORESERVACION);
                    reservasConAsignacion = reservasConAsignacion.Where(f => f.NUMERORESERVACION == row.NUMERORESERVACION);

                    if (datosReserva != null)
                    {
                        listaReservaciones.Add(datosReserva);
                        reservacionesConAsignacion.Add(row);
                    }


                    foreach (var row2 in reservasConAsignacion)
                    {
                        guias = guias.Where(x => x.CEDULA.Equals(row2.CEDULAGUIA));
                        guiasAsignados.Add(guias);
                        guias = from p in baseDatos.GUIAS_EMPLEADO select p;
                    }


                    if (guiasAsignados.Count() > 0)
                    {

                        totalGuias = guiasAsignados.ElementAt(0);

                        for (int y = 1; y < guiasAsignados.Count(); ++y)
                        {
                            totalGuias = totalGuias.Concat(guiasAsignados.ElementAt(y));
                        }

                        totalGuiasAsignados.Add(totalGuias);

                    }

                    guias = from p in baseDatos.GUIAS_EMPLEADO select p;
                    reservasConAsignacion = baseDatos.GUIAS_ASIGNACION;
                    guiasAsignados.Clear();
                }


                //Se asigna la cantidad de paginas
                if (reservacion.Count() == 0)
                {
                    ViewBag.TotalPages = 1;
                }
                else
                {
                    var count = reservacion.Count();
                    decimal totalPages = count / (decimal)pageSize;
                    ViewBag.TotalPages = Math.Ceiling(totalPages);
                }


                //Los guias asociados a las reservaciones a las cuales está asignado el guía externo
                table.empleados = totalGuiasAsignados.ToPagedList(pageNumber, pageSize);

                //Las reservaciones a las cuales el guía externo está asignado
                table.reservacionesAsignadas = reservacionesConAsignacion.ToPagedList(pageNumber, pageSize);


            }

            List<GUIAS_RESERVACION> listaTotalReservaciones = new List<GUIAS_RESERVACION>();

            if (rol.Contains("Local") || rol.Contains("Interno") || rol.Contains("Global"))
            {
                listaTotalReservaciones = reservacion.ToList();
            }
            else
            {
                listaTotalReservaciones = listaReservaciones;
            }

            var datos = listaTotalReservaciones.OrderBy(e => e.NUMERORESERVACION);
            switch (sortOrder)
            {
                case "Reservacion":
                    datos = listaTotalReservaciones.OrderBy(e => e.NUMERORESERVACION);
                    break;
                case "Nombre":
                    datos = listaTotalReservaciones.OrderBy(e => e.NOMBRESOLICITANTE);
                    break;
                case "Estacion":
                    datos = listaTotalReservaciones.OrderBy(e => e.NOMBREESTACION);
                    break;
                case "Personas":
                    datos = listaTotalReservaciones.OrderBy(e => e.NUMEROPERSONAS);
                    break;
                case "Fecha":
                    Debug.WriteLine("entre a ordenar fecha");
                    datos = listaTotalReservaciones.OrderBy(e => e.FECHAENTRA);
                    break;
                default:
                    datos = listaTotalReservaciones.OrderBy(e => e.NUMERORESERVACION);
                    break;
            }

            table.totalReservaciones = datos.ToPagedList(pageNumber, pageSize);

            ViewBag.MessagesInOnePage = table.totalReservaciones;
            ViewBag.PageNumber = pageNumber;

            return View(table);
        }

       
        public ActionResult Notificaciones(string sortOrder, string currentFilter1, string currentFilter2, string fechaDesde, string fechaHasta, int? page)  //**  string currentFilter2, string fechaHasta
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.ReservacionSortParm = String.IsNullOrEmpty(sortOrder) ? "Reservacion" : "";
            ViewBag.NombreSortParm = String.IsNullOrEmpty(sortOrder) ? "Nombre" : "";
            ViewBag.EstacionSortParm = String.IsNullOrEmpty(sortOrder) ? "Estacion" : "";
            ViewBag.PersonasSortParm = String.IsNullOrEmpty(sortOrder) ? "Personas" : "";
            ViewBag.FechaSortParm = String.IsNullOrEmpty(sortOrder) ? "Fecha" : "";
            ViewBag.HoraSortParm = String.IsNullOrEmpty(sortOrder) ? "Hora" : "";
            ViewBag.GuiasAsignadosSortParm = String.IsNullOrEmpty(sortOrder) ? "Guías Asignados" : "";

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            ViewBag.pageNumber = pageNumber;

            AsignacionModelos table = new AsignacionModelos();
            var reservacion = from r in baseDatos.GUIAS_RESERVACION select r; //Todas las reservaciones del sistema

            DateTime fechaInicio;
            DateTime fechaFin;
            string estacion = "";
            string rol = Session["RolUsuarioLogueado"].ToString();

            //Ninguna fecha es vacía
            if (!(String.IsNullOrEmpty(fechaDesde)) && !(String.IsNullOrEmpty(fechaHasta)))
            {
                ViewBag.CurrentFilter1 = fechaDesde;
                ViewBag.CurrentFilter2 = fechaHasta;
            }
            //Solo la fecha inicial es vacía
            else if (String.IsNullOrEmpty(fechaDesde) && !(String.IsNullOrEmpty(fechaHasta)))
            {
                ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();
                ViewBag.CurrentFilter2 = fechaHasta;
                fechaDesde = Convert.ToString(DateTime.Now);
                fechaHasta = currentFilter2;
            }
            //Solo la fecha final es vacía
            else if (!(String.IsNullOrEmpty(fechaDesde)) && String.IsNullOrEmpty(fechaHasta))
            {
                ViewBag.CurrentFilter1 = fechaDesde;
                ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(7)).Trim();
                fechaDesde = currentFilter1;
                fechaHasta = Convert.ToString(DateTime.Now.AddDays(7));
            }
            //Las fechas son vacias
            else if ((String.IsNullOrEmpty(fechaDesde)) && String.IsNullOrEmpty(fechaHasta))
            {
                string v1 = ViewBag.CurrentFilter1;
                string v2 = ViewBag.CurrentFilter2;

                if ((String.IsNullOrEmpty(currentFilter1) && (String.IsNullOrEmpty(currentFilter2))))
                {
                    ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();
                    ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(7)).Trim();
                    fechaDesde = Convert.ToString(DateTime.Now);
                    fechaHasta = Convert.ToString(DateTime.Now.AddDays(7));
                }
                else if ((String.IsNullOrEmpty(currentFilter1)))
                {
                    ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
                    ViewBag.CurrentFilter2 = fechaHasta;
                    fechaDesde = String.Format("{0:dd/MM/yyyy}", DateTime.Now);
                    fechaHasta = currentFilter2;
                }
                else if ((String.IsNullOrEmpty(currentFilter2)))
                {
                    ViewBag.CurrentFilter1 = fechaDesde;
                    ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(7)).Trim();
                    fechaDesde = currentFilter1;
                    fechaHasta = String.Format("{0:dd/MM/yyyy}", DateTime.Now.AddDays(7)).Trim();
                }
                else
                {
                    fechaDesde = currentFilter1;
                    fechaHasta = currentFilter2;
                    ViewBag.CurrentFilter1 = fechaDesde;
                    ViewBag.CurrentFilter2 = fechaHasta;
                }
            }

            if (rol.Contains("Global") || rol.Contains("Local"))
            {
                //Si el usuario es local y no puso ninguna fecha solo ve lo de su estacion
                if (rol.Contains("Local"))
                {
                    estacion = Session["EstacionUsuarioLogueado"].ToString();
                    reservacion = reservacion.Where(e => e.NOMBREESTACION.Equals(estacion));
                }
                //  Si el usuario puso la fecha inicial y fecha final 
                if (!String.IsNullOrEmpty(fechaDesde) && !String.IsNullOrEmpty(fechaHasta))
                {
                    fechaInicio = Convert.ToDateTime(fechaDesde);
                    fechaFin = Convert.ToDateTime(fechaHasta);
                    if (rol.Contains("Local"))
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA >= fechaInicio && e.FECHASALE <= fechaFin && e.NOMBREESTACION.Equals(estacion)));
                    }
                    else
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA >= fechaInicio && e.FECHASALE <= fechaFin));
                    }
                    page = 1;
                }
                // Si el usuario solo puso la fecha final y la de inicial esta vacía
                else if (String.IsNullOrEmpty(fechaDesde) && !(String.IsNullOrEmpty(fechaHasta)))
                {
                    fechaFin = Convert.ToDateTime(fechaHasta);
                    if (rol.Contains("Local"))
                    {
                        reservacion = (reservacion.Where(e => e.FECHASALE<= fechaFin && e.NOMBREESTACION.Equals(estacion)));
                    }
                    else
                    {
                        reservacion = (reservacion.Where(e => e.FECHASALE <= fechaFin));
                    }
                    page = 1;
                }
                //Solo la fecha final es vacía
                else if (!(String.IsNullOrEmpty(fechaDesde)) && String.IsNullOrEmpty(fechaHasta))
                {
                    fechaInicio = Convert.ToDateTime(fechaDesde);
                    if (rol.Contains("Local"))
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA >= fechaInicio && e.NOMBREESTACION.Equals(estacion)));
                    }
                    else
                    {
                        reservacion = (reservacion.Where(e => e.FECHAENTRA >= fechaInicio));
                    }
                    page = 1;
                }

                //Todos los guias del sistema
                var guias = from p in baseDatos.GUIAS_EMPLEADO select p;

                //** ESTO NO SE USA POR EL MOMENTO
                //Lista que contiene los guias
                List<IEnumerable<GUIAS_EMPLEADO>> guiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();
                //Lista que contiene los guias de todas las reservaciones
                List<IEnumerable<GUIAS_EMPLEADO>> totalGuiasAsignados = new List<IEnumerable<GUIAS_EMPLEADO>>();
                //Lista que contiene el total de guias para cada reservacion
                IEnumerable<GUIAS_EMPLEADO> totalGuias;
                //Lista que contiene los guias de todas las reservaciones
                List<IEnumerable<GUIAS_RESERVACION>> totalReservas = new List<IEnumerable<GUIAS_RESERVACION>>();

                List<GUIAS_RESERVACION> listaReservaciones = new List<GUIAS_RESERVACION>();           
                var datosDeLaVista = from r in baseDatos.V_GUIAS_RESERVADOS select r;
                GUIAS_RESERVACION reserva;
                V_GUIAS_RESERVADOS tuplaVistaActual;
                var vistaReserva = from r in baseDatos.V_GUIAS_RESERVADOS select r; //Se tienen todas las vistas del sistema 
                foreach (var row in reservacion) // la lista de reservas aqui ya esta filtrada 
                {
                    reserva = baseDatos.GUIAS_RESERVACION.FirstOrDefault(i => i.NUMERORESERVACION.Equals(row.NUMERORESERVACION));
                    if (reserva != null && vistaReserva != null)
                    {
                        listaReservaciones.Add(reserva);
                        foreach (var row2 in vistaReserva)
                        {
                            tuplaVistaActual = baseDatos.V_GUIAS_RESERVADOS.FirstOrDefault(i => i.ID.Equals(row2.ID));
                            if (reserva.NUMERORESERVACION == tuplaVistaActual.ID) {
                                if (reserva.ULTIMAMODIFICACION != tuplaVistaActual.ULTIMA_MODIFICACION)
                                {
                                    
                                }
                         }

                        }
                    }
                    guias = from p in baseDatos.GUIAS_EMPLEADO select p;
                }

                //Se asigna la cantidad de paginas
                if (reservacion.Count() == 0)
                {
                    ViewBag.TotalPages = 1;
                }
                else
                {
                    var count = reservacion.Count();
                    decimal totalPages = count / (decimal)pageSize;
                    ViewBag.TotalPages = Math.Ceiling(totalPages);
                }

                //Todos los guias asociados a las reservaciones
                table.empleados = totalGuiasAsignados.ToPagedList(pageNumber, pageSize);

                //Todas las reservaciones que tienen UN NUMERO DE ID 
                table.reservaciones = listaReservaciones.ToPagedList(pageNumber, pageSize);
            }

            ViewBag.CurrentFilter = fechaDesde;

            var empleados = from e in baseDatos.GUIAS_EMPLEADO select e;

            if (!String.IsNullOrEmpty(fechaDesde))
            {
                empleados = empleados.Where(e => e.APELLIDO1.Contains(fechaDesde)
                                       || e.NOMBREEMPLEADO.Contains(fechaDesde) || e.APELLIDO2.Contains(fechaDesde)
                                       || e.NOMBREESTACION.Contains(fechaDesde) || e.TIPOEMPLEADO.Contains(fechaDesde)
                                       || e.USUARIO.Contains(fechaDesde) || e.EMAIL.Contains(fechaDesde));
            }

            switch (sortOrder)
            {
                case "Reservacion":
                    empleados = empleados.OrderBy(e => e.NOMBREEMPLEADO);
                    break;
                case "Nombre":
                    empleados = empleados.OrderBy(e => e.APELLIDO1);
                    break;
                case "Estacion":
                    empleados = empleados.OrderBy(e => e.APELLIDO2);
                    break;
                case "Personas":
                    empleados = empleados.OrderBy(e => e.NOMBREESTACION);
                    break;
                case "Fecha":
                    empleados = empleados.OrderBy(e => e.TIPOEMPLEADO);
                    break;
                case "Hora":
                    empleados = empleados.OrderBy(e => e.APELLIDO2);
                    break;
                case "Turno":
                    empleados = empleados.OrderBy(e => e.NOMBREESTACION);
                    break;
                case "Guías Asignados":
                    empleados = empleados.OrderBy(e => e.TIPOEMPLEADO);
                    break;
                default:
                    empleados = empleados.OrderBy(e => e.NOMBREESTACION);
                    break;
            }


            return View(empleados.ToPagedList(pageNumber, pageSize));
        }

        

        [HttpGet]
        public ActionResult ConsultarAsignacion(int? page)
        {
            var semana = DateTime.Now.ToString("dd/MM/yyyy");
            ViewBag.fechaLunes = DateTime.Now.DayOfWeek.ToString();
            switch (DateTime.Now.DayOfWeek)
            {
               
                case System.DayOfWeek.Monday:
                    ViewBag.fechaLunes = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaMartes = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.AddDays(4).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(5).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(6).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Tuesday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMartes = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(4).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(5).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Wednesday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(4).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Thursday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(3).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Friday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado= DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(2).ToString("dd/MM/yyyy");

                    break;

                case System.DayOfWeek.Saturday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.AddDays(1).ToString("dd/MM/yyyy");
                    break;

                case System.DayOfWeek.Sunday:
                    ViewBag.fechaLunes = DateTime.Now.Subtract(new TimeSpan(6, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMartes = DateTime.Now.Subtract(new TimeSpan(5, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaMiercoles = DateTime.Now.Subtract(new TimeSpan(4, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaJueves = DateTime.Now.Subtract(new TimeSpan(3, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaViernes = DateTime.Now.Subtract(new TimeSpan(2, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaSabado = DateTime.Now.Subtract(new TimeSpan(1, 0, 0, 0)).ToString("dd/MM/yyyy");
                    ViewBag.fechaDomingo = DateTime.Now.ToString("dd/MM/yyyy");
                    break;

            }

            GUIAS_EMPLEADO empleado = baseDatos.GUIAS_EMPLEADO.Find(Session["IdUsuarioLogueado"].ToString());
            var id = Session["IdUsuarioLogueado"];
            if (empleado.TIPOEMPLEADO == "Guía Externo")
            {
                var empleados = baseDatos.GUIAS_EMPLEADO.Where(e=> e.CEDULA == id.ToString());
                empleados = empleados.OrderBy(e => e.NOMBREEMPLEADO);
                int pageSize = 8;
                int pageNumber = (page ?? 1);
                return View(empleados.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                var empleados = baseDatos.GUIAS_EMPLEADO.Where(e=> e.TIPOEMPLEADO == "Guía Interno" || e.TIPOEMPLEADO == "Guía Externo" || e.TIPOEMPLEADO == "Administrador Local/Guía Interno");
                empleados = empleados.OrderBy(e => e.NOMBREEMPLEADO);
                int pageSize = 8;
                int pageNumber = (page ?? 1);
                return View(empleados.ToPagedList(pageNumber, pageSize));
            }


        }
        // GET: Inicio
        /*Método GET de la pantalla ListaUsuarios*/
        public ActionResult Inicio(int? page)
        {
            return View();
        }

        [HttpPost]
        public ActionResult ConsultarAsignacion(DateTime semanaABuscar)
        {
            // if (semanaABuscar.)
            ViewBag.fechaLunes = semanaABuscar.ToString("dd/MM/yyyy");
            ViewBag.fechaMartes = semanaABuscar.AddDays(1).ToString("dd/MM/yyyy");
            ViewBag.fechaMiercoles = semanaABuscar.AddDays(2).ToString("dd/MM/yyyy");
            ViewBag.fechaJueves = semanaABuscar.AddDays(3).ToString("dd/MM/yyyy");
            ViewBag.fechaViernes = semanaABuscar.AddDays(4).ToString("dd/MM/yyyy");
            ViewBag.fechaSabado = semanaABuscar.AddDays(5).ToString("dd/MM/yyyy");
            ViewBag.fechaDomingo = semanaABuscar.AddDays(6).ToString("dd/MM/yyyy");

            GUIAS_EMPLEADO empleado = baseDatos.GUIAS_EMPLEADO.Find(Session["IdUsuarioLogueado"].ToString());
            var id = Session["IdUsuarioLogueado"];
            if (empleado.TIPOEMPLEADO == "Guía Externo")
            {
                var empleados = baseDatos.GUIAS_EMPLEADO.Where(e => e.CEDULA == id.ToString());
                empleados = empleados.OrderBy(e => e.NOMBREEMPLEADO);
                int pageSize = 8;
                int pageNumber = 1;
                return View(empleados.ToPagedList(pageNumber, pageSize));
            }
            else
            {
                var empleados = baseDatos.GUIAS_EMPLEADO.Where(e => e.TIPOEMPLEADO == "Guía Interno" || e.TIPOEMPLEADO == "Guía Externo" || e.TIPOEMPLEADO == "Administrador Local/Guía Interno");
                empleados = empleados.OrderBy(e => e.NOMBREEMPLEADO);
                int pageSize = 8;
                int pageNumber = 1;
                return View(empleados.ToPagedList(pageNumber, pageSize));
            }
        }

        public ActionResult AsignarReservacionDetallada(string id, int? page)
        {
            ViewBag.cambios = new List<string>();

            ViewBag.reserva = id;
            string identificacion = id.ToString();

            AsignacionModelos modelo;

            modelo = new AsignacionModelos(baseDatos.GUIAS_RESERVACION.Find(identificacion));
            ViewBag.fecha = String.Format("{0:M/d/yyyy}", modelo.modeloReservacion.FECHAENTRA).Trim();

            List<V_GUIAS_RESERVADOS> reservacionVista = baseDatos.V_GUIAS_RESERVADOS.Where(p => p.ID.Equals(id)).ToList();
            GUIAS_RESERVACION reservacionTabla = baseDatos.GUIAS_RESERVACION.Find(id);

            if (reservacionVista.Count != 0)
            {

                //Si los datos de la tabla y la vista para esta reservación no son iguales se despliega al usuario cuales son los cambios
                if (reservacionVista[0].ULTIMA_MODIFICACION != reservacionTabla.ULTIMAMODIFICACION)
                {

                    if (reservacionVista[0].NOMBRE != null && !reservacionVista[0].NOMBRE.Equals(reservacionTabla.NOMBRESOLICITANTE))
                    {
                        ViewBag.cambios.Add("Nombre solicitante: " + reservacionVista[0].NOMBRE);
                    }

                    if (reservacionVista[0].APELLIDOS != null &&  !reservacionVista[0].APELLIDOS.Equals(reservacionTabla.APELLIDOSSOLICITANTE))
                    {
                        ViewBag.cambios.Add("Apellidos solicitante: " + reservacionVista[0].APELLIDOS);
                    }

                    if (reservacionVista[0].ENTRA != null && (reservacionVista[0].ENTRA != reservacionTabla.FECHAENTRA))
                    {
                        string result = String.Format("{0:M/d/yyyy}", reservacionVista[0].ENTRA);
                        ViewBag.cambios.Add("Fecha entrada: " + result);
                    }

                    if (reservacionVista[0].SALE != null && !reservacionVista[0].SALE.Equals(reservacionTabla.FECHASALE))
                    {
                        ViewBag.cambios.Add("Fecha salida: " + reservacionVista[0].SALE);

                    }
                    if (reservacionVista[0].PAX != null && !reservacionVista[0].PAX.Equals(reservacionTabla.NUMEROPERSONAS))
                    {
                        ViewBag.cambios.Add("Número personas: " + reservacionVista[0].PAX);
                    }
                    if (reservacionVista[0].ESTACION != null &&  !reservacionVista[0].ESTACION.Equals(reservacionTabla.NOMBREESTACION))
                    {
                        ViewBag.cambios.Add("Estación: " + reservacionVista[0].ESTACION);
                    }

                }
                else
                {
                    ViewBag.cambios.Add("Ninguno");
                }

            }

            List<string> guias = baseDatos.GUIAS_ASIGNACION.Where(p => p.NUMERORESERVACION.Equals(identificacion)).Select(s => s.CEDULAGUIA).ToList();
            List<GUIAS_EMPLEADO> guiasLibres = baseDatos.GUIAS_EMPLEADO.Where(p => !guias.Contains(p.CEDULA) && p.TIPOEMPLEADO.Contains("Guía") && p.NOMBREESTACION.Equals(modelo.modeloReservacion.NOMBREESTACION) ).ToList();
            List<GUIAS_EMPLEADO> guiasAsociados = baseDatos.GUIAS_EMPLEADO.Where(p => guias.Contains(p.CEDULA) && p.TIPOEMPLEADO.Contains("Guía")).ToList();

            /* Se define tamaño de la pagina para la paginación de guías disponibles */
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            ViewBag.pageNumber = pageNumber;

            /*Hay que filtrar el maximo de asignaciones para ese día del guía disponible, ya que no pueden ser mas de 4 y que sea un rol de día libre se ponga de otro color*/
            modelo.guiasDisponibles = guiasLibres;
            modelo.guiasAsignados = guiasAsociados;


            if (modelo == null)
            {
                return HttpNotFound();
            }
            else
            {
                //Se asigna la cantidad de paginas
                if (modelo.guiasDisponibles.Count() == 0)
                {
                    ViewBag.TotalPages = 1;
                }
                else
                {
                    var count = modelo.guiasDisponibles.Count();
                    decimal totalPages = count / (decimal)pageSize;
                    ViewBag.TotalPages = Math.Ceiling(totalPages);
                    decimal numero = ViewBag.TotalPages;
                }

                modelo.totalGuiasDisponibles = modelo.guiasDisponibles.ToPagedList(pageNumber, pageSize);
                ViewBag.MessagesInOnePage = modelo.totalGuiasDisponibles;
                ViewBag.PageNumber = pageNumber;

            }

            return View(modelo);
        }

        public ActionResult aceptarCambios(string reservacion)
        {

            GUIAS_RESERVACION contador = baseDatos.GUIAS_RESERVACION.Find(reservacion);
            List<V_GUIAS_RESERVADOS> reservaciones = baseDatos.V_GUIAS_RESERVADOS.Where(p => p.ID.Equals(reservacion)).ToList();
             GUIAS_RESERVACION actualizado = new GUIAS_RESERVACION();
             actualizado.NUMERORESERVACION = reservaciones[0].ID;
             actualizado.APELLIDOSSOLICITANTE = reservaciones[0].APELLIDOS;
             actualizado.FECHAENTRA = reservaciones[0].ENTRA;
             actualizado.FECHASALE = reservaciones[0].SALE;
             actualizado.HORA = "8:00am";
             actualizado.NOMBREESTACION = reservaciones[0].ESTACION;
             actualizado.NOMBRESOLICITANTE = reservaciones[0].NOMBRE;
                    //nuevo.NOTAS = reservaciones[i].
             actualizado.NUMEROPERSONAS = reservaciones[0].PAX;
             actualizado.ULTIMAMODIFICACION = reservaciones[0].ULTIMA_MODIFICACION;

             baseDatos.Entry(contador).CurrentValues.SetValues(actualizado);
             baseDatos.SaveChanges();
                
            ViewBag.reserva = reservacion;
            AsignacionModelos modelo = new AsignacionModelos();

            return View(modelo);
        }

        public ActionResult agregarGuia(string id, string reservacion, int rowCount)
        {
            if (id != null)
            {
                GUIAS_ASIGNACION modelo = new GUIAS_ASIGNACION();
                string iden = id.ToString();
                modelo.CEDULAGUIA = iden;
                modelo.NUMERORESERVACION = reservacion;

                baseDatos.GUIAS_ASIGNACION.Add(modelo);
                baseDatos.SaveChanges();
            }

            string cedula = id.ToString();
            GUIAS_EMPLEADO empleado = baseDatos.GUIAS_EMPLEADO.Find(cedula);
            AsignacionModelos mod = new AsignacionModelos();
            mod.guiasDisponibles.Add(empleado);
            
            ViewBag.rowCount = rowCount + 1;
            ViewBag.reserva = reservacion;
            return View(mod);
        }

        public ActionResult eliminarGuia(string id, string reservacion, int rowCount)
        {
            if (id != null)
            {
                string identificacion = id.ToString();
                List<GUIAS_ASIGNACION> asignacion = baseDatos.GUIAS_ASIGNACION.Where(p =>  p.CEDULAGUIA.Equals(identificacion) && p.NUMERORESERVACION.Equals(reservacion)).ToList();
                if (asignacion != null && asignacion.Count() == 1)
                {
                    baseDatos.GUIAS_ASIGNACION.Remove(asignacion.ElementAt(0));
                    baseDatos.SaveChanges();
                }
            }

            string cedula = id.ToString();
            GUIAS_EMPLEADO empleado = baseDatos.GUIAS_EMPLEADO.Find(cedula);
            AsignacionModelos mod = new AsignacionModelos();
            mod.guiasAsignados.Add(empleado);

            ViewBag.rowCount = rowCount + 1;
            ViewBag.reserva = reservacion;
            return View(mod);
        }



        //mostrar la reservacion del usuario correspondiente
        public ActionResult ConsultarAsignacionDetallada(int? id, string fecha, string turno)
        {
            
            //string numReservacion = "";
            ReservacionesModelos modelo;
            //List<GUIAS_RESERVACION> lista = null;
            List<string> acompañantes = new List<string>();
            string identificacion = id.ToString();
            GUIAS_ASIGNACION asignacion;
            string reserv;
            GUIAS_EMPLEADO empleado = baseDatos.GUIAS_EMPLEADO.Find(identificacion);
            GUIAS_RESERVACION reservacionAsignada = null;
            DateTime date = Convert.ToDateTime(fecha);
            string nombreGuias = "";
            Boolean indicador = true;
            int indice = 0;

            try
            {

                List<string> numReservacion = baseDatos.GUIAS_ASIGNACION.Where(i => i.CEDULAGUIA.Equals(identificacion) && i.TURNO.Equals(turno)).Select(a => a.NUMERORESERVACION).ToList();
                

         
                    while (numReservacion != null && indicador == true && indice < numReservacion.Count())
                    {
                        reserv = numReservacion.ElementAt(indice);
                        List<GUIAS_RESERVACION> reservacionAuxiliar = baseDatos.GUIAS_RESERVACION.Where(i => i.NUMERORESERVACION == reserv).ToList();       
                        if (reservacionAuxiliar != null && reservacionAuxiliar.Count() != 0)
                        {
                           
                            if (reservacionAuxiliar.ElementAt(0).FECHAENTRA.ToString() == date.ToString())
                            {
                                indicador = false;
                                string cedula = "";
                                reservacionAsignada = reservacionAuxiliar.ElementAt(0);
                                List<GUIAS_ASIGNACION> cedEmpleados = baseDatos.GUIAS_ASIGNACION.Where(i => i.NUMERORESERVACION == reservacionAsignada.NUMERORESERVACION).ToList();
                                if (cedEmpleados != null && cedEmpleados.Count() != 0)
                                {
                                    foreach (var ced in cedEmpleados)
                                    {
                                        cedula = ced.CEDULAGUIA.ToString();
                                        List<GUIAS_EMPLEADO> estructuraAuxiliar = baseDatos.GUIAS_EMPLEADO.Where(i => i.CEDULA == cedula).ToList();
                                        if (estructuraAuxiliar != null)
                                        {
                                            nombreGuias = estructuraAuxiliar.ElementAt(0).NOMBREEMPLEADO.ToString() + " " + estructuraAuxiliar.ElementAt(0).APELLIDO1.ToString();
                                            acompañantes.Add(nombreGuias);
                                        }

                                       
                                    }

                                }

                        }else
                        {
                            ++indice;
                        }

                                            
                        }else
                        {
                            ++indice;
                        }
                }
            }
            catch(Exception e){
            }
            modelo = new ReservacionesModelos
            {
                reservacion = reservacionAsignada,
                compañeros = acompañantes,
                TURNO = turno,
                modeloEmpleado = empleado
            };
            return View(modelo);
            

        }



    }
}