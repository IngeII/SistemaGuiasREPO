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


        public ActionResult AsignarDiasLibresDetallada(int? id)
        {

            string identificacion;
            DIASLIBRES modelo;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            identificacion = id.ToString();

            modelo = new DIASLIBRES(baseDatos.GUIAS_EMPLEADO.Find(identificacion));

            // modelo.modeloEmpleado.ESTADO = baseDatos.GUIAS_EMPLEADO.Find(identificacion).ESTADO;
            if (modelo == null)
            {
                return HttpNotFound();
            }

            return View(modelo);
        }

        public ActionResult AsignarRol(string sortOrder, string currentFilter1, string currentFilter2, string fechaDesde, string fechaHasta, int? page)
        {

            return View();
        }
        public ActionResult ModificarRol()
        {
            return View();

        }

        public ActionResult EliminarRol()
        {
            return View();

        }
    }
}