using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanManagementSystem.Data;
using LoanManagementSystem.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace LoanManagementSystem.Controllers
{
    [Authorize(Roles = "admin,office")]
    public class CustomersController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _hostEnvironment;

        public CustomersController(ApplicationDbContext context, IWebHostEnvironment hostEnvironment)
        {
            _context = context;
            _hostEnvironment = hostEnvironment;
        }

        // Show active customers only for non-admins; all for admin
        public async Task<IActionResult> Index()
        {
            var customers = _context.Customers.AsQueryable();

            if (!User.IsInRole("admin"))
            {
                customers = customers.Where(c => !c.IsDeleted);
            }

            return View(await customers.ToListAsync());
        }

        // Separate page for deleted customers
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> DeletedCustomers()
        {
            var deletedCustomers = await _context.Customers
                .Where(c => c.IsDeleted)
                .OrderByDescending(c => c.UpdatedAt ?? c.CreatedAt)
                .ToListAsync();

            return View(deletedCustomers);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        public IActionResult Create()
        {
            var customer = new Customer
            {
                DateOfBirth = DateTime.Now.AddYears(-18) // Default age: 18 years
            };
            return View(customer);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FullName,Email,Phone,Address,DateOfBirth,Gender,Occupation,AnnualIncome,PassportPhotoFile")] Customer customer)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    if (customer.PassportPhotoFile != null && customer.PassportPhotoFile.Length > 0)
                    {
                        customer.PassportPhotoPath = await SaveImage(customer.PassportPhotoFile);
                    }

                    customer.CreatedAt = DateTime.Now;
                    customer.IsDeleted = false;
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving customer: " + ex.Message);
                }
            }
            return View(customer);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FindAsync(id);
            if (customer == null) return NotFound();

            return View(customer);
        }

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Edit(
    int id,
    [Bind("CustomerId,FullName,Email,Phone,Address,DateOfBirth,Gender,Occupation,AnnualIncome,PassportPhotoFile,PassportPhotoPath,CreatedAt")] Customer customer,
    bool removePhoto = false)
{
    if (id != customer.CustomerId) return NotFound();

    var existingCustomer = await _context.Customers.FindAsync(id);
    if (existingCustomer == null) return NotFound();

    if (ModelState.IsValid)
    {
        try
        {
            // Handle photo removal
            if (removePhoto && !string.IsNullOrEmpty(existingCustomer.PassportPhotoPath))
            {
                DeleteImage(existingCustomer.PassportPhotoPath);
                existingCustomer.PassportPhotoPath = null;
            }

            // Handle new photo upload
            if (customer.PassportPhotoFile != null && customer.PassportPhotoFile.Length > 0)
            {
                if (!string.IsNullOrEmpty(existingCustomer.PassportPhotoPath))
                {
                    DeleteImage(existingCustomer.PassportPhotoPath);
                }
                existingCustomer.PassportPhotoPath = await SaveImage(customer.PassportPhotoFile);
            }

            // Update other fields
            existingCustomer.FullName = customer.FullName;
            existingCustomer.Email = customer.Email;
            existingCustomer.Phone = customer.Phone;
            existingCustomer.Address = customer.Address;
            existingCustomer.DateOfBirth = customer.DateOfBirth;
            existingCustomer.Gender = customer.Gender;
            existingCustomer.Occupation = customer.Occupation;
            existingCustomer.AnnualIncome = customer.AnnualIncome;
            existingCustomer.UpdatedAt = DateTime.Now;

            _context.Update(existingCustomer);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!CustomerExists(customer.CustomerId))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }
        return RedirectToAction(nameof(Index));
    }

    return View(customer);
}



        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var customer = await _context.Customers.FirstOrDefaultAsync(c => c.CustomerId == id);
            if (customer == null) return NotFound();

            return View(customer);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer != null)
            {
                // Soft delete instead of permanent deletion
                customer.IsDeleted = true;
                customer.UpdatedAt = DateTime.Now;

                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool CustomerExists(int customerId)
        {
            return _context.Customers.Any(e => e.CustomerId == customerId);
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            string extension = Path.GetExtension(imageFile.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/images/customers/", fileName);

            Directory.CreateDirectory(Path.GetDirectoryName(path));

            using (var fileStream = new FileStream(path, FileMode.Create))
            {
                await imageFile.CopyToAsync(fileStream);
            }

            return "/images/customers/" + fileName;
        }

        private void DeleteImage(string imagePath)
        {
            if (string.IsNullOrEmpty(imagePath)) return;

            string wwwRootPath = _hostEnvironment.WebRootPath;
            var fullPath = Path.Combine(wwwRootPath, imagePath.TrimStart('/'));

            if (System.IO.File.Exists(fullPath))
            {
                System.IO.File.Delete(fullPath);
            }
        }
    }
}
