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

        public async Task<IActionResult> Index()
        {
            return View(await _context.Customers.ToListAsync());
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
            // Initialize a new Customer with default values
            var customer = new Customer
            {
                DateOfBirth = DateTime.Now.AddYears(-18) // Default to 18 years ago
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
                    // Handle file upload
                    if (customer.PassportPhotoFile != null && customer.PassportPhotoFile.Length > 0)
                    {
                        customer.PassportPhotoPath = await SaveImage(customer.PassportPhotoFile);
                    }

                    customer.CreatedAt = DateTime.Now;
                    _context.Add(customer);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Error saving customer: " + ex.Message);
                }
            }

            // If we got this far, something failed; redisplay form
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
        public async Task<IActionResult> Edit(int id, [Bind("CustomerId,FullName,Email,Phone,Address,DateOfBirth,Gender,Occupation,AnnualIncome,PassportPhotoFile,PassportPhotoPath,CreatedAt")] Customer customer, bool removePhoto = false)
        {
            if (id != customer.CustomerId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingCustomer = await _context.Customers.FindAsync(id);

                    // Handle photo removal
                    if (removePhoto && !string.IsNullOrEmpty(existingCustomer.PassportPhotoPath))
                    {
                        DeleteImage(existingCustomer.PassportPhotoPath);
                        existingCustomer.PassportPhotoPath = null;
                    }

                    // Handle new photo upload
                    if (customer.PassportPhotoFile != null && customer.PassportPhotoFile.Length > 0)
                    {
                        // Delete old image if exists
                        if (!string.IsNullOrEmpty(existingCustomer.PassportPhotoPath))
                        {
                            DeleteImage(existingCustomer.PassportPhotoPath);
                        }
                        // Save new image
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

        private bool CustomerExists(int customerId)
        {
            throw new NotImplementedException();
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
                // Delete associated photo if it exists
                if (!string.IsNullOrEmpty(customer.PassportPhotoPath))
                {
                    DeleteImage(customer.PassportPhotoPath);
                }

                _context.Customers.Remove(customer);
                await _context.SaveChangesAsync();
            }

            return RedirectToAction(nameof(Index));
        }

        private async Task<string> SaveImage(IFormFile imageFile)
        {
            string wwwRootPath = _hostEnvironment.WebRootPath;
            string fileName = Path.GetFileNameWithoutExtension(imageFile.FileName);
            string extension = Path.GetExtension(imageFile.FileName);
            fileName = fileName + DateTime.Now.ToString("yymmssfff") + extension;
            string path = Path.Combine(wwwRootPath + "/images/customers/", fileName);

            // Create directory if it doesn't exist
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