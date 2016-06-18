using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace GuiasOET.Models
{
    using System;
    using System.Collections.Generic;
    public class V_GUIAS_RESERVADOS
    {
        public string ID { get; set; }
        public string SALUDOID { get; set; }
        public string NOMBRE { get; set; }
        public string APELLIDOS { get; set; }
        public int PAX { get; set; }
        public DateTime ENTRA { get; set; }
        public DateTime SALE { get; set; }
        public DateTime SOLICITADAEL { get; set; }
        public DateTime MODIFICADO { get; set; }
        public int ULTIMA_MODIFICACION{ get; set; }
        public string ESTACION { get; set; }
    }
}

