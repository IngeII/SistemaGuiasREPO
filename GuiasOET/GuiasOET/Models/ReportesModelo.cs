using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace GuiasOET.Models
{
    public class ReportesModelo
    {

        /*   public IPagedList<GUIAS_RESERVACION> reservaciones;
           public IPagedList<IEnumerable<GUIAS_EMPLEADO>> empleados;
           public IPagedList<GUIAS_ASIGNACION> reservacionesAsignadas;
           public IPagedList<GUIAS_RESERVACION> totalReservaciones { get; set; } */


        public List<GUIAS_RESERVACION> reservaciones = new List<GUIAS_RESERVACION>();
        public List<IEnumerable<GUIAS_EMPLEADO>> empleados = new List<IEnumerable<GUIAS_EMPLEADO>>();
        public List<GUIAS_ASIGNACION> reservacionesAsignadas = new List<GUIAS_ASIGNACION>();
        public List<GUIAS_RESERVACION> totalReservaciones = new List<GUIAS_RESERVACION>();
        public List<DateTime> fechasReservaciones = new List<DateTime>();
        public List<string> subTotales = new List<string>();
        public List<int> listaAsociacion = new List<int>();


        //Vista de días libres
        public List<List<GUIAS_ROLDIASLIBRES>> diasLibres = new List<List<GUIAS_ROLDIASLIBRES>>();
        public List<GUIAS_EMPLEADO> empleadosDiasLibres = new List<GUIAS_EMPLEADO>();
        public List<bool> diasLibreEmpleado = new List<bool>();


        public ReportesModelo()
        {

        }
    }
}