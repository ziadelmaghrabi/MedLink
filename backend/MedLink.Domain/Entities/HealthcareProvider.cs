using MedLink.Domain.Common;

namespace MedLink.Domain.Entities
{
     
    public class HealthcareProvider : BaseEntity
    {
        private readonly List<Department> _departments = new();
        private readonly List<Doctor> _doctors = new();
        private readonly List<ClinicAdministrator> _administrators = new();
        private readonly List<ProviderInsurance> _acceptedInsurers = new();

        public string Name { get; private set; } = string.Empty;

        public IReadOnlyCollection<Department> Departments => _departments.AsReadOnly();
        public IReadOnlyCollection<Doctor> Doctors => _doctors.AsReadOnly();
        public IReadOnlyCollection<ClinicAdministrator> Administrators => _administrators.AsReadOnly();

         
        public IReadOnlyCollection<ProviderInsurance> AcceptedInsurers => _acceptedInsurers.AsReadOnly();

        private HealthcareProvider() { }

        public HealthcareProvider(string name)
        {
            Name = Guard.RequireText(name, nameof(name));
        }

        public void Rename(string name)
        {
            Name = Guard.RequireText(name, nameof(name));
        }

        public void AcceptInsurer(int insuranceCompanyId)
        {
            Guard.RequireId(insuranceCompanyId, nameof(insuranceCompanyId));

            if (_acceptedInsurers.Any(x => x.InsuranceCompanyId == insuranceCompanyId))
                return;

            _acceptedInsurers.Add(new ProviderInsurance(insuranceCompanyId));
        }

        public void RemoveInsurer(int insuranceCompanyId)
        {
            var link = _acceptedInsurers.FirstOrDefault(x => x.InsuranceCompanyId == insuranceCompanyId);
            if (link is not null)
                _acceptedInsurers.Remove(link);
        }
    }
}
