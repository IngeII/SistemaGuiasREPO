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
    public class AsignacionDiasLibresController : Controller
    {
        private Entities1 baseDatos = new Entities1();
        private string ident = "";
        // GET: ListaGuias
        /*Método GET de la pantalla ListaGuias*/
        public ActionResult ListaGuias(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NombreSortParm = String.IsNullOrEmpty(sortOrder) ? "Nombre" : "";
            ViewBag.Ape1SortParm = String.IsNullOrEmpty(sortOrder) ? "Apellido1" : "";
            ViewBag.Ape2SortParm = String.IsNullOrEmpty(sortOrder) ? "Apellido2" : "";
            ViewBag.EstacionSortParm = String.IsNullOrEmpty(sortOrder) ? "Estacion" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var empleados = from e in baseDatos.GUIAS_EMPLEADO select e;
            empleados = empleados.Where(e => e.TIPOEMPLEADO.Contains("Guía"));

            if (Session["RolUsuarioLogueado"].ToString().Contains("Local"))
            {
                string estacion = Session["EstacionUsuarioLogueado"].ToString();
                empleados = empleados.Where(e => e.NOMBREESTACION.Equals(estacion));
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                if (Session["RolUsuarioLogueado"].ToString().Contains("Global"))
                {
                    empleados = empleados.Where(e => e.APELLIDO1.Contains(searchString)
                                       || e.NOMBREEMPLEADO.Contains(searchString) || e.APELLIDO2.Contains(searchString)
                                       || e.NOMBREESTACION.Contains(searchString)
                                       || e.USUARIO.Contains(searchString) || e.EMAIL.Contains(searchString));
                }
                else if (Session["RolUsuarioLogueado"].ToString().Contains("Local"))
                {
                    string estacion = Session["EstacionUsuarioLogueado"].ToString();

                    empleados = empleados.Where(e => e.APELLIDO1.Contains(searchString)
                                       || e.NOMBREEMPLEADO.Contains(searchString) || e.APELLIDO2.Contains(searchString)
                                       || e.NOMBREESTACION.Contains(searchString)
                                       || e.USUARIO.Contains(searchString) || e.EMAIL.Contains(searchString) && e.NOMBREESTACION.Equals(estacion));
                }

            }

            switch (sortOrder)
            {
                case "Nombre":
                    empleados = empleados.OrderBy(e => e.NOMBREEMPLEADO);
                    break;
                case "Apellido1":
                    empleados = empleados.OrderBy(e => e.APELLIDO1);
                    break;
                case "Apellido2":
                    empleados = empleados.OrderBy(e => e.APELLIDO2);
                    break;
                case "Estacion":
                    empleados = empleados.OrderBy(e => e.NOMBREESTACION);
                    break;
                default:
                    empleados = empleados.OrderBy(e => e.NOMBREESTACION);
                    break;
            }

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(empleados.ToPagedList(pageNumber, pageSize));
        }

        [HttpGet]
        public ActionResult AsignarDiasLibresDetallada(int? page, int? id, string sortOrder, string currentFilter1, string currentFilter2, string fechaInicio, string fechaFin, string ident)
        {

            string identificacion;
            DIASLIBRES modelo;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.fecha = String.IsNullOrEmpty(sortOrder) ? "Fecha" : "";
            ViewBag.tipo = String.IsNullOrEmpty(sortOrder) ? "Tipodialibre" : "";

            if (!(String.IsNullOrEmpty(fechaInicio)) && !(String.IsNullOrEmpty(fechaFin)))
            {

                ViewBag.CurrentFilter1 = fechaInicio;
                ViewBag.CurrentFilter2 = fechaFin;
            }

            //Solo la fecha inicial es vacía
            else if (String.IsNullOrEmpty(fechaInicio) && !(String.IsNullOrEmpty(fechaFin)))
            {

                ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();
                ViewBag.CurrentFilter2 = fechaFin;

                string f1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();

                fechaFin = currentFilter2;
                fechaInicio = Convert.ToString(f1);

            }
            //Solo la fecha final es vacía
            else if (!(String.IsNullOrEmpty(fechaInicio)) && String.IsNullOrEmpty(fechaFin))
            {

                ViewBag.CurrentFilter1 = fechaInicio;
                ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(30)).Trim();

                string f2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(30)).Trim();

                fechaInicio = currentFilter1;
                fechaFin = Convert.ToString(f2);


            }
            //Las fechas son vacias
            else if ((String.IsNullOrEmpty(fechaInicio)) && String.IsNullOrEmpty(fechaFin))
            {
                string v1 = ViewBag.CurrentFilter1;
                string v2 = ViewBag.CurrentFilter2;

                if ((String.IsNullOrEmpty(currentFilter1) && (String.IsNullOrEmpty(currentFilter2))))
                {
                    ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();
                    ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(30)).Trim();

                    string f1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();
                    string f2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(30)).Trim();

                    fechaInicio = Convert.ToString(f1);
                    fechaFin = Convert.ToString(f2);
                }
                else if ((String.IsNullOrEmpty(currentFilter1)))
                {
                    ViewBag.CurrentFilter1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now);
                    ViewBag.CurrentFilter2 = fechaFin;

                    string f1 = String.Format("{0:yyyy-MM-dd}", DateTime.Now).Trim();

                    fechaInicio = Convert.ToString(f1);
                    fechaFin = currentFilter2;
                }
                else if ((String.IsNullOrEmpty(currentFilter2)))
                {

                    ViewBag.CurrentFilter1 = fechaInicio;
                    ViewBag.CurrentFilter2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(30)).Trim();

                    string f2 = String.Format("{0:yyyy-MM-dd}", DateTime.Now.AddDays(30)).Trim();

                    fechaInicio = currentFilter1;
                    fechaFin = Convert.ToString(f2);

                }
                else
                {
                    fechaInicio = currentFilter1;
                    fechaFin = currentFilter2;

                    ViewBag.CurrentFilter1 = fechaInicio;
                    ViewBag.CurrentFilter2 = fechaFin;

                }
            }
            identificacion = "";
            if (id == null && ident == "")
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            else
            {
                if (id != null)
                {
                    identificacion = id.ToString();
                    ViewBag.identificacion = identificacion;
                    ViewBag.identificacion2 = identificacion;
                }
                else
                {
                    if (ident != "")
                    {
                        identificacion = ident;
                        ViewBag.identificacion = ident;
                        ViewBag.identificacion2 = ident;
                    }
                }
            }


            modelo = new DIASLIBRES(baseDatos.GUIAS_EMPLEADO.Find(identificacion));
            List<GUIAS_ROLDIASLIBRES> dias;
            if (!String.IsNullOrEmpty(fechaInicio) && !String.IsNullOrEmpty(fechaFin))
            {
                DateTime fI = Convert.ToDateTime(fechaInicio);
                DateTime fF = Convert.ToDateTime(fechaFin);
                dias = baseDatos.GUIAS_ROLDIASLIBRES.Where(p => p.CEDULAINTERNO == identificacion && p.FECHA >= fI && p.FECHA <= fF).ToList();
            }

            else
            {
                dias = baseDatos.GUIAS_ROLDIASLIBRES.Where(p => p.CEDULAINTERNO == identificacion).ToList();
            }

            modelo.tRolDiaLibre = dias;
            if (modelo == null)
            {
                return HttpNotFound();
            }

            /* Se define tamaño de la pagina para la paginación de guías disponibles */
            int pageSize = 8;
            int pageNumber = (page ?? 1);

            ViewBag.pageNumber = pageNumber;
            modelo.totalRolDiaLibre = modelo.tRolDiaLibre.ToPagedList(pageNumber, pageSize);
            ViewBag.MessagesInOnePage = modelo.tRolDiaLibre;
            ViewBag.PageNumber = pageNumber;

            return View(modelo);
        }


        public ActionResult AsignarRol(int? id, string tipo, string fecha)
        {
            Console.Write("Entro");

            return RedirectToAction("AsignarDiasLibresDetallada", new { id = id });
        }

        public ActionResult EliminarRol(int? id, DateTime fecha)
        {
            string admin1 = "Administrador Local/ Guía Interno";
            string admin2 = "Administrador Global";
            string admin3 = "Administrador Local";

            if (id != null)
            {
                string idLogueado = Session["IdUsuarioLogueado"].ToString();
                string identificacion = id.ToString();

                List<GUIAS_EMPLEADO> empleado = baseDatos.GUIAS_EMPLEADO.Where(p => p.CEDULA.Equals(idLogueado) && ((p.TIPOEMPLEADO.Equals(admin1)) || (p.TIPOEMPLEADO.Equals(admin2)) || (p.TIPOEMPLEADO.Equals(admin3)))).ToList();

                List<GUIAS_ROLDIASLIBRES> dialibre = baseDatos.GUIAS_ROLDIASLIBRES.Where(p => p.CEDULAINTERNO.Equals(identificacion) && p.FECHA.Equals(fecha)).ToList();

                if (empleado.Count > 0)
                {
                    if (dialibre != null)
                    {
                        baseDatos.GUIAS_ROLDIASLIBRES.Remove(dialibre.ElementAt(0));
                        baseDatos.SaveChanges();
                        this.Flash("Éxito", "Día libre eliminado.");
                    }
                }
                else
                {
                    this.Flash("Éxito", "Sólo adiministradores pueden eliminar.");
                }
            }
            return RedirectToAction("AsignarDiasLibresDetallada", new { id = id });
        }

        [HttpGet]
        public ActionResult AsignarRol(string ide, string fechaDesde, string tipo)
        {
            string admin1 = "Administrador Local/ Guía Interno";
            string admin2 = "Administrador Global";
            string admin3 = "Administrador Local";
            string idLogueado = Session["IdUsuarioLogueado"].ToString();

            List<GUIAS_EMPLEADO> empleado = baseDatos.GUIAS_EMPLEADO.Where(p => p.CEDULA.Equals(idLogueado) && ((p.TIPOEMPLEADO.Equals(admin1)) || (p.TIPOEMPLEADO.Equals(admin2)) || (p.TIPOEMPLEADO.Equals(admin3)))).ToList();
            if (empleado.Count > 0)
            {

                if (fechaDesde == "" && tipo == "")
                {
                    this.Flash("Éxito", "Recuerde completar todos los campos.");
                }
                else
                {
                    if (fechaDesde == "" && tipo != "")
                    {
                        this.Flash("Éxito", "Recuerde completar la fecha.");
                    }
                    else
                    {
                        if (fechaDesde != "" && tipo == "")
                        {
                            this.Flash("Éxito", "Recuerde completar el tipo.");

                        }
                        else
                        {
                            Console.Write(ide);
                            Console.Write(fechaDesde);
                            Console.Write(tipo);

                            DateTime fechaV = Convert.ToDateTime(fechaDesde);

                            string fecha = fechaV.ToString("dd/MM/yyyy");

                            DateTime f = Convert.ToDateTime(fecha);

                            List<GUIAS_ROLDIASLIBRES> dialibre = baseDatos.GUIAS_ROLDIASLIBRES.Where(p => p.CEDULAINTERNO.Equals(ide) && p.FECHA.Equals(f) && p.TIPODIALIBRE.Equals(tipo)).ToList();

                            if (dialibre.Count == 0)
                            {
                                DIASLIBRES modelo = new DIASLIBRES();
                                modelo.roles1.TIPODIALIBRE = tipo;
                                modelo.roles1.CEDULAINTERNO = ide;
                                modelo.roles1.FECHA = f;

                                baseDatos.GUIAS_ROLDIASLIBRES.Add(modelo.roles1);
                                baseDatos.SaveChanges();

                                this.Flash("Éxito", "Día libre agregado.");
                            }
                            else
                            {
                                this.Flash("Éxito", "Ya existe un día libre asignado.");

                            }
                        }
                    }
                }
            }
            else
            {
                this.Flash("Éxito", "Sólo adiministradores pueden agregar.");
            }
            int id = Int32.Parse(ide);

            return RedirectToAction("AsignarDiasLibresDetallada", new { id = id });
        }
    }
}
