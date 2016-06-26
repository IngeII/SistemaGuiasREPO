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
    
    public partial class GUIAS_TELEFONO
    {
        [StringLength(9)]
        [Display(Name = "C�dula:")]
        public string CEDULAEMPLEADO { get; set; }

        [StringLength(11)]
        [Display(Name = "Tel�fono:")]
        [DataType(DataType.PhoneNumber)]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "El tel�fono solo puede estar compuesto por n�meros")]
        public string TELEFONO { get; set; }
    
        public virtual GUIAS_EMPLEADO GUIAS_EMPLEADO { get; set; }
    }
}
