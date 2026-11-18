using MedLink.Domain.Common;

namespace MedLink.Domain.Entities
{
     
    public class InsuranceCompany : BaseEntity
    {
        private readonly List<ProviderInsurance> _acceptedByProviders = new();

        public string Name { get; private set; } = string.Empty;

        public IReadOnlyCollection<ProviderInsurance> AcceptedByProviders => _acceptedByProviders.AsReadOnly();

        private InsuranceCompany() { }

        public InsuranceCompany(string name)
        {
            Name = Guard.RequireText(name, nameof(name));
        }

        public void Rename(string name)
        {
            Name = Guard.RequireText(name, nameof(name));
        }
    }
}
