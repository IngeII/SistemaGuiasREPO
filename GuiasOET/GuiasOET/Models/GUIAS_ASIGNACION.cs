//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated from a template.
//
//     Manual changes to this file may cause unexpected behavior in your application.
//     Manual changes to this file will be overwritten if the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GuiasOET.Models
{
    using System;
    using System.Collections.Generic;
    
    public partial class GUIAS_ASIGNACION
    {
        public string NOTASASOCIA { get; set; }
        public string NUMERORESERVACION { get; set; }
        public string TURNO { get; set; }
        public string CEDULAGUIA { get; set; }
    
        public virtual GUIAS_EMPLEADO GUIAS_EMPLEADO { get; set; }
        public virtual GUIAS_RESERVACION GUIAS_RESERVACION { get; set; }
        public virtual GUIAS_TURNO GUIAS_TURNO { get; set; }
    }
}
