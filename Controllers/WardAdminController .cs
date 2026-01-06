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
    [Authorize(Roles = "WARDADMIN,DOCTOR")]
    public class WardAdminController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        public WardAdminController(ApplicationDbContext context, NotificationService notificationService)
        {
            _notificationService = notificationService;
            _context = context;
        }


        public async Task<IActionResult> WardAdminDashboard()
        {
            var currentUserName = User.Identity.Name;

            // Get employeeId from User.Claims (same pattern as AdminController)
            int? employeeId = null;
            var employeeIdClaim = User.Claims.FirstOrDefault(c => c.Type == "EmployeeID" || c.Type == "employeeId" || c.Type == "sub" || c.Type.EndsWith("nameidentifier"));
            if (employeeIdClaim != null && int.TryParse(employeeIdClaim.Value, out int parsedId))
            {
                employeeId = parsedId;
            }

            // Get unread notification count (same pattern as AdminController)
            ViewData["UnreadNotificationCount"] = employeeId.HasValue
                ? await _notificationService.GetUnreadCountAsync(employeeId.Value)
                : 0;



            return View();
        }




        public async Task<IActionResult> Patients()
        {
            var patients = await _context.Patients
                .IgnoreQueryFilters() // Bypass any global filters like soft deletes
                .Where(p => p.GetStatus != Status.Delete && p.GetStatus != Status.Discharged) // Exclude deleted and admitted
                .ToListAsync();

            return View(patients);
        }


        public IActionResult CreatePatient()
        {
            ViewBag.GenderList = GetGenderSelectList();
            return View();
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreatePatient(Patient model)
        {
            try
            {
                model.GetStatus = Status.Active;
                _context.Patients.Add(model);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Patient created successfully!";
                return RedirectToAction("Patients");

            }
            catch (DbUpdateException)
            {
                TempData["ErrorMessage"] = "Unable to save changes. " +
                    "Try again, and if the problem persists, " +
                    "see your system administrator.";
            }

            return View(model); // show form again if save fails

        }

        public async Task<IActionResult> EditPatient(int id)
        {
            //var patient = await _context.Patients.FindAsync(id);
            var patient = await _context.Patients
                 .IgnoreQueryFilters()
                 .FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null || patient.GetStatus == Status.Delete)
                return NotFound();

            ViewBag.GenderList = GetGenderSelectList(); // 🔥 Make sure this is added
            return View(patient);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatient(Patient model)
        {


            //var patient = await _context.Patients.FindAsync(model.PatientId);
            var patient = await _context.Patients
                    .IgnoreQueryFilters()
                    .FirstOrDefaultAsync(p => p.PatientId /*== id*/ == model.PatientId);
            if (patient == null)
                return NotFound();

            // Update properties
            patient.FirstName = model.FirstName;
            patient.LastName = model.LastName;
            patient.DOB = model.DOB;
            patient.Gender = model.Gender;
            patient.IdNumber = model.IdNumber;
            patient.Cellphone = model.Cellphone;

            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient updated successfully!";
            return RedirectToAction("Patients");
        }



        public async Task<IActionResult> DeletePatient(int id)
        {

            var patient = await _context.Patients
                 .IgnoreQueryFilters()
                 .FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null || patient.GetStatus == Status.Delete)
                return NotFound();

            return View(patient);
        }

        [HttpPost, ActionName("DeletePatient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePatientConfirmed(int id)
        {
            //var patient = await _context.Patients.FindAsync(id);
            var patient = await _context.Patients
                 .IgnoreQueryFilters()
                 .FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null)
                return NotFound();

            patient.GetStatus = Status.Delete;
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient deleted successfully!";
            return RedirectToAction("Patients");
        }





        public async Task<IActionResult> DeletedPatients()
        {
            var deletedPatients = await _context.Patients
                .IgnoreQueryFilters()
                .Where(a => a.GetStatus == Status.Delete)
                .ToListAsync();



            return View(deletedPatients);
        }






        public async Task<IActionResult> RestorePatient(int id)

        {
            var patient = await _context.Patients
               .IgnoreQueryFilters()
               .FirstOrDefaultAsync(a => a.PatientId == id);

            patient.GetStatus = Status.Active;
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient restored successfully!";
            return RedirectToAction("Patients");
        }





        // GET: Patient Details
        public async Task<IActionResult> PatientDetails(int id)
        {
            var patient = await _context.Patients
                .IgnoreQueryFilters() // Bypass any global query filter like GetStatus == Active
                .FirstOrDefaultAsync(p => p.PatientId == id);
            if (patient == null || patient.GetStatus == Status.Delete)
                return NotFound();
            return View(patient);
        }
























        public async Task<IActionResult> Admit()
        {
            var model = new AdmitPatientViewModel
            {
                AllergyOptions = await _context.Allergies.ToListAsync(),
                MedicationOptions = await _context.Medications.ToListAsync(),
                ConditionOptions = await _context.Conditions.ToListAsync(),

                PatientOptions = await _context.Patients
                    .Where(p => p.GetStatus == Status.Active)
                    .Select(p => new Patient
                    {
                        PatientId = p.PatientId,
                        FirstName = p.FirstName + " " + p.LastName
                    })
                    .OrderBy(p => p.FirstName)
                    .ToListAsync(),

                DoctorOptions = await _context.Employees
                    .Where(e => e.Role == UserRole.DOCTOR && e.IsActive == Status.Active)
                    .Select(e => new Employee
                    {
                        EmployeeID = e.EmployeeID,
                        FirstName = e.FirstName + " " + e.LastName
                    })
                    .OrderBy(e => e.FirstName)
                    .ToListAsync(),

                NurseOptions = await _context.Employees
                    .Where(e => e.Role == UserRole.NURSE && e.IsActive == Status.Active)
                    .Select(e => new Employee
                    {
                        EmployeeID = e.EmployeeID,
                        FirstName = e.FirstName + " " + e.LastName
                    })
                    .OrderBy(e => e.FirstName)
                    .ToListAsync()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Admit(AdmitPatientViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm.AllergyOptions = await _context.Allergies.ToListAsync();
                vm.MedicationOptions = await _context.Medications.ToListAsync();
                vm.ConditionOptions = await _context.Conditions.ToListAsync();
                vm.PatientOptions = await _context.Patients.Where(p => p.GetStatus == Status.Active).ToListAsync();
                vm.DoctorOptions = await _context.Employees.Where(e => e.Role == UserRole.DOCTOR && e.IsActive == Status.Active).ToListAsync();
                vm.NurseOptions = await _context.Employees.Where(e => e.Role == UserRole.NURSE && e.IsActive == Status.Active).ToListAsync();

                return View(vm);
            }

            var patient = await _context.Patients
                .Include(p => p.PatientAllergies)
                .Include(p => p.PatientMedications)
                .Include(p => p.PatientConditions)
                .FirstOrDefaultAsync(p => p.PatientId == vm.SelectedPatientId);

            if (patient == null)
                return NotFound("Patient not found.");

            // Re-assign allergies, meds, conditions
            patient.PatientAllergies = vm.SelectedAllergyIds.Select(id => new PatientAllergy { AllergyId = id, Status = Status.Active }).ToList();
            patient.PatientMedications = vm.SelectedMedicationIds.Select(id => new PatientMedication { MedicationId = id, Status = Status.Active }).ToList();
            patient.PatientConditions = vm.SelectedConditionIds.Select(id => new PatientCondition { ConditionId = id, Status = Status.Active }).ToList();

            patient.GetStatus = Status.Admitted;

            var admission = new Admission
            {
                PatientId = patient.PatientId,
                AdmissionDate = DateTime.Now,
                Notes = "Admitted via system",
                AdmissionStatus = Status.Active,
                EmployeeID = vm.SelectedDoctorId,
                NurseID = vm.SelectedNurseId
            };
            // ✅ Send notifications to assigned doctor and nurse
            _context.Admissions.Add(admission);
            await _context.SaveChangesAsync(); // <-- IMPORTANT: get admission.AdmissionId assigned

            // Now create notifications (admission.AdmissionId will be valid)
            var currentUser = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == User.Identity.Name);
            if (currentUser != null)
            {
                await _notificationService.CreatePatientAssignmentNotification(
                    patient.PatientId,
                    admission.AdmissionId,
                    vm.SelectedDoctorId,
                    vm.SelectedNurseId,
                    currentUser.EmployeeID
                );
            }

            TempData["SuccessMessage"] = "Patient admitted successfully with assigned doctor and nurse!";
            return RedirectToAction("AdmittedPatients");
        }




        public async Task<IActionResult> AdmittedPatients()
        {
            var patients = await _context.Patients
                .IgnoreQueryFilters()
                .Include(p => p.Admissions)
                    .ThenInclude(a => a.Doctor)
                .Include(p => p.Admissions)
                    .ThenInclude(a => a.Nurse) // ✅ include Nurse
                .Include(p => p.PatientAllergies).ThenInclude(pa => pa.Allergy)
                .Include(p => p.PatientMedications).ThenInclude(pm => pm.Medication)
                .Include(p => p.PatientConditions).ThenInclude(pc => pc.Condition)
                .Where(p => p.Admissions.Any(a => a.AdmissionStatus == Status.Active))
                .ToListAsync();

            var admittedList = patients.Select(p =>
            {
                var latestAdmission = p.Admissions
                    .Where(a => a.AdmissionStatus == Status.Active)
                    .OrderByDescending(a => a.AdmissionDate)
                    .FirstOrDefault();

                return new AdmittedPatientListViewModel
                {
                    PatientId = p.PatientId,
                    FullName = p.FirstName + " " + p.LastName,
                    DOB = p.DOB,
                    Gender = p.Gender,
                    IdNumber = p.IdNumber,
                    Cellphone = p.Cellphone,
                    AdmissionDate = latestAdmission?.AdmissionDate ?? DateTime.MinValue,
                    AdmissionId = latestAdmission?.AdmissionId ?? 0,
                    AdmissionStatus = latestAdmission?.AdmissionStatus ?? Status.Delete,

                    // ✅ map doctor and nurse names
                    DoctorName = latestAdmission?.Doctor != null
                        ? $"{latestAdmission.Doctor.FirstName} {latestAdmission.Doctor.LastName}"
                        : "N/A",
                    NurseName = latestAdmission?.Nurse != null
                        ? $"{latestAdmission.Nurse.FirstName} {latestAdmission.Nurse.LastName}"
                        : "N/A",

                    Allergies = p.PatientAllergies.Select(a => a.Allergy.Name).ToList(),
                    Medications = p.PatientMedications.Select(m => m.Medication.Name).ToList(),
                    Conditions = p.PatientConditions.Select(c => c.Condition.Name).ToList(),
                };
            }).ToList();

            return View(admittedList);
        }



        public async Task<IActionResult> EditAdmission(int id)
        {
            var admission = await _context.Admissions
                .IgnoreQueryFilters()
                .Include(a => a.Patient)
                .ThenInclude(p => p.PatientAllergies)
                .Include(a => a.Patient.PatientMedications)
                .Include(a => a.Patient.PatientConditions)
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            if (admission == null)
                return NotFound();

            var model = new EditAdmissionViewModel
            {
                AdmissionId = admission.AdmissionId,
                PatientId = admission.PatientId,
                SelectedAllergyIds = admission.Patient.PatientAllergies.Select(a => a.AllergyId).ToList(),
                SelectedMedicationIds = admission.Patient.PatientMedications.Select(m => m.MedicationId).ToList(),
                SelectedConditionIds = admission.Patient.PatientConditions.Select(c => c.ConditionId).ToList(),
                SelectedDoctorId = admission.EmployeeID,
                SelectedNurseId = admission.NurseID,
                AllergyOptions = await _context.Allergies.ToListAsync(),
                MedicationOptions = await _context.Medications.ToListAsync(),
                ConditionOptions = await _context.Conditions.ToListAsync(),



                DoctorOptions = await _context.Employees.Where(e => e.Role == UserRole.DOCTOR).ToListAsync(),
                NurseOptions = await _context.Employees.Where(e => e.Role == UserRole.NURSE).ToListAsync()

            };

            return View(model);
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditAdmission(EditAdmissionViewModel vm)
        {
            // Make sure lists are not null - simplified collection initialization
            vm.SelectedAllergyIds ??= new List<int>();
            vm.SelectedMedicationIds ??= new List<int>();
            vm.SelectedConditionIds ??= new List<int>();

            if (!ModelState.IsValid)
            {
                vm.AllergyOptions = await _context.Allergies.ToListAsync();
                vm.MedicationOptions = await _context.Medications.ToListAsync();
                vm.ConditionOptions = await _context.Conditions.ToListAsync();
                vm.DoctorOptions = await _context.Employees.Where(e => e.Role == UserRole.DOCTOR).ToListAsync();
                vm.NurseOptions = await _context.Employees.Where(e => e.Role == UserRole.NURSE).ToListAsync();
                return View(vm);
            }

            var admission = await _context.Admissions
                .IgnoreQueryFilters()
                .Include(a => a.Patient)
                    .ThenInclude(p => p.PatientAllergies)
                .Include(a => a.Patient.PatientMedications)
                .Include(a => a.Patient.PatientConditions)
                .FirstOrDefaultAsync(a => a.AdmissionId == vm.AdmissionId);

            if (admission == null)
                return NotFound();

            // ✅ FIX: Declare and calculate the change detection variables
            bool doctorChanged = admission.EmployeeID != vm.SelectedDoctorId;
            bool nurseChanged = admission.NurseID != vm.SelectedNurseId;

            // Update doctor & nurse
            admission.EmployeeID = vm.SelectedDoctorId;
            admission.NurseID = vm.SelectedNurseId;

            // Ensure collections are not null - simplified
            admission.Patient.PatientAllergies ??= new List<PatientAllergy>();
            admission.Patient.PatientMedications ??= new List<PatientMedication>();
            admission.Patient.PatientConditions ??= new List<PatientCondition>();

            // Clear old selections
            admission.Patient.PatientAllergies.Clear();
            admission.Patient.PatientMedications.Clear();
            admission.Patient.PatientConditions.Clear();

            // Assign new selections - simplified collection initialization
            admission.Patient.PatientAllergies = vm.SelectedAllergyIds
                .Select(id => new PatientAllergy { PatientId = admission.PatientId, AllergyId = id, Status = Status.Active })
                .ToList();

            admission.Patient.PatientMedications = vm.SelectedMedicationIds
                .Select(id => new PatientMedication { PatientId = admission.PatientId, MedicationId = id, Status = Status.Active })
                .ToList();

            admission.Patient.PatientConditions = vm.SelectedConditionIds
                .Select(id => new PatientCondition { PatientId = admission.PatientId, ConditionId = id, Status = Status.Active })
                .ToList();

            await _context.SaveChangesAsync();

            // ✅ FIX: Now the variables exist and can be used
            if (doctorChanged || nurseChanged)
            {
                var currentUser = await _context.Employees.FirstOrDefaultAsync(e => e.UserName == User.Identity.Name);
                if (currentUser != null)
                {
                    await _notificationService.CreateAdmissionUpdateNotification(
                        admission.AdmissionId,
                        $"Patient {admission.Patient.FirstName} {admission.Patient.LastName}'s admission has been updated.",
                        currentUser.EmployeeID,
                        NotificationPriority.Normal
                    );
                }
            }

            TempData["SuccessMessage"] = "Admission updated successfully!";
            return RedirectToAction("AdmittedPatients");
        }







        [HttpGet]
        public async Task<IActionResult> DeleteAdmission(int id)
        {
            var admission = await _context.Admissions
                .IgnoreQueryFilters()
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            if (admission == null)
                return NotFound();

            return View(admission);
        }



        [HttpPost, ActionName("DeleteAdmission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteAdmissionConfirmed(int id)
        {
            var admission = await _context.Admissions
                .IgnoreQueryFilters()
                .Include(a => a.Patient)
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            if (admission == null)
                return NotFound();

            // Update admission status (soft delete)
            admission.AdmissionStatus = Status.Delete;

            _context.Admissions.Update(admission);
            _context.Patients.Update(admission.Patient);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Admission deleted.";
            return RedirectToAction("AdmittedPatients");
        }


        public async Task<IActionResult> DeletedAdmissionsList()
        {
            var deletedAdmissions = await _context.Admissions
                .IgnoreQueryFilters()
                .Where(a => a.AdmissionStatus == Status.Delete)
                .Include(a => a.Patient)
                .Select(a => new AdmissionWithPatientViewModel
                {
                    AdmissionId = a.AdmissionId,
                    AdmissionDate = a.AdmissionDate,
                    Notes = a.Notes,
                    PatientId = a.PatientId,
                    FirstName = a.Patient.FirstName,
                    LastName = a.Patient.LastName
                })
                .ToListAsync();

            return View(deletedAdmissions);
        }



        public async Task<IActionResult> DeletedAdmission(int id)
        {
            var admission = await _context.Admissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.AdmissionId == id && a.AdmissionStatus == Status.Delete);

            if (admission == null)
                return NotFound();

            return View(admission); // Confirmation page
        }



        [HttpPost, ActionName("DeletedAdmission")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreAdmissionConfirmed(int id)
        {
            var admission = await _context.Admissions
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.AdmissionId == id);

            if (admission == null)
                return NotFound();

            admission.AdmissionStatus = Status.Active;
            _context.Admissions.Update(admission);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Admission restored successfully.";
            return RedirectToAction("DeletedAdmissionsList");
        }


        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var patient = await _context.Patients
                .Include(p => p.PatientAllergies).ThenInclude(pa => pa.Allergy)
                .Include(p => p.PatientMedications).ThenInclude(pm => pm.Medication)
                .Include(p => p.PatientConditions).ThenInclude(pc => pc.Condition)
                .Include(p => p.Admissions)
                    .ThenInclude(a => a.Doctor)
                .Include(p => p.Admissions)
                    .ThenInclude(a => a.Nurse)
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found.";
                return RedirectToAction("Index");
            }

            return View(patient);
        }



















        // GET: Create Patient Folder
        public async Task<IActionResult> CreatePatientFolder(int id)
        {
            var patient = await _context.Patients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PatientId == id);

            if (patient == null || patient.GetStatus == Status.Delete || patient.GetStatus != Status.Admitted)
            {
                TempData["ErrorMessage"] = "Patient is either deleted or not currently admitted.";
                return RedirectToAction("Patients"); // or a custom error view
            }

            // Check if a folder already exists for this patient
            var existingFolder = await _context.PatientFolders
                .IgnoreQueryFilters() // So we include even soft-deleted folders
                .FirstOrDefaultAsync(f => f.PatientId == id);

            if (existingFolder != null)
            {
                TempData["InfoMessage"] = "Patient folder already exists.";
                return RedirectToAction("ViewPatientFolder", new { id = existingFolder.FolderId });
            }

            // Create new folder
            var folder = new PatientFolder
            {
                PatientId = patient.PatientId,
                CreatedOn = DateTime.Now,
                OpenedBy = User.Identity?.Name ?? "System",
                Notes = string.Empty,
                Status = Status.Active
            };

            _context.PatientFolders.Add(folder);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient folder created successfully!";
            return RedirectToAction("ViewPatientFolder", new { id = folder.FolderId });
        }


        public async Task<IActionResult> ViewPatientFolder(int id)
        {
            var folder = await _context.PatientFolders
                .IgnoreQueryFilters()
                .Include(f => f.Patient)
                    .ThenInclude(p => p.Admissions.OrderByDescending(a => a.AdmissionDate).Take(1))
                        .ThenInclude(a => a.Doctor) // include doctor
                .Include(f => f.Patient)
                    .ThenInclude(p => p.Admissions)
                        .ThenInclude(a => a.Nurse) // include nurse
                .Include(f => f.Patient)
                    .ThenInclude(p => p.PatientAllergies)
                        .ThenInclude(pa => pa.Allergy)
                .Include(f => f.Patient)
                    .ThenInclude(p => p.PatientMedications)
                        .ThenInclude(pm => pm.Medication)
                .Include(f => f.Patient)
                    .ThenInclude(p => p.PatientConditions)
                        .ThenInclude(pc => pc.Condition)
                .FirstOrDefaultAsync(f => f.FolderId == id);


            if (!string.IsNullOrEmpty(folder.OpenedBy))
            {
                var employee = await _context.Employees
                    .FirstOrDefaultAsync(e => e.FirstName + " " + e.LastName == folder.OpenedBy);

                if (employee != null && employee.Role == UserRole.WARDADMIN)
                {
                    folder.OpenedBy = $"{employee.FirstName} {employee.LastName}";
                }
            }


            if (folder == null || folder.Status == Status.Delete)
                return NotFound();

            return View(folder);
        }








        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPatientFolder(int FolderId, string Notes)
        {
            var folder = await _context.PatientFolders.FirstOrDefaultAsync(f => f.FolderId == FolderId);
            if (folder == null)
                return NotFound();

            folder.Notes = Notes;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Notes updated successfully!";
            return RedirectToAction("ViewPatientFolder", new { id = FolderId });
        }



        // POST: Delete Patient Folder (Soft Delete)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePatientFolder(int id)
        {
            var folder = await _context.PatientFolders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.FolderId == id);

            if (folder == null)
                return NotFound();

            folder.Status = Status.Delete;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient folder deleted successfully.";
            return RedirectToAction("Patients"); // or folder list view
        }


        public async Task<IActionResult> DeletedFolders()
        {
            var deletedFolders = await _context.PatientFolders
                .IgnoreQueryFilters() // To get folders regardless of Status filter
                .Where(f => f.Status == Status.Delete)
                .Include(f => f.Patient) // eager load Patient for display
                .ToListAsync();

            return View(deletedFolders);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestorePatientFolder(int id)
        {
            var folder = await _context.PatientFolders
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(f => f.FolderId == id);

            if (folder == null || folder.Status != Status.Delete)
                return NotFound();

            folder.Status = Status.Active;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Patient folder restored successfully.";
            return RedirectToAction("DeletedFolders");
        }
























        public async Task<IActionResult> BedAssignmentsList()
        {
            var assignments = await _context.BedAssignments
                .IgnoreQueryFilters()
                .Where(a => a.AssignmentStatus == Status.Active)
                .Include(a => a.Patient)
                .Include(a => a.Bed)
                    .ThenInclude(b => b.Ward)
                .ToListAsync();

            return View(assignments);
        }



        [HttpGet]
        public async Task<IActionResult> AssignBed(int id)
        {
            try
            {
                var patient = await _context.Patients
                    .IgnoreQueryFilters()
                    .Include(p => p.Admissions)
                    .FirstOrDefaultAsync(p => p.PatientId == id);

                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction("AdmittedPatients");
                }

                // Get the active admission (should be only one)
                var activeAdmission = patient.Admissions
                    .FirstOrDefault(a => a.AdmissionStatus == Status.Active);

                if (activeAdmission == null)
                {
                    TempData["ErrorMessage"] = "Patient is not currently admitted.";
                    return RedirectToAction("AdmittedPatients");
                }

                // Check if patient already has an active bed assignment
                var existingAssignment = await _context.BedAssignments
                    .AnyAsync(ba => ba.PatientId == id && ba.AssignmentStatus == Status.Active);

                if (existingAssignment)
                {
                    TempData["ErrorMessage"] = "Patient already has an active bed assignment.";
                    return RedirectToAction("BedAssignmentsList");
                }

                // Get available beds
                var beds = await _context.Beds
                    .Include(b => b.Ward)
                    .Where(b => b.BedStatus == Status.Active &&
                        !_context.BedAssignments.Any(a => a.BedId == b.BedId && a.AssignmentStatus == Status.Active))
                    .Select(b => new SelectListItem
                    {
                        Value = b.BedId.ToString(),
                        Text = $"{b.BedNumber} (Ward: {b.Ward.WardName})"
                    })
                    .ToListAsync();

                // Initialize the list if null (defensive programming)
                beds ??= new List<SelectListItem>();

                if (!beds.Any())
                {
                    TempData["ErrorMessage"] = "No beds are currently available.";
                    return RedirectToAction("AdmittedPatients");
                }

                var vm = new AssignBedViewModel
                {
                    PatientId = patient.PatientId,
                    FullName = patient.FullName,
                    AvailableBeds = beds,
                    AssignmentId = activeAdmission.AdmissionId // You might need this
                };

                return View(vm);
            }
            catch (Exception ex)
            {



                TempData["ErrorMessage"] = "An error occurred while loading bed assignment page.";
                return RedirectToAction("AdmittedPatients");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignBed(AssignBedViewModel model)
        {
            if (!ModelState.IsValid)
            {
                // Reload available beds if model state is invalid
                await ReloadAvailableBeds(model);
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Verify patient exists and has active admission
                var patient = await _context.Patients
                    .IgnoreQueryFilters()
                    .Include(p => p.Admissions)
                    .FirstOrDefaultAsync(p => p.PatientId == model.PatientId);

                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found.";
                    return RedirectToAction("Patients");
                }

                var activeAdmission = patient.Admissions
                    .FirstOrDefault(a => a.AdmissionStatus == Status.Active);

                if (activeAdmission == null)
                {
                    TempData["ErrorMessage"] = "Patient is not currently admitted.";
                    return RedirectToAction("AdmittedPatients");
                }

                // Check if patient already has an active bed assignment (race condition check)
                var existingAssignment = await _context.BedAssignments
                    .FirstOrDefaultAsync(ba => ba.PatientId == model.PatientId && ba.AssignmentStatus == Status.Active);

                if (existingAssignment != null)
                {
                    TempData["ErrorMessage"] = "Patient already has an active bed assignment.";
                    return RedirectToAction("BedAssignmentsList");
                }

                // Verify the selected bed is still available
                var selectedBed = await _context.Beds
                    .Include(b => b.Ward)
                    .FirstOrDefaultAsync(b => b.BedId == model.BedId && b.BedStatus == Status.Active);

                if (selectedBed == null)
                {
                    ModelState.AddModelError("BedId", "Selected bed is not available.");
                    await ReloadAvailableBeds(model);
                    return View(model);
                }

                // Check if bed is still unassigned (race condition check)
                var bedAssigned = await _context.BedAssignments
                    .AnyAsync(ba => ba.BedId == model.BedId && ba.AssignmentStatus == Status.Active);

                if (bedAssigned)
                {
                    ModelState.AddModelError("BedId", "Selected bed has already been assigned to another patient.");
                    await ReloadAvailableBeds(model);
                    return View(model);
                }

                // Create new bed assignment based on your actual model
                var bedAssignment = new BedAssignment
                {
                    PatientId = model.PatientId,
                    BedId = model.BedId,
                    AssignedDate = DateTime.Now,
                    AssignmentStatus = Status.Active,


                };
                selectedBed.Status = BedStatus.Occupied;
                _context.BedAssignments.Add(bedAssignment);
                _context.Beds.Update(selectedBed);
                // Save changes
                var result = await _context.SaveChangesAsync();

                if (result > 0)
                {
                    await transaction.CommitAsync();

                    TempData["SuccessMessage"] = $"Bed {selectedBed.BedNumber} in {selectedBed.Ward.WardName} has been successfully assigned to {patient.FullName}.";

                    // Redirect back to admitted patients
                    return RedirectToAction("BedAssignmentsList");
                }
                else
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Failed to assign bed. Please try again.";
                    await ReloadAvailableBeds(model);
                    return View(model);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "The bed assignment failed due to a concurrency conflict. Please try again.";
                return RedirectToAction("AdmittedPatients");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                // Log the exception if you have logging configured
                // _logger?.LogError(ex, "Error occurred while assigning bed to patient {PatientId}", model.PatientId);

                TempData["ErrorMessage"] = "An error occurred while assigning the bed. Please try again.";
                await ReloadAvailableBeds(model);
                return View(model);
            }
        }



        [HttpGet]
        public async Task<IActionResult> EditBedAssignment(int id)
        {
            try
            {
                // Get the existing bed assignment
                var assignment = await _context.BedAssignments
                    .IgnoreQueryFilters()
                    .Include(a => a.Patient)
                    .Include(a => a.Bed)
                        .ThenInclude(b => b.Ward)
                    .FirstOrDefaultAsync(a => a.AssignmentId == id);

                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "Bed assignment not found.";
                    return RedirectToAction("BedAssignmentsList");
                }

                // Check if assignment is still active
                if (assignment.AssignmentStatus != Status.Active)
                {
                    TempData["ErrorMessage"] = "Cannot edit inactive bed assignment.";
                    return RedirectToAction("BedAssignmentsList");
                }

                // Get available beds (excluding current bed but including it in the list)
                var availableBeds = await _context.Beds
                    .Include(b => b.Ward)
                    .Where(b => b.BedStatus == Status.Active &&
                        (!_context.BedAssignments.Any(a => a.BedId == b.BedId && a.AssignmentStatus == Status.Active)
                        || b.BedId == assignment.BedId)) // Include current bed
                    .Select(b => new SelectListItem
                    {
                        Value = b.BedId.ToString(),
                        Text = $"{b.BedNumber} (Ward: {b.Ward.WardName})",
                        Selected = b.BedId == assignment.BedId // Mark current bed as selected
                    })
                    .ToListAsync();

                if (!availableBeds.Any())
                {
                    TempData["ErrorMessage"] = "No beds are currently available for reassignment.";
                    return RedirectToAction("BedAssignmentsList");
                }

                var viewModel = new EditBedAssignmentViewModel
                {
                    AssignmentId = assignment.AssignmentId,
                    PatientId = assignment.PatientId,
                    PatientName = assignment.Patient.FullName,
                    CurrentBedId = assignment.BedId,
                    NewBedId = assignment.BedId, // Default to current bed
                    CurrentBedInfo = $"{assignment.Bed.BedNumber} (Ward: {assignment.Bed.Ward.WardName})",
                    AssignedDate = assignment.AssignedDate,
                    AvailableBeds = availableBeds
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {

                TempData["ErrorMessage"] = "An error occurred while loading the bed assignment.";
                return RedirectToAction("BedAssignmentsList");
            }
        }



        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditBedAssignment(EditBedAssignmentViewModel model)
        {
            if (!ModelState.IsValid)
            {
                await ReloadEditBedData(model);
                return View(model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var existingAssignment = await _context.BedAssignments
                    .IgnoreQueryFilters()
                    .Include(a => a.Patient)
                    .Include(a => a.Bed)
                        .ThenInclude(b => b.Ward)
                    .FirstOrDefaultAsync(a => a.AssignmentId == model.AssignmentId);

                if (existingAssignment == null)
                {
                    TempData["ErrorMessage"] = "Bed assignment not found.";
                    return RedirectToAction("BedAssignmentsList");
                }

                if (existingAssignment.AssignmentStatus != Status.Active)
                {
                    TempData["ErrorMessage"] = "Cannot edit inactive bed assignment.";
                    return RedirectToAction("BedAssignmentsList");
                }

                // If the bed is changing, handle status updates
                if (model.NewBedId != existingAssignment.BedId)
                {
                    // Fetch the new bed
                    var newBed = await _context.Beds
                        .Include(b => b.Ward)
                        .FirstOrDefaultAsync(b => b.BedId == model.NewBedId && b.BedStatus == Status.Active);

                    if (newBed == null)
                    {
                        ModelState.AddModelError("NewBedId", "Selected bed is not available.");
                        await ReloadEditBedData(model);
                        return View(model);
                    }

                    var bedAlreadyAssigned = await _context.BedAssignments
                        .AnyAsync(ba => ba.BedId == model.NewBedId &&
                                        ba.AssignmentStatus == Status.Active &&
                                        ba.AssignmentId != model.AssignmentId);

                    if (bedAlreadyAssigned)
                    {
                        ModelState.AddModelError("NewBedId", "Selected bed is already assigned to another patient.");
                        await ReloadEditBedData(model);
                        return View(model);
                    }

                    // ✅ Update old bed status to Available
                    var oldBed = await _context.Beds.FindAsync(existingAssignment.BedId);
                    if (oldBed != null)
                    {
                        oldBed.Status = BedStatus.Available;
                        _context.Beds.Update(oldBed);
                    }

                    // ✅ Set new bed as Occupied
                    newBed.Status = BedStatus.Occupied;
                    _context.Beds.Update(newBed);

                    // Update assignment to new bed
                    existingAssignment.BedId = model.NewBedId;

                    TempData["SuccessMessage"] = $"Patient {existingAssignment.Patient.FullName} has been moved from {oldBed?.BedNumber} to {newBed.BedNumber} in {newBed.Ward.WardName}.";
                }
                else
                {
                    TempData["SuccessMessage"] = $"Bed assignment for {existingAssignment.Patient.FullName} has been updated.";
                }

                // Update assignment date or any other fields
                existingAssignment.AssignedDate = model.AssignedDate;

                var result = await _context.SaveChangesAsync();
                if (result > 0)
                {
                    await transaction.CommitAsync();
                    return RedirectToAction("BedAssignmentsList");
                }
                else
                {
                    await transaction.RollbackAsync();
                    TempData["ErrorMessage"] = "Failed to update bed assignment. Please try again.";
                    await ReloadEditBedData(model);
                    return View(model);
                }
            }
            catch (DbUpdateConcurrencyException)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "The bed assignment was modified by another user. Please try again.";
                return RedirectToAction("BedAssignmentsList");
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "An error occurred while updating the bed assignment.";
                await ReloadEditBedData(model);
                return View(model);
            }
        }






        [HttpGet]
        public async Task<IActionResult> DeleteBedAssignment(int id)
        {
            var assignment = await _context.BedAssignments
                .IgnoreQueryFilters()
                .Include(a => a.Patient)
                .Include(a => a.Bed)
                    .ThenInclude(b => b.Ward)
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null || assignment.AssignmentStatus != Status.Active)
            {
                TempData["ErrorMessage"] = "Bed assignment not found or already deleted.";
                return RedirectToAction("BedAssignmentsList");
            }

            return View(assignment); // View strongly typed to `BedAssignment`
        }


        [HttpPost, ActionName("DeleteBedAssignment")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteBedAssignmentConfirmed(int id)
        {
            var assignment = await _context.BedAssignments
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null || assignment.AssignmentStatus != Status.Active)
            {
                TempData["ErrorMessage"] = "Bed assignment not found or already deleted.";
                return RedirectToAction("BedAssignmentsList");
            }

            assignment.AssignmentStatus = Status.Delete;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Bed assignment removed successfully.";
            return RedirectToAction("BedAssignmentsList");
        }





        public async Task<IActionResult> DeletedBedAssignments()
        {
            var deletedAssignments = await _context.BedAssignments
                .IgnoreQueryFilters()
                .Where(a => a.AssignmentStatus == Status.Delete)
                .Include(a => a.Patient)
                .Include(a => a.Bed)
                    .ThenInclude(b => b.Ward)
                .ToListAsync();

            return View(deletedAssignments);
        }




        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreBedAssignment(int id)
        {
            try
            {
                var assignment = await _context.BedAssignments
                    .IgnoreQueryFilters()
                    .Include(a => a.Bed)
                    .FirstOrDefaultAsync(a => a.AssignmentId == id);

                if (assignment == null)
                {
                    TempData["ErrorMessage"] = "Bed assignment not found.";
                    return RedirectToAction("DeletedBedAssignments");
                }

                // Ensure the bed is still available (not assigned to someone else)
                var bedTaken = await _context.BedAssignments
                    .AnyAsync(a => a.BedId == assignment.BedId && a.AssignmentStatus == Status.Active);

                if (bedTaken)
                {
                    TempData["ErrorMessage"] = $"Bed {assignment.Bed.BedNumber} is already assigned to another patient.";
                    return RedirectToAction("DeletedBedAssignments");
                }

                // Restore the assignment
                assignment.AssignmentStatus = Status.Active;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Bed assignment restored successfully.";
                return RedirectToAction("BedAssignmentsList");
            }
            catch (Exception)
            {
                TempData["ErrorMessage"] = "An error occurred while restoring the bed assignment.";
                return RedirectToAction("DeletedBedAssignments");
            }
        }















        [HttpGet]
        public async Task<IActionResult> DischargePatient(int id) // AssignmentId
        {
            var assignment = await _context.BedAssignments
                .IgnoreQueryFilters()
                .Include(a => a.Bed).ThenInclude(b => b.Ward)
                .Include(a => a.Patient).ThenInclude(p => p.Admissions)
                .SingleOrDefaultAsync(a => a.AssignmentId == id);

            if (assignment == null || assignment.AssignmentStatus != Status.Active)
            {
                TempData["ErrorMessage"] = "Active bed assignment not found.";
                return RedirectToAction("BedAssignmentsList");
            }

            var activeAdmission = assignment.Patient.Admissions
                .FirstOrDefault(a => a.AdmissionStatus == Status.Active);

            if (activeAdmission == null)
            {
                TempData["ErrorMessage"] = "Patient is not currently admitted.";
                return RedirectToAction("BedAssignmentsList");
            }

            ViewBag.AdmissionId = activeAdmission.AdmissionId;
            return View(assignment);
        }



        [HttpPost, ActionName("DischargePatient")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DischargePatientConfirmed(int id) // id = AdmissionId
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var admission = await _context.Admissions
                    .IgnoreQueryFilters()
                    .Include(a => a.Patient)
                    .FirstOrDefaultAsync(a => a.AdmissionId == id && a.AdmissionStatus == Status.Active);

                if (admission == null)
                {
                    TempData["ErrorMessage"] = "Admission not found or already discharged.";
                    return RedirectToAction("BedAssignmentsList");
                }

                var bedAssignment = await _context.BedAssignments
                    .IgnoreQueryFilters()
                    .Include(a => a.Bed)
                    .FirstOrDefaultAsync(a => a.PatientId == admission.PatientId && a.AssignmentStatus == Status.Active);

                if (bedAssignment == null)
                {
                    TempData["ErrorMessage"] = "Patient does not have an active bed assignment.";
                    return RedirectToAction("BedAssignmentsList");
                }

                // ✅ Update only bed assignment and patient
                bedAssignment.AssignmentStatus = Status.Delete;
                bedAssignment.Bed.Status = BedStatus.Available;

                // ✅ DO NOT change admission.AdmissionStatus
                admission.Patient.GetStatus = Status.Discharged;

                _context.BedAssignments.Update(bedAssignment);
                _context.Beds.Update(bedAssignment.Bed);
                _context.Patients.Update(admission.Patient);

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                TempData["SuccessMessage"] = $"Patient {admission.Patient.FullName} has been discharged successfully.";
                return RedirectToAction("DischargedPatients");
            }
            catch
            {
                await transaction.RollbackAsync();
                TempData["ErrorMessage"] = "An error occurred while discharging the patient.";
                return RedirectToAction("DischargedPatients");
            }
        }


        public async Task<IActionResult> DischargedPatients()
        {
            var dischargedPatients = await _context.Patients
                .IgnoreQueryFilters()
                .Where(p => p.GetStatus == Status.Discharged)
                .Include(p => p.Admissions)
                .ToListAsync();

            return View(dischargedPatients); // View strongly typed to List<Patient>
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RestoreDischargedPatient(int id)
        {
            var patient = await _context.Patients
                .IgnoreQueryFilters()
                .FirstOrDefaultAsync(p => p.PatientId == id && p.GetStatus == Status.Discharged);

            if (patient == null)
            {
                TempData["ErrorMessage"] = "Patient not found or is not deleted.";
                return RedirectToAction("DischargedPatients");
            }

            patient.GetStatus = Status.Admitted;
            _context.Patients.Update(patient);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Patient {patient.FullName} has been restored.";
            return RedirectToAction("DischargedPatients");
        }





































































































































































































































































        private async Task ReloadAvailableBeds(AssignBedViewModel model)
        {
            try
            {
                var beds = await _context.Beds
                    .Include(b => b.Ward)
                    .Where(b => b.BedStatus == Status.Active &&
                        !_context.BedAssignments.Any(a => a.BedId == b.BedId && a.AssignmentStatus == Status.Active))
                    .Select(b => new SelectListItem
                    {
                        Value = b.BedId.ToString(),
                        Text = $"{b.BedNumber} (Ward: {b.Ward.WardName})"
                    })
                    .ToListAsync();

                model.AvailableBeds = beds ?? new List<SelectListItem>();

                // Reload patient name if needed
                if (string.IsNullOrEmpty(model.FullName))
                {
                    var patient = await _context.Patients
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.PatientId == model.PatientId);
                    model.FullName = patient?.FullName ?? "";
                }
            }
            catch
            {
                model.AvailableBeds = new List<SelectListItem>();
                // Ensure FullName is not null
                if (string.IsNullOrEmpty(model.FullName))
                {
                    model.FullName = "";
                }
            }
        }

        private SelectList GetGenderSelectList()
        {
            return new SelectList(Enum.GetValues(typeof(GenderType))
                .Cast<GenderType>()
                .OrderBy(g => g.ToString()) // Added OrderBy
                .Select(g => new { ID = g, Name = g.ToString() }), "ID", "Name");
        }

        private async Task ReloadEditBedData(EditBedAssignmentViewModel model)
        {
            try
            {
                // Get available beds (excluding beds assigned to other patients)
                var availableBeds = await _context.Beds
                    .Include(b => b.Ward)
                    .Where(b => b.BedStatus == Status.Active &&
                        (!_context.BedAssignments.Any(a => a.BedId == b.BedId && a.AssignmentStatus == Status.Active)
                        || b.BedId == model.CurrentBedId)) // Include current bed
                    .Select(b => new SelectListItem
                    {
                        Value = b.BedId.ToString(),
                        Text = $"{b.BedNumber} (Ward: {b.Ward.WardName})",
                        Selected = b.BedId == model.NewBedId
                    })
                    .ToListAsync();

                model.AvailableBeds = availableBeds ?? new List<SelectListItem>();

                // Reload patient name if needed
                if (string.IsNullOrEmpty(model.PatientName))
                {
                    var patient = await _context.Patients
                        .IgnoreQueryFilters()
                        .FirstOrDefaultAsync(p => p.PatientId == model.PatientId);
                    model.PatientName = patient?.FullName ?? "";
                }

                // Reload current bed info if needed
                if (string.IsNullOrEmpty(model.CurrentBedInfo))
                {
                    var currentBed = await _context.Beds
                        .Include(b => b.Ward)
                        .FirstOrDefaultAsync(b => b.BedId == model.CurrentBedId);
                    if (currentBed != null)
                    {
                        model.CurrentBedInfo = $"{currentBed.BedNumber} (Ward: {currentBed.Ward.WardName})";
                    }
                }
            }
            catch
            {
                model.AvailableBeds = new List<SelectListItem>();
                if (string.IsNullOrEmpty(model.PatientName))
                {
                    model.PatientName = "";
                }
                if (string.IsNullOrEmpty(model.CurrentBedInfo))
                {
                    model.CurrentBedInfo = "";
                }
            }
        }
    }
}