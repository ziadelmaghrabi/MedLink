using MedLink.Domain.Common;
using MedLink.Domain.Enums;

namespace MedLink.Domain.Entities
{
     
    public class Payment : BaseEntity
    {
     
        public int AppointmentId { get; private set; }
        public Appointment Appointment { get; private set; } = null!;

        public PaymentMethod Method { get; private set; }
        public PaymentStatus Status { get; private set; }

        public decimal? Amount { get; private set; }
         
        public string? GatewayReference { get; private set; }

        private Payment() { }

        public Payment(int appointmentId, PaymentMethod method, decimal? amount)
        {
            AppointmentId = Guard.RequireId(appointmentId, nameof(appointmentId));
            Method = method;
            Amount = amount.HasValue ? Guard.RequireNonNegative(amount.Value, nameof(amount)) : null;
            Status = PaymentStatus.Pending;
        }

        public void MarkPaid(string? gatewayReference = null)
        {
            Status = PaymentStatus.Paid;
            GatewayReference = Guard.OptionalText(gatewayReference);
        }

        public void MarkFailed()
        {
            Status = PaymentStatus.Failed;
        }
    }
}
