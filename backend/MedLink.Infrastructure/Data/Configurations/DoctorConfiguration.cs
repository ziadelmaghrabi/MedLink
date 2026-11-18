using MedLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedLink.Infrastructure.Data.Configurations;

public sealed class DoctorConfiguration : IEntityTypeConfiguration<Doctor>
{
    public void Configure(EntityTypeBuilder<Doctor> builder)
    {
        builder.HasKey(doctor => doctor.Id);

        builder.Property(doctor => doctor.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(doctor => doctor.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(doctor => doctor.PhoneNumber)
            .HasMaxLength(32);

        builder.Property(doctor => doctor.LicenseNumber)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasIndex(doctor => doctor.LicenseNumber)
            .IsUnique();

        builder.Property(doctor => doctor.Bio)
            .HasMaxLength(2000);

        builder.Property(doctor => doctor.ConsultationFee)
            .HasPrecision(10, 2);

        builder.Property(doctor => doctor.IsActive)
            .IsRequired();
    }
}
