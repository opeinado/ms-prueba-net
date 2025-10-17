using CreditPro.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CreditPro.Infrastructure.Persistence.Configurations;

public class CreditApplicationConfiguration : IEntityTypeConfiguration<CreditApplication>
{
    public void Configure(EntityTypeBuilder<CreditApplication> builder)
    {
        builder.ToTable("credit_applications");

        builder.HasKey(x => x.ApplicationId);

        builder.Property(x => x.ApplicationId)
            .HasColumnName("application_id");

        builder.Property(x => x.CustomerId)
            .HasColumnName("customer_id")
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CreditAmount)
            .HasColumnName("credit_amount")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(x => x.ApplicationDate)
            .HasColumnName("application_date")
            .IsRequired();

        builder.Property(x => x.Status)
            .HasColumnName("status")
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(e => e.CollateralDescription).HasMaxLength(500);

        builder.Property(e => e.DescripcionFinal)
            .IsRequired()
            .HasDefaultValue("hola soy descripcion final");
    }
}
