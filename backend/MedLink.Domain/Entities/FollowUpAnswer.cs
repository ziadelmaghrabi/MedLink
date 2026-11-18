using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{
     
    public class FollowUpAnswer : BaseEntity
    {
        public int TriageCaseId { get; private set; }
        public TriageCase TriageCase { get; private set; } = null!;

        public string Question { get; private set; } = string.Empty;
        public string? Answer { get; private set; }
        public IntakeAnswerState State { get; private set; }

        private FollowUpAnswer() { }

     
        internal FollowUpAnswer(string question, string? answer)
        {
            Question = Guard.RequireText(question, nameof(question));
            Answer = Guard.OptionalText(answer);
            State = Answer is null ? IntakeAnswerState.Uncertain : IntakeAnswerState.Answered;
        }
    }
}
