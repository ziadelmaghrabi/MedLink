using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{ 

    public class Reminder : BaseEntity
    {
        public int PrescriptionId { get; private set; }
        public Prescription Prescription { get; private set; } = null!;

        public DateTime ScheduledTime { get; private set; }
        public ReminderStatus Status { get; private set; }

        private Reminder() { }

        
        internal Reminder(DateTime scheduledTime)
        {
            ScheduledTime = scheduledTime;
            Status = ReminderStatus.Scheduled;
        }

        public void MarkTaken()
        {
            Status = ReminderStatus.Taken;
        }

        public void Snooze(DateTime newScheduledTime)
        {
            ScheduledTime = newScheduledTime;
            Status = ReminderStatus.Snoozed;
        }
         
        public void MarkNotTaken()
        {
            Status = ReminderStatus.NotTaken;
        }
    }
}
