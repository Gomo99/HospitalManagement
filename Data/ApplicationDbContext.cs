using HospitalManagement.Models;
using HospitalManagement.ViewModel;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace HospitalManagement.Data
{
    public class ApplicationDbContext : DbContext
    {

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }


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





        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Login>().HasNoKey();

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













        }




    }
}

