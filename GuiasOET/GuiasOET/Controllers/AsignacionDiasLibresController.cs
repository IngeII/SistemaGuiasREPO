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


        public ActionResult AsignarDiasLibresDetallada(int? page, int? id, string sortOrder, string currentFilter1, string currentFilter2)
        {

            string identificacion;
            DIASLIBRES modelo;

            ViewBag.CurrentSort = sortOrder;
            ViewBag.fecha = String.IsNullOrEmpty(sortOrder) ? "Fecha" : "";
            ViewBag.tipo = String.IsNullOrEmpty(sortOrder) ? "Tipodialibre" : "";

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            identificacion = id.ToString();

            modelo = new DIASLIBRES(baseDatos.GUIAS_EMPLEADO.Find(identificacion));

            //List<string> guias = baseDatos.GUIAS_EMPLEADO.Select(s => s.CEDULA).ToList();

            List<GUIAS_ROLDIASLIBRES> dias = baseDatos.GUIAS_ROLDIASLIBRES.Where(p => p.CEDULAINTERNO == identificacion).ToList();
            /*List<GUIAS_ROLDIASLIBRES> dias = (from b in baseDatos.GUIAS_ROLDIASLIBRES
                   where b.CEDULAINTERNO.Equals(identificacion)
                   select b).ToList();*/
            modelo.tRolDiaLibre = dias;
            //modelo.modeloEmpleado.ESTADO = baseDatos.GUIAS_EMPLEADO.Find(identificacion).ESTADO;
            if (modelo == null)
            {
                return HttpNotFound();
            }

            /* Se define tamaño de la pagina para la paginación de guías disponibles */
            int pageSize = 8;
            int pageNumber = (page ?? 1);
            ident = identificacion;
            ViewBag.identificacion = identificacion;
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
            if (id != null)
            {
                string identificacion = id.ToString();
                List<GUIAS_ROLDIASLIBRES> reservacion = baseDatos.GUIAS_ROLDIASLIBRES.Where(p => p.CEDULAINTERNO.Equals(identificacion) && p.FECHA.Equals(fecha)).ToList();

                if (reservacion != null)
                {
                    baseDatos.GUIAS_ROLDIASLIBRES.Remove(reservacion.ElementAt(0));
                    baseDatos.SaveChanges();
                }
            }
            return RedirectToAction("AsignarDiasLibresDetallada", new { id = id });
        }

        [HttpGet]
        public ActionResult AsignarRol(string ide, DateTime fechaDesde, string tipo)
        {
            Console.Write(ide);
            Console.Write(fechaDesde);
            Console.Write(tipo);


            string fecha = fechaDesde.ToString("dd/MM/yyyy");

            DateTime f = Convert.ToDateTime(fecha);

            DIASLIBRES modelo = new DIASLIBRES();
            modelo.roles1.TIPODIALIBRE = tipo;
            modelo.roles1.CEDULAINTERNO = ide;
            modelo.roles1.FECHA = f;

            baseDatos.GUIAS_ROLDIASLIBRES.Add(modelo.roles1);
            baseDatos.SaveChanges();
            /*
            string consulta = "insert into guias_roldiaslibres values ('" + fecha + "', '" + ide + "', '" + tipo + "');";
            baseDatos.Database.SqlQuery<GUIAS_ROLDIASLIBRES>(consulta);
            baseDatos.SaveChanges();
            */
            int id = Int32.Parse(ide);
            return RedirectToAction("AsignarDiasLibresDetallada", new { id = id });
        }
    }
}
