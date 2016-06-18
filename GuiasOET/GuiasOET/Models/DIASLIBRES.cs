using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using PagedList;

namespace GuiasOET.Models
{
    public class DIASLIBRES
    {
        public List<GuiasOET.Models.GUIAS_EMPLEADO> guias = new List<GuiasOET.Models.GUIAS_EMPLEADO>();
        public GuiasOET.Models.GUIAS_EMPLEADO guias1 { get; set; }

        public IPagedList<GUIAS_ROLDIASLIBRES> roldialibre { get; set; }

        public DIASLIBRES(GuiasOET.Models.GUIAS_EMPLEADO empleado)
        {
            guias1 = empleado;
        }
    }
}
