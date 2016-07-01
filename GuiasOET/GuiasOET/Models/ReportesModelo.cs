using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace GuiasOET.Models
{
    public class ReportesModelo
    {

        public List<GUIAS_RESERVACION> reservaciones = new List<GUIAS_RESERVACION>();
        public List<IEnumerable<GUIAS_EMPLEADO>> empleados = new List<IEnumerable<GUIAS_EMPLEADO>>();
        public List<GUIAS_ASIGNACION> reservacionesAsignadas = new List<GUIAS_ASIGNACION>();
        public List<GUIAS_RESERVACION> totalReservaciones = new List<GUIAS_RESERVACION>();
        public List<DateTime> fechasReservaciones = new List<DateTime>();
        public List<string> subTotales = new List<string>();
        public List<int> listaAsociacion = new List<int>();
        public List<GUIAS_TURNO> turnoReservacion = new List<GUIAS_TURNO>();


        //Vista de días libres
        public List<List<GUIAS_ROLDIASLIBRES>> diasLibres = new List<List<GUIAS_ROLDIASLIBRES>>();
        public List<GUIAS_EMPLEADO> empleadosDiasLibres = new List<GUIAS_EMPLEADO>();
        public List<bool> diasLibreEmpleado = new List<bool>();


        //Se guardan los empleados para cada una de las reservaciones
        public List<List<GUIAS_EMPLEADO>> empleadosReservaciones = new List<List<GUIAS_EMPLEADO>>();


        //Se guardan los empleados para cada una de las reservaciones
        public List<GUIAS_EMPLEADO> empleadosReserva = new List<GUIAS_EMPLEADO>();

        //Se guarda la lista de las reservaciones asignadas
        public List<List<GUIAS_ASIGNACION>> listaReservacionesAsignadas = new List<List<GUIAS_ASIGNACION>>();

        public List<GUIAS_ASIGNACION> listaAsignacionReservaciones = new List<GUIAS_ASIGNACION>();


        public ReportesModelo()
        {

        }
    }
}