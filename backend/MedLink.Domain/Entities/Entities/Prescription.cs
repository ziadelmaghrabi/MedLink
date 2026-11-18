using MedLink.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Domain.Entities
{
    public class Prescription : BaseEntity
    {
        private readonly List<Reminder> _reminders = new();

        public int MedicalRecordId { get; private set; }
        public MedicalRecord MedicalRecord { get; private set; } = null!;

        public int DoctorId { get; private set; }
        public Doctor Doctor { get; private set; } = null!;
         
        public string Drug { get; private set; } = string.Empty;
        public string Dose { get; private set; } = string.Empty;
        public string Frequency { get; private set; } = string.Empty;
        public DateOnly StartDate { get; private set; }
        public string Duration { get; private set; } = string.Empty;
        public string? Instructions { get; private set; }

        public IReadOnlyCollection<Reminder> Reminders => _reminders.AsReadOnly();

        private Prescription() { }

        public Prescription(
            int medicalRecordId,
            int doctorId,
            string drug,
            string dose,
            string frequency,
            DateOnly startDate,
            string duration,
            string? instructions)
        {
            MedicalRecordId = Guard.RequireId(medicalRecordId, nameof(medicalRecordId));
            DoctorId = Guard.RequireId(doctorId, nameof(doctorId));
            Drug = Guard.RequireText(drug, nameof(drug));
            Dose = Guard.RequireText(dose, nameof(dose));
            Frequency = Guard.RequireText(frequency, nameof(frequency));
            StartDate = startDate;
            Duration = Guard.RequireText(duration, nameof(duration));
            Instructions = Guard.OptionalText(instructions);
        }

         
        public void AddReminder(DateTime scheduledTime)
        {
            _reminders.Add(new Reminder(scheduledTime));
        }
    }
}


