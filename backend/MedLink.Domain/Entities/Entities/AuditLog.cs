using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{
    public class AuditLog : BaseEntity
    {
        public int UserId { get; private set; }
        public User User { get; private set; } = null!;

        public int MedicalRecordId { get; private set; }
        public MedicalRecord MedicalRecord { get; private set; } = null!;

        public AuditAction Action { get; private set; }
        public DateTime Timestamp { get; private set; }

        private AuditLog() { }

        public AuditLog(int userId, int medicalRecordId, AuditAction action, DateTime timestamp)
        {
            UserId = Guard.RequireId(userId, nameof(userId));
            MedicalRecordId = Guard.RequireId(medicalRecordId, nameof(medicalRecordId));
            Action = action;
            Timestamp = timestamp;
        }
    }
}
