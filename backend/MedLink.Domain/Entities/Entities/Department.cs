using MedLink.Domain.Common;

namespace MedLink.Domain.Entities
{
    /// <summary>One record for now: Internal Medicine.</summary>
    public class Department : BaseEntity
    {
        private readonly List<Clinic> _clinics = new();

        public int ProviderId { get; private set; }
        public HealthcareProvider Provider { get; private set; } = null!;

        public string Name { get; private set; } = string.Empty;

        public IReadOnlyCollection<Clinic> Clinics => _clinics.AsReadOnly();

        private Department() { }

        public Department(int providerId, string name)
        {
            ProviderId = Guard.RequireId(providerId, nameof(providerId));
            Name = Guard.RequireText(name, nameof(name));
        }

        public void Rename(string name)
        {
            Name = Guard.RequireText(name, nameof(name));
        }
    }
}
