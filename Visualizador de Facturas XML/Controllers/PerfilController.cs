using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Visualizador_de_Facturas_XML.Data;
using Visualizador_de_Facturas_XML.Models;

namespace Visualizador_de_Facturas_XML.Controllers
{
    public class PerfilController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PerfilController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Perfil
        public async Task<IActionResult> Index()
        {
            return View(await _context.Perfiles.ToListAsync());
        }

        // GET: Perfil/Details/5
        public async Task<IActionResult> Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfiles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (perfil == null)
            {
                return NotFound();
            }
            ViewData["Facturas"] = _context.Facturas.Where(f => f.PerfilID == perfil.Id).ToList();
            return PartialView(perfil);

        }

        // GET: Perfil/Create
        public IActionResult Create()
        {
            return PartialView();
        }

        // POST: Perfil/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Nombre")] Perfil perfil)
        {
            if (ModelState.IsValid)
            {
                _context.Add(perfil);
                await _context.SaveChangesAsync();
                return Json(new { success = true, redirect = true, redirectUrl = Url.Action("Index") });
            }
            return PartialView(perfil);
        }

        // GET: Perfil/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfiles.FindAsync(id);
            if (perfil == null)
            {
                return NotFound();
            }
            return PartialView(perfil);
        }

        // POST: Perfil/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Nombre")] Perfil perfil)
        {
            if (id != perfil.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(perfil);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PerfilExists(perfil.Id))
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
            return PartialView(perfil);
        }

        // GET: Perfil/Delete/5
        public async Task<IActionResult> Delete(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var perfil = await _context.Perfiles
                .FirstOrDefaultAsync(m => m.Id == id);
            if (perfil == null)
            {
                return NotFound();
            }

            return PartialView(perfil);
        }

        // POST: Perfil/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var perfil = await _context.Perfiles.FindAsync(id);
            if (perfil != null)
            {
                _context.Perfiles.Remove(perfil);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PerfilExists(int id)
        {
            return _context.Perfiles.Any(e => e.Id == id);
        }
    }
}
