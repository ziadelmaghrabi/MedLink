using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{
    
    public class AIDraft : BaseEntity
    {
        private readonly List<DifferentialItem> _differentials = new();

        public int MedicalRecordId { get; private set; }
        public MedicalRecord MedicalRecord { get; private set; } = null!;

        public string Summary { get; private set; } = string.Empty;
        public string? MissingInformation { get; private set; }

        public AIDraftStatus Status { get; private set; }

        public IReadOnlyCollection<DifferentialItem> Differentials => _differentials.AsReadOnly();

        private AIDraft() { }

        public AIDraft(int medicalRecordId, string summary, string? missingInformation)
        {
            MedicalRecordId = Guard.RequireId(medicalRecordId, nameof(medicalRecordId));
            Summary = Guard.RequireText(summary, nameof(summary));
            MissingInformation = Guard.OptionalText(missingInformation);
            Status = AIDraftStatus.Draft;
        }

        
        public void EditSummary(string summary)
        {
            Summary = Guard.RequireText(summary, nameof(summary));
        }

        public void AddDifferential(int rank, string diagnosis, string supportingFindings, string? missingInformation)
        {
            _differentials.Add(new DifferentialItem(rank, diagnosis, supportingFindings, missingInformation));
        }
    }
}
