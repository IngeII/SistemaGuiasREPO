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
    using System.ComponentModel.DataAnnotations.Schema;
    public partial class GUIAS_EMPLEADO
    {
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2214:DoNotCallOverridableMethodsInConstructors")]
        public GUIAS_EMPLEADO()
        {
            this.GUIAS_TELEFONO = new HashSet<GUIAS_TELEFONO>();
            this.GUIAS_ASIGNACION = new HashSet<GUIAS_ASIGNACION>();
        }

        [Required(ErrorMessage = "La cédula es un campo requerido.")]
        [StringLength(9)]
        [Display(Name = "Cédula:")]
        [RegularExpression(@"^[0-9]+$", ErrorMessage = "La cédula solo puede estar compuesta por números")]
        public string CEDULA { get; set; }

        [StringLength(20)]
        [Display(Name = "Nombre:")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "El nombre solo puede estar compuesto por letras")]
        public string NOMBREEMPLEADO { get; set; }

        [StringLength(20)]
        [Display(Name = "Primer apellido:")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "El primer apellido solo puede estar compuesto por letras")]
        public string APELLIDO1 { get; set; }

        [StringLength(20)]
        [Display(Name = "Segundo apellido:")]
        [RegularExpression(@"^[a-zA-Z''-'\s]+$", ErrorMessage = "El segundo apellido solo puede estar compuesto por letras")]
        public string APELLIDO2 { get; set; }

        [Required(ErrorMessage = "El email es un campo requerido.")]
        [StringLength(30)]
        [Display(Name = "Email:")]
        [DataType(DataType.EmailAddress)]
        public string EMAIL { get; set; }

        [Required(ErrorMessage = "El estado es un campo requerido.")]
        [Range(0, 1)]
        [Display(Name = "Estado:")]
        public decimal ESTADO { get; set; }

        [StringLength(100)]
        [Display(Name = "Dirección:")]
        public string DIRECCION { get; set; }

        [Required(ErrorMessage = "El usuario es un campo requerido.")]
        [StringLength(20)]
        [Display(Name = "Usuario:")]
        public string USUARIO { get; set; }

        [Required(ErrorMessage = "La contraseña es un campo requerido.")]
        [StringLength(30)]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña:")]
        public string CONTRASENA { get; set; }

        //[Required]
        [StringLength(50)]
        [Display(Name = "Rol:")]
        public string TIPOEMPLEADO { get; set; }

        //[Required]
        [StringLength(55)]
        [Display(Name = "Nombre estación:")]
        public string NOMBREESTACION { get; set; }

        //[Required]
        [Range(0, 1)]
        public Nullable<decimal> CONFIRMAREMAIL { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirmar*: ")]
        [Compare("CONTRASENA", ErrorMessage = "*La contraseña y la confirmación de la contraseña no coinciden")]
        [NotMapped]
        public string CONFIRMACIONCONTRASENA { get; set; }

        public virtual GUIAS_ESTACION GUIAS_ESTACION { get; set; }
        public virtual ICollection<GUIAS_ROLDIASLIBRES> GUIAS_ROLDIASLIBRES { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GUIAS_TELEFONO> GUIAS_TELEFONO { get; set; }
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2227:CollectionPropertiesShouldBeReadOnly")]
        public virtual ICollection<GUIAS_ASIGNACION> GUIAS_ASIGNACION { get; set; }
    }
}