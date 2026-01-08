using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HospitalManagement.Migrations
{
    /// <inheritdoc />
    public partial class InitialCleanSeed : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Allergies",
                columns: table => new
                {
                    AllergyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    AlleryStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Allergies", x => x.AllergyId);
                });

            migrationBuilder.CreateTable(
                name: "Conditions",
                columns: table => new
                {
                    ConditionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ConditionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Conditions", x => x.ConditionId);
                });

            migrationBuilder.CreateTable(
                name: "Consumables",
                columns: table => new
                {
                    ConsumableId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ConsumableStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUpdatedDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumables", x => x.ConsumableId);
                });

            migrationBuilder.CreateTable(
                name: "Employees",
                columns: table => new
                {
                    EmployeeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    UserName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    PasswordHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Role = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    HireDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsActive = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    EmailVerificationTokenHash = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    EmailVerificationTokenExpires = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsTwoFactorEnabled = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    TwoFactorSecretKey = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    TwoFactorRecoveryCodes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetPin = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ResetPinExpiration = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FailedLoginAttempts = table.Column<int>(type: "int", nullable: false),
                    LockoutEnd = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Employees", x => x.EmployeeID);
                });

            migrationBuilder.CreateTable(
                name: "HospitalInfos",
                columns: table => new
                {
                    HospitalInfoId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    HospitalName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    PhoneNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    WebsiteUrl = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    HospitalInfoStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HospitalInfos", x => x.HospitalInfoId);
                });

            migrationBuilder.CreateTable(
                name: "Login",
                columns: table => new
                {
                    UserNameorEmail = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Password = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RememberDevice = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    RememberMe = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false)
                },
                constraints: table =>
                {
                });

            migrationBuilder.CreateTable(
                name: "LoginAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LoginTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Success = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    IpAddress = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UserAgent = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LoginAudits", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Medications",
                columns: table => new
                {
                    MedicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Quantity = table.Column<int>(type: "int", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    MedicationStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Medications", x => x.MedicationId);
                });

            migrationBuilder.CreateTable(
                name: "Patients",
                columns: table => new
                {
                    PatientId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FirstName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    DOB = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Gender = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IdNumber = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Cellphone = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    GetStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Patients", x => x.PatientId);
                });

            migrationBuilder.CreateTable(
                name: "Wards",
                columns: table => new
                {
                    WardId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    WardName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    WardStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Capacity = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Wards", x => x.WardId);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    MessageId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Subject = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: false),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    SentDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    IsRead = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    IsDeletedBySender = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    IsDeletedByReceiver = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    IsGroupMessage = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    AttachmentUrl = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.MessageId);
                    table.ForeignKey(
                        name: "FK_Messages_Employees_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Messages_Employees_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TrustedDevices",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    DeviceId = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    DeviceName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    LastUsed = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ExpiryDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrustedDevices", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TrustedDevices_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserPreferences",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    ThemePreference = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastUpdated = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserPreferences", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserPreferences_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Admissions",
                columns: table => new
                {
                    AdmissionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AdmissionDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    EmployeeID = table.Column<int>(type: "int", nullable: false),
                    NurseID = table.Column<int>(type: "int", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    DischargeDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdmissionStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Admissions", x => x.AdmissionId);
                    table.ForeignKey(
                        name: "FK_Admissions_Employees_EmployeeID",
                        column: x => x.EmployeeID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_Employees_NurseID",
                        column: x => x.NurseID,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Admissions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Discharges",
                columns: table => new
                {
                    DischargeId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EmployeeId = table.Column<int>(type: "int", nullable: false),
                    WardAdminId = table.Column<int>(type: "int", nullable: false),
                    DischargeDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    DischargeStatus = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    PatientId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Discharges", x => x.DischargeId);
                    table.ForeignKey(
                        name: "FK_Discharges_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Discharges_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientAllergies",
                columns: table => new
                {
                    PatientAllergyId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    AllergyId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientAllergies", x => x.PatientAllergyId);
                    table.ForeignKey(
                        name: "FK_PatientAllergies_Allergies_AllergyId",
                        column: x => x.AllergyId,
                        principalTable: "Allergies",
                        principalColumn: "AllergyId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientAllergies_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientConditions",
                columns: table => new
                {
                    PatientConditionId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    ConditionId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientConditions", x => x.PatientConditionId);
                    table.ForeignKey(
                        name: "FK_PatientConditions_Conditions_ConditionId",
                        column: x => x.ConditionId,
                        principalTable: "Conditions",
                        principalColumn: "ConditionId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientConditions_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientFolders",
                columns: table => new
                {
                    FolderId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    CreatedOn = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    OpenedBy = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientFolders", x => x.FolderId);
                    table.ForeignKey(
                        name: "FK_PatientFolders_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientMedications",
                columns: table => new
                {
                    PatientMedicationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MedicationId = table.Column<int>(type: "int", nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMedications", x => x.PatientMedicationId);
                    table.ForeignKey(
                        name: "FK_PatientMedications_Medications_MedicationId",
                        column: x => x.MedicationId,
                        principalTable: "Medications",
                        principalColumn: "MedicationId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_PatientMedications_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PatientMovements",
                columns: table => new
                {
                    MovementId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    MovementType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    MovementTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    MovementStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PatientMovements", x => x.MovementId);
                    table.ForeignKey(
                        name: "FK_PatientMovements_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Beds",
                columns: table => new
                {
                    BedId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    BedNumber = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    WardId = table.Column<int>(type: "int", nullable: false),
                    BedStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Beds", x => x.BedId);
                    table.ForeignKey(
                        name: "FK_Beds_Wards_WardId",
                        column: x => x.WardId,
                        principalTable: "Wards",
                        principalColumn: "WardId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    NotificationId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Message = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SenderId = table.Column<int>(type: "int", nullable: true),
                    ReceiverId = table.Column<int>(type: "int", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    Priority = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReadDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    AdmissionId = table.Column<int>(type: "int", nullable: true),
                    PatientId = table.Column<int>(type: "int", nullable: true),
                    ActionUrl = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IsActive = table.Column<string>(type: "nvarchar(5)", maxLength: 5, nullable: false),
                    MessageId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.NotificationId);
                    table.ForeignKey(
                        name: "FK_Notifications_Admissions_AdmissionId",
                        column: x => x.AdmissionId,
                        principalTable: "Admissions",
                        principalColumn: "AdmissionId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Employees_ReceiverId",
                        column: x => x.ReceiverId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Employees_SenderId",
                        column: x => x.SenderId,
                        principalTable: "Employees",
                        principalColumn: "EmployeeID",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Notifications_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "BedAssignments",
                columns: table => new
                {
                    AssignmentId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    PatientId = table.Column<int>(type: "int", nullable: false),
                    BedId = table.Column<int>(type: "int", nullable: false),
                    AssignedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    AssignmentStatus = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BedAssignments", x => x.AssignmentId);
                    table.ForeignKey(
                        name: "FK_BedAssignments_Beds_BedId",
                        column: x => x.BedId,
                        principalTable: "Beds",
                        principalColumn: "BedId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BedAssignments_Patients_PatientId",
                        column: x => x.PatientId,
                        principalTable: "Patients",
                        principalColumn: "PatientId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Allergies",
                columns: new[] { "AllergyId", "AlleryStatus", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Active", "Allergic reaction to penicillin-based antibiotics", "Penicillin" },
                    { 2, "Active", "Skin irritation or respiratory issues from latex exposure", "Latex" },
                    { 3, "Active", "Severe anaphylactic reaction to peanuts", "Peanuts" },
                    { 4, "Active", "Sneezing, congestion, or asthma from dust exposure", "Dust Mites" },
                    { 5, "Active", "Swelling or anaphylaxis after insect stings", "Bee Stings" },
                    { 6, "Delete", "Severe allergic reaction to crustaceans and mollusks", "Shellfish" },
                    { 7, "Active", "Digestive issues or skin reactions from egg consumption", "Eggs" },
                    { 8, "Delete", "Lactose intolerance or dairy protein allergy", "Milk/Dairy" },
                    { 9, "Active", "Respiratory or skin reactions to aspirin and NSAIDs", "Aspirin" },
                    { 10, "Delete", "Anaphylactic reaction to almonds, walnuts, cashews", "Tree Nuts" },
                    { 11, "Active", "Seasonal allergic rhinitis and hay fever symptoms", "Pollen" },
                    { 12, "Delete", "Allergic reaction to iodine-based contrast agents", "Iodine" },
                    { 13, "Delete", "Skin rash or severe reactions to sulfonamide antibiotics", "Sulfa Drugs" },
                    { 14, "Active", "Contact dermatitis from nickel-containing metals", "Nickel" },
                    { 15, "Delete", "Respiratory symptoms from cat and dog allergens", "Pet Dander" }
                });

            migrationBuilder.InsertData(
                table: "Conditions",
                columns: new[] { "ConditionId", "ConditionStatus", "Description", "Name" },
                values: new object[,]
                {
                    { 1, "Active", "Chronic blood sugar condition", "Diabetes" },
                    { 2, "Active", "High blood pressure", "Hypertension" },
                    { 3, "Active", "Respiratory condition", "Asthma" },
                    { 4, "Active", "Joint inflammation", "Arthritis" },
                    { 5, "Active", "Neurological disorder", "Epilepsy" },
                    { 6, "Delete", "Cardiovascular disorders and heart conditions", "Heart Disease" },
                    { 7, "Active", "Progressive loss of kidney function", "Chronic Kidney Disease" },
                    { 8, "Delete", "Chronic obstructive pulmonary disease", "COPD" },
                    { 9, "Active", "Mental health disorder affecting mood", "Depression" },
                    { 10, "Delete", "Excessive worry and fear responses", "Anxiety Disorder" },
                    { 11, "Delete", "Brain injury due to interrupted blood supply", "Stroke" },
                    { 12, "Delete", "Malignant tumor growth and spread", "Cancer" },
                    { 13, "Delete", "Bone density loss and fracture risk", "Osteoporosis" },
                    { 14, "Active", "Severe recurring headaches with neurological symptoms", "Migraine" },
                    { 15, "Active", "Stomach acid flowing back into esophagus", "Gastroesophageal Reflux" }
                });

            migrationBuilder.InsertData(
                table: "Consumables",
                columns: new[] { "ConsumableId", "ConsumableStatus", "CreatedDate", "Description", "ExpiryDate", "LastUpdatedDate", "Name", "Quantity", "Type" },
                values: new object[,]
                {
                    { 1, "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Disposable gloves", new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Gloves", 200, "Medication" },
                    { 2, "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sterile syringes", new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Syringes", 150, "Diagnostic" },
                    { 3, "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Absorbent sheets", new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Linen Savers", 300, "Surgical" },
                    { 4, "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "IV administration sets", new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "IV Drip Set", 75, "Other" },
                    { 5, "Active", new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Surgical face masks", new DateTime(2027, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2025, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Face Masks", 500, "Diagnostic" }
                });

            migrationBuilder.InsertData(
                table: "Employees",
                columns: new[] { "EmployeeID", "Email", "EmailVerificationTokenExpires", "EmailVerificationTokenHash", "FailedLoginAttempts", "FirstName", "Gender", "HireDate", "IsActive", "IsTwoFactorEnabled", "LastName", "LockoutEnd", "PasswordHash", "ResetPin", "ResetPinExpiration", "Role", "TwoFactorRecoveryCodes", "TwoFactorSecretKey", "UserName" },
                values: new object[,]
                {
                    { 1, "admin@example.com", null, null, 0, "Admin", "Male", null, "Active", "False", "User", null, "Password123", null, null, "ADMINISTRATOR", null, null, "adminuser" },
                    { 2, "nurse1@example.com", null, null, 0, "Nina", "Male", null, "Active", "False", "Nurse", null, "Password123", null, null, "NURSE", null, null, "nurse1" },
                    { 3, "doctor1@example.com", null, null, 0, "David", "Male", null, "Active", "False", "Doctor", null, "Password123", null, null, "DOCTOR", null, null, "doctor1" },
                    { 4, "scriptmgr@example.com", null, null, 0, "Sara", "Male", null, "Active", "False", "ScriptManager", null, "Password123", null, null, "SCRIPTMANAGER", null, null, "scriptmgr" },
                    { 5, "consumablemgr@example.com", null, null, 0, "Chris", "Male", null, "Active", "False", "ConsumableManager", null, "Password123", null, null, "CONSUMABLESMANAGER", null, null, "consumablemgr" },
                    { 6, "wardadmin@example.com", null, null, 0, "Wanda", "Male", null, "Active", "False", "WardAdmin", null, "Password123", null, null, "WARDADMIN", null, null, "wardadmin" },
                    { 7, "nursingsister@example.com", null, null, 0, "Nancy", "Male", null, "Active", "False", "NursingSister", null, "Password123", null, null, "NURSINGSISTER", null, null, "nursingsister" }
                });

            migrationBuilder.InsertData(
                table: "Medications",
                columns: new[] { "MedicationId", "Description", "ExpiryDate", "MedicationStatus", "Name", "Quantity", "Type" },
                values: new object[,]
                {
                    { 1, "Pain relief and fever reducer", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Paracetamol", 1000, "Prescription" },
                    { 2, "Anti-inflammatory", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Ibuprofen", 800, "Other" },
                    { 3, "Strong painkiller (controlled)", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Morphine", 100, "Supplement" },
                    { 4, "Antibiotic", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Amoxicillin", 500, "Prescription" },
                    { 5, "Used for anxiety and seizures", new DateTime(2026, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Diazepam", 300, "OverTheCounter" },
                    { 6, "Broad-spectrum antibiotic", new DateTime(2027, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Ciprofloxacin", 400, "Prescription" },
                    { 7, "Allergy relief", new DateTime(2026, 10, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Loratadine", 600, "OverTheCounter" },
                    { 8, "Used to treat type 2 diabetes", new DateTime(2027, 3, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Metformin", 700, "Prescription" },
                    { 9, "Used to treat acid reflux", new DateTime(2027, 7, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Omeprazole", 450, "Prescription" },
                    { 10, "Bronchodilator for asthma", new DateTime(2026, 12, 31, 0, 0, 0, 0, DateTimeKind.Unspecified), "Active", "Salbutamol", 350, "Prescription" }
                });

            migrationBuilder.InsertData(
                table: "Patients",
                columns: new[] { "PatientId", "Cellphone", "DOB", "FirstName", "Gender", "GetStatus", "IdNumber", "LastName" },
                values: new object[,]
                {
                    { 1, "0812345678", new DateTime(2000, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Test", "Male", "Active", "0000000000000", "Patient" },
                    { 2, "0823456789", new DateTime(1995, 5, 20, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lerato", "Female", "Active", "9505201234087", "Mokoena" },
                    { 3, "0834567890", new DateTime(1988, 11, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sizwe", "Male", "Active", "8811105674085", "Dlamini" },
                    { 4, "0845678901", new DateTime(2002, 3, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), "Thandi", "Female", "Active", "0203157890083", "Ngubane" },
                    { 5, "0856789012", new DateTime(1990, 7, 25, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nomsa", "Female", "Delete", "9007251234089", "Mthembu" },
                    { 6, "0867890123", new DateTime(1985, 12, 5, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mandla", "Male", "Delete", "8512055678091", "Khumalo" },
                    { 7, "0878901234", new DateTime(1998, 9, 18, 0, 0, 0, 0, DateTimeKind.Unspecified), "Precious", "Female", "Delete", "9809187890085", "Sithole" },
                    { 8, "0889012345", new DateTime(1992, 4, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bongani", "Male", "Delete", "9204125674087", "Ndlovu" },
                    { 9, "0890123456", new DateTime(1987, 8, 30, 0, 0, 0, 0, DateTimeKind.Unspecified), "Zanele", "Female", "Delete", "8708301234083", "Mahlangu" },
                    { 10, "0801234567", new DateTime(1993, 1, 22, 0, 0, 0, 0, DateTimeKind.Unspecified), "Sipho", "Male", "Delete", "9301225678089", "Radebe" },
                    { 11, "0812345679", new DateTime(1996, 6, 8, 0, 0, 0, 0, DateTimeKind.Unspecified), "Lindiwe", "Female", "Active", "9606087890081", "Cele" },
                    { 12, "0823456780", new DateTime(1989, 10, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), "Themba", "Male", "Active", "8910145674085", "Zulu" },
                    { 13, "0834567891", new DateTime(2001, 2, 28, 0, 0, 0, 0, DateTimeKind.Unspecified), "Nonhlanhla", "Female", "Delete", "0102281234087", "Maseko" },
                    { 14, "0845678902", new DateTime(1994, 11, 3, 0, 0, 0, 0, DateTimeKind.Unspecified), "Mthunzi", "Male", "Delete", "9411035678083", "Shabalala" }
                });

            migrationBuilder.InsertData(
                table: "Wards",
                columns: new[] { "WardId", "Capacity", "Description", "WardName", "WardStatus" },
                values: new object[,]
                {
                    { 1, 10, "Basic treatment ward", "General Ward", "Active" },
                    { 2, 5, "Intensive care unit", "ICU", "Active" },
                    { 3, 8, "For surgical patients", "Surgical Ward", "Active" },
                    { 4, 12, "Children and adolescent care", "Pediatric Ward", "Active" },
                    { 5, 15, "Maternity and newborn care", "Maternity Ward", "Active" },
                    { 6, 8, "Heart and cardiovascular patients", "Cardiology Ward", "Delete" },
                    { 7, 10, "Bone and joint surgery patients", "Orthopedic Ward", "Active" },
                    { 8, 6, "Cancer treatment and care", "Oncology Ward", "Delete" },
                    { 9, 20, "Emergency and trauma patients", "Emergency Ward", "Active" },
                    { 10, 8, "Brain and nervous system disorders", "Neurology Ward", "Delete" },
                    { 11, 12, "Mental health and psychiatric care", "Psychiatric Ward", "Active" },
                    { 12, 14, "Elderly patient care", "Geriatric Ward", "Delete" },
                    { 13, 10, "Physical therapy and recovery", "Rehabilitation Ward", "Delete" }
                });

            migrationBuilder.InsertData(
                table: "Beds",
                columns: new[] { "BedId", "BedNumber", "BedStatus", "Status", "WardId" },
                values: new object[,]
                {
                    { 1, "G1", "Active", "Available", 1 },
                    { 2, "G2", "Active", "Available", 1 },
                    { 3, "ICU1", "Active", "Available", 2 },
                    { 4, "S1", "Active", "Available", 3 },
                    { 5, "S2", "Active", "Available", 3 },
                    { 6, "G3", "Delete", "Available", 1 },
                    { 7, "ICU2", "Delete", "Available", 2 },
                    { 8, "ICU3", "Active", "Available", 2 },
                    { 9, "S3", "Delete", "Available", 3 },
                    { 10, "P1", "Active", "Available", 4 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_EmployeeID",
                table: "Admissions",
                column: "EmployeeID");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_NurseID",
                table: "Admissions",
                column: "NurseID");

            migrationBuilder.CreateIndex(
                name: "IX_Admissions_PatientId",
                table: "Admissions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_BedAssignments_BedId",
                table: "BedAssignments",
                column: "BedId");

            migrationBuilder.CreateIndex(
                name: "IX_BedAssignments_PatientId",
                table: "BedAssignments",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Beds_WardId",
                table: "Beds",
                column: "WardId");

            migrationBuilder.CreateIndex(
                name: "IX_Discharges_EmployeeId",
                table: "Discharges",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_Discharges_PatientId",
                table: "Discharges",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId",
                table: "Messages",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId_IsDeletedByReceiver_ReadDate",
                table: "Messages",
                columns: new[] { "ReceiverId", "IsDeletedByReceiver", "ReadDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_ReceiverId_IsRead_SentDate",
                table: "Messages",
                columns: new[] { "ReceiverId", "IsRead", "SentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId",
                table: "Messages",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId_IsDeletedBySender",
                table: "Messages",
                columns: new[] { "SenderId", "IsDeletedBySender" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SenderId_SentDate",
                table: "Messages",
                columns: new[] { "SenderId", "SentDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Messages_SentDate",
                table: "Messages",
                column: "SentDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AdmissionId",
                table: "Notifications",
                column: "AdmissionId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedDate",
                table: "Notifications",
                column: "CreatedDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_PatientId",
                table: "Notifications",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReadDate",
                table: "Notifications",
                column: "ReadDate");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReceiverId",
                table: "Notifications",
                column: "ReceiverId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_ReceiverId_IsActive_ReadDate",
                table: "Notifications",
                columns: new[] { "ReceiverId", "IsActive", "ReadDate" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_SenderId",
                table: "Notifications",
                column: "SenderId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergies_AllergyId",
                table: "PatientAllergies",
                column: "AllergyId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientAllergies_PatientId",
                table: "PatientAllergies",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConditions_ConditionId",
                table: "PatientConditions",
                column: "ConditionId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientConditions_PatientId",
                table: "PatientConditions",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientFolders_PatientId",
                table: "PatientFolders",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedications_MedicationId",
                table: "PatientMedications",
                column: "MedicationId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMedications_PatientId",
                table: "PatientMedications",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_PatientMovements_PatientId",
                table: "PatientMovements",
                column: "PatientId");

            migrationBuilder.CreateIndex(
                name: "IX_TrustedDevices_EmployeeId",
                table: "TrustedDevices",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_UserPreferences_EmployeeId",
                table: "UserPreferences",
                column: "EmployeeId",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "BedAssignments");

            migrationBuilder.DropTable(
                name: "Consumables");

            migrationBuilder.DropTable(
                name: "Discharges");

            migrationBuilder.DropTable(
                name: "HospitalInfos");

            migrationBuilder.DropTable(
                name: "Login");

            migrationBuilder.DropTable(
                name: "LoginAudits");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "PatientAllergies");

            migrationBuilder.DropTable(
                name: "PatientConditions");

            migrationBuilder.DropTable(
                name: "PatientFolders");

            migrationBuilder.DropTable(
                name: "PatientMedications");

            migrationBuilder.DropTable(
                name: "PatientMovements");

            migrationBuilder.DropTable(
                name: "TrustedDevices");

            migrationBuilder.DropTable(
                name: "UserPreferences");

            migrationBuilder.DropTable(
                name: "Beds");

            migrationBuilder.DropTable(
                name: "Admissions");

            migrationBuilder.DropTable(
                name: "Allergies");

            migrationBuilder.DropTable(
                name: "Conditions");

            migrationBuilder.DropTable(
                name: "Medications");

            migrationBuilder.DropTable(
                name: "Wards");

            migrationBuilder.DropTable(
                name: "Employees");

            migrationBuilder.DropTable(
                name: "Patients");
        }
    }
}
