using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace GuiasOET.Models
{
    public class ReportesModelo
    {

        public IPagedList<GUIAS_RESERVACION> reservaciones;
        public IPagedList<IEnumerable<GUIAS_EMPLEADO>> empleados;
        public IPagedList<GUIAS_ASIGNACION> reservacionesAsignadas;
        public IPagedList<GUIAS_RESERVACION> totalReservaciones { get; set; }

        public ReportesModelo()
        {

        }
    }
}