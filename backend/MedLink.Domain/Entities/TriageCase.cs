using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{
     
    public class TriageCase : BaseEntity
    {
        private readonly List<FollowUpAnswer> _followUpAnswers = new();

        public int PatientId { get; private set; }
        public Patient Patient { get; private set; } = null!;

        public string Symptoms { get; private set; } = string.Empty;
        public string Onset { get; private set; } = string.Empty;
        public string Duration { get; private set; } = string.Empty;

         
        public string Severity { get; private set; } = string.Empty;

        public string? PastMedicalHistory { get; private set; }

        public IReadOnlyCollection<FollowUpAnswer> FollowUpAnswers => _followUpAnswers.AsReadOnly();

        public ScopeResult ScopeResult { get; private set; }

        
        public int? SuggestedClinicId { get; private set; }
        public Clinic? SuggestedClinic { get; private set; }

        public string Rationale { get; private set; } = string.Empty;

          
        public string? ModelVersion { get; private set; }

        public VerificationStatus VerificationStatus { get; private set; }

        public DateTime CreatedAt { get; private set; }

        private TriageCase() { }

        public TriageCase(
            int patientId,
            string symptoms,
            string onset,
            string duration,
            string severity,
            string? pastMedicalHistory,
            ScopeResult scopeResult,
            int? suggestedClinicId,
            string rationale,
            string? modelVersion,
            DateTime createdAt)
        {
            if (scopeResult == ScopeResult.OutsideInternalMedicine && suggestedClinicId is not null)
                throw new ArgumentException("No clinic is suggested when the case is outside Internal Medicine.", nameof(suggestedClinicId));

            if (scopeResult == ScopeResult.WithinInternalMedicine && suggestedClinicId is null)
                throw new ArgumentException("A clinic must be suggested when the case is within Internal Medicine.", nameof(suggestedClinicId));

            PatientId = Guard.RequireId(patientId, nameof(patientId));
            Symptoms = Guard.RequireText(symptoms, nameof(symptoms));
            Onset = Guard.RequireText(onset, nameof(onset));
            Duration = Guard.RequireText(duration, nameof(duration));
            Severity = Guard.RequireText(severity, nameof(severity));
            PastMedicalHistory = Guard.OptionalText(pastMedicalHistory);
            ScopeResult = scopeResult;
            SuggestedClinicId = suggestedClinicId.HasValue ? Guard.RequireId(suggestedClinicId.Value, nameof(suggestedClinicId)) : null;
            Rationale = Guard.RequireText(rationale, nameof(rationale));
            ModelVersion = Guard.OptionalText(modelVersion);
            VerificationStatus = VerificationStatus.Unverified;
            CreatedAt = createdAt;
        }
 
        public void AddFollowUpAnswer(string question, string? answer)
        {
            _followUpAnswers.Add(new FollowUpAnswer(question, answer));
        }
    }
}
