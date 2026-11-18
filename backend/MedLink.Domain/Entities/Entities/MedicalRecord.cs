using MedLink.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Domain.Entities
{
    public class MedicalRecord : BaseEntity
    {
        private readonly List<HistoryEntry> _historyEntries = new();
        private readonly List<Prescription> _prescriptions = new();
        private readonly List<AIDraft> _aiDrafts = new();
 
        public int PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;
 
        public string? FinalDiagnosis { get; private set; }

        public IReadOnlyCollection<HistoryEntry> HistoryEntries => _historyEntries.AsReadOnly();
        public IReadOnlyCollection<Prescription> Prescriptions => _prescriptions.AsReadOnly();
        public IReadOnlyCollection<AIDraft> AIDrafts => _aiDrafts.AsReadOnly();

        private MedicalRecord() { }

        public MedicalRecord(int patientId)
        {
            PatientId = Guard.RequireId(patientId, nameof(patientId));
        }

        public void AddHistoryEntry(string content, DateTime createdAt)
        {
            _historyEntries.Add(new HistoryEntry(content, createdAt));
        }

        public void RecordFinalDiagnosis(string diagnosis)
        {
            FinalDiagnosis = Guard.RequireText(diagnosis, nameof(diagnosis));
        }
    }
}
