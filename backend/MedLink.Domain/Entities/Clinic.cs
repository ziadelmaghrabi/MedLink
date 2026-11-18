using MedLink.Domain.Common;

namespace MedLink.Domain.Entities
{
     
    public class Clinic : BaseEntity
    {
        private readonly List<Doctor> _doctors = new();
        private readonly List<TriageCase> _suggestedInTriageCases = new();

        public int DepartmentId { get; private set; }
        public Department Department { get; private set; } = null!;

        public string Name { get; private set; } = string.Empty;

         
        public bool IsAvailable { get; private set; }

        public IReadOnlyCollection<Doctor> Doctors => _doctors.AsReadOnly();
        public IReadOnlyCollection<TriageCase> SuggestedInTriageCases => _suggestedInTriageCases.AsReadOnly();

        private Clinic() { }

        public Clinic(int departmentId, string name, bool isAvailable = true)
        {
            DepartmentId = Guard.RequireId(departmentId, nameof(departmentId));
            Name = Guard.RequireText(name, nameof(name));
            IsAvailable = isAvailable;
        }

        public void Rename(string name)
        {
            Name = Guard.RequireText(name, nameof(name));
        }

        public void SetAvailability(bool isAvailable)
        {
            IsAvailable = isAvailable;
        }
    }
}
