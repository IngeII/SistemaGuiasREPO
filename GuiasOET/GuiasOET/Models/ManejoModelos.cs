using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuiasOET.Models
{
    public class ManejoModelos
    {
        public GuiasOET.Models.GUIAS_EMPLEADO modeloEmpleado { get; set; }
        public GuiasOET.Models.GUIAS_TELEFONO modeloTelefono { get; set; }
        public GuiasOET.Models.GUIAS_TELEFONO modeloTelefono2 { get; set; }
        public GuiasOET.Models.GUIAS_TELEFONO modeloTelefono3 { get; set; }
        public GuiasOET.Models.GUIAS_TELEFONO modeloTelefono4 { get; set; }

        public ManejoModelos(GuiasOET.Models.GUIAS_EMPLEADO empleado)
        {
            modeloEmpleado = empleado;
        }

        public ManejoModelos(GuiasOET.Models.GUIAS_EMPLEADO empleado, GuiasOET.Models.GUIAS_TELEFONO telefono)
        {

            modeloEmpleado = empleado;
            modeloTelefono = telefono;
        }

        public ManejoModelos(GuiasOET.Models.GUIAS_EMPLEADO empleado, GuiasOET.Models.GUIAS_TELEFONO telefono, GuiasOET.Models.GUIAS_TELEFONO telefono2, GuiasOET.Models.GUIAS_TELEFONO telefono3, GuiasOET.Models.GUIAS_TELEFONO telefono4)
        {

            modeloEmpleado = empleado;
            modeloTelefono = telefono;
            modeloTelefono2 = telefono2;
            modeloTelefono3 = telefono3;
            modeloTelefono4 = telefono4;

        }

        public ManejoModelos(GuiasOET.Models.GUIAS_EMPLEADO empleado, IEnumerable<GUIAS_TELEFONO> telefonos)
        {

            int indice = 0;
            modeloEmpleado = empleado;

            if (telefonos!=null) {
                while (indice < telefonos.Count()) {

                    switch (indice)
                    {
                        case 0:
                            modeloTelefono = telefonos.ElementAt(indice);
                            break;
                        case 1:
                            modeloTelefono2 = telefonos.ElementAt(indice);
                            break;
                        case 2:
                            modeloTelefono3 = telefonos.ElementAt(indice);
                            break;
                        default:
                            modeloTelefono4 = telefonos.ElementAt(indice);
                            break;

                    }
                    ++indice;
                }
            }
            

        }




        public ManejoModelos()
        {

        }

    }
}