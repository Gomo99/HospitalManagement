using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using HospitalManagement.Data;
using HospitalManagement.Models;
using HospitalManagement.ViewModel;
using HospitalManagement.Services;
using HospitalManagement.AppStatus;

namespace HospitalManagement.Controllers
{
    [Authorize(Roles = "NURSE,NURSINGSISTER")]
    public class NurseController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly ILogger<NurseController> _logger;

        public NurseController(
            ApplicationDbContext context,
            NotificationService notificationService,
            ILogger<NurseController> logger)
        {
            _context = context;
            _notificationService = notificationService;
            _logger = logger;
        }

        public async Task<IActionResult> NurseDashboard()
        {
            try
            {
                // Get current nurse
                var currentUserName = User.Identity.Name;
                var currentNurse = await _context.Employees
                    .FirstOrDefaultAsync(e => e.UserName == currentUserName &&
                                              (e.Role == UserRole.NURSE || e.Role == UserRole.NURSINGSISTER) &&
                                              e.IsActive == Status.Active);

                if (currentNurse == null)
                    return RedirectToAction("AccessDenied", "Account");

                // Get unread notifications
                ViewData["UnreadNotificationCount"] = await _notificationService.GetUnreadCountAsync(currentNurse.EmployeeID);

                // Get assigned patients
                var assignedPatients = await GetAssignedPatients(currentNurse.EmployeeID);

                // Get pending tasks
                var pendingInstructions = await _context.DoctorInstructions
                    .Where(di => di.Status == Status.Active &&
                                 !di.IsCompleted &&
                                 di.Patient != null &&
                                 di.Patient.Admissions.Any(a => a.NurseID == currentNurse.EmployeeID))
                    .CountAsync();

                ViewData["AssignedPatientCount"] = assignedPatients.Count;
                ViewData["PendingInstructionsCount"] = pendingInstructions;

                return View(assignedPatients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading nurse dashboard");
                TempData["ErrorMessage"] = "Error loading dashboard. Please try again.";
                return RedirectToAction("Index", "Home");
            }
        }

        #region Vital Signs Management

        [HttpGet]
        public async Task<IActionResult> RecordVitals(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("NurseDashboard");
                }

                var viewModel = new RecordVitalsViewModel
                {
                    PatientId = patientId,
                    PatientName = patient.FullName
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading record vitals page for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading page. Please try again.";
                return RedirectToAction("NurseDashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordVitals(RecordVitalsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload patient name
                    var patient = await _context.Patients.FindAsync(model.PatientId);
                    model.PatientName = patient?.FullName;
                    return View(model);
                }

                // Get current nurse
                var currentNurse = await GetCurrentNurse();
                if (currentNurse == null)
                    return RedirectToAction("AccessDenied", "Account");

                // Create vital signs record
                var vitalSigns = new VitalSigns
                {
                    PatientId = model.PatientId,
                    TakenByEmployeeId = currentNurse.EmployeeID,
                    RecordedDateTime = DateTime.Now,
                    BloodPressureSystolic = model.BloodPressureSystolic,
                    BloodPressureDiastolic = model.BloodPressureDiastolic,
                    Temperature = model.Temperature,
                    TemperatureUnit = model.TemperatureUnit ?? TemperatureUnit.Celsius,
                    HeartRate = model.HeartRate,
                    RespiratoryRate = model.RespiratoryRate,
                    OxygenSaturation = model.OxygenSaturation,
                    BloodSugar = model.BloodSugar,
                    GlucoseUnit = model.GlucoseUnit ?? GlucoseUnit.mg_dL,
                    PainLevel = model.PainLevel,
                    Notes = model.Notes,
                    Status = Status.Active
                };

                _context.VitalSigns.Add(vitalSigns);
                await _context.SaveChangesAsync();

                // Log the activity
                _logger.LogInformation("Nurse {NurseId} recorded vitals for patient {PatientId}",
                    currentNurse.EmployeeID, model.PatientId);

                TempData["SuccessMessage"] = "Vital signs recorded successfully!";
                return RedirectToAction("PatientVitalsHistory", new { patientId = model.PatientId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording vitals for patient {PatientId}", model.PatientId);
                TempData["ErrorMessage"] = "Error recording vital signs. Please try again.";

                // Reload patient name
                var patient = await _context.Patients.FindAsync(model.PatientId);
                model.PatientName = patient?.FullName;

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PatientVitalsHistory(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("NurseDashboard");
                }

                var vitalSigns = await _context.VitalSigns
                    .Where(v => v.PatientId == patientId && v.Status == Status.Active)
                    .Include(v => v.TakenBy)
                    .OrderByDescending(v => v.RecordedDateTime)
                    .ToListAsync();

                ViewData["PatientName"] = patient.FullName;
                ViewData["PatientId"] = patientId;

                return View(vitalSigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vitals history for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading vital signs history.";
                return RedirectToAction("NurseDashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewVitalSigns(int id)
        {
            try
            {
                var vitalSigns = await _context.VitalSigns
                    .Include(v => v.Patient)
                    .Include(v => v.TakenBy)
                    .FirstOrDefaultAsync(v => v.VitalSignsId == id && v.Status == Status.Active);

                if (vitalSigns == null)
                {
                    TempData["ErrorMessage"] = "Vital signs record not found.";
                    return RedirectToAction("NurseDashboard");
                }

                // Verify nurse has access to this patient
                var currentNurse = await GetCurrentNurse();
                var isAssigned = await IsPatientAssignedToNurse(vitalSigns.PatientId, currentNurse.EmployeeID);

                if (!isAssigned)
                {
                    TempData["ErrorMessage"] = "You do not have access to this patient's records.";
                    return RedirectToAction("NurseDashboard");
                }

                return View(vitalSigns);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing vital signs {VitalSignsId}", id);
                TempData["ErrorMessage"] = "Error loading vital signs record.";
                return RedirectToAction("NurseDashboard");
            }
        }

        #endregion

        #region Treatment Management

        [HttpGet]
        public async Task<IActionResult> RecordTreatment(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("NurseDashboard");
                }

                var viewModel = new TreatmentViewModel
                {
                    PatientId = patientId,
                    PatientName = patient.FullName,
                    AvailableMedications = await _context.Medications
                        .Where(m => m.MedicationStatus == Status.Active)
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading record treatment page for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading page. Please try again.";
                return RedirectToAction("NurseDashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordTreatment(TreatmentViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload available medications
                    model.AvailableMedications = await _context.Medications
                        .Where(m => m.MedicationStatus == Status.Active)
                        .ToListAsync();

                    var patient = await _context.Patients.FindAsync(model.PatientId);
                    model.PatientName = patient?.FullName;
                    return View(model);
                }

                // Get current nurse
                var currentNurse = await GetCurrentNurse();
                if (currentNurse == null)
                    return RedirectToAction("AccessDenied", "Account");

                // Create treatment record
                var treatment = new Treatment
                {
                    PatientId = model.PatientId,
                    AdministeredByEmployeeId = currentNurse.EmployeeID,
                    TreatmentDateTime = DateTime.Now,
                    TreatmentType = model.TreatmentType,
                    Description = model.Description,
                    Details = model.Details,
                    MedicationId = model.MedicationId,
                    Dosage = model.Dosage,
                    DosageUnit = model.DosageUnit,
                    Status = Status.Active
                };

                _context.Treatments.Add(treatment);
                await _context.SaveChangesAsync();

                // If treatment is wound dressing, check if doctor needs to be notified
                if (model.TreatmentType == TreatmentType.WoundDressing &&
                    model.Details?.ToLower().Contains("infection") == true)
                {
                    await NotifyDoctorAboutWoundInfection(model.PatientId, currentNurse.EmployeeID);
                }

                _logger.LogInformation("Nurse {NurseId} recorded treatment for patient {PatientId}",
                    currentNurse.EmployeeID, model.PatientId);

                TempData["SuccessMessage"] = "Treatment recorded successfully!";
                return RedirectToAction("PatientTreatmentHistory", new { patientId = model.PatientId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording treatment for patient {PatientId}", model.PatientId);
                TempData["ErrorMessage"] = "Error recording treatment. Please try again.";

                // Reload data
                model.AvailableMedications = await _context.Medications
                    .Where(m => m.MedicationStatus == Status.Active)
                    .ToListAsync();

                var patient = await _context.Patients.FindAsync(model.PatientId);
                model.PatientName = patient?.FullName;

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PatientTreatmentHistory(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("NurseDashboard");
                }

                var treatments = await _context.Treatments
                    .Where(t => t.PatientId == patientId && t.Status == Status.Active)
                    .Include(t => t.AdministeredBy)
                    .Include(t => t.Medication)
                    .OrderByDescending(t => t.TreatmentDateTime)
                    .ToListAsync();

                ViewData["PatientName"] = patient.FullName;
                ViewData["PatientId"] = patientId;

                return View(treatments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading treatment history for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading treatment history.";
                return RedirectToAction("NurseDashboard");
            }
        }

        #endregion

        #region Medication Administration

        [HttpGet]
        public async Task<IActionResult> AdministerMedication(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("NurseDashboard");
                }

                // Get current nurse to check role
                var currentNurse = await GetCurrentNurse();
                var isNursingSister = currentNurse?.Role == UserRole.NURSINGSISTER;

                // Get available medications based on nurse role
                var availableMedications = await _context.Medications
                    .Include(m => m.MedicationSchedules)
                    .Where(m => m.MedicationStatus == Status.Active)
                    .ToListAsync();

                // Filter medications based on nurse role
                if (!isNursingSister)
                {
                    // Regular nurses can only administer up to Schedule 4
                    availableMedications = availableMedications
                        .Where(m => m.MedicationSchedules == null ||
                                   !m.MedicationSchedules.Any() ||
                                   m.MedicationSchedules.Max(ms => ms.ScheduleLevel) <= 4)
                        .ToList();
                }

                var viewModel = new AdministerMedicationViewModel
                {
                    PatientId = patientId,
                    PatientName = patient.FullName,
                    AvailableMedications = availableMedications
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading administer medication page for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading page. Please try again.";
                return RedirectToAction("NurseDashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdministerMedication(AdministerMedicationViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return await ReloadAdministerMedicationModel(model);
                }

                // Get current nurse and verify role
                var currentNurse = await GetCurrentNurse();
                if (currentNurse == null)
                    return RedirectToAction("AccessDenied", "Account");

                // Check medication schedule restrictions
                var medication = await _context.Medications
                    .Include(m => m.MedicationSchedules)
                    .FirstOrDefaultAsync(m => m.MedicationId == model.MedicationId && m.MedicationStatus == Status.Active);

                if (medication == null)
                {
                    ModelState.AddModelError("MedicationId", "Medication not found.");
                    return await ReloadAdministerMedicationModel(model);
                }

                // Check if nurse can administer this medication
                if (currentNurse.Role == UserRole.NURSE &&
                    medication.MedicationSchedules != null &&
                    medication.MedicationSchedules.Any() &&
                    medication.MedicationSchedules.Max(ms => ms.ScheduleLevel) >= 5)
                {
                    TempData["ErrorMessage"] = "You are not authorized to administer Schedule 5 or higher medications. Please contact a Nursing Sister.";
                    return await ReloadAdministerMedicationModel(model);
                }

                // Record medication administration as a treatment
                var treatment = new Treatment
                {
                    PatientId = model.PatientId,
                    AdministeredByEmployeeId = currentNurse.EmployeeID,
                    TreatmentDateTime = DateTime.Now,
                    TreatmentType = TreatmentType.MedicationAdministration,
                    Description = $"Administered {medication.Name} - {model.Dosage}{model.DosageUnit}",
                    Details = $"Reason: {model.Reason}. Notes: {model.Notes}",
                    MedicationId = model.MedicationId,
                    Dosage = model.Dosage,
                    DosageUnit = model.DosageUnit,
                    Status = Status.Active
                };

                _context.Treatments.Add(treatment);
                await _context.SaveChangesAsync();

                // Log medication administration
                _logger.LogInformation(
                    "Nurse {NurseId} ({NurseRole}) administered medication {MedicationId} to patient {PatientId}",
                    currentNurse.EmployeeID, currentNurse.Role, model.MedicationId, model.PatientId);

                TempData["SuccessMessage"] = $"Medication {medication.Name} administered successfully!";
                return RedirectToAction("PatientTreatmentHistory", new { patientId = model.PatientId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error administering medication to patient {PatientId}", model.PatientId);
                TempData["ErrorMessage"] = "Error administering medication. Please try again.";
                return await ReloadAdministerMedicationModel(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> MedicationHistory(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("NurseDashboard");
                }

                var medicationTreatments = await _context.Treatments
                    .Where(t => t.PatientId == patientId &&
                               t.Status == Status.Active &&
                               t.TreatmentType == TreatmentType.MedicationAdministration)
                    .Include(t => t.AdministeredBy)
                    .Include(t => t.Medication)
                    .OrderByDescending(t => t.TreatmentDateTime)
                    .ToListAsync();

                ViewData["PatientName"] = patient.FullName;
                ViewData["PatientId"] = patientId;

                return View(medicationTreatments);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading medication history for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading medication history.";
                return RedirectToAction("NurseDashboard");
            }
        }

        #endregion

        #region Doctor Instructions

        [HttpGet]
        public async Task<IActionResult> ContactDoctor(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("NurseDashboard");
                }

                // Get patient's assigned doctor from admission
                var admission = await _context.Admissions
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.PatientId == patientId &&
                                             a.AdmissionStatus == Status.Active);

                var viewModel = new DoctorInstructionsViewModel
                {
                    PatientId = patientId,
                    PatientName = patient.FullName,
                    DoctorId = admission?.EmployeeID ?? 0,
                    AvailableDoctors = await _context.Employees
                        .Where(e => e.Role == UserRole.DOCTOR && e.IsActive == Status.Active)
                        .OrderBy(e => e.FirstName)
                        .ToListAsync()
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading contact doctor page for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading page. Please try again.";
                return RedirectToAction("NurseDashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ContactDoctor(DoctorInstructionsViewModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // Reload available doctors
                    model.AvailableDoctors = await _context.Employees
                        .Where(e => e.Role == UserRole.DOCTOR && e.IsActive == Status.Active)
                        .OrderBy(e => e.FirstName)
                        .ToListAsync();

                    var patient = await _context.Patients.FindAsync(model.PatientId);
                    model.PatientName = patient?.FullName;
                    return View(model);
                }

                // Get current nurse
                var currentNurse = await GetCurrentNurse();
                if (currentNurse == null)
                    return RedirectToAction("AccessDenied", "Account");

                // Create doctor instructions record
                var instructions = new DoctorInstructions
                {
                    PatientId = model.PatientId,
                    DoctorId = model.DoctorId,
                    RecordedByEmployeeId = currentNurse.EmployeeID,
                    InstructionDate = DateTime.Now,
                    Title = model.Title,
                    Instructions = model.Instructions,
                    FollowUpActions = model.FollowUpActions,
                    FollowUpDate = model.FollowUpDate,
                    IsCompleted = false,
                    Status = Status.Active
                };

                _context.DoctorInstructions.Add(instructions);
                await _context.SaveChangesAsync();

                // Create notification for doctor using the existing method
                await _notificationService.CreateAdmissionUpdateNotification(
                    instructions.PatientId, // Use patientId as admissionId for this purpose
                    $"New instructions from Nurse: {model.Title}",
                    currentNurse.EmployeeID,
                    NotificationPriority.Normal
                );

                _logger.LogInformation("Nurse {NurseId} contacted doctor {DoctorId} about patient {PatientId}",
                    currentNurse.EmployeeID, model.DoctorId, model.PatientId);

                TempData["SuccessMessage"] = "Doctor instructions recorded successfully! Doctor has been notified.";
                return RedirectToAction("ViewDoctorInstructions", new { patientId = model.PatientId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error recording doctor instructions for patient {PatientId}", model.PatientId);
                TempData["ErrorMessage"] = "Error recording doctor instructions. Please try again.";

                // Reload data
                model.AvailableDoctors = await _context.Employees
                    .Where(e => e.Role == UserRole.DOCTOR && e.IsActive == Status.Active)
                    .OrderBy(e => e.FirstName)
                    .ToListAsync();

                var patient = await _context.Patients.FindAsync(model.PatientId);
                model.PatientName = patient?.FullName;

                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewDoctorInstructions(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("NurseDashboard");
                }

                var instructions = await _context.DoctorInstructions
                    .Where(di => di.PatientId == patientId && di.Status == Status.Active)
                    .Include(di => di.Doctor)
                    .Include(di => di.RecordedBy)
                    .Include(di => di.CompletedBy)
                    .OrderByDescending(di => di.InstructionDate)
                    .ToListAsync();

                var viewModel = new PatientInstructionsViewModel
                {
                    PatientId = patientId,
                    PatientName = patient.FullName,
                    Instructions = instructions
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading doctor instructions for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading doctor instructions.";
                return RedirectToAction("NurseDashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> ViewInstructionDetails(int id)
        {
            try
            {
                var instruction = await _context.DoctorInstructions
                    .Include(di => di.Patient)
                    .Include(di => di.Doctor)
                    .Include(di => di.RecordedBy)
                    .Include(di => di.CompletedBy)
                    .FirstOrDefaultAsync(di => di.InstructionId == id && di.Status == Status.Active);

                if (instruction == null)
                {
                    TempData["ErrorMessage"] = "Instruction not found.";
                    return RedirectToAction("NurseDashboard");
                }

                // Verify nurse has access to this patient
                var currentNurse = await GetCurrentNurse();
                var isAssigned = await IsPatientAssignedToNurse(instruction.PatientId, currentNurse.EmployeeID);

                if (!isAssigned)
                {
                    TempData["ErrorMessage"] = "You do not have access to this patient's instructions.";
                    return RedirectToAction("NurseDashboard");
                }

                return View(instruction);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error viewing instruction details {InstructionId}", id);
                TempData["ErrorMessage"] = "Error loading instruction details.";
                return RedirectToAction("NurseDashboard");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MarkInstructionComplete(int instructionId)
        {
            try
            {
                var instruction = await _context.DoctorInstructions
                    .Include(di => di.Patient)
                    .FirstOrDefaultAsync(di => di.InstructionId == instructionId);

                if (instruction == null)
                {
                    TempData["ErrorMessage"] = "Instruction not found.";
                    return RedirectToAction("NurseDashboard");
                }

                // Verify nurse has access to this patient
                var currentNurse = await GetCurrentNurse();
                var isAssigned = await IsPatientAssignedToNurse(instruction.PatientId, currentNurse.EmployeeID);

                if (!isAssigned)
                {
                    TempData["ErrorMessage"] = "You do not have access to complete this instruction.";
                    return RedirectToAction("NurseDashboard");
                }

                // Mark as completed
                instruction.IsCompleted = true;
                instruction.CompletedDate = DateTime.Now;
                instruction.CompletedByEmployeeId = currentNurse.EmployeeID;

                _context.DoctorInstructions.Update(instruction);
                await _context.SaveChangesAsync();

                // Notify doctor about completion using existing method
                await _notificationService.CreateAdmissionUpdateNotification(
                    instruction.PatientId,
                    $"Instructions completed: {instruction.Title}",
                    currentNurse.EmployeeID,
                    NotificationPriority.Normal
                );

                TempData["SuccessMessage"] = "Instructions marked as completed! Doctor has been notified.";
                return RedirectToAction("ViewDoctorInstructions", new { patientId = instruction.PatientId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error marking instruction {InstructionId} as complete", instructionId);
                TempData["ErrorMessage"] = "Error completing instructions. Please try again.";
                return RedirectToAction("ViewDoctorInstructions", new { patientId = instructionId });
            }
        }

        #endregion

        #region Patient Management

        [HttpGet]
        public async Task<IActionResult> MyPatients()
        {
            try
            {
                var currentNurse = await GetCurrentNurse();
                if (currentNurse == null)
                    return RedirectToAction("AccessDenied", "Account");

                var assignedPatients = await GetAssignedPatients(currentNurse.EmployeeID);
                return View(assignedPatients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading assigned patients");
                TempData["ErrorMessage"] = "Error loading patients. Please try again.";
                return RedirectToAction("NurseDashboard");
            }
        }

        [HttpGet]
        public async Task<IActionResult> PatientOverview(int patientId)
        {
            try
            {
                // Verify patient exists and is assigned to current nurse
                var patient = await GetAssignedPatient(patientId);
                if (patient == null)
                {
                    TempData["ErrorMessage"] = "Patient not found or not assigned to you.";
                    return RedirectToAction("MyPatients");
                }

                // Get latest vital signs
                var latestVitals = await _context.VitalSigns
                    .Where(v => v.PatientId == patientId && v.Status == Status.Active)
                    .OrderByDescending(v => v.RecordedDateTime)
                    .FirstOrDefaultAsync();

                // Get latest doctor instructions
                var latestInstruction = await _context.DoctorInstructions
                    .Where(di => di.PatientId == patientId &&
                               di.Status == Status.Active &&
                               !di.IsCompleted)
                    .OrderByDescending(di => di.InstructionDate)
                    .FirstOrDefaultAsync();

                // Get recent treatments (last 3)
                var recentTreatments = await _context.Treatments
                    .Where(t => t.PatientId == patientId && t.Status == Status.Active)
                    .Include(t => t.Medication)
                    .OrderByDescending(t => t.TreatmentDateTime)
                    .Take(3)
                    .ToListAsync();

                ViewData["LatestVitals"] = latestVitals;
                ViewData["LatestInstruction"] = latestInstruction;
                ViewData["RecentTreatments"] = recentTreatments;

                return View(patient);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading patient overview for patient {PatientId}", patientId);
                TempData["ErrorMessage"] = "Error loading patient overview.";
                return RedirectToAction("MyPatients");
            }
        }

        #endregion

        #region Helper Methods

        private async Task<Employee?> GetCurrentNurse()
        {
            var currentUserName = User.Identity.Name;
            return await _context.Employees
                .FirstOrDefaultAsync(e => e.UserName == currentUserName &&
                                         (e.Role == UserRole.NURSE || e.Role == UserRole.NURSINGSISTER) &&
                                         e.IsActive == Status.Active);
        }

        private async Task<Patient?> GetAssignedPatient(int patientId)
        {
            var currentNurse = await GetCurrentNurse();
            if (currentNurse == null)
                return null;

            return await _context.Patients
                .Include(p => p.Admissions)
                .FirstOrDefaultAsync(p => p.PatientId == patientId &&
                                         p.GetStatus == Status.Admitted &&
                                         p.Admissions.Any(a => a.AdmissionStatus == Status.Active &&
                                                             (a.NurseID == currentNurse.EmployeeID ||
                                                              a.EmployeeID == currentNurse.EmployeeID)));
        }

        private async Task<List<Patient>> GetAssignedPatients(int nurseId)
        {
            return await _context.Patients
                .Where(p => p.GetStatus == Status.Admitted &&
                           p.Admissions.Any(a => a.AdmissionStatus == Status.Active &&
                                               (a.NurseID == nurseId || a.EmployeeID == nurseId)))
                .OrderBy(p => p.FirstName)
                .ToListAsync();
        }

        private async Task<bool> IsPatientAssignedToNurse(int patientId, int nurseId)
        {
            return await _context.Patients
                .AnyAsync(p => p.PatientId == patientId &&
                              p.GetStatus == Status.Admitted &&
                              p.Admissions.Any(a => a.AdmissionStatus == Status.Active &&
                                                  (a.NurseID == nurseId || a.EmployeeID == nurseId)));
        }

        private async Task<IActionResult> ReloadAdministerMedicationModel(AdministerMedicationViewModel model)
        {
            var currentNurse = await GetCurrentNurse();
            var isNursingSister = currentNurse?.Role == UserRole.NURSINGSISTER;

            model.AvailableMedications = await _context.Medications
                .Include(m => m.MedicationSchedules)
                .Where(m => m.MedicationStatus == Status.Active)
                .Where(m => isNursingSister ||
                           m.MedicationSchedules == null ||
                           !m.MedicationSchedules.Any() ||
                           m.MedicationSchedules.Max(ms => ms.ScheduleLevel) <= 4)
                .ToListAsync();

            var patient = await _context.Patients.FindAsync(model.PatientId);
            model.PatientName = patient?.FullName;

            return View(model);
        }

        private async Task NotifyDoctorAboutWoundInfection(int patientId, int nurseId)
        {
            try
            {
                // Get patient's assigned doctor
                var admission = await _context.Admissions
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.PatientId == patientId &&
                                             a.AdmissionStatus == Status.Active);

                if (admission?.Doctor != null)
                {
                    // Use existing notification method
                    await _notificationService.CreateAdmissionUpdateNotification(
                        admission.AdmissionId,
                        $"Patient {admission.Patient.FullName} shows signs of wound infection. Immediate review recommended.",
                        nurseId,
                        NotificationPriority.High
                    );
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error notifying doctor about wound infection for patient {PatientId}", patientId);
            }
        }


       
        #endregion
    }
}