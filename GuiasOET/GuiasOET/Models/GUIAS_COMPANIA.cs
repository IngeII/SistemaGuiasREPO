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
    
    public partial class GUIAS_COMPANIA
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GUIAS_COMPANIA()
        {
            this.GUIAS_ESTACION = new HashSet<GUIAS_ESTACION>();
        }
    
        public string NOMBRECOMPANIA { get; set; }
        public string UBICACION { get; set; }
    
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GUIAS_ESTACION> GUIAS_ESTACION { get; set; }
    }
}
