using MedLink.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace MedLink.Infrastructure.Data.Configurations;

public sealed class PatientConfiguration : IEntityTypeConfiguration<Patient>
{
    public void Configure(EntityTypeBuilder<Patient> builder)
    {
        builder.HasKey(patient => patient.Id);

        builder.Property(patient => patient.FirstName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(patient => patient.LastName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(patient => patient.DateOfBirth)
            .HasColumnType("date");

        builder.Property(patient => patient.Gender)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(patient => patient.PhoneNumber)
            .HasMaxLength(32);

        builder.Property(patient => patient.Address)
            .HasMaxLength(500);
    }
}
