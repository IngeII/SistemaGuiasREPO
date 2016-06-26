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
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    
    public partial class GUIAS_RESERVACION
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GUIAS_RESERVACION()
        {
            this.GUIAS_ASIGNACION = new HashSet<GUIAS_ASIGNACION>();
        }

        [Display(Name = "N�mero:")]
        public string NUMERORESERVACION { get; set; }

        [Display(Name = "Solicitante:")]
        public string NOMBRESOLICITANTE { get; set; }
        public string APELLIDOSSOLICITANTE { get; set; }

        [Display(Name = "Pack:")]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:###}")]
        public Nullable<decimal> NUMEROPERSONAS { get; set; }

        [Display(Name = "Fecha:")]
        [DataType(DataType.Date)]
        [DisplayFormat(ApplyFormatInEditMode = true, DataFormatString = "{0:M/d/yyyy}")]
        public Nullable<System.DateTime> FECHAENTRA { get; set; }
        public Nullable<System.DateTime> FECHASALE { get; set; }

        [Display(Name = "Hora:")]
        public string HORA { get; set; }

        [Display(Name = "Estaci�n")]
        public string NOMBREESTACION { get; set; }
        public Nullable<decimal> ULTIMAMODIFICACION { get; set; }


        [Display(Name = "Notas:")]
        public string NOTAS { get; set; }
        public Nullable<decimal> CONFIRMACION { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GUIAS_ASIGNACION> GUIAS_ASIGNACION { get; set; }
    }
}
