using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GuiasOET.Models
{
    /*  [MetadataType(typeof(EMPLEADOMetadata))]
      public partial class GUIAS_EMPLEADO
      {
      } */

    public partial class GUIAS_EMPLEADO
    {
        public class ReestablecerContraseñaMetadata
        {
            [Required(ErrorMessage = "*Debe ingresar el nombre de usuario o correo ")]
            [Display(Name = "Usuario/Email:")]
            [NotMapped]
            public string USUARIO_EMAIL { get; set; }

            [Required(ErrorMessage = "*Debe ingresar la contraseña")]
            [DataType(DataType.Password)]
            [Display(Name = "Nueva Contraseña: ")]
            public string CONTRASENA { get; set; }


            // este es un atributo de GUIAS_EMPLEADO que no esta en la Base de Datos
            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Contraseña: ")]
            [Compare("CONTRASENA", ErrorMessage = "*La contraseña y la confirmación de la contraseña no coinciden")]
            [NotMapped]
            public string CONFIRMACIONCONTRASENA { get; set; }
        }

        public class LoginMetadata
        {
            [Required(ErrorMessage = "*Debe ingresar el nombre de usuario o correo ")]
            [Display(Name = "Usuario/Email:")]
            [NotMapped]
            public string USUARIO_EMAIL { get; set; }

            [Required(ErrorMessage = "*Debe ingresar la contraseña")]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña:")]
            public string CONTRASENA { get; set; }
        }


        public class OlvidarContrasenaMetadata
        {
            [Required(ErrorMessage = "*Debe ingresar el nombre de usuario o correo ")]
            [Display(Name = "Usuario/Email:")]
            [NotMapped]
            public string USUARIO_EMAIL { get; set; }

        }





    }

    /*public partial class ManejoModelos {
        public class InsertarUsuarioCampos
        {
            [Required(ErrorMessage = "*El campo cédula es obligatorio")]
            [StringLength(9)]
            [Display(Name = "Cédula*:")]
            public string CEDULA { get; set; }

            [StringLength(20)]
            [Display(Name = "Nombre:")]
            public string NOMBREEMPLEADO { get; set; }

            [StringLength(20)]
            [Display(Name = "Primer apellido:")]
            public string APELLIDO1 { get; set; }

            [StringLength(20)]
            [Display(Name = "Segundo apellido:")]
            public string APELLIDO2 { get; set; }

            [StringLength(30)]
            [Display(Name = "Email:")]
            public string EMAIL { get; set; }

            [Required]
            [Range(0, 1)]
            [Display(Name = "Estado:")]
            public decimal ESTADO { get; set; }

            [StringLength(100)]
            [Display(Name = "Dirección:")]
            public string DIRECCION { get; set; }

            [StringLength(20)]
            [Required(ErrorMessage = "*El campo usuario es obligatorio")]
            [Display(Name = "Usuario*:")]
            public string USUARIO { get; set; }

            [Required(ErrorMessage = "*El campo contraseña es obligatorio")]
            [StringLength(30)]
            [DataType(DataType.Password)]
            [Display(Name = "Contraseña*:")]
            public string CONTRASENA { get; set; }

            //[Required(ErrorMessage = "*El rol es obligatorio")]
            [StringLength(50)]
            [Display(Name = "Rol*:")]
            public string TIPOEMPLEADO { get; set; }

            //[Required(ErrorMessage = "*El nombre de la estación es obligatorio")]
            [StringLength(55)]
            [Display(Name = "Nombre estación:")]
            public string NOMBREESTACION { get; set; }

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar*: ")]
            [Compare("CONTRASENA", ErrorMessage = "*La contraseña y la confirmación de la contraseña no coinciden")]
            [NotMapped]
            public string CONFIRMACIONCONTRASENA { get; set; }

            [StringLength(11)]
            [Display(Name = "Teléfono:")]
            public string TELEFONO { get; set; }
        }
    }*/

}
