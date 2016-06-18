using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace GuiasOET.Models
{
    public class ReservacionesModelos
    {
        public GuiasOET.Models.GUIAS_EMPLEADO modeloEmpleado { get; set; }
        public GUIAS_RESERVACION reservacion { get; set; }
        [Display(Name = "Guías: ")]
        public IEnumerable<string> compañeros { get; set; }
        [Display(Name = "Turno:")]
        public string TURNO { get; set; }

    }
}