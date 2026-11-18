namespace MedLink.Core.Entities;

public sealed class Doctor
{
    public Guid Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Bio { get; set; }
    public decimal ConsultationFee { get; set; }
    public bool IsActive { get; set; }
}
