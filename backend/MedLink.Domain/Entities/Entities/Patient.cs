using MedLink.Domain.Common;
using MedLink.Domain.Constants;

namespace MedLink.Domain.Entities
{
    public class Patient : User
    {
        private readonly List<TriageCase> _triageCases = new();
        private readonly List<Appointment> _appointments = new();

        public DateOnly BirthDate { get; private set; }
         
        public int? SelectedInsuranceId { get; private set; }
        public InsuranceCompany? SelectedInsurance { get; private set; }

        
        public MedicalRecord? MedicalRecord { get; private set; }

        public IReadOnlyCollection<TriageCase> TriageCases => _triageCases.AsReadOnly();
        public IReadOnlyCollection<Appointment> Appointments => _appointments.AsReadOnly();

        public override string RoleName => RoleNames.Patient;

        private Patient() { }

        public Patient(string fullName, DateOnly birthDate)
            : base(fullName)
        {
            if (birthDate > DateOnly.FromDateTime(DateTime.UtcNow))
                throw new ArgumentException("Birth date cannot be in the future.", nameof(birthDate));

            BirthDate = birthDate;
        }

        public void SelectInsurance(int insuranceCompanyId)
        {
            SelectedInsuranceId = Guard.RequireId(insuranceCompanyId, nameof(insuranceCompanyId));
        }

        public void ClearInsurance()
        {
            SelectedInsuranceId = null;
        }
    }
}
