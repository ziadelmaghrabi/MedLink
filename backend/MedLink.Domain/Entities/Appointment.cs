using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{
    public class Appointment : BaseEntity
    {
        public int PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public int DoctorId { get; private set; }
        public Doctor Doctor { get; private set; } = null!;

         
        public int DoctorSlotId { get; private set; }
        public DoctorSlot Slot { get; private set; } = null!;

        public AppointmentStatus Status { get; private set; }

         
        public int? TriageCaseId { get; private set; }
        public TriageCase? TriageCase { get; private set; }
         
        public int? SelectedInsuranceId { get; private set; }
        public InsuranceCompany? SelectedInsurance { get; private set; }

         
        public Payment? Payment { get; private set; }

        private Appointment() { }

 
        public Appointment(int patientId, DoctorSlot slot, int? triageCaseId = null)
        {
            ArgumentNullException.ThrowIfNull(slot);

            PatientId = Guard.RequireId(patientId, nameof(patientId));
            DoctorSlotId = Guard.RequireId(slot.Id, nameof(slot));
            DoctorId = slot.DoctorId;
            TriageCaseId = triageCaseId.HasValue ? Guard.RequireId(triageCaseId.Value, nameof(triageCaseId)) : null;

            slot.Book();
            Slot = slot;
            Status = AppointmentStatus.PendingPayment;
        }

        public void SelectInsurance(int insuranceCompanyId)
        {
            SelectedInsuranceId = Guard.RequireId(insuranceCompanyId, nameof(insuranceCompanyId));
        }
         
        public void ConfirmBooking()
        {
            Status = AppointmentStatus.Booked;
        }
    }
}
