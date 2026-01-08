using HospitalManagement.AppStatus;
using HospitalManagement.Models;
using HospitalManagement.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System.Reflection.Emit;

namespace HospitalManagement.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // All your existing DbSet properties here...
        public DbSet<Ward> Wards { get; set; }
        public DbSet<Bed> Beds { get; set; }
        public DbSet<Consumable> Consumables { get; set; }
        public DbSet<Medication> Medications { get; set; }
        public DbSet<Allergy> Allergies { get; set; }
        public DbSet<Condition> Conditions { get; set; }
        public DbSet<HospitalInfo> HospitalInfos { get; set; }
        public DbSet<Employee> Employees { get; set; }
        public DbSet<Patient> Patients { get; set; }
        public DbSet<Admission> Admissions { get; set; }
        public DbSet<PatientAllergy> PatientAllergies { get; set; }
        public DbSet<PatientMedication> PatientMedications { get; set; }
        public DbSet<PatientCondition> PatientConditions { get; set; }
        public DbSet<PatientFolder> PatientFolders { get; set; }
        public DbSet<BedAssignment> BedAssignments { get; set; }
        public DbSet<PatientMovement> PatientMovements { get; set; }
        public DbSet<Message> Messages { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Discharge> Discharges { get; set; }
        public DbSet<TrustedDevice> TrustedDevices { get; set; }
        public DbSet<LoginAudit> LoginAudits { get; set; }
        public DbSet<UserPreference> UserPreferences { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Login>().HasNoKey();

            // Enum to string conversions
            foreach (var entityType in modelBuilder.Model.GetEntityTypes())
            {
                foreach (var property in entityType.GetProperties())
                {
                    var clrType = property.ClrType;

                    // Convert enums to strings
                    if (clrType.IsEnum)
                    {
                        var converterType = typeof(EnumToStringConverter<>).MakeGenericType(clrType);
                        var converter = (ValueConverter)Activator.CreateInstance(converterType)!;
                        property.SetValueConverter(converter);
                    }

                    // Convert bools to strings
                    if (clrType == typeof(bool))
                    {
                        var boolConverter = new ValueConverter<bool, string>(
                            v => v.ToString(),      // bool → string
                            v => bool.Parse(v)      // string → bool
                        );
                        property.SetValueConverter(boolConverter);
                        property.SetMaxLength(5); // "true" or "false"
                    }
                }
            }


            modelBuilder.Entity<Allergy>().HasQueryFilter(a => a.AlleryStatus == Status.Active);
            modelBuilder.Entity<Ward>().HasQueryFilter(a => a.WardStatus == Status.Active);
            modelBuilder.Entity<Bed>().HasQueryFilter(a => a.BedStatus == Status.Active);
            modelBuilder.Entity<Medication>().HasQueryFilter(a => a.MedicationStatus == Status.Active);
            modelBuilder.Entity<Condition>().HasQueryFilter(a => a.ConditionStatus == Status.Active);
            modelBuilder.Entity<Consumable>().HasQueryFilter(a => a.ConsumableStatus == Status.Active);
            modelBuilder.Entity<Employee>().HasQueryFilter(a => a.IsActive == Status.Active);
            modelBuilder.Entity<HospitalInfo>().HasQueryFilter(h => h.HospitalInfoStatus == Status.Active);
            modelBuilder.Entity<Admission>().HasQueryFilter(h => h.AdmissionStatus == Status.Active);
            modelBuilder.Entity<PatientAllergy>().HasQueryFilter(h => h.Status == Status.Active);
            modelBuilder.Entity<PatientMedication>().HasQueryFilter(h => h.Status == Status.Active);
            modelBuilder.Entity<PatientCondition>().HasQueryFilter(h => h.Status == Status.Active);
            modelBuilder.Entity<PatientFolder>().HasQueryFilter(h => h.Status == Status.Active);
            modelBuilder.Entity<Patient>().HasQueryFilter(h => h.GetStatus == Status.Active);
            modelBuilder.Entity<BedAssignment>().HasQueryFilter(h => h.AssignmentStatus == Status.Active);
            modelBuilder.Entity<PatientMovement>().HasQueryFilter(h => h.MovementStatus == Status.Active);
            modelBuilder.Entity<Discharge>().HasQueryFilter(h => h.DischargeStatus == Status.Active);






            modelBuilder.Entity<Allergy>().HasData(
      new Allergy { AllergyId = 1, Name = "Penicillin", Description = "Allergic reaction to penicillin-based antibiotics", AlleryStatus = Status.Active },
      new Allergy { AllergyId = 2, Name = "Latex", Description = "Skin irritation or respiratory issues from latex exposure", AlleryStatus = Status.Active },
      new Allergy { AllergyId = 3, Name = "Peanuts", Description = "Severe anaphylactic reaction to peanuts", AlleryStatus = Status.Active },
      new Allergy { AllergyId = 4, Name = "Dust Mites", Description = "Sneezing, congestion, or asthma from dust exposure", AlleryStatus = Status.Active },
      new Allergy { AllergyId = 5, Name = "Bee Stings", Description = "Swelling or anaphylaxis after insect stings", AlleryStatus = Status.Active },


      new Allergy { AllergyId = 6, Name = "Shellfish", Description = "Severe allergic reaction to crustaceans and mollusks", AlleryStatus = Status.Delete },
      new Allergy { AllergyId = 7, Name = "Eggs", Description = "Digestive issues or skin reactions from egg consumption", AlleryStatus = Status.Active },
      new Allergy { AllergyId = 8, Name = "Milk/Dairy", Description = "Lactose intolerance or dairy protein allergy", AlleryStatus = Status.Delete },
      new Allergy { AllergyId = 9, Name = "Aspirin", Description = "Respiratory or skin reactions to aspirin and NSAIDs", AlleryStatus = Status.Active },
      new Allergy { AllergyId = 10, Name = "Tree Nuts", Description = "Anaphylactic reaction to almonds, walnuts, cashews", AlleryStatus = Status.Delete },
      new Allergy { AllergyId = 11, Name = "Pollen", Description = "Seasonal allergic rhinitis and hay fever symptoms", AlleryStatus = Status.Active },
      new Allergy { AllergyId = 12, Name = "Iodine", Description = "Allergic reaction to iodine-based contrast agents", AlleryStatus = Status.Delete },
      new Allergy { AllergyId = 13, Name = "Sulfa Drugs", Description = "Skin rash or severe reactions to sulfonamide antibiotics", AlleryStatus = Status.Delete },
      new Allergy { AllergyId = 14, Name = "Nickel", Description = "Contact dermatitis from nickel-containing metals", AlleryStatus = Status.Active },
      new Allergy { AllergyId = 15, Name = "Pet Dander", Description = "Respiratory symptoms from cat and dog allergens", AlleryStatus = Status.Delete }
);







            modelBuilder.Entity<Ward>().HasData(
    new Ward { WardId = 1, WardName = "General Ward", Description = "Basic treatment ward", Capacity = 10, WardStatus = Status.Active },
    new Ward { WardId = 2, WardName = "ICU", Description = "Intensive care unit", Capacity = 5, WardStatus = Status.Active },
    new Ward { WardId = 3, WardName = "Surgical Ward", Description = "For surgical patients", Capacity = 8, WardStatus = Status.Active },
        new Ward { WardId = 4, WardName = "Pediatric Ward", Description = "Children and adolescent care", Capacity = 12, WardStatus = Status.Active },
        new Ward { WardId = 5, WardName = "Maternity Ward", Description = "Maternity and newborn care", Capacity = 15, WardStatus = Status.Active },
        new Ward { WardId = 6, WardName = "Cardiology Ward", Description = "Heart and cardiovascular patients", Capacity = 8, WardStatus = Status.Delete },
        new Ward { WardId = 7, WardName = "Orthopedic Ward", Description = "Bone and joint surgery patients", Capacity = 10, WardStatus = Status.Active },
        new Ward { WardId = 8, WardName = "Oncology Ward", Description = "Cancer treatment and care", Capacity = 6, WardStatus = Status.Delete },
        new Ward { WardId = 9, WardName = "Emergency Ward", Description = "Emergency and trauma patients", Capacity = 20, WardStatus = Status.Active },
        new Ward { WardId = 10, WardName = "Neurology Ward", Description = "Brain and nervous system disorders", Capacity = 8, WardStatus = Status.Delete },
        new Ward { WardId = 11, WardName = "Psychiatric Ward", Description = "Mental health and psychiatric care", Capacity = 12, WardStatus = Status.Active },
        new Ward { WardId = 12, WardName = "Geriatric Ward", Description = "Elderly patient care", Capacity = 14, WardStatus = Status.Delete },
        new Ward { WardId = 13, WardName = "Rehabilitation Ward", Description = "Physical therapy and recovery", Capacity = 10, WardStatus = Status.Delete }
);



            modelBuilder.Entity<Bed>().HasData(
new Bed { BedId = 1, BedNumber = "G1", Status = BedStatus.Available, WardId = 1, BedStatus = Status.Active },
new Bed { BedId = 2, BedNumber = "G2", Status = BedStatus.Available, WardId = 1, BedStatus = Status.Active },
new Bed { BedId = 3, BedNumber = "ICU1", Status = BedStatus.Available, WardId = 2, BedStatus = Status.Active },
new Bed { BedId = 4, BedNumber = "S1", Status = BedStatus.Available, WardId = 3, BedStatus = Status.Active },
new Bed { BedId = 5, BedNumber = "S2", Status = BedStatus.Available, WardId = 3, BedStatus = Status.Active },

// Add these 5 additional beds to your existing seed data

new Bed { BedId = 6, BedNumber = "G3", Status = BedStatus.Available, WardId = 1, BedStatus = Status.Delete },
new Bed { BedId = 7, BedNumber = "ICU2", Status = BedStatus.Available, WardId = 2, BedStatus = Status.Delete },
new Bed { BedId = 8, BedNumber = "ICU3", Status = BedStatus.Available, WardId = 2, BedStatus = Status.Active },
new Bed { BedId = 9, BedNumber = "S3", Status = BedStatus.Available, WardId = 3, BedStatus = Status.Delete },
new Bed { BedId = 10, BedNumber = "P1", Status = BedStatus.Available, WardId = 4, BedStatus = Status.Active }
);


            modelBuilder.Entity<Condition>().HasData(
new Condition { ConditionId = 1, Name = "Diabetes", Description = "Chronic blood sugar condition", ConditionStatus = Status.Active },
new Condition { ConditionId = 2, Name = "Hypertension", Description = "High blood pressure", ConditionStatus = Status.Active },
new Condition { ConditionId = 3, Name = "Asthma", Description = "Respiratory condition", ConditionStatus = Status.Active },
new Condition { ConditionId = 4, Name = "Arthritis", Description = "Joint inflammation", ConditionStatus = Status.Active },
new Condition { ConditionId = 5, Name = "Epilepsy", Description = "Neurological disorder", ConditionStatus = Status.Active },


new Condition { ConditionId = 6, Name = "Heart Disease", Description = "Cardiovascular disorders and heart conditions", ConditionStatus = Status.Delete },
new Condition { ConditionId = 7, Name = "Chronic Kidney Disease", Description = "Progressive loss of kidney function", ConditionStatus = Status.Active },
new Condition { ConditionId = 8, Name = "COPD", Description = "Chronic obstructive pulmonary disease", ConditionStatus = Status.Delete },
new Condition { ConditionId = 9, Name = "Depression", Description = "Mental health disorder affecting mood", ConditionStatus = Status.Active },
new Condition { ConditionId = 10, Name = "Anxiety Disorder", Description = "Excessive worry and fear responses", ConditionStatus = Status.Delete },
new Condition { ConditionId = 11, Name = "Stroke", Description = "Brain injury due to interrupted blood supply", ConditionStatus = Status.Delete },
new Condition { ConditionId = 12, Name = "Cancer", Description = "Malignant tumor growth and spread", ConditionStatus = Status.Delete },
new Condition { ConditionId = 13, Name = "Osteoporosis", Description = "Bone density loss and fracture risk", ConditionStatus = Status.Delete },
new Condition { ConditionId = 14, Name = "Migraine", Description = "Severe recurring headaches with neurological symptoms", ConditionStatus = Status.Active },
new Condition { ConditionId = 15, Name = "Gastroesophageal Reflux", Description = "Stomach acid flowing back into esophagus", ConditionStatus = Status.Active }

);




            modelBuilder.Entity<Consumable>().HasData(
   new Consumable
   {
       ConsumableId = 1,
       Name = "Gloves",
       Type = ConsumableType.Medication,
       Quantity = 200,
       ExpiryDate = new DateTime(2027, 01, 01),
       ConsumableStatus = Status.Active,
       Description = "Disposable gloves",
       CreatedDate = new DateTime(2025, 01, 01),
       LastUpdatedDate = new DateTime(2025, 01, 01)

   },
   new Consumable
   {
       ConsumableId = 2,
       Name = "Syringes",
       Type = ConsumableType.Diagnostic,
       Quantity = 150,
       ExpiryDate = new DateTime(2027, 01, 01),
       ConsumableStatus = Status.Active,
       Description = "Sterile syringes",
       CreatedDate = new DateTime(2025, 01, 01),
       LastUpdatedDate = new DateTime(2025, 01, 01)

   },
   new Consumable
   {
       ConsumableId = 3,
       Name = "Linen Savers",
       Type = ConsumableType.Surgical,
       Quantity = 300,
       ExpiryDate = new DateTime(2027, 01, 01),
       ConsumableStatus = Status.Active,
       Description = "Absorbent sheets",
       CreatedDate = new DateTime(2025, 01, 01),
       LastUpdatedDate = new DateTime(2025, 01, 01)

   },
   new Consumable
   {
       ConsumableId = 4,
       Name = "IV Drip Set",
       Type = ConsumableType.Other,
       Quantity = 75,
       ExpiryDate = new DateTime(2027, 01, 01),
       ConsumableStatus = Status.Active,
       Description = "IV administration sets",
       CreatedDate = new DateTime(2025, 01, 01),
       LastUpdatedDate = new DateTime(2025, 01, 01)

   },
   new Consumable
   {
       ConsumableId = 5,
       Name = "Face Masks",
       Type = ConsumableType.Diagnostic,
       Quantity = 500,
       ExpiryDate = new DateTime(2027, 01, 01),
       ConsumableStatus = Status.Active,
       Description = "Surgical face masks",
       CreatedDate = new DateTime(2025, 01, 01),
       LastUpdatedDate = new DateTime(2025, 01, 01)

   }
);





















            modelBuilder.Entity<Medication>().HasData(
    new Medication
    {
        MedicationId = 1,
        Name = "Paracetamol",
        Type = MedicationType.Prescription,
        Quantity = 1000,
        ExpiryDate = new DateTime(2026, 01, 01),
        MedicationStatus = Status.Active,
        Description = "Pain relief and fever reducer"
    },
    new Medication
    {
        MedicationId = 2,
        Name = "Ibuprofen",
        Type = MedicationType.Other,
        Quantity = 800,
        ExpiryDate = new DateTime(2026, 01, 01),
        MedicationStatus = Status.Active,
        Description = "Anti-inflammatory"
    },
    new Medication
    {
        MedicationId = 3,
        Name = "Morphine",
        Type = MedicationType.Supplement,
        Quantity = 100,
        ExpiryDate = new DateTime(2026, 01, 01),
        MedicationStatus = Status.Active,
        Description = "Strong painkiller (controlled)"
    },
    new Medication
    {
        MedicationId = 4,
        Name = "Amoxicillin",
        Type = MedicationType.Prescription,
        Quantity = 500,
        ExpiryDate = new DateTime(2026, 01, 01),
        MedicationStatus = Status.Active,
        Description = "Antibiotic"
    },
    new Medication
    {
        MedicationId = 5,
        Name = "Diazepam",
        Type = MedicationType.OverTheCounter,
        Quantity = 300,
        ExpiryDate = new DateTime(2026, 01, 01),
        MedicationStatus = Status.Active,
        Description = "Used for anxiety and seizures"
    },


     new Medication
     {
         MedicationId = 6,
         Name = "Ciprofloxacin",
         Type = MedicationType.Prescription,
         Quantity = 400,
         ExpiryDate = new DateTime(2027, 05, 01),
         MedicationStatus = Status.Active,
         Description = "Broad-spectrum antibiotic"
     },
    new Medication
    {
        MedicationId = 7,
        Name = "Loratadine",
        Type = MedicationType.OverTheCounter,
        Quantity = 600,
        ExpiryDate = new DateTime(2026, 10, 15),
        MedicationStatus = Status.Active,
        Description = "Allergy relief"
    },
    new Medication
    {
        MedicationId = 8,
        Name = "Metformin",
        Type = MedicationType.Prescription,
        Quantity = 700,
        ExpiryDate = new DateTime(2027, 03, 30),
        MedicationStatus = Status.Active,
        Description = "Used to treat type 2 diabetes"
    },
    new Medication
    {
        MedicationId = 9,
        Name = "Omeprazole",
        Type = MedicationType.Prescription,
        Quantity = 450,
        ExpiryDate = new DateTime(2027, 07, 20),
        MedicationStatus = Status.Active,
        Description = "Used to treat acid reflux"
    },
    new Medication
    {
        MedicationId = 10,
        Name = "Salbutamol",
        Type = MedicationType.Prescription,
        Quantity = 350,
        ExpiryDate = new DateTime(2026, 12, 31),
        MedicationStatus = Status.Active,
        Description = "Bronchodilator for asthma"
    }
);



            modelBuilder.Entity<Notification>(entity =>
            {
                // Admission relationship (optional)
                entity.HasOne(n => n.Admission)
                    .WithMany()
                    .HasForeignKey(n => n.AdmissionId)
                    .OnDelete(DeleteBehavior.Restrict) // Or SetNull if you want
                    .IsRequired(false); // Make it optional

                // Patient relationship (optional)
                entity.HasOne(n => n.Patient)
                    .WithMany()
                    .HasForeignKey(n => n.PatientId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Sender relationship
                entity.HasOne(n => n.Sender)
                    .WithMany()
                    .HasForeignKey(n => n.SenderId)
                    .OnDelete(DeleteBehavior.Restrict)
                    .IsRequired(false);

                // Receiver relationship
                entity.HasOne(n => n.Receiver)
                    .WithMany()
                    .HasForeignKey(n => n.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);
            });




            modelBuilder.Entity<Patient>().HasData(
 new Patient
 {
     PatientId = 1,
     FirstName = "Test",
     LastName = "Patient",
     DOB = new DateTime(2000, 1, 1),
     Gender = GenderType.Male,
     IdNumber = "0000000000000",
     Cellphone = "0812345678",
     GetStatus = Status.Active
 },
 new Patient
 {
     PatientId = 2,
     FirstName = "Lerato",
     LastName = "Mokoena",
     DOB = new DateTime(1995, 5, 20),
     Gender = GenderType.Female,
     IdNumber = "9505201234087",
     Cellphone = "0823456789",
     GetStatus = Status.Active
 },
 new Patient
 {
     PatientId = 3,
     FirstName = "Sizwe",
     LastName = "Dlamini",
     DOB = new DateTime(1988, 11, 10),
     Gender = GenderType.Male,
     IdNumber = "8811105674085",
     Cellphone = "0834567890",
     GetStatus = Status.Active
 },
 new Patient
 {
     PatientId = 4,
     FirstName = "Thandi",
     LastName = "Ngubane",
     DOB = new DateTime(2002, 3, 15),
     Gender = GenderType.Female,
     IdNumber = "0203157890083",
     Cellphone = "0845678901",
     GetStatus = Status.Active
 },



 new Patient
 {
     PatientId = 5,
     FirstName = "Nomsa",
     LastName = "Mthembu",
     DOB = new DateTime(1990, 7, 25),
     Gender = GenderType.Female,
     IdNumber = "9007251234089",
     Cellphone = "0856789012",
     GetStatus = Status.Delete
 },
new Patient
{
 PatientId = 6,
 FirstName = "Mandla",
 LastName = "Khumalo",
 DOB = new DateTime(1985, 12, 5),
 Gender = GenderType.Male,
 IdNumber = "8512055678091",
 Cellphone = "0867890123",
 GetStatus = Status.Delete
},
new Patient
{
 PatientId = 7,
 FirstName = "Precious",
 LastName = "Sithole",
 DOB = new DateTime(1998, 9, 18),
 Gender = GenderType.Female,
 IdNumber = "9809187890085",
 Cellphone = "0878901234",
 GetStatus = Status.Delete
},
new Patient
{
 PatientId = 8,
 FirstName = "Bongani",
 LastName = "Ndlovu",
 DOB = new DateTime(1992, 4, 12),
 Gender = GenderType.Male,
 IdNumber = "9204125674087",
 Cellphone = "0889012345",
 GetStatus = Status.Delete
},
new Patient
{
 PatientId = 9,
 FirstName = "Zanele",
 LastName = "Mahlangu",
 DOB = new DateTime(1987, 8, 30),
 Gender = GenderType.Female,
 IdNumber = "8708301234083",
 Cellphone = "0890123456",
 GetStatus = Status.Delete
},
new Patient
{
 PatientId = 10,
 FirstName = "Sipho",
 LastName = "Radebe",
 DOB = new DateTime(1993, 1, 22),
 Gender = GenderType.Male,
 IdNumber = "9301225678089",
 Cellphone = "0801234567",
 GetStatus = Status.Delete
},
new Patient
{
 PatientId = 11,
 FirstName = "Lindiwe",
 LastName = "Cele",
 DOB = new DateTime(1996, 6, 8),
 Gender = GenderType.Female,
 IdNumber = "9606087890081",
 Cellphone = "0812345679",
 GetStatus = Status.Active
},
new Patient
{
 PatientId = 12,
 FirstName = "Themba",
 LastName = "Zulu",
 DOB = new DateTime(1989, 10, 14),
 Gender = GenderType.Male,
 IdNumber = "8910145674085",
 Cellphone = "0823456780",
 GetStatus = Status.Active
},
new Patient
{
 PatientId = 13,
 FirstName = "Nonhlanhla",
 LastName = "Maseko",
 DOB = new DateTime(2001, 2, 28),
 Gender = GenderType.Female,
 IdNumber = "0102281234087",
 Cellphone = "0834567891",
 GetStatus = Status.Delete
},
new Patient
{
 PatientId = 14,
 FirstName = "Mthunzi",
 LastName = "Shabalala",
 DOB = new DateTime(1994, 11, 3),
 Gender = GenderType.Male,
 IdNumber = "9411035678083",
 Cellphone = "0845678902",
 GetStatus = Status.Delete
}


);




            modelBuilder.Entity<Admission>()
       .HasOne(a => a.Doctor)
       .WithMany() // no navigation back
       .HasForeignKey(a => a.EmployeeID)
       .OnDelete(DeleteBehavior.Restrict); // 👈 prevent cascade

            // Nurse relationship
            modelBuilder.Entity<Admission>()
                .HasOne(a => a.Nurse)
                .WithMany() // no navigation back
                .HasForeignKey(a => a.NurseID)
                .OnDelete(DeleteBehavior.Restrict);



            modelBuilder.Entity<Message>()
                   .HasOne(m => m.Sender)
                   .WithMany(e => e.SentMessages)
                   .HasForeignKey(m => m.SenderId)
                   .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(e => e.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete

            // Optional: Add indexes for better performance
            modelBuilder.Entity<Message>()
                .HasIndex(m => m.SenderId);

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.ReceiverId);

            modelBuilder.Entity<Message>()
                .HasIndex(m => m.SentDate);

            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ReceiverId, m.IsDeletedByReceiver, m.ReadDate });

            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.SenderId, m.IsDeletedBySender });




            modelBuilder.Entity<Notification>(entity =>
            {
                // Relationships
                entity.HasOne(n => n.Sender)
                    .WithMany()
                    .HasForeignKey(n => n.SenderId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Receiver)
                    .WithMany()
                    .HasForeignKey(n => n.ReceiverId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Admission)
                    .WithMany()
                    .HasForeignKey(n => n.AdmissionId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(n => n.Patient)
                    .WithMany()
                    .HasForeignKey(n => n.PatientId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ✅ FIX: Ignore computed properties
                entity.Ignore(n => n.IsRead);

                // Indexes
                entity.HasIndex(n => n.ReceiverId);
                entity.HasIndex(n => new { n.ReceiverId, n.IsActive, n.ReadDate }); // Use ReadDate instead of IsRead
                entity.HasIndex(n => n.CreatedDate);
                entity.HasIndex(n => n.ReadDate); // Add index for ReadDate
            });














            // User Preferences relationship
            modelBuilder.Entity<Employee>()
          .HasOne(e => e.Preferences)
          .WithOne(p => p.Employee)
          .HasForeignKey<UserPreference>(p => p.EmployeeId)
          .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserPreference>()
                .HasIndex(p => p.EmployeeId)
                .IsUnique();



            modelBuilder.Entity<Message>()
      .HasOne(m => m.Sender)
      .WithMany(e => e.SentMessages)
      .HasForeignKey(m => m.SenderId)
      .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete for sender

            modelBuilder.Entity<Message>()
                .HasOne(m => m.Receiver)
                .WithMany(e => e.ReceivedMessages)
                .HasForeignKey(m => m.ReceiverId)
                .OnDelete(DeleteBehavior.Restrict); // Prevent cascade delete for receiver

            // Mark SenderId and ReceiverId as required
            modelBuilder.Entity<Message>()
                .Property(m => m.SenderId)
                .IsRequired();

            modelBuilder.Entity<Message>()
                .Property(m => m.ReceiverId)
                .IsRequired();

            // Add index for better query performance
            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.ReceiverId, m.IsRead, m.SentDate });

            modelBuilder.Entity<Message>()
                .HasIndex(m => new { m.SenderId, m.SentDate });







            modelBuilder.Entity<Employee>().HasData(
    new Employee
    {
        EmployeeID = 1,
        UserName = "adminuser",
        FirstName = "Admin",
        LastName = "User",
        Email = "admin@example.com",
        Role = UserRole.ADMINISTRATOR,
        PasswordHash = "Password123",  // Plain text password
        IsActive = Status.Active
    },
    new Employee
    {
        EmployeeID = 2,
        UserName = "nurse1",
        FirstName = "Nina",
        LastName = "Nurse",
        Email = "nurse1@example.com",
        Role = UserRole.NURSE,
        PasswordHash = "Password123",
        IsActive = Status.Active
    },
    new Employee
    {
        EmployeeID = 3,
        UserName = "doctor1",
        FirstName = "David",
        LastName = "Doctor",
        Email = "doctor1@example.com",
        Role = UserRole.DOCTOR,
        PasswordHash = "Password123",
        IsActive = Status.Active
    },
    new Employee
    {
        EmployeeID = 4,
        UserName = "scriptmgr",
        FirstName = "Sara",
        LastName = "ScriptManager",
        Email = "scriptmgr@example.com",
        Role = UserRole.SCRIPTMANAGER,
        PasswordHash = "Password123",
        IsActive = Status.Active
    },
    new Employee
    {
        EmployeeID = 5,
        UserName = "consumablemgr",
        FirstName = "Chris",
        LastName = "ConsumableManager",
        Email = "consumablemgr@example.com",
        Role = UserRole.CONSUMABLESMANAGER,
        PasswordHash = "Password123",
        IsActive = Status.Active
    },
    new Employee
    {
        EmployeeID = 6,
        UserName = "wardadmin",
        FirstName = "Wanda",
        LastName = "WardAdmin",
        Email = "wardadmin@example.com",
        Role = UserRole.WARDADMIN,
        PasswordHash = "Password123",
        IsActive = Status.Active
    },
    new Employee
    {
        EmployeeID = 7,
        UserName = "nursingsister",
        FirstName = "Nancy",
        LastName = "NursingSister",
        Email = "nursingsister@example.com",
        Role = UserRole.NURSINGSISTER,
        PasswordHash = "Password123",
        IsActive = Status.Active
    }
);

















        }













    }
}