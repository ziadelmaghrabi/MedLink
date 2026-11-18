using MedLink.Domain.Common;

namespace MedLink.Domain.Entities
{
    
    public class DoctorSlot : BaseEntity
    {
        public int DoctorId { get; private set; }
        public Doctor Doctor { get; private set; } = null!;

        public DateTime StartsAt { get; private set; }
        public DateTime EndsAt { get; private set; }

        public bool IsBooked { get; private set; }

         
        public Appointment? Appointment { get; private set; }

        private DoctorSlot() { }

        public DoctorSlot(int doctorId, DateTime startsAt, DateTime endsAt)
        {
            if (endsAt <= startsAt)
                throw new ArgumentException("A slot must end after it starts.", nameof(endsAt));

            DoctorId = Guard.RequireId(doctorId, nameof(doctorId));
            StartsAt = startsAt;
            EndsAt = endsAt;
        }

        public void Book()
        {
            if (IsBooked)
                throw new InvalidOperationException("This slot is already booked.");

            IsBooked = true;
        }

        public void Release()
        {
            IsBooked = false;
        }
    }
}
