using MedLink.Domain.Common;
using MedLink.Domain.Constants;

namespace MedLink.Domain.Entities
{
    public class Doctor : User
    {
        private readonly List<DoctorSlot> _slots = new();
        private readonly List<Appointment> _appointments = new();
        private readonly List<Prescription> _prescriptions = new();

         
        public int ProviderId { get; private set; }
        public HealthcareProvider Provider { get; private set; } = null!;

        
        public int ClinicId { get; private set; }
        public Clinic Clinic { get; private set; } = null!;

         
        public decimal ConsultationFee { get; private set; }

        public IReadOnlyCollection<DoctorSlot> Slots => _slots.AsReadOnly();
        public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();
        public IReadOnlyCollection<Prescription> Prescriptions => _prescriptions.AsReadOnly();

        public override string RoleName => RoleNames.Doctor;

        private Doctor() { }

        public Doctor(string fullName, int providerId, int clinicId, decimal consultationFee)
            : base(fullName)
        {
            ProviderId = Guard.RequireId(providerId, nameof(providerId));
            ClinicId = Guard.RequireId(clinicId, nameof(clinicId));
            ConsultationFee = Guard.RequireNonNegative(consultationFee, nameof(consultationFee));
        }

        public void AssignToClinic(int clinicId)
        {
            ClinicId = Guard.RequireId(clinicId, nameof(clinicId));
        }

        public void SetConsultationFee(decimal fee)
        {
            ConsultationFee = Guard.RequireNonNegative(fee, nameof(fee));
        }
    }
}
