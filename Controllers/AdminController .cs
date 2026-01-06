using HospitalManagement.AppStatus;
using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.Services;
using HospitalManagement.ViewModel;
using iTextSharp.text;
using iTextSharp.text.pdf;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace HospitalManagement.Controllers
{

    [Authorize(Roles = "ADMINISTRATOR,CONSUMABLESMANAGER")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly EmailService _emailService;  // ← Use concrete class
        private readonly NotificationService _notificationService;

        public AdminController(ApplicationDbContext context, EmailService emailService, NotificationService notificationService)
        {
            _context = context;
            _emailService = emailService;
            _notificationService = notificationService;
        }



        public async Task<IActionResult> AdminDashboard()
        {
            var currentUserName = User.Identity.Name;

            var today = DateTime.Today;
            var weekAgo = today.AddDays(-6); // past 7 days

            // Assuming LoginAudit table or similar with Username and LoginTime
            var weeklyLogins = await _context.LoginAudits
                .Where(x => x.LoginTime.Date >= weekAgo)
                .GroupBy(x => x.LoginTime.Date)
                .Select(g => new LoginActivityData
                {
                    Date = g.Key.ToString("ddd"), // e.g. Mon, Tue
                    Count = g.Count()
                })
                .ToListAsync();

            // FIX: Get employeeId from User.Claims
            int? employeeId = null;
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeID" || c.Type == "employeeId" || c.Type == "sub" || c.Type.EndsWith("nameidentifier"));
            if (employeeIdClaim != null && int.TryParse(employeeIdClaim.Value, out int parsedId))
            {
                employeeId = parsedId;
            }

            // Just store the count
            ViewData["UnreadNotificationCount"] = employeeId.HasValue
                ? await _notificationService.GetUnreadCountAsync(employeeId.Value)
                : 0;

            var model = new AdminDashboardViewModel
            {
                UserName = currentUserName,
                TotalBeds = await _context.Beds.CountAsync(b => b.Status == BedStatus.Available),
                TotalWards = await _context.Wards.CountAsync(w => w.WardStatus == Status.Active),
                TotalEmployees = await _context.Employees.CountAsync(e => e.IsActive == Status.Active),
                TotalConsumables = await _context.Consumables.CountAsync(c => c.ConsumableStatus == Status.Active),
                TotalMedications = await _context.Medications.CountAsync(m => m.MedicationStatus == Status.Active),
                ActiveAllergies = await _context.Allergies.CountAsync(a => a.AlleryStatus == Status.Active),
                WeeklyLogins = weeklyLogins
            };

            return View(model);
        }

















        // Allergy Management - AJAX Operations


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMultipleAjax([FromBody] List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "No allergies selected.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var allergies = await _context.Allergies
                .IgnoreQueryFilters()
                .Where(a => selectedIds.Contains(a.AllergyId))
                .ToListAsync();

            foreach (var allergy in allergies)
            {
                allergy.AlleryStatus = Status.Active;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{allergies.Count} allergy(s) restored.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultipleAjax([FromBody] List<int> selectedIds)
        {
            if (selectedIds == null || !selectedIds.Any())
            {
                TempData["ErrorMessage"] = "No allergies selected.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var allergies = await _context.Allergies
                .Where(a => selectedIds.Contains(a.AllergyId))
                .ToListAsync();

            foreach (var allergy in allergies)
            {
                allergy.AlleryStatus = Status.Delete;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{allergies.Count} allergy(s) deleted.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }



        // Allergy Management 

        public async Task<IActionResult> ListOfAllergies()
        {
            var list = await _context.Allergies.ToListAsync();
            return View(list);
        }

        public async Task<IActionResult> DetailsOfAllergies(int id)
        {
            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy == null || allergy.AlleryStatus == Status.Delete)
            {
                TempData["ErrorMessage"] = "Allergy not found.";
                return NotFound();
            }

            return View(allergy);
        }

        // GET
        public IActionResult AddAllergy() => View();

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddAllergy(Allergy allergy)
        {
            if (ModelState.IsValid)
            {
                allergy.AlleryStatus = Status.Active;
                _context.Allergies.Add(allergy);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Allergy added successfully.";
                return RedirectToAction(nameof(ListOfAllergies));
            }

            TempData["ErrorMessage"] = "Failed to add allergy. Please check input.";
            return View(allergy);
        }

        // GET
        public async Task<IActionResult> EditAllergy(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid allergy ID.";
                return NotFound();
            }

            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy == null)
            {
                TempData["ErrorMessage"] = "Allergy not found.";
                return NotFound();
            }

            return View(allergy);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAllergy(int id, Allergy allergy)
        {
            if (id != allergy.AllergyId)
            {
                TempData["ErrorMessage"] = "Mismatch in allergy ID.";
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(allergy);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Allergy updated successfully.";
                    return RedirectToAction(nameof(ListOfAllergies));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Allergies.Any(a => a.AllergyId == id))
                    {
                        TempData["ErrorMessage"] = "Allergy no longer exists.";
                        return NotFound();
                    }
                    else
                    {
                        TempData["ErrorMessage"] = "A concurrency error occurred.";
                        throw;
                    }
                }
            }

            TempData["ErrorMessage"] = "Invalid data. Please try again.";
            return View(allergy);
        }

        // GET
        public async Task<IActionResult> DeleteAllergy(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid allergy ID.";
                return NotFound();
            }

            var allergy = await _context.Allergies.FirstOrDefaultAsync(a => a.AllergyId == id);
            if (allergy == null)
            {
                TempData["ErrorMessage"] = "Allergy not found.";
                return NotFound();
            }

            return View(allergy);
        }

        // POST (SOFT DELETE)
        [HttpPost, ActionName("DeleteAllergy")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAllergyConfirmed(int id)
        {
            var allergy = await _context.Allergies.FindAsync(id);
            if (allergy == null)
            {
                TempData["ErrorMessage"] = "Allergy not found.";
                return NotFound();
            }

            allergy.AlleryStatus = Status.Delete;
            _context.Update(allergy);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Allergy deleted successfully.";
            return RedirectToAction(nameof(ListOfAllergies));
        }

        // OPTIONAL: View Deleted
        public async Task<IActionResult> DeletedAllergy()
        {
            var deleted = await _context.Allergies
                .IgnoreQueryFilters()
                .Where(a => a.AlleryStatus == Status.Delete)
                .ToListAsync();

            return View(deleted);
        }

        // OPTIONAL: Restore
        public async Task<IActionResult> RestoreAllergy(int id)
        {
            var allergy = await _context.Allergies
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.AllergyId == id);

            if (allergy == null)
            {
                TempData["ErrorMessage"] = "Allergy not found.";
                return NotFound();
            }

            allergy.AlleryStatus = Status.Active;
            _context.Update(allergy);
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Allergy restored successfully.";
            return RedirectToAction(nameof(ListOfAllergies));
        }

        public async Task<IActionResult> AllergyReport()
        {
            try
            {
                var allergies = await _context.Allergies.ToListAsync();

                using (var stream = new MemoryStream())
                {
                    using (var document = new Document(PageSize.A4, 25, 25, 30, 30))
                    {
                        var writer = PdfWriter.GetInstance(document, stream);
                        writer.CloseStream = false;

                        // PDF Metadata
                        document.AddTitle("Allergy Report");
                        document.AddAuthor("Hospital Management System");
                        document.AddCreationDate();

                        document.Open();

                        // Fonts
                        var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                        var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                        var cellFont = FontFactory.GetFont(FontFactory.HELVETICA, 10);
                        var footerFont = FontFactory.GetFont(FontFactory.HELVETICA_OBLIQUE, 9);

                        // Title
                        document.Add(new Paragraph("Allergy Report", titleFont));
                        document.Add(new Paragraph(" "));

                        if (!allergies.Any())
                        {
                            document.Add(new Paragraph("No allergy data available.", cellFont));
                        }
                        else
                        {
                            // Table Setup
                            PdfPTable table = new PdfPTable(3);
                            table.WidthPercentage = 100;
                            table.SetWidths(new float[] { 2f, 4f, 2f });

                            // Headers
                            string[] headers = { "Allergy Name", "Description", "Status" };
                            foreach (var header in headers)
                            {
                                var cell = new PdfPCell(new Phrase(header, headerFont))
                                {
                                    BackgroundColor = BaseColor.LightGray,
                                    Padding = 5,
                                    HorizontalAlignment = Element.ALIGN_CENTER
                                };
                                table.AddCell(cell);
                            }

                            // Rows
                            foreach (var allergy in allergies)
                            {
                                table.AddCell(new Phrase(allergy.Name ?? "-", cellFont));
                                table.AddCell(new Phrase(allergy.Description ?? "-", cellFont));
                                table.AddCell(new Phrase(allergy.AlleryStatus.ToString(), cellFont));
                            }

                            document.Add(table);
                        }

                        document.Add(new Paragraph("\nGenerated on: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"), footerFont));

                        document.Close();
                    }

                    stream.Position = 0;
                    return File(stream.ToArray(), "application/pdf", "AllergyReport.pdf");
                }
            }
            catch (Exception ex)
            {
                // You can log the exception here using a logger if needed
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
        }


































































































        // Ward Management

        public async Task<IActionResult> ListOfWards()
        {
            var wards = await _context.Wards
                .Select(w => new WardListItemVm
                {
                    WardId = w.WardId,
                    WardName = w.WardName,
                    Description = w.Description,
                    Capacity = w.Capacity,
                    WardStatus = w.WardStatus,
                    ActiveBeds = w.Beds.Count(b => b.BedStatus == Status.Active)
                })
                .ToListAsync();

            return View(wards);
        }

        public async Task<IActionResult> DetailsOfWards(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward == null || ward.WardStatus == Status.Delete)
            {
                TempData["ErrorMessage"] = "Ward not found or has been deleted.";
                return NotFound();
            }

            return View(ward);
        }

        // GET: AddWard
        public IActionResult AddWard()
        {
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddWard(Ward ward)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return BadRequest(ModelState);
            }

            ward.WardStatus = Status.Active;
            _context.Wards.Add(ward);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ward added successfully.";
            return RedirectToAction(nameof(ListOfWards));
        }

        public async Task<IActionResult> EditWard(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid ward ID.";
                return NotFound();
            }

            var ward = await _context.Wards.FindAsync(id);
            if (ward == null)
            {
                TempData["ErrorMessage"] = "Ward not found.";
                return NotFound();
            }

            return View(ward);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditWard(int id, Ward ward)
        {
            if (id != ward.WardId)
            {
                TempData["ErrorMessage"] = "Ward ID mismatch.";
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return BadRequest(ModelState);
            }

            try
            {
                _context.Update(ward);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Ward updated successfully.";
                return RedirectToAction(nameof(ListOfWards));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Wards.Any(w => w.WardId == id))
                {
                    TempData["ErrorMessage"] = "Ward no longer exists.";
                    return NotFound();
                }
                else
                {
                    TempData["ErrorMessage"] = "Concurrency error occurred.";
                    throw;
                }
            }
        }

        public async Task<IActionResult> DeleteWard(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid ward ID.";
                return NotFound();
            }

            var ward = await _context.Wards.FirstOrDefaultAsync(a => a.WardId == id);
            if (ward == null)
            {
                TempData["ErrorMessage"] = "Ward not found.";
                return NotFound();
            }

            return View(ward);
        }

        [HttpPost, ActionName("DeleteWard")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteWardConfirmed(int id)
        {
            var ward = await _context.Wards.FindAsync(id);
            if (ward == null)
            {
                TempData["ErrorMessage"] = "Ward not found.";
                return NotFound();
            }

            ward.WardStatus = Status.Delete;
            _context.Update(ward);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ward deleted successfully.";
            return RedirectToAction(nameof(ListOfWards));
        }

        public async Task<IActionResult> DeletedWard()
        {
            var deleted = await _context.Wards
                .IgnoreQueryFilters()
                .Where(w => w.WardStatus == Status.Delete)
                .ToListAsync();

            return View(deleted);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreWard(int id)
        {
            var ward = await _context.Wards
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(w => w.WardId == id && w.WardStatus == Status.Delete);

            if (ward == null)
            {
                TempData["ErrorMessage"] = "Ward not found or already active.";
                return NotFound();
            }

            ward.WardStatus = Status.Active;
            _context.Update(ward);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Ward restored successfully.";
            return RedirectToAction(nameof(ListOfWards));
        }

        public async Task<IActionResult> WardReport()
        {
            var wards = await _context.Wards.ToListAsync();

            using (var stream = new MemoryStream())
            {
                // Create the document and set margins
                var document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Title
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var normalFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                document.Add(new Paragraph("Ward Report", titleFont));
                document.Add(new Paragraph($"Generated on: {DateTime.Now}", normalFont));
                document.Add(new Paragraph("\n"));

                // Create a table with 4 columns
                PdfPTable table = new PdfPTable(4)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 2f, 3f, 1.5f, 1f });

                // Add table headers
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                table.AddCell(new PdfPCell(new Phrase("Ward Name", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Description", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Status", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Capacity", headerFont)));

                // Add ward data
                foreach (var ward in wards)
                {
                    table.AddCell(new Phrase(ward.WardName ?? "", normalFont));
                    table.AddCell(new Phrase(ward.Description ?? "", normalFont));
                    table.AddCell(new Phrase(ward.WardStatus.ToString(), normalFont));
                    table.AddCell(new Phrase(ward.Capacity.ToString(), normalFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "WardReport.pdf");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMultipleWards([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No wards selected for deletion.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var wards = await _context.Wards
                .Where(w => ids.Contains(w.WardId))
                .ToListAsync();

            foreach (var ward in wards)
            {
                ward.WardStatus = Status.Delete;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{wards.Count} ward(s) deleted successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMultipleWards([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No wards selected for restoration.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var wards = await _context.Wards
                .IgnoreQueryFilters()
                .Where(w => ids.Contains(w.WardId) && w.WardStatus == Status.Delete)
                .ToListAsync();

            foreach (var ward in wards)
            {
                ward.WardStatus = Status.Active;
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{wards.Count} ward(s) restored successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }

















































































































        // Condition Management

        public async Task<IActionResult> ListOfConditions()
        {
            var conditions = await _context.Conditions.ToListAsync();
            return View(conditions);
        }

        public async Task<IActionResult> DetailsOfConditions(int id)
        {
            var condition = await _context.Conditions.FindAsync(id);
            if (condition == null || condition.ConditionStatus == Status.Delete)
            {
                TempData["ErrorMessage"] = "Condition not found or has been deleted.";
                return NotFound();
            }

            return View(condition);
        }


        // GET: AddConditions
        public IActionResult AddConditions()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConditions(Condition condition)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid condition data.";
                return View(condition);
            }

            condition.ConditionStatus = Status.Active;

            _context.Conditions.Add(condition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Condition added successfully.";
            return RedirectToAction(nameof(ListOfConditions));
        }

        public async Task<IActionResult> EditCondtions(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid condition ID.";
                return NotFound();
            }

            var condition = await _context.Conditions.FindAsync(id);
            if (condition == null)
            {
                TempData["ErrorMessage"] = "Condition not found.";
                return NotFound();
            }

            return View(condition);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCondtions(int id, Condition condition)
        {
            if (id != condition.ConditionId)
            {
                TempData["ErrorMessage"] = "ID mismatch.";
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Invalid input data.";
                return View(condition);
            }

            try
            {
                _context.Update(condition);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Condition updated successfully.";
                return RedirectToAction(nameof(ListOfConditions));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Conditions.Any(c => c.ConditionId == id))
                {
                    TempData["ErrorMessage"] = "Condition no longer exists.";
                    return NotFound();
                }
                else
                {
                    TempData["ErrorMessage"] = "Concurrency error occurred.";
                    throw;
                }
            }
        }

        public async Task<IActionResult> DeleteCondition(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid condition ID.";
                return NotFound();
            }

            var condition = await _context.Conditions.FirstOrDefaultAsync(c => c.ConditionId == id);
            if (condition == null)
            {
                TempData["ErrorMessage"] = "Condition not found.";
                return NotFound();
            }

            return View(condition);
        }

        [HttpPost, ActionName("DeleteCondition")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConditionConfirmed(int id)
        {
            var condition = await _context.Conditions.FindAsync(id);
            if (condition == null)
            {
                TempData["ErrorMessage"] = "Condition not found.";
                return NotFound();
            }

            condition.ConditionStatus = Status.Delete;
            _context.Update(condition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Condition deleted successfully.";
            return RedirectToAction(nameof(ListOfConditions));
        }

        public async Task<IActionResult> DeletedCondition()
        {
            var deleted = await _context.Conditions
                .IgnoreQueryFilters()
                .Where(c => c.ConditionStatus == Status.Delete)
                .ToListAsync();

            return View(deleted);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreCondition(int id)
        {
            var condition = await _context.Conditions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.ConditionId == id && c.ConditionStatus == Status.Delete);

            if (condition == null)
            {
                TempData["ErrorMessage"] = "Condition not found or already active.";
                return NotFound();
            }

            condition.ConditionStatus = Status.Active;
            _context.Update(condition);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Condition restored successfully.";
            return RedirectToAction(nameof(ListOfConditions));
        }

        public async Task<IActionResult> ConditionReport()
        {
            var conditions = await _context.Conditions.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 12);

                // Title
                document.Add(new Paragraph("Condition Report", titleFont));
                document.Add(new Paragraph($"Generated on: {DateTime.Now}", bodyFont));
                document.Add(new Paragraph("\n"));

                // Create table with 3 columns
                PdfPTable table = new PdfPTable(3)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 2f, 4f, 2f });

                // Table Headers
                table.AddCell(new PdfPCell(new Phrase("Condition Name", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Description", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Status", headerFont)));

                // Table Body
                foreach (var condition in conditions)
                {
                    table.AddCell(new Phrase(condition.Name ?? "", bodyFont));
                    table.AddCell(new Phrase(condition.Description ?? "", bodyFont));
                    table.AddCell(new Phrase(condition.ConditionStatus.ToString(), bodyFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "ConditionReport.pdf");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteConditions([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No conditions selected for deletion.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var conditions = await _context.Conditions
                .Where(c => ids.Contains(c.ConditionId) && c.ConditionStatus == Status.Active)
                .ToListAsync();

            if (!conditions.Any())
            {
                TempData["ErrorMessage"] = "No active conditions found for the selected IDs.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            foreach (var condition in conditions)
            {
                condition.ConditionStatus = Status.Delete;
            }

            _context.UpdateRange(conditions);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{conditions.Count} condition(s) deleted successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkRestoreConditions([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No conditions selected for restoration.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var conditions = await _context.Conditions
                .IgnoreQueryFilters()
                .Where(c => ids.Contains(c.ConditionId) && c.ConditionStatus == Status.Delete)
                .ToListAsync();

            if (!conditions.Any())
            {
                TempData["ErrorMessage"] = "No deleted conditions found for the selected IDs.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            foreach (var condition in conditions)
            {
                condition.ConditionStatus = Status.Active;
            }

            _context.UpdateRange(conditions);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{conditions.Count} condition(s) restored successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }































































































        // Consumable Management
        public async Task<IActionResult> ListOfConsumables()
        {
            var consumables = await _context.Consumables.ToListAsync();
            return View(consumables);
        }

        public async Task<IActionResult> DetailsOfConsumables(int id)
        {
            var consumable = await _context.Consumables.FindAsync(id);
            if (consumable == null || consumable.ConsumableStatus == Status.Delete)
            {
                TempData["ErrorMessage"] = "Consumable not found or has been deleted.";
                return NotFound();
            }

            return View(consumable);
        }

        public IActionResult AddConsumables()
        {
            ViewBag.ConsumableTypeList = GetConsumableTypeSelectList();
            return View();
        }

        // POST Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddConsumables(Consumable consumable)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.ConsumableTypeList = GetConsumableTypeSelectList();
                return View(consumable);
            }



            consumable.ConsumableStatus = Status.Active;
            _context.Add(consumable);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Consumable added successfully.";
            return RedirectToAction(nameof(ListOfConsumables));
        }

        // GET Edit
        public async Task<IActionResult> EditConsumables(int? id)
        {
            if (id == null) return NotFound();

            var consumable = await _context.Consumables.FindAsync(id);
            if (consumable == null || consumable.ConsumableStatus == Status.Delete)
                return NotFound();

            ViewBag.ConsumableTypeList = GetConsumableTypeSelectList();
            return View(consumable);
        }

        // POST Edit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditConsumables(int id, Consumable consumable)
        {
            if (id != consumable.ConsumableId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.ConsumableTypeList = GetConsumableTypeSelectList();
                return View(consumable);
            }

            try
            {
                _context.Update(consumable);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Consumable updated successfully.";
                return RedirectToAction(nameof(ListOfConsumables));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Consumables.Any(e => e.ConsumableId == id))
                    return NotFound();

                throw;
            }
        }
        public async Task<IActionResult> DeleteConsumables(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid ID.";
                return NotFound();
            }

            var consumable = await _context.Consumables.FirstOrDefaultAsync(c => c.ConsumableId == id);
            if (consumable == null)
            {
                TempData["ErrorMessage"] = "Consumable not found.";
                return NotFound();
            }

            return View(consumable);
        }

        [HttpPost, ActionName("DeleteConsumables")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConsumableConfirm(int id)
        {
            var consumable = await _context.Consumables.FindAsync(id);
            if (consumable == null)
            {
                TempData["ErrorMessage"] = "Consumable not found.";
                return NotFound();
            }

            consumable.ConsumableStatus = Status.Delete;
            consumable.UpdateLastUpdated();

            _context.Update(consumable);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Consumable deleted (soft) successfully.";
            return RedirectToAction(nameof(ListOfConsumables));
        }

        public async Task<IActionResult> DeletedConsumables()
        {
            var deletedConsumables = await _context.Consumables
                .IgnoreQueryFilters()
                .Where(c => c.ConsumableStatus == Status.Delete)
                .ToListAsync();

            return View(deletedConsumables);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreConsumables(int id)
        {
            var consumable = await _context.Consumables
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(c => c.ConsumableId == id && c.ConsumableStatus == Status.Delete);

            if (consumable == null)
            {
                TempData["ErrorMessage"] = "Consumable not found or already active.";
                return NotFound();
            }

            consumable.ConsumableStatus = Status.Active;
            consumable.UpdateLastUpdated();

            _context.Update(consumable);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Consumable restored successfully.";
            return RedirectToAction(nameof(ListOfConsumables));
        }




        public async Task<IActionResult> ConsumableReport()
        {
            var consumables = await _context.Consumables.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                // Title
                document.Add(new Paragraph("Consumable Report", titleFont));
                document.Add(new Paragraph($"Generated on: {DateTime.Now}", bodyFont));
                document.Add(new Paragraph("\n"));

                // Create table with 6 columns
                PdfPTable table = new PdfPTable(6)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 2.2f, 1.5f, 1f, 2f, 1.5f, 2.5f });

                // Header row
                table.AddCell(new PdfPCell(new Phrase("Name", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Type", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Quantity", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Expiry Date", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Status", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Description", headerFont)));

                // Data rows
                foreach (var item in consumables)
                {
                    table.AddCell(new Phrase(item.Name ?? "", bodyFont));
                    table.AddCell(new Phrase(item.Type.ToString(), bodyFont));
                    table.AddCell(new Phrase(item.Quantity.ToString(), bodyFont));
                    table.AddCell(new Phrase(item.ExpiryDate.ToString("dd/MM/yyyy"), bodyFont));
                    table.AddCell(new Phrase(item.ConsumableStatus.ToString(), bodyFont));
                    table.AddCell(new Phrase(item.Description ?? "", bodyFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "ConsumableReport.pdf");
            }
        }


        // BULK DELETE (soft delete) – from the active list
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteConsumables([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No consumables selected for deletion.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var toDelete = await _context.Consumables
                .Where(c => ids.Contains(c.ConsumableId) && c.ConsumableStatus == Status.Active)
                .ToListAsync();

            if (!toDelete.Any())
            {
                TempData["ErrorMessage"] = "No active consumables found for the selected IDs.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            foreach (var c in toDelete)
            {
                c.ConsumableStatus = Status.Delete;
                c.UpdateLastUpdated();
            }

            _context.UpdateRange(toDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{toDelete.Count} consumable(s) deleted successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }


        // BULK RESTORE – from the deleted list
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkRestoreConsumables([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No consumables selected for restoration.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var toRestore = await _context.Consumables
                .IgnoreQueryFilters()
                .Where(c => ids.Contains(c.ConsumableId) && c.ConsumableStatus == Status.Delete)
                .ToListAsync();

            if (!toRestore.Any())
            {
                TempData["ErrorMessage"] = "No deleted consumables found for the selected IDs.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            foreach (var c in toRestore)
            {
                c.ConsumableStatus = Status.Active;
                c.UpdateLastUpdated();
            }

            _context.UpdateRange(toRestore);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{toRestore.Count} consumable(s) restored successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }










































































        // Medication Management
        public async Task<IActionResult> ListOfMedication()
        {
            var meds = await _context.Medications.ToListAsync();
            return View(meds);
        }

        public async Task<IActionResult> DetailsOfMedication(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null || medication.MedicationStatus == Status.Delete)
            {
                TempData["ErrorMessage"] = "Medication not found or deleted.";
                return NotFound();
            }

            return View(medication);
        }
        // GET: Add Medication
        public IActionResult AddMedication()
        {
            ViewBag.MedicationTypeList = GetMedicationTypeSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddMedication(Medication medication)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.MedicationTypeList = GetMedicationTypeSelectList();
                return View(medication);
            }

            medication.MedicationStatus = Status.Active;

            _context.Medications.Add(medication);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medication added successfully.";
            return RedirectToAction(nameof(ListOfMedication));
        }

        // GET: Edit Medication
        public async Task<IActionResult> EditMedication(int? id)
        {
            if (id == null) return NotFound();

            var medication = await _context.Medications.FindAsync(id);
            if (medication == null) return NotFound();

            ViewBag.MedicationTypeList = GetMedicationTypeSelectList();
            return View(medication);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditMedication(int id, Medication medication)
        {
            if (id != medication.MedicationId) return NotFound();

            if (!ModelState.IsValid)
            {
                ViewBag.MedicationTypeList = GetMedicationTypeSelectList();
                return View(medication);
            }

            try
            {
                _context.Medications.Update(medication);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Medication updated successfully.";
                return RedirectToAction(nameof(ListOfMedication));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Medications.Any(e => e.MedicationId == id))
                {
                    return NotFound();
                }
                throw;
            }
        }
        public async Task<IActionResult> DeleteMedication(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid ID.";
                return NotFound();
            }

            var medication = await _context.Medications.FirstOrDefaultAsync(m => m.MedicationId == id);
            if (medication == null)
            {
                TempData["ErrorMessage"] = "Medication not found.";
                return NotFound();
            }

            return View(medication);
        }

        [HttpPost, ActionName("DeleteMedication")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteMedicationConfirmed(int id)
        {
            var medication = await _context.Medications.FindAsync(id);
            if (medication == null)
            {
                TempData["ErrorMessage"] = "Medication not found.";
                return NotFound();
            }

            medication.MedicationStatus = Status.Delete;
            _context.Update(medication);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medication deleted successfully.";
            return RedirectToAction(nameof(ListOfMedication));
        }

        public async Task<IActionResult> DeletedMedications()
        {
            var deletedMeds = await _context.Medications
                .IgnoreQueryFilters()
                .Where(m => m.MedicationStatus == Status.Delete)
                .ToListAsync();

            return View(deletedMeds);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreMedications(int id)
        {
            var med = await _context.Medications
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(m => m.MedicationId == id && m.MedicationStatus == Status.Delete);

            if (med == null)
            {
                TempData["ErrorMessage"] = "Medication not found or already active.";
                return NotFound();
            }

            med.MedicationStatus = Status.Active;
            _context.Update(med);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Medication restored successfully.";
            return RedirectToAction(nameof(ListOfMedication));
        }



        public async Task<IActionResult> MedicationReport()
        {
            var medications = await _context.Medications.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);

                // Title
                document.Add(new Paragraph("Medication Report", titleFont));
                document.Add(new Paragraph($"Generated on: {DateTime.Now}", bodyFont));
                document.Add(new Paragraph("\n"));

                // Create a table with 6 columns
                PdfPTable table = new PdfPTable(6)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 2f, 1.5f, 1f, 2f, 2.5f, 1.5f });

                // Add headers
                table.AddCell(new PdfPCell(new Phrase("Name", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Type", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Quantity", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Expiry Date", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Description", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Status", headerFont)));

                // Add data
                foreach (var med in medications)
                {
                    table.AddCell(new Phrase(med.Name ?? "", bodyFont));
                    table.AddCell(new Phrase(med.Type.ToString(), bodyFont));
                    table.AddCell(new Phrase(med.Quantity.ToString(), bodyFont));
                    table.AddCell(new Phrase(med.ExpiryDate.ToString("dd/MM/yyyy"), bodyFont));
                    table.AddCell(new Phrase(med.Description ?? "", bodyFont));
                    table.AddCell(new Phrase(med.MedicationStatus.ToString(), bodyFont));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "MedicationReport.pdf");
            }
        }




        // BULK DELETE (soft delete) – from the active list
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteMedications([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No medications selected for deletion.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var meds = await _context.Medications
                .Where(m => ids.Contains(m.MedicationId) && m.MedicationStatus == Status.Active)
                .ToListAsync();

            if (!meds.Any())
            {
                TempData["ErrorMessage"] = "No active medications found for the selected IDs.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            foreach (var m in meds)
                m.MedicationStatus = Status.Delete;

            _context.UpdateRange(meds);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{meds.Count} medication(s) deleted successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }

        // BULK RESTORE – from the deleted list
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkRestoreMedications([FromBody] List<int> ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No medications selected for restoration.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            var meds = await _context.Medications
                .IgnoreQueryFilters()
                .Where(m => ids.Contains(m.MedicationId) && m.MedicationStatus == Status.Delete)
                .ToListAsync();

            if (!meds.Any())
            {
                TempData["ErrorMessage"] = "No deleted medications found for the selected IDs.";
                return Json(new { success = false, message = TempData["ErrorMessage"] });
            }

            foreach (var m in meds)
                m.MedicationStatus = Status.Active;

            _context.UpdateRange(meds);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{meds.Count} medication(s) restored successfully.";
            return Json(new { success = true, message = TempData["SuccessMessage"] });
        }






















































































        // Bed Management

        public async Task<IActionResult> ListOfBeds()
        {
            var beds = await _context.Beds
                .Include(b => b.Ward)
                .ToListAsync();

            return View(beds);
        }

        public async Task<IActionResult> DetailsOfBeds(int id)
        {
            var bed = await _context.Beds
                .Include(b => b.Ward)
                .FirstOrDefaultAsync(b => b.BedId == id);

            if (bed == null || bed.BedStatus == Status.Delete)
            {
                TempData["ErrorMessage"] = "Bed not found or deleted.";
                return NotFound();
            }

            return View(bed);
        }

        public IActionResult AddBed()
        {
            PopulateWardList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddBed(Bed bed)
        {
            PopulateWardList();

            var ward = await _context.Wards
                .Include(w => w.Beds)
                .FirstOrDefaultAsync(w => w.WardId == bed.WardId && w.WardStatus == Status.Active);

            if (ward == null)
            {
                TempData["ErrorMessage"] = "Selected ward does not exist.";
                return RedirectToAction(nameof(ListOfBeds));
            }

            // Count active beds in this ward
            int currentBedCount = ward.Beds.Count(b => b.BedStatus == Status.Active);

            if (currentBedCount >= ward.Capacity)
            {
                TempData["ErrorMessage"] = $"Cannot add more beds. The ward '{ward.WardName}' has reached its maximum capacity of {ward.Capacity} beds.";
                return RedirectToAction(nameof(ListOfBeds));
            }

            bed.Status = BedStatus.Available;
            bed.BedStatus = Status.Active;

            _context.Beds.Add(bed);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bed added successfully.";
            return RedirectToAction(nameof(ListOfBeds));
        }


        public async Task<IActionResult> EditBeds(int? id)
        {
            if (id == null) return RedirectToAction(nameof(ListOfBeds));

            var bed = await _context.Beds.FindAsync(id);
            if (bed == null) return RedirectToAction(nameof(ListOfBeds));

            PopulateWardList();
            return View(bed);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBeds(int id, Bed bed)
        {
            if (id != bed.BedId)
            {
                TempData["ErrorMessage"] = "Bed ID mismatch.";
                PopulateWardList();
                return View(bed);
            }

            PopulateWardList();



            try
            {
                bed.BedStatus = Status.Active;
                _context.Update(bed);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bed updated successfully.";
                return RedirectToAction(nameof(ListOfBeds));
            }
            catch
            {
                TempData["ErrorMessage"] = "Concurrency error occurred.";
                throw;
            }
        }
        public async Task<IActionResult> DeleteBed(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid ID.";
                return NotFound();
            }

            var bed = await _context.Beds.FirstOrDefaultAsync(b => b.BedId == id);
            if (bed == null)
            {
                TempData["ErrorMessage"] = "Bed not found.";
                return NotFound();
            }

            return View(bed);
        }

        [HttpPost, ActionName("DeleteBed")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBedConfirmed(int id)
        {
            var bed = await _context.Beds.FindAsync(id);
            if (bed == null)
            {
                TempData["ErrorMessage"] = "Bed not found.";
                return NotFound();
            }

            bed.BedStatus = Status.Delete;
            _context.Update(bed);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bed deleted successfully.";
            return RedirectToAction(nameof(ListOfBeds));
        }

        public async Task<IActionResult> DeletedBeds()
        {
            var deletedBeds = await _context.Beds
                .IgnoreQueryFilters()
                .Include(b => b.Ward)
                .Where(b => b.BedStatus == Status.Delete)
                .ToListAsync();

            return View(deletedBeds);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreBeds(int id)
        {
            var bed = await _context.Beds
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(b => b.BedId == id && b.BedStatus == Status.Delete);

            if (bed == null)
            {
                TempData["ErrorMessage"] = "Bed not found or already active.";
                return NotFound();
            }

            bed.BedStatus = Status.Active;
            _context.Update(bed);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bed restored successfully.";
            return RedirectToAction(nameof(ListOfBeds));
        }




        public async Task<IActionResult> BedReport()
        {
            var beds = await _context.Beds
                .Include(b => b.Ward)
                .ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                var warningFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.Red);

                // Title
                document.Add(new Paragraph("Bed Report", titleFont));
                document.Add(new Paragraph($"Generated on: {DateTime.Now}", bodyFont));
                document.Add(new Paragraph("\n"));

                // Table: Bed Details
                PdfPTable table = new PdfPTable(4)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 1.5f, 2f, 2f, 1.5f });

                // Headers
                table.AddCell(new PdfPCell(new Phrase("Bed Number", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Ward Name", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Bed Status", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Record Status", headerFont)));

                foreach (var bed in beds)
                {
                    table.AddCell(new Phrase(bed.BedNumber, bodyFont));
                    table.AddCell(new Phrase(bed.Ward?.WardName ?? "N/A", bodyFont));
                    table.AddCell(new Phrase(bed.Status.ToString(), bodyFont));
                    table.AddCell(new Phrase(bed.BedStatus.ToString(), bodyFont));
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));

                // Totals: Available & Occupied Beds
                int availableCount = beds.Count(b => b.Status == BedStatus.Available);
                int occupiedCount = beds.Count(b => b.Status == BedStatus.Occupied);

                document.Add(new Paragraph($"🛏️ Total Available Beds: {availableCount}", bodyFont));
                document.Add(new Paragraph($"🛌 Total Occupied Beds: {occupiedCount}", bodyFont));
                document.Add(new Paragraph("\n"));

                // Totals per ward with capacity check
                var wardGroups = beds
                    .Where(b => b.Ward != null)
                    .GroupBy(b => b.Ward)
                    .Select(g => new
                    {
                        WardName = g.Key.WardName,
                        Capacity = g.Key.Capacity,
                        TotalBeds = g.Count()
                    }).ToList();

                document.Add(new Paragraph("Total Beds Per Ward", headerFont));
                document.Add(new Paragraph("\n"));

                PdfPTable summaryTable = new PdfPTable(3)
                {
                    WidthPercentage = 80
                };
                summaryTable.SetWidths(new float[] { 3f, 1f, 1f });

                summaryTable.AddCell(new PdfPCell(new Phrase("Ward Name", headerFont)));
                summaryTable.AddCell(new PdfPCell(new Phrase("Capacity", headerFont)));
                summaryTable.AddCell(new PdfPCell(new Phrase("Total Beds", headerFont)));

                foreach (var ward in wardGroups)
                {
                    var wardFont = ward.TotalBeds > ward.Capacity ? warningFont : bodyFont;

                    summaryTable.AddCell(new Phrase(ward.WardName, wardFont));
                    summaryTable.AddCell(new Phrase(ward.Capacity.ToString(), wardFont));
                    summaryTable.AddCell(new Phrase(ward.TotalBeds.ToString(), wardFont));
                }

                document.Add(summaryTable);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "BedReport.pdf");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkRestoreBeds(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No bed IDs provided for restoration.";
                return RedirectToAction(nameof(ListOfBeds));
            }

            var bedsToRestore = await _context.Beds
                .IgnoreQueryFilters()
                .Where(b => ids.Contains(b.BedId) && b.BedStatus == Status.Delete)
                .ToListAsync();

            if (!bedsToRestore.Any())
            {
                TempData["ErrorMessage"] = "No deleted beds found for the selected IDs.";
                return RedirectToAction(nameof(DeletedBeds));
            }

            foreach (var bed in bedsToRestore)
                bed.BedStatus = Status.Active;

            _context.UpdateRange(bedsToRestore);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{bedsToRestore.Count} bed(s) restored successfully.";
            return RedirectToAction(nameof(ListOfBeds));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteBeds(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No bed IDs provided for deletion.";
                return RedirectToAction(nameof(ListOfBeds));
            }

            var bedsToDelete = await _context.Beds
                .Where(b => ids.Contains(b.BedId) && b.BedStatus == Status.Active)
                .ToListAsync();

            if (!bedsToDelete.Any())
            {
                TempData["ErrorMessage"] = "No active beds found for the selected IDs.";
                return RedirectToAction(nameof(ListOfBeds));
            }

            foreach (var bed in bedsToDelete)
                bed.BedStatus = Status.Delete;

            _context.UpdateRange(bedsToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{bedsToDelete.Count} bed(s) deleted successfully.";
            return RedirectToAction(nameof(ListOfBeds));
        }


















































































        // Hospital Management

        public async Task<IActionResult> ListOfHospitalInfos()
        {
            var hospitals = await _context.HospitalInfos.ToListAsync();
            return View(hospitals);
        }

        public async Task<IActionResult> DetailsOfHospitalInfos(int id)
        {
            var hospital = await _context.HospitalInfos
                 .FirstOrDefaultAsync(h => h.HospitalInfoId == id && h.HospitalInfoStatus == Status.Active);

            if (hospital == null)
            {
                TempData["ErrorMessage"] = "Hospital not found or has been deleted.";
                return NotFound();
            }

            return View(hospital);
        }

        public IActionResult AddHospitalInfos()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddHospitalInfos(HospitalInfo hospital)
        {
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please correct the form errors.";
                return View(hospital);
            }

            hospital.HospitalInfoStatus = Status.Active;
            _context.HospitalInfos.Add(hospital);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hospital added successfully.";
            return RedirectToAction(nameof(ListOfHospitalInfos));
        }

        public async Task<IActionResult> EditHospitalInfos(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid hospital ID.";
                return NotFound();
            }

            var hospital = await _context.HospitalInfos.FindAsync(id);
            if (hospital == null)
            {
                TempData["ErrorMessage"] = "Hospital not found.";
                return NotFound();
            }

            return View(hospital);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditHospitalInfos(int id, HospitalInfo updated)
        {
            if (id != updated.HospitalInfoId)
            {
                TempData["ErrorMessage"] = "Hospital ID mismatch.";
                return NotFound();
            }

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please fix validation errors.";
                return View(updated);
            }

            var hospital = await _context.HospitalInfos.FindAsync(id);
            if (hospital == null || hospital.HospitalInfoStatus == Status.Delete)
            {
                TempData["ErrorMessage"] = "Hospital not found or already deleted.";
                return NotFound();
            }

            hospital.HospitalName = updated.HospitalName;
            hospital.Address = updated.Address;
            hospital.PhoneNumber = updated.PhoneNumber;
            hospital.Email = updated.Email;
            hospital.WebsiteUrl = updated.WebsiteUrl;

            _context.Update(hospital);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hospital updated successfully.";
            return RedirectToAction(nameof(ListOfHospitalInfos));
        }

        public async Task<IActionResult> DeleteHospitalInfos(int? id)
        {
            if (id == null)
            {
                TempData["ErrorMessage"] = "Invalid hospital ID.";
                return NotFound();
            }

            var hospital = await _context.HospitalInfos.FirstOrDefaultAsync(a => a.HospitalInfoId == id);
            if (hospital == null)
            {
                TempData["ErrorMessage"] = "Hospital not found.";
                return NotFound();
            }

            return View(hospital);
        }

        [HttpPost, ActionName("DeleteHospitalInfos")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteHospitalInfosConfirmed(int id)
        {
            var hospital = await _context.HospitalInfos.FindAsync(id);
            if (hospital == null || hospital.HospitalInfoStatus == Status.Delete)
            {
                TempData["ErrorMessage"] = "Hospital not found or already deleted.";
                return NotFound();
            }

            hospital.HospitalInfoStatus = Status.Delete;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hospital deleted successfully.";
            return RedirectToAction(nameof(ListOfHospitalInfos));
        }

        public async Task<IActionResult> DeletedHospitalInfos()
        {
            var deletedHospitals = await _context.HospitalInfos
                .IgnoreQueryFilters()
                .Where(h => h.HospitalInfoStatus == Status.Delete)
                .ToListAsync();

            return View(deletedHospitals);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreHospitalInfos(int id)
        {
            var hospital = await _context.HospitalInfos
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(h => h.HospitalInfoId == id && h.HospitalInfoStatus == Status.Delete);

            if (hospital == null)
            {
                TempData["ErrorMessage"] = "Hospital not found or already active.";
                return NotFound();
            }

            hospital.HospitalInfoStatus = Status.Active;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Hospital restored successfully.";
            return RedirectToAction(nameof(ListOfHospitalInfos));
        }
























































































        // Employee Management



        // GET: List all active employees
        public async Task<IActionResult> ListOfEmployees()
        {
            var employees = await _context.Employees.ToListAsync();
            return View(employees);
        }

        // GET: Details of a single employee
        public async Task<IActionResult> DetailsOfEmployee(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null || employee.IsActive == Status.Delete)
                return NotFound();

            return View(employee);
        }

        // GET: Create new employee
        public IActionResult AddEmployee()
        {
            ViewBag.GenderList = GetGenderSelectList();
            ViewBag.RoleList = GetRoleSelectList();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddEmployee(Employee employee)
        {


            employee.PasswordHash = null;

            // Generate email verification token
            string token = Guid.NewGuid().ToString("N");
            employee.EmailVerificationTokenHash = token;
            employee.EmailVerificationTokenExpires = DateTime.UtcNow.AddHours(24);

            // Set initial status as inactive until email is verified
            employee.IsActive = Status.Inactive;

            _context.Add(employee);
            await _context.SaveChangesAsync();

            // ✅ Send verification email to the employee
            try
            {
                var verificationLink = Url.Action(
                    "VerifyEmail",
                    "Account",
                    new { userId = employee.EmployeeID, token = token },
                    protocol: HttpContext.Request.Scheme
                );

                var placeholders = new Dictionary<string, string>
            {
                { "EmployeeName", $"{employee.FirstName} {employee.LastName}" },
                { "VerificationLink", verificationLink },
                { "AdminName", User.Identity.Name }
            };

                await _emailService.SendEmailWithTemplateAsync(
                    employee.Email,
                    "Welcome to Our Team - Verify Your Email",
                    "EmployeeWelcomeTemplate.html",
                    placeholders
                );

                TempData["SuccessMessage"] = "Employee added successfully. Verification email sent!";
            }
            catch (Exception ex)
            {
                // If email fails, delete the employee record to avoid security issues
                _context.Employees.Remove(employee);
                await _context.SaveChangesAsync();

                TempData["ErrorMessage"] = $"Failed to send verification email: {ex.Message}";
                ViewBag.GenderList = GetGenderSelectList();
                ViewBag.RoleList = GetRoleSelectList();
                return View(employee);
            }

            return RedirectToAction(nameof(ListOfEmployees));


            TempData["ErrorMessage"] = "Failed to add employee. Check form.";
            ViewBag.GenderList = GetGenderSelectList();
            ViewBag.RoleList = GetRoleSelectList();
            return View(employee);
        }

        public async Task<IActionResult> EditEmployee(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null || employee.IsActive == Status.Delete)
                return NotFound();

            ViewBag.GenderList = GetGenderSelectList();
            ViewBag.RoleList = GetRoleSelectList();
            return View(employee);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditEmployee(int id, Employee updated)
        {
            if (id != updated.EmployeeID) return NotFound();


            var existing = await _context.Employees.FindAsync(id);
            if (existing == null || existing.IsActive == Status.Delete)
                return NotFound();

            try
            {
                existing.FirstName = updated.FirstName;
                existing.LastName = updated.LastName;
                existing.UserName = updated.UserName;
                existing.Email = updated.Email;
                existing.Gender = updated.Gender;
                existing.Role = updated.Role;
                existing.HireDate = updated.HireDate;


                _context.Update(existing);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee updated successfully.";
                return RedirectToAction(nameof(ListOfEmployees));
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Employees.Any(e => e.EmployeeID == id))
                    return NotFound();
                throw;
            }


            TempData["ErrorMessage"] = "Failed to update employee.";
            ViewBag.GenderList = GetGenderSelectList();
            ViewBag.RoleList = GetRoleSelectList();
            return View(updated);
        }

        // GET: Confirm deletion
        public async Task<IActionResult> DeleteEmployee(int? id)
        {
            if (id == null) return NotFound();

            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            return View(employee);
        }

        // POST: Soft delete employee
        [HttpPost, ActionName("DeleteEmployee")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteEmployeeConfirmed(int id)
        {
            var employee = await _context.Employees.FindAsync(id);
            if (employee == null) return NotFound();

            employee.IsActive = Status.Delete;
            _context.Update(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee deleted (soft delete).";
            return RedirectToAction(nameof(ListOfEmployees));
        }

        // GET: View soft-deleted employees
        public async Task<IActionResult> DeletedEmployees()
        {
            var deleted = await _context.Employees
                .IgnoreQueryFilters()
                .Where(e => e.IsActive == Status.Delete)
                .ToListAsync();

            return View(deleted);
        }

        // POST: Restore soft-deleted employee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreEmployee(int id)
        {
            var employee = await _context.Employees
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(e => e.EmployeeID == id && e.IsActive == Status.Delete);

            if (employee == null) return NotFound();

            employee.IsActive = Status.Active;
            _context.Update(employee);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Employee restored successfully.";
            return RedirectToAction(nameof(ListOfEmployees));
        }


        public async Task<IActionResult> EmployeeReport()
        {
            var employees = await _context.Employees.ToListAsync();

            using (var stream = new MemoryStream())
            {
                Document document = new Document(PageSize.A4, 25, 25, 30, 30);
                PdfWriter.GetInstance(document, stream);
                document.Open();

                // Fonts
                var titleFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 18);
                var headerFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 12);
                var bodyFont = FontFactory.GetFont(FontFactory.HELVETICA, 11);
                var redFont = FontFactory.GetFont(FontFactory.HELVETICA_BOLD, 11, BaseColor.Red);

                // Title
                document.Add(new Paragraph("Employee Report", titleFont));
                document.Add(new Paragraph($"Generated on: {DateTime.Now:yyyy-MM-dd HH:mm}", bodyFont));
                document.Add(new Paragraph("\n"));

                // Table with columns: ID, Full Name, Username, Email, Gender, Role, Hire Date, Status
                PdfPTable table = new PdfPTable(8)
                {
                    WidthPercentage = 100
                };
                table.SetWidths(new float[] { 1f, 2f, 2f, 3f, 1.5f, 1.5f, 2f, 1.5f });

                // Headers
                table.AddCell(new PdfPCell(new Phrase("ID", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Full Name", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Username", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Email", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Gender", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Role", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Hire Date", headerFont)));
                table.AddCell(new PdfPCell(new Phrase("Status", headerFont)));

                foreach (var emp in employees)
                {
                    var isActive = emp.IsActive == AppStatus.Status.Active;
                    var fontToUse = isActive ? bodyFont : redFont; // Red font for inactive employees

                    table.AddCell(new PdfPCell(new Phrase(emp.EmployeeID.ToString(), fontToUse)));
                    table.AddCell(new PdfPCell(new Phrase($"{emp.FirstName} {emp.LastName}", fontToUse)));
                    table.AddCell(new PdfPCell(new Phrase(emp.UserName, fontToUse)));
                    table.AddCell(new PdfPCell(new Phrase(emp.Email, fontToUse)));
                    table.AddCell(new PdfPCell(new Phrase(emp.Gender.ToString(), fontToUse)));
                    table.AddCell(new PdfPCell(new Phrase(emp.Role.ToString(), fontToUse)));
                    table.AddCell(new PdfPCell(new Phrase(emp.HireDate?.ToString("yyyy-MM-dd") ?? "-", fontToUse)));
                    table.AddCell(new PdfPCell(new Phrase(emp.IsActive.ToString(), fontToUse)));
                }

                document.Add(table);
                document.Close();

                return File(stream.ToArray(), "application/pdf", "EmployeeReport.pdf");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkRestoreEmployees(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No employee IDs provided for restoration.";
                return RedirectToAction(nameof(DeletedEmployees));
            }

            var employeesToRestore = await _context.Employees
                .IgnoreQueryFilters()
                .Where(e => ids.Contains(e.EmployeeID) && e.IsActive == Status.Delete)
                .ToListAsync();

            if (!employeesToRestore.Any())
            {
                TempData["ErrorMessage"] = "No deleted employees found for the selected IDs.";
                return RedirectToAction(nameof(DeletedEmployees));
            }

            foreach (var emp in employeesToRestore)
                emp.IsActive = Status.Active;

            _context.UpdateRange(employeesToRestore);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{employeesToRestore.Count} employee(s) restored successfully.";
            return RedirectToAction(nameof(ListOfEmployees));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BulkDeleteEmployees(int[] ids)
        {
            if (ids == null || !ids.Any())
            {
                TempData["ErrorMessage"] = "No employee IDs provided for deletion.";
                return RedirectToAction(nameof(ListOfEmployees));
            }

            var employeesToDelete = await _context.Employees
                .Where(e => ids.Contains(e.EmployeeID) && e.IsActive == Status.Active)
                .ToListAsync();

            if (!employeesToDelete.Any())
            {
                TempData["ErrorMessage"] = "No active employees found for the selected IDs.";
                return RedirectToAction(nameof(ListOfEmployees));
            }

            foreach (var emp in employeesToDelete)
                emp.IsActive = Status.Delete;

            _context.UpdateRange(employeesToDelete);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"{employeesToDelete.Count} employee(s) deleted successfully.";
            return RedirectToAction(nameof(ListOfEmployees));
        }





















































































































































































































































































































        private void PopulateWardList()
        {
            ViewBag.WardList = new SelectList(
                _context.Wards.Where(w => w.WardStatus == Status.Active).OrderBy(w => w.WardName),
                "WardId",
                "WardName");


            var eligibleWards = _context.Wards
                .Include(w => w.Beds)
                .Where(w => w.WardStatus == Status.Active)
                .ToList()
                .Where(w => w.Beds.Count(b => b.BedStatus == Status.Active) < w.Capacity)
                .OrderBy(w => w.WardName) // Added OrderBy
                .ToList();

            ViewBag.WardList = eligibleWards.Select(w => new SelectListItem
            {
                Value = w.WardId.ToString(),
                Text = $"{w.WardName} (Capacity: {w.Capacity - w.Beds.Count(b => b.BedStatus == Status.Active)} left)"
            }).OrderBy(s => s.Text).ToList(); // Added OrderBy
        }

        private SelectList GetGenderSelectList()
        {
            return new SelectList(Enum.GetValues(typeof(GenderType))
                .Cast<GenderType>()
                .OrderBy(g => g.ToString()) // Added OrderBy
                .Select(g => new { ID = g, Name = g.ToString() }), "ID", "Name");
        }

        private SelectList GetRoleSelectList()
        {
            return new SelectList(Enum.GetValues(typeof(UserRole))
                .Cast<UserRole>()
                .OrderBy(r => r.ToString()) // Added OrderBy
                .Select(r => new { ID = r, Name = r.ToString() }), "ID", "Name");
        }

        private List<SelectListItem> GetMedicationTypeSelectList()
        {
            var values = Enum.GetValues(typeof(MedicationType)).Cast<MedicationType>();

            var list = values.Select(v =>
            {
                var field = v.GetType().GetField(v.ToString());
                var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
                var text = displayAttr != null ? displayAttr.Name : v.ToString();

                return new SelectListItem
                {
                    Value = ((int)v).ToString(),
                    Text = text
                };
            }).OrderBy(s => s.Text).ToList(); // Added OrderBy

            return list;
        }

        private List<SelectListItem> GetConsumableTypeSelectList()
        {
            var values = Enum.GetValues(typeof(ConsumableType)).Cast<ConsumableType>();

            var list = values.Select(v =>
            {
                var field = v.GetType().GetField(v.ToString());
                var displayAttr = field.GetCustomAttribute<DisplayAttribute>();
                var text = displayAttr != null ? displayAttr.Name : v.ToString();

                return new SelectListItem
                {
                    Value = ((int)v).ToString(),
                    Text = text
                };
            }).OrderBy(s => s.Text).ToList(); // Added OrderBy

            return list;
        }





    }
}
