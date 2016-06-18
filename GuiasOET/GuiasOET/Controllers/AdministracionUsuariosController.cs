using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using GuiasOET.Models;
using System.Diagnostics;
using System.Data.Entity.Infrastructure;
using System.Net;
using System.Web.UI.HtmlControls;
using System.Data.Entity;
using PagedList;
using MvcFlashMessages;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Collections;

namespace GuiasOET.Controllers
{
    public class AdministracionUsuariosController : Controller
    {

        private Entities1 baseDatos = new Entities1();

        /*Método para cargar el combobox de estación*/
        private void CargarEstacionesDropDownList(object estacionSeleccionada = null)
        {
            var EstacionesQuery = from d in baseDatos.GUIAS_ESTACION
                                  orderby d.NOMBREESTACION
                                  select d;
            ViewBag.NOMBREESTACION = new SelectList(EstacionesQuery, "NOMBREESTACION", "NOMBREESTACION", estacionSeleccionada);
        }

        // GET: /Seguridad/Login
        [AllowAnonymous]
        public ActionResult Login()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(string USUARIO_EMAIL, string CONTRASENA)
        {
            //Se retorna la primera aparicion del empleado que tiene como usuario el especificado por el usuario,
            //como el usuario es un campo único no habrán problemas. 
            GUIAS_EMPLEADO Guia;
            bool esCorreo = USUARIO_EMAIL.Contains("@");

            if (esCorreo == true)
            {
                Guia = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.EMAIL == USUARIO_EMAIL);
            }
            else
            {
                Guia = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.USUARIO == USUARIO_EMAIL);
            }


            if (Guia != null)
            {
                if (Guia.CONTRASENA == CONTRASENA)
                {
                    ModelState.Clear();
                    Session["IdUsuarioLogueado"] = Guia.CEDULA.ToString();
                    if (Guia.NOMBREEMPLEADO == null)
                    {
                        Session["NombreUsuarioLogueado"] = null;
                    }
                    else if (Guia.APELLIDO1 == null && Guia.APELLIDO2 == null || Guia.APELLIDO1 == null)
                    {
                        Session["NombreUsuarioLogueado"] = Guia.NOMBREEMPLEADO.ToString();
                    }
                    else if (Guia.APELLIDO2 == null)
                    {
                        Session["NombreUsuarioLogueado"] = Guia.NOMBREEMPLEADO.ToString() + " " + Guia.APELLIDO1.ToString();
                    }
                    else
                    {
                        Session["NombreUsuarioLogueado"] = Guia.NOMBREEMPLEADO.ToString() + " " + Guia.APELLIDO1.ToString() + " " + Guia.APELLIDO2.ToString();
                    }

                    Session["RolUsuarioLogueado"] = Guia.TIPOEMPLEADO.ToString();
                    Session["EstacionUsuarioLogueado"] = Guia.NOMBREESTACION.ToString();

                    return RedirectToAction("Inicio");
                }
                else
                {
                    ModelState.AddModelError("", "La contraseña ingresada es inválida");
                    return View();
                }
            }

            else
            {

                ModelState.Clear();
                ModelState.AddModelError("", "El usuario no se encuentra en el sistema");
                return View();
            }


        }


        // GET: ListaUsuarios
        /*Método GET de la pantalla ListaUsuarios*/
        public ActionResult ListaUsuarios(string sortOrder, string currentFilter, string searchString, int? page)
        {
            ViewBag.CurrentSort = sortOrder;
            ViewBag.NombreSortParm = String.IsNullOrEmpty(sortOrder) ? "Nombre" : "";
            ViewBag.Ape1SortParm = String.IsNullOrEmpty(sortOrder) ? "Apellido1" : "";
            ViewBag.Ape2SortParm = String.IsNullOrEmpty(sortOrder) ? "Apellido2" : "";
            ViewBag.EstacionSortParm = String.IsNullOrEmpty(sortOrder) ? "Estacion" : "";
            ViewBag.RolSortParm = String.IsNullOrEmpty(sortOrder) ? "Rol" : "";

            if (searchString != null)
            {
                page = 1;
            }
            else
            {
                searchString = currentFilter;
            }

            ViewBag.CurrentFilter = searchString;

            var empleados = from e in baseDatos.GUIAS_EMPLEADO select e;

            if (Session["RolUsuarioLogueado"].ToString().Contains("Local") || Session["RolUsuarioLogueado"].ToString().Contains("Interno"))
            {
                string estacion = Session["EstacionUsuarioLogueado"].ToString();
                empleados = empleados.Where(e => e.NOMBREESTACION.Equals(estacion));
            }
            else if (Session["RolUsuarioLogueado"].ToString().Contains("Externo"))
            {
                string cedula = Session["IdUsuarioLogueado"].ToString();
                empleados = empleados.Where(e => e.CEDULA.Equals(cedula));
            }

            if (!String.IsNullOrEmpty(searchString))
            {
                if (Session["RolUsuarioLogueado"].ToString().Contains("Global"))
                {
                    empleados = empleados.Where(e => e.APELLIDO1.Contains(searchString)
                                       || e.NOMBREEMPLEADO.Contains(searchString) || e.APELLIDO2.Contains(searchString)
                                       || e.NOMBREESTACION.Contains(searchString) || e.TIPOEMPLEADO.Contains(searchString)
                                       || e.USUARIO.Contains(searchString) || e.EMAIL.Contains(searchString));
                }
                else if (Session["RolUsuarioLogueado"].ToString().Contains("Local") || Session["RolUsuarioLogueado"].ToString().Contains("Interno"))
                {
                    string estacion = Session["EstacionUsuarioLogueado"].ToString();

                    empleados = empleados.Where(e => e.APELLIDO1.Contains(searchString)
                                       || e.NOMBREEMPLEADO.Contains(searchString) || e.APELLIDO2.Contains(searchString)
                                       || e.NOMBREESTACION.Contains(searchString) || e.TIPOEMPLEADO.Contains(searchString)
                                       || e.USUARIO.Contains(searchString) || e.EMAIL.Contains(searchString) && e.NOMBREESTACION.Equals(estacion));
                }
                else if (Session["RolUsuarioLogueado"].ToString().Contains("Externo"))
                {
                    string cedula = Session["IdUsuarioLogueado"].ToString();
                    empleados = empleados.Where(e => e.APELLIDO1.Contains(searchString)
                                       || e.NOMBREEMPLEADO.Contains(searchString) || e.APELLIDO2.Contains(searchString)
                                       || e.NOMBREESTACION.Contains(searchString) || e.TIPOEMPLEADO.Contains(searchString)
                                       || e.USUARIO.Contains(searchString) || e.EMAIL.Contains(searchString) && e.CEDULA.Equals(cedula));
                }

            }

            switch (sortOrder)
            {
                case "Nombre":
                    empleados = empleados.OrderBy(e => e.NOMBREEMPLEADO);
                    break;
                case "Apellido1":
                    empleados = empleados.OrderBy(e => e.APELLIDO1);
                    break;
                case "Apellido2":
                    empleados = empleados.OrderBy(e => e.APELLIDO2);
                    break;
                case "Estacion":
                    empleados = empleados.OrderBy(e => e.NOMBREESTACION);
                    break;
                case "Rol":
                    empleados = empleados.OrderBy(e => e.TIPOEMPLEADO);
                    break;
                default:
                    empleados = empleados.OrderBy(e => e.NOMBREESTACION);
                    break;
            }

            int pageSize = 8;
            int pageNumber = (page ?? 1);
            return View(empleados.ToPagedList(pageNumber, pageSize));
        }
        // GET: Inicio
        /*Método GET de la pantalla ListaUsuarios*/
        public ActionResult Inicio(int? page)
        {
            return View();
        }
        /*Método get de la pantalla InsertarUsuario*/
        public ActionResult InsertarUsuario()
        {
            CargarEstacionesDropDownList();
            return View();
        }

        // POST: /AdministracionUsuarios/Insertar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult InsertarUsuario(ManejoModelos nuevoUsuario, string tipoUsuario)
        {
            /*Se obtiene el tipo de usuario con los radiobutton*/
            nuevoUsuario.modeloEmpleado.TIPOEMPLEADO = tipoUsuario;

            /*Si el tipo de empleado es cualquier tipo de guía hay que verificar que haya dado los datos completos*/
            if (nuevoUsuario.modeloEmpleado.TIPOEMPLEADO.Contains("Guía"))
            {
                /*Si falta algún dato personal del guía no se debe permitir la inserción*/
                if (nuevoUsuario.modeloEmpleado.NOMBREEMPLEADO == null || nuevoUsuario.modeloEmpleado.APELLIDO1 == null || nuevoUsuario.modeloEmpleado.APELLIDO2 == null || nuevoUsuario.modeloEmpleado.DIRECCION == null)
                {
                    ModelState.AddModelError("", "Para agregar un guía los datos correspondientes a nombre, apellidos y dirección son requeridos.");
                }
                else
                {
                    /*Si es un guía pero no se le asoció ninguna estación no debe dejar guardarlo*/
                    if (nuevoUsuario.modeloEmpleado.NOMBREESTACION.Contains("Ninguna"))
                    {
                        ModelState.AddModelError("", "El guía debe tener asociado una estación en el sistema.");
                    }
                    /*Si si se asoció una estación válida al guía se guarda en la base de datos*/
                    else
                    {

                        /*Validación de los telefonos falta*/

                        nuevoUsuario.modeloEmpleado.CONFIRMAREMAIL = 0;

                        /*Si el modelo es válido se guarda en la base como una tupla*/
                        if (ModelState.IsValid)
                        {
                            GUIAS_EMPLEADO usuarios = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.USUARIO == nuevoUsuario.modeloEmpleado.USUARIO);
                            GUIAS_EMPLEADO cedula = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.CEDULA == nuevoUsuario.modeloEmpleado.CEDULA);
                            if (usuarios == null && cedula == null)
                            {
                                baseDatos.GUIAS_EMPLEADO.Add(nuevoUsuario.modeloEmpleado);
                                baseDatos.SaveChanges();
                                insertarTelefonos(nuevoUsuario);
                                CargarEstacionesDropDownList();
                                this.Flash("Éxito", "Nuevo usuario creado con éxito");
                                return RedirectToAction("InsertarUsuario");
                            }
                            else if (usuarios != null)
                            {
                                ModelState.AddModelError("", "Ya existe existe nombre de usuario en el sistema.");
                            }
                            else if (cedula != null)
                            {
                                ModelState.AddModelError("", "Ya existe este número de cédula en el sistema.");
                            }
                        }
                        /*Si el modelo es inválido no lo agrega a la base*/
                        else
                        {
                            /*Muestra un mensaje de error al usuario*/
                            if (nuevoUsuario.modeloEmpleado.CONTRASENA == null)
                            {
                                ModelState.AddModelError("", "Debe ingresar una contraseña para este usuario.");
                            }
                            else
                            {
                                if (nuevoUsuario.modeloEmpleado.CONTRASENA.Contains(nuevoUsuario.modeloEmpleado.CONFIRMACIONCONTRASENA))
                                {
                                    // ModelState.AddModelError("", "Ya existe otro usuario con esta cédula o nombre de usuario en el sistema.");
                                }
                            }
                            CargarEstacionesDropDownList();
                            ViewBag.tipoUsuario = nuevoUsuario.modeloEmpleado.TIPOEMPLEADO;
                            return View(nuevoUsuario);
                        }
                    }
                }
            }

            /*Si el rol es cualquier tipo de administrador*/
            else
            {
                /*validar telefonos*/
                nuevoUsuario.modeloEmpleado.CONFIRMAREMAIL = 0;

                /*Si no se escogió ninguna estación y el tipo de empleado es Administrador Local no debe dejar guardar en la base*/
                if (nuevoUsuario.modeloEmpleado.NOMBREESTACION.Contains("Ninguna") && nuevoUsuario.modeloEmpleado.TIPOEMPLEADO.Contains("Local"))
                {
                    ModelState.AddModelError("", "El administrador local debe tener asociado una estación en el sistema.");

                }
                /*Si el usuario es administrador global o escogió una estación entra a este else*/
                else
                {
                    /*Se realiza una ultima verificación que si el tipo de usuario es administrador Global no sea asociado a ninguna estación*/
                    if (nuevoUsuario.modeloEmpleado.TIPOEMPLEADO.Contains("Global"))
                    {
                        nuevoUsuario.modeloEmpleado.NOMBREESTACION = "Ninguna";
                    }

                    /*Si el modelo es válido se guarda en la base como una tupla*/
                    if (ModelState.IsValid)
                    {
                        GUIAS_EMPLEADO usuarios = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.USUARIO == nuevoUsuario.modeloEmpleado.USUARIO);
                        GUIAS_EMPLEADO cedula = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.CEDULA == nuevoUsuario.modeloEmpleado.CEDULA);
                        if (usuarios == null && cedula == null)
                        {
                            baseDatos.GUIAS_EMPLEADO.Add(nuevoUsuario.modeloEmpleado);
                            baseDatos.SaveChanges();
                            insertarTelefonos(nuevoUsuario);
                            CargarEstacionesDropDownList();
                            //service.Invoke(); // Do something awesome.
                            this.Flash("Éxito", "Nuevo usuario creado con éxito");
                            return RedirectToAction("InsertarUsuario");
                        }
                        else if (usuarios != null)
                        {
                            ModelState.AddModelError("", "Ya existe existe nombre de usuario en el sistema.");
                        }
                        else if (cedula != null)
                        {
                            ModelState.AddModelError("", "Ya existe este número de cédula en el sistema.");
                        }
                    }

                    /*Si el modelo no es válido no se guarda en la base de datos*/
                    else
                    {
                        /*Muestra un mensaje de error al usuario*/
                        if (nuevoUsuario.modeloEmpleado.CONTRASENA == null)
                        {
                            ModelState.AddModelError("", "Debe ingresar una contraseña para este usuario.");
                        }
                        else
                        {
                            if (nuevoUsuario.modeloEmpleado.CONTRASENA.Contains(nuevoUsuario.modeloEmpleado.CONFIRMACIONCONTRASENA))
                            {
                                //ModelState.AddModelError("", "Ya existe otro usuario con esta cédula o nombre de usuario en el sistema.");
                            }
                        }
                        CargarEstacionesDropDownList();
                        //service.Invoke(); // Do something awesome.
                        ViewBag.tipoUsuario = nuevoUsuario.modeloEmpleado.TIPOEMPLEADO;
                        var action = Url.Action("Index", "Flash");
                        return View(nuevoUsuario);
                    }
                }
            }

            CargarEstacionesDropDownList();
            //service.Invoke(); // Do something awesome.
            return View();
        }

        public void insertarTelefonos(ManejoModelos telefonos)
        {
            if (telefonos.modeloTelefono.TELEFONO != null)
            {
                telefonos.modeloTelefono.CEDULAEMPLEADO = telefonos.modeloEmpleado.CEDULA;
                baseDatos.GUIAS_TELEFONO.Add(telefonos.modeloTelefono);
                baseDatos.SaveChanges();
            }
            if (telefonos.modeloTelefono2.TELEFONO != null)
            {
                telefonos.modeloTelefono2.CEDULAEMPLEADO = telefonos.modeloEmpleado.CEDULA;
                baseDatos.GUIAS_TELEFONO.Add(telefonos.modeloTelefono2);
                baseDatos.SaveChanges();
            }
            if (telefonos.modeloTelefono3.TELEFONO != null)
            {
                telefonos.modeloTelefono3.CEDULAEMPLEADO = telefonos.modeloEmpleado.CEDULA;
                baseDatos.GUIAS_TELEFONO.Add(telefonos.modeloTelefono3);
                baseDatos.SaveChanges();
            }
            if (telefonos.modeloTelefono4.TELEFONO != null)
            {
                telefonos.modeloTelefono4.CEDULAEMPLEADO = telefonos.modeloEmpleado.CEDULA;
                baseDatos.GUIAS_TELEFONO.Add(telefonos.modeloTelefono4);
                baseDatos.SaveChanges();
            }
        }

        // GET: Modificar usuario
        public ActionResult ConsultarUsuario(int? id)
        {

            string identificacion;
            ManejoModelos modelo;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            identificacion = id.ToString();

            string consulta = "SELECT * FROM GUIAS_TELEFONO WHERE CedulaEmpleado ='" + identificacion + "'";

            IEnumerable<GUIAS_TELEFONO> telefonos = baseDatos.Database.SqlQuery<GUIAS_TELEFONO>(consulta);

            modelo = new ManejoModelos(baseDatos.GUIAS_EMPLEADO.Find(identificacion), telefonos);

            // modelo.modeloEmpleado.ESTADO = baseDatos.GUIAS_EMPLEADO.Find(identificacion).ESTADO;
            if (modelo == null)
            {
                return HttpNotFound();
            }

            ViewBag.tipoUsuario = modelo.modeloEmpleado.TIPOEMPLEADO;

            if (modelo.modeloEmpleado.ESTADO == 0)
            {
                ViewBag.estado = "Inactivo";
            }
            else
            {
                ViewBag.estado = "Activo";
            }

            return View(modelo);
        }



        // GET: Modificar usuario
        public ActionResult ModificarUsuario(int? id)
        {

            string identificacion;
            ManejoModelos modelo;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            identificacion = id.ToString();

            string consulta = "SELECT * FROM GUIAS_TELEFONO WHERE CedulaEmpleado ='" + identificacion + "'";

            IEnumerable<GUIAS_TELEFONO> telefonos = baseDatos.Database.SqlQuery<GUIAS_TELEFONO>(consulta);

            modelo = new ManejoModelos(baseDatos.GUIAS_EMPLEADO.Find(identificacion), telefonos);

            // modelo.modeloEmpleado.ESTADO = baseDatos.GUIAS_EMPLEADO.Find(identificacion).ESTADO;
            modelo.modeloEmpleado.CONFIRMACIONCONTRASENA = modelo.modeloEmpleado.CONTRASENA;
            if (modelo == null)
            {
                return HttpNotFound();
            }
            // Genera una variable de tipo lista con opciones para un ListBox.
            bool activo = modelo.modeloEmpleado.ESTADO == 1;
            bool inactivo = modelo.modeloEmpleado.ESTADO == 0;
            ViewBag.opciones = new List<System.Web.Mvc.SelectListItem> {
                new System.Web.Mvc.SelectListItem { Text = "Activo", Value = "1", Selected = activo},
                new System.Web.Mvc.SelectListItem { Text = "Inactivo", Value = "0", Selected = inactivo }
            };

            return View(modelo);
        }


        public void borrarTelefonos(string id)
        {

            GUIAS_TELEFONO T;

            T = baseDatos.GUIAS_TELEFONO.FirstOrDefault(i => i.CEDULAEMPLEADO == id);
            while (T != null)
            {
                baseDatos.GUIAS_TELEFONO.Remove(T);
                baseDatos.SaveChanges();
                T = baseDatos.GUIAS_TELEFONO.FirstOrDefault(i => i.CEDULAEMPLEADO == id);
            }


        }


        [HttpPost, ActionName("ModificarUsuario")]
        [ValidateAntiForgeryToken]
        public ActionResult ModificarPost(int? id, string estado, ManejoModelos usuario)
        {
            ManejoModelos modelo;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string identificacion = id.ToString();

            borrarTelefonos(identificacion);

            modelo = new ManejoModelos(baseDatos.GUIAS_EMPLEADO.Find(identificacion));

            var employeeToUpdate = modelo;


            if (employeeToUpdate.modeloEmpleado.TIPOEMPLEADO.Contains("Guía"))
            {
                /*Si falta algún dato personal del guía no se debe permitir la inserción*/
                if (employeeToUpdate.modeloEmpleado.NOMBREEMPLEADO == null || employeeToUpdate.modeloEmpleado.APELLIDO1 == null || employeeToUpdate.modeloEmpleado.APELLIDO2 == null || employeeToUpdate.modeloEmpleado.DIRECCION == null)
                {
                    ModelState.AddModelError("", "Para modificar un guía los datos correspondientes a nombre, apellidos y dirección son requeridos.");
                }
                else
                {
                    /*Si es un guía pero no se le asoció ninguna estación no debe dejar guardarlo*/
                    if (employeeToUpdate.modeloEmpleado.NOMBREESTACION.Contains("Ninguna"))
                    {
                        ModelState.AddModelError("", "El guía debe tener asociado una estación en el sistema.");
                    }
                    /*Si si se asoció una estación válida al guía se guarda en la base de datos*/
                    else
                    {

                        /*Validación de los telefonos falta*/

                        employeeToUpdate.modeloEmpleado.CONFIRMAREMAIL = 0;

                        /*Si el modelo es válido se guarda en la base como una tupla*/
                        if (TryUpdateModel(employeeToUpdate))
                        {
                            try
                            {
                                this.Flash("Éxito", "Usuario modificado con éxito");
                                employeeToUpdate.modeloEmpleado.ESTADO = Int32.Parse(estado);
                                baseDatos.SaveChanges();
                                insertarTelefonos(usuario);

                            }
                            catch (RetryLimitExceededException /* dex */)
                            {
                                //Log the error (uncomment dex variable name and add a line here to write a log.
                                ModelState.AddModelError("", "No es posible modificar en este momento, intente más tarde");
                            }
                        }
                        else
                        {
                            ModelState.AddModelError("", "Verifique que los campos obligatorios, posean datos");

                            /*Muestra un mensaje de error al usuario*/
                            if (employeeToUpdate.modeloEmpleado.CONTRASENA == null)
                            {
                                ModelState.AddModelError("", "Debe ingresar una contraseña para este usuario.");
                            }
                            else
                            {
                                if (employeeToUpdate.modeloEmpleado.CONTRASENA.Contains(employeeToUpdate.modeloEmpleado.CONFIRMACIONCONTRASENA))
                                {
                                    ModelState.AddModelError("", "Ya existe otro usuario con esta cédula o nombre de usuario en el sistema.");
                                }
                            }

                        }
                    }
                }
            }

            /*Si el rol es cualquier tipo de administrador*/
            else
            {
                /*validar telefonos*/
                employeeToUpdate.modeloEmpleado.CONFIRMAREMAIL = 0;

                /*Si no se escogió ninguna estación y el tipo de empleado es Administrador Local no debe dejar guardar en la base*/
                if (employeeToUpdate.modeloEmpleado.NOMBREESTACION.Contains("Ninguna") && employeeToUpdate.modeloEmpleado.TIPOEMPLEADO.Contains("Local"))
                {
                    ModelState.AddModelError("", "El administrador local debe tener asociado una estación en el sistema.");

                }
                /*Si el usuario es administrador global o escogió una estación entra a este else*/
                else
                {
                    /*Se realiza una ultima verificación que si el tipo de usuario es administrador Global no sea asociado a ninguna estación*/
                    if (employeeToUpdate.modeloEmpleado.TIPOEMPLEADO.Contains("Global"))
                    {
                        employeeToUpdate.modeloEmpleado.NOMBREESTACION = "Ninguna";
                    }

                    /*Si el modelo es válido se guarda en la base como una tupla*/
                    if (TryUpdateModel(employeeToUpdate))
                    {
                        try
                        {
                            this.Flash("Éxito", "Usuario modificado con éxito");
                            employeeToUpdate.modeloEmpleado.ESTADO = Int32.Parse(estado);
                            baseDatos.SaveChanges();
                            insertarTelefonos(usuario);

                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.
                            ModelState.AddModelError("", "No es posible modificar en este momento, intente más tarde");
                        }
                    }
                    /*Si el modelo no es válido no se guarda en la base de datos*/
                    else
                    {
                        /*Muestra un mensaje de error al usuario*/
                        if (employeeToUpdate.modeloEmpleado.CONTRASENA == null)
                        {
                            ModelState.AddModelError("", "Debe ingresar una contraseña para este usuario.");
                        }
                        else
                        {
                            if (employeeToUpdate.modeloEmpleado.CONTRASENA.Contains(employeeToUpdate.modeloEmpleado.CONFIRMACIONCONTRASENA))
                            {
                                ModelState.AddModelError("", "Ya existe otro usuario con esta cédula o nombre de usuario en el sistema.");
                            }
                        }

                    }
                }

            }


            bool activo = modelo.modeloEmpleado.ESTADO == 1;
            bool inactivo = modelo.modeloEmpleado.ESTADO == 0;
            ViewBag.opciones = new List<SelectListItem> {
                new SelectListItem { Text = "Activo", Value = "1", Selected = activo},
                new SelectListItem { Text = "Inactivo", Value = "0", Selected = inactivo }
            };
            return View(employeeToUpdate);
        }

        // GET: /AdministracionUsuarios/OlvidarContrasena
        public ActionResult OlvidarContrasena()
        {
            return View();
        }


        // GET: /Seguridad/ReestablecerContraseña
        [AllowAnonymous]
        public ActionResult ModificarContrasena()
        {
            return View();
        }



        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> OlvidarContrasena(string USUARIO_EMAIL)
        {

            GUIAS_EMPLEADO GuiaActualizar;
            GUIAS_CONFIGURACIONCORREO GuiaConfiguracionCorreo = baseDatos.GUIAS_CONFIGURACIONCORREO.FirstOrDefault();
            string emailEmisor = GuiaConfiguracionCorreo.EMAIL;
            Debug.WriteLine(emailEmisor);
            string contrasenaEmisor = GuiaConfiguracionCorreo.CONTRASENA;
            Debug.WriteLine(contrasenaEmisor);
            string puertoStr = GuiaConfiguracionCorreo.PUERTO;
            Debug.WriteLine(puertoStr);
            int puerto = Int32.Parse(puertoStr);
            Debug.WriteLine(puerto);



            string host = GuiaConfiguracionCorreo.HOST;


            bool esCorreo = USUARIO_EMAIL.Contains("@");
            if (esCorreo == true)
            {
                GuiaActualizar = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.EMAIL == USUARIO_EMAIL);
            }
            else
            {
                GuiaActualizar = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.USUARIO == USUARIO_EMAIL);
            }

            if (GuiaActualizar != null)
            {
                string email = GuiaActualizar.EMAIL;
                string nombreUsuario = "";
                if (GuiaActualizar.NOMBREEMPLEADO != null)
                {
                    nombreUsuario = GuiaActualizar.NOMBREEMPLEADO;
                }

                GuiaActualizar.CONFIRMACIONCONTRASENA = GuiaActualizar.CONTRASENA;
                GuiaActualizar.CONFIRMAREMAIL = 1;

                if (TryUpdateModel(GuiaActualizar, new string[] { "CONFIRMAREMAIL" }))
                {
                    try
                    {
                        baseDatos.SaveChanges();
                        var message = new MailMessage();
                        message.To.Add(new MailAddress(email));  // replace with valid value 
                        message.From = new MailAddress(emailEmisor);
                        message.Subject = "Restablezca la contraseña solicitada de su cuenta OET/ESINTRO";
                        var callbackUrl = Url.Action("ReestablecerContraseña", "AdministracionUsuarios", null, protocol: Request.Url.Scheme);
                        string mensaje;
                        if (nombreUsuario != "")
                        {
                            mensaje = "Estimado/a" + nombreUsuario + ":" + "<br/><br/>" + "Ha solicitado que se restablezca su contraseña. Haga clic en el siguiente vínculo, el cual le llevará a una página web de OET/ESINTRO en la que podrá cambiar la contraseña:" + "<br/><br/>" + "<a href =\"" + callbackUrl + "\">Restablecer contraseña</a> " + "<br/><br/>" + " Gracias, " + "<br/><br/>" + "Equipo administrador OET/ESINTRO";
                        }
                        else
                        {
                            mensaje = "Ha solicitado que se restablezca su contraseña. Haga clic en el siguiente vínculo, el cual le llevará a una página web de OET/ESINTRO en la que podrá cambiar la contraseña:" + "<br/><br/>" + "<a href =\"" + callbackUrl + "\">Restablecer contraseña</a> " + "<br/><br/>" + " Gracias, " + "<br/><br/>" + "Equipo administrador OET/ESINTRO";
                        }

                        message.Body = string.Format(mensaje);
                        message.IsBodyHtml = true;

                        using (var smtp = new SmtpClient())
                        {
                            var credential = new NetworkCredential
                            {
                                UserName = emailEmisor,  // replace with valid value
                                Password = contrasenaEmisor  // replace with valid value
                            };
                            smtp.Credentials = credential;
                            smtp.Host = host;
                            smtp.Port = puerto;
                            smtp.EnableSsl = true;
                            await smtp.SendMailAsync(message);
                            return RedirectToAction("ConfirmarOlvidoContrasena");
                        }
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        ModelState.AddModelError("", "Los datos no se pudieron guardar");
                        return RedirectToAction("OlvidarContrasena");
                    }

                }

                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", "No se pudieron guarda cambios");
                    return View("OlvidarContrasena");
                }
            }
            else
            {
                ModelState.Clear();
                ModelState.AddModelError("", "El usuario o correo ingresado no se encuentra en el sistema");
                return View("OlvidarContrasena");
            }

        }




        // GET: /AdministracionUsuarios/ConfirmarOlvidoContrasena
        public ActionResult ConfirmarOlvidoContrasena()
        {
            return View();
        }

        // GET: /Seguridad/ReestablecerContraseña
        public ActionResult ReestablecerContraseña()
        {
            return View("ReestablecerContraseña");
        }

        // POST: /AdministracionUsuarios/ReestablecerContraseña
        [HttpPost]
        [AllowAnonymous]
        [ValidateAntiForgeryToken]
        public ActionResult ReestablecerContraseña(string USUARIO_EMAIL, string contrasena, string confirmacioncontrasena)
        {

            GUIAS_EMPLEADO GuiaActualizar;
            bool esCorreo = USUARIO_EMAIL.Contains("@");
            if (esCorreo == true)
            {
                GuiaActualizar = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.EMAIL == USUARIO_EMAIL);
            }
            else
            {
                GuiaActualizar = baseDatos.GUIAS_EMPLEADO.FirstOrDefault(i => i.USUARIO == USUARIO_EMAIL);
            }

            if (GuiaActualizar != null)
            {

                if (GuiaActualizar.CONFIRMAREMAIL == 1)
                {
                    GuiaActualizar.CONFIRMACIONCONTRASENA = GuiaActualizar.CONTRASENA;
                    GuiaActualizar.CONFIRMAREMAIL = 0;
                    if (TryUpdateModel(GuiaActualizar))
                    {
                        try
                        {
                            baseDatos.SaveChanges();
                            ModelState.Clear();
                            return RedirectToAction("Login");
                        }
                        catch (RetryLimitExceededException /* dex */)
                        {
                            //Log the error (uncomment dex variable name and add a line here to write a log.
                            ModelState.AddModelError("", "No fue posible guardar los cambios");
                            return View("ReestablecerContraseña");
                        }
                    }
                    else
                    {
                        ModelState.AddModelError("", "No fue posible guardar los cambios");
                        return View("ReestablecerContraseña");
                    }
                }
                else
                {
                    ModelState.Clear();
                    ModelState.AddModelError("", "Debe realizar una solicitud para restablecer la contraseña");
                    return View();
                }
            }
            else
            {

                ModelState.Clear();
                ModelState.AddModelError("", "El usuario o correo ingresado no se encuentra en el sistema");
                return View();
            }
        }


        [HttpGet, ActionName("CerrarSesionLogin")]
        public ActionResult CerrarSesionLogin()
        {
            Debug.WriteLine("entre a cerrar sesion");
            Session["IdUsuarioLogueado"] = null;
            Session["NombreUsuarioLogueado"] = null;
            Session["RolUsuarioLogueado"] = null;
            Session["EstacionUsuarioLogueado"] = null;
            return RedirectToActionPermanent("Login");
        }


        // GET: Eliminar usuario

        public ActionResult DesactivarUsuario(int? id)
        {
            CargarEstacionesDropDownList();
            ManejoModelos modelo;

            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            string identificacion = id.ToString();

            modelo = new ManejoModelos(baseDatos.GUIAS_EMPLEADO.Find(identificacion));

            var employeeToUpdate = modelo;
            decimal i = 0;
            var a=  employeeToUpdate.modeloEmpleado.ESTADO;
            var b =employeeToUpdate.modeloEmpleado.CEDULA;
            if (employeeToUpdate.modeloEmpleado.TIPOEMPLEADO.Contains("Guía"))
            {

                employeeToUpdate.modeloEmpleado.CONFIRMAREMAIL = 0;

                //employeeToUpdate.modeloEmpleado.ESTADO = 0;
                if (TryUpdateModel(employeeToUpdate))
                {
                    try
                    {
                        this.Flash("Éxito", "Usuario desactivado con éxito");
                        employeeToUpdate.modeloEmpleado.ESTADO = i;
                        baseDatos.SaveChanges();

                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        //Log the error (uncomment dex variable name and add a line here to write a log.
                        ModelState.AddModelError("", "No es posible desactivar el usuario en este momento, intente más tarde");
                    }
                }
            }
            else
            {
                if (employeeToUpdate.modeloEmpleado.TIPOEMPLEADO.Contains("Global"))
                {
                    employeeToUpdate.modeloEmpleado.NOMBREESTACION = "Ninguna";
                }

                if (TryUpdateModel(employeeToUpdate))
                {
                    try
                    {
                        this.Flash("Éxito", "Usuario desactivado con éxito");
                        employeeToUpdate.modeloEmpleado.ESTADO = 0;
                        baseDatos.SaveChanges();
                    }
                    catch (RetryLimitExceededException /* dex */)
                    {
                        ModelState.AddModelError("", "No es posible desactivar el usuario en este momento, intente más tarde");
                    }
                }
                /*Si el modelo no es válido no se guarda en la base de datos*/
            }
            return RedirectToAction("ListaUsuarios");
        }
    }
}