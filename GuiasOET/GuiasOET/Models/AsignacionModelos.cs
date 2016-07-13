using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace GuiasOET.Models
{
    public class AsignacionModelos
    {
        public GuiasOET.Models.GUIAS_RESERVACION modeloReservacion { get; set; }
        public List<GuiasOET.Models.GUIAS_EMPLEADO> guiasDisponibles = new List<GuiasOET.Models.GUIAS_EMPLEADO>();
        public List<GuiasOET.Models.GUIAS_EMPLEADO> guiasAsignados = new List<GuiasOET.Models.GUIAS_EMPLEADO>();
        public GuiasOET.Models.GUIAS_ASIGNACION asignacionGuias { get; set; }
        public IPagedList<GUIAS_EMPLEADO> totalGuiasDisponibles { get; set; } // esta

        public IPagedList<GUIAS_RESERVACION> reservaciones;
        public IPagedList<IEnumerable<GUIAS_EMPLEADO>> empleados;
        public IPagedList<GUIAS_ASIGNACION> reservacionesAsignadas;
        public IPagedList<GUIAS_RESERVACION> totalReservaciones { get; set; }
        public List<GuiasOET.Models.V_GUIAS_RESERVADOS> vistaReservaciones;
        public List<bool> cambiosReservaciones = new List<bool>();

        public IPagedList<GUIAS_ROLDIASLIBRES> rol { get; set; }

        public AsignacionModelos()
        {

        }

        public AsignacionModelos(GuiasOET.Models.GUIAS_RESERVACION reservacion)
        {
            modeloReservacion = reservacion;
        }

        public AsignacionModelos(GuiasOET.Models.GUIAS_EMPLEADO empleado)
        {

        }

    }

}