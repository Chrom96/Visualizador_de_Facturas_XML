using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Visualizador_de_Facturas_XML.Data;
using Visualizador_de_Facturas_XML.Models;
using System.Text.Json;

namespace Visualizador_de_Facturas_XML.Controllers
{

    public class FacturasController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FacturasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Facturas
        public async Task<IActionResult> Index(int? perfilId)
        {
            // Obtener la lista de perfiles para llenar el combo box
            var perfiles = await _context.Perfiles.ToListAsync();
            ViewBag.PerfilId = new SelectList(perfiles, "Id", "Nombre"); // Asegúrate de que "Nombre" es la propiedad que deseas mostrar

            // Filtrar las facturas por PerfilId si se proporciona
            IQueryable<Factura> facturasQuery = _context.Facturas.Include(f => f.Perfil); // Cambia var a IQueryable<Factura>

            if (perfilId != null)
            {
                facturasQuery = facturasQuery.Where(f => f.PerfilID == perfilId);
            }

            return View(await facturasQuery.ToListAsync());
        }

        // GET: Facturas/Details/5
        public async Task<IActionResult> Details(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.Perfil)
                .FirstOrDefaultAsync(m => m.UUID == id);

            if (factura == null)
            {
                return NotFound();
            }

            // Verifica que el XML esté presente en el campo XmlContent
            if (factura.XmlContent != null && factura.XmlContent.Length > 0)
            {
                var xmlContentString = Encoding.UTF8.GetString(factura.XmlContent);

                var xmlDocument = new XmlDocument();
                xmlDocument.LoadXml(xmlContentString);

                var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                nsmgr.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");
                nsmgr.AddNamespace("cfdi", "http://www.sat.gob.mx/cfd/4"); // Agrega el namespace para CFDI

                // Extraer datos del XML
                ViewBag.XmlUUID = xmlDocument.SelectSingleNode("//tfd:TimbreFiscalDigital/@UUID", nsmgr).Value;
                ViewBag.FechaEmision = xmlDocument.SelectSingleNode("//cfdi:Comprobante/@Fecha", nsmgr).Value;
                ViewBag.SubTotal = xmlDocument.SelectSingleNode("//cfdi:Comprobante/@SubTotal", nsmgr).Value;
                ViewBag.Total = xmlDocument.SelectSingleNode("//cfdi:Comprobante/@Total", nsmgr).Value;
                ViewBag.Iva =  Convert.ToDouble(ViewBag.Total) - Convert.ToDouble(ViewBag.SubTotal);
                ViewBag.Receptor = xmlDocument.SelectSingleNode("//cfdi:Comprobante/cfdi:Receptor/@Nombre", nsmgr).Value;
                ViewBag.ReceptorRfc = xmlDocument.SelectSingleNode("//cfdi:Comprobante/cfdi:Receptor/@Rfc", nsmgr).Value;
                ViewBag.Emisor = xmlDocument.SelectSingleNode("//cfdi:Comprobante/cfdi:Emisor/@Nombre", nsmgr).Value;
                ViewBag.EmisorRfc = xmlDocument.SelectSingleNode("//cfdi:Comprobante/cfdi:Emisor/@Rfc", nsmgr).Value;


                //
                // Lista para almacenar conceptos
                var conceptos = new List<ConceptoViewModel>();

                // Extraer conceptos
                var conceptoNodes = xmlDocument.SelectNodes("//cfdi:Conceptos/cfdi:Concepto", nsmgr);
                foreach (XmlNode conceptoNode in conceptoNodes)
                {
                    var concepto = new ConceptoViewModel
                    {
                        ClaveProdServ = conceptoNode.Attributes["ClaveProdServ"]?.Value,
                        Cantidad = conceptoNode.Attributes["Cantidad"]?.Value,
                        ClaveUnidad = conceptoNode.Attributes["ClaveUnidad"]?.Value,
                        Unidad = conceptoNode.Attributes["Unidad"]?.Value,
                        Descripcion = conceptoNode.Attributes["Descripcion"]?.Value,
                        ValorUnitario = conceptoNode.Attributes["ValorUnitario"]?.Value,
                        Importe = conceptoNode.Attributes["Importe"]?.Value
                    };

                    // Extraer impuestos
                    var impuestosNode = conceptoNode.SelectSingleNode("cfdi:Impuestos/cfdi:Traslados", nsmgr);
                    if (impuestosNode != null)
                    {
                        var traslados = impuestosNode.SelectNodes("cfdi:Traslado", nsmgr);
                        foreach (XmlNode traslado in traslados)
                        {
                            concepto.ImpuestoBase = traslado.Attributes["Base"]?.Value;
                            concepto.ImpuestoTipo = traslado.Attributes["Impuesto"]?.Value;
                            concepto.ImpuestoTasa = traslado.Attributes["TasaOCuota"]?.Value;
                            concepto.ImpuestoImporte = traslado.Attributes["Importe"]?.Value;
                        }
                    }

                    conceptos.Add(concepto);
                }
              

                ViewBag.Conceptos = conceptos; // Almacena la lista de conceptos en ViewBag

            }

            return PartialView(factura);
        }

        // GET: Facturas/Create
        public IActionResult Create()
        {
            ViewData["Perfiles2"] = _context.Perfiles.ToList();
            return PartialView();
        }

        // POST: Facturas/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("PerfilID")] Factura factura, IFormFile XmlContent)
        {
            if (ModelState.ContainsKey("XmlContent"))
            {
                ModelState.Remove("XmlContent");
            }
            if (ModelState.ContainsKey("Perfil"))
            {
                ModelState.Remove("Perfil");
            }
            if (ModelState.ContainsKey("UUID"))
            {
                ModelState.Remove("UUID");
            }

            // Validar si PerfilID es numérico
            if (!int.TryParse(factura.PerfilID.ToString(), out int perfilId))
            {
                ModelState.AddModelError("PerfilID", "El PerfilID debe ser un valor numérico.");
            }



            if (XmlContent == null || XmlContent.Length == 0)
            {
                ModelState.AddModelError("XmlContent", "Debe seleccionar un archivo XML válido.");
            }

            // Verificar si ya existe una factura con ese UUID
            bool PerfilExiste = await _context.Perfiles.AnyAsync(p => p.Id == factura.PerfilID);
            if (!PerfilExiste)
            {
                ModelState.AddModelError("PerfilID", "Selecciona un perfil existente.");
                ViewData["Perfiles2"] = _context.Perfiles.ToList();
                return PartialView();
            }

            if (ModelState.IsValid)
            {
                using (var stream = new MemoryStream())
                {
                    await XmlContent.CopyToAsync(stream);
                    byte[] xmlBytes = stream.ToArray();
                    var xmlContent = Encoding.UTF8.GetString(xmlBytes);

                    // Asignar el XML cargado a la propiedad byte[] en el modelo
                    factura.XmlContent = xmlBytes;

                    // Cargar el XML y extraer el UUID
                    var xmlDocument = new XmlDocument();
                    xmlDocument.LoadXml(xmlContent);
                    // Configurar el Namespace Manager para usar el espacio de nombres correcto

                    var nsmgr = new XmlNamespaceManager(xmlDocument.NameTable);
                    nsmgr.AddNamespace("tfd", "http://www.sat.gob.mx/TimbreFiscalDigital");

                    var uuidNode = xmlDocument.SelectSingleNode("//tfd:TimbreFiscalDigital/@UUID", nsmgr);
                    if (uuidNode != null)
                    {
                        string uuid = uuidNode.Value;

                        // Verificar si ya existe una factura con ese UUID
                        bool uuidExists = await _context.Facturas.AnyAsync(f => f.UUID == uuid);
                        if (uuidExists)
                        {
                            // Manejar el caso en que el UUID ya exista
                            ModelState.AddModelError("XmlContent", "La factura ya existe.");
                            ViewData["Perfiles2"] = _context.Perfiles.ToList();
                            return PartialView(factura); // Retorna la vista con el error si el UUID ya existe
                        }

                        factura.UUID = uuidNode.Value;
                    }

                    // Guardar la factura
                    _context.Add(factura);
                    await _context.SaveChangesAsync();
                    return Json(new { success = true, redirect = true, redirectUrl = Url.Action("Index") });
                }
            }
            ViewData["Perfiles2"] = _context.Perfiles.ToList();
            return PartialView(factura);
        }

        // GET: Facturas/Edit/5
        public async Task<IActionResult> Edit(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas.FindAsync(id);
            if (factura == null)
            {
                return NotFound();
            }
            ViewData["Perfiles"] = _context.Perfiles.ToList();
            return PartialView(factura);
        }

        // POST: Facturas/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(string id, [Bind("PerfilID, UUID")] Factura factura)
        {
            if (id != factura.UUID)
            {
                return NotFound();
            }
            // Remover propiedades no necesarias del ModelState
            if (ModelState.ContainsKey("XmlContent"))
            {
                ModelState.Remove("XmlContent");
            }
            if (ModelState.ContainsKey("Perfil"))
            {
                ModelState.Remove("Perfil");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Obtener la factura original desde la base de datos
                    var existingFactura = await _context.Facturas.FindAsync(id);
                    if (existingFactura == null)
                    {
                        return NotFound();
                    }
                    // Verificar si ya existe una factura con ese UUID
                    bool PerfilExiste = await _context.Perfiles.AnyAsync(p => p.Id == factura.PerfilID);
                    if (!PerfilExiste)
                    {
                        // Manejar el caso en que el UUID ya exista
                        ModelState.AddModelError("PerfilID", "Selecciona un perfil existente.");
                        factura = existingFactura;
                        ViewData["Perfiles"] = _context.Perfiles.ToList();
                        return PartialView(factura);
                    }
                    // Solo actualizar el PerfilID (dejando el UUID sin cambios)
                    existingFactura.PerfilID = factura.PerfilID;

                    _context.Update(existingFactura);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!FacturaExists(factura.UUID))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return Json(new { success = true, redirect = true, redirectUrl = Url.Action("Index") });
            }
            ViewData["PerfilID"] = new SelectList(_context.Perfiles, "Id", "Id", factura.PerfilID);
            return PartialView(factura);
        }

        // GET: Facturas/Delete/5
        public async Task<IActionResult> Delete(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var factura = await _context.Facturas
                .Include(f => f.Perfil)
                .FirstOrDefaultAsync(m => m.UUID == id);
            if (factura == null)
            {
                return NotFound();
            }

            return PartialView(factura);
        }

        // POST: Facturas/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(string id)
        {
            var factura = await _context.Facturas.FindAsync(id);
            if (factura != null)
            {
                _context.Facturas.Remove(factura);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool FacturaExists(string id)
        {
            return _context.Facturas.Any(e => e.UUID == id);
        }
    }

    public class ConceptoViewModel
    {
        public string ClaveProdServ { get; set; }
        public string Cantidad { get; set; }
        public string ClaveUnidad { get; set; }
        public string Unidad { get; set; }
        public string Descripcion { get; set; }
        public string ValorUnitario { get; set; }
        public string Importe { get; set; }
        public string ImpuestoBase { get; set; }
        public string ImpuestoTipo { get; set; }
        public string ImpuestoTasa { get; set; }
        public string ImpuestoImporte { get; set; }
    }
    public class TrasladadoViewModel
    {
        public string Base { get; set; }
        public string Impuesto { get; set; }

        public string TasaOCuota { get; set; }
        public string Importe { get; set; }
    }
}
