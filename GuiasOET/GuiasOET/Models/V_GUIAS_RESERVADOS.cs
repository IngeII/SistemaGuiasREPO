//------------------------------------------------------------------------------
// <auto-generated>
//     Este código se generó a partir de una plantilla.
//
//     Los cambios manuales en este archivo pueden causar un comportamiento inesperado de la aplicación.
//     Los cambios manuales en este archivo se sobrescribirán si se regenera el código.
// </auto-generated>
//------------------------------------------------------------------------------

namespace GuiasOET.Models
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;

    public partial class V_GUIAS_RESERVADOS
    {
        public string ID { get; set; }
        public string NOMBRE { get; set; }
        public string APELLIDOS { get; set; }
        public Nullable<decimal> PAX { get; set; }
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:M/d/yyyy}")]
        public Nullable<System.DateTime> ENTRA { get; set; }
        public Nullable<System.DateTime> SALE { get; set; }
        public Nullable<long> ULTIMA_MODIFICACION { get; set; }
        public string ESTACION { get; set; }
    }
}
