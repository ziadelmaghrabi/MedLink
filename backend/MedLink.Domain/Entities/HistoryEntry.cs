using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{
     
    public class HistoryEntry : BaseEntity
    {
        public int MedicalRecordId { get; private set; }
        public MedicalRecord MedicalRecord { get; private set; } = null!;

        public string Content { get; private set; } = string.Empty;

        
        public VerificationStatus Status { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private HistoryEntry() { }

         
        internal HistoryEntry(string content, DateTime createdAt)
        {
            Content = Guard.RequireText(content, nameof(content));
            Status = VerificationStatus.DoctorConfirmed;
            CreatedAt = createdAt;
        }
    }
}
