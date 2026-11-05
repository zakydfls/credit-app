using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using CreditApplicationEntity = CreditApplication.Domain.Entities.CreditApplication;
using CreditApplication.Domain.Entities;

namespace CreditApplication.Infrastructure.Persistence.Configurations;

public sealed class CreditApplicationConfiguration
    : IEntityTypeConfiguration<CreditApplicationEntity>
{
    public void Configure(EntityTypeBuilder<CreditApplicationEntity> builder)
    {
        builder.ToTable("pengajuan_kredit", table =>
        {
            table.HasCheckConstraint(
                "ck_pengajuan_kredit_plafon_positive",
                "plafon > 0");
            table.HasCheckConstraint(
                "ck_pengajuan_kredit_bunga_range",
                "bunga > 0 AND bunga <= 100");
            table.HasCheckConstraint(
                "ck_pengajuan_kredit_tenor_positive",
                "tenor > 0");
            table.HasCheckConstraint(
                "ck_pengajuan_kredit_angsuran_positive",
                "angsuran > 0");
        });

        builder.HasKey(application => application.Id);
        builder.Property(application => application.Id)
            .HasColumnName("id")
            .HasColumnType("uuid");

        builder.Property(application => application.UserId)
            .HasColumnName("user_id");
        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(application => application.UserId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasIndex(application => application.UserId)
            .HasDatabaseName("ix_pengajuan_kredit_user_id");

        builder.Property(application => application.Plafon)
            .HasColumnName("plafon")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(application => application.Bunga)
            .HasColumnName("bunga")
            .HasColumnType("numeric(5,2)")
            .IsRequired();

        builder.Property(application => application.Tenor)
            .HasColumnName("tenor")
            .HasColumnType("integer")
            .IsRequired();

        builder.Property(application => application.Angsuran)
            .HasColumnName("angsuran")
            .HasColumnType("numeric(18,2)")
            .IsRequired();

        builder.Property(application => application.CreatedAt)
            .HasColumnName("created_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.Property(application => application.UpdatedAt)
            .HasColumnName("updated_at")
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder.HasIndex(application => application.Plafon)
            .HasDatabaseName("ix_pengajuan_kredit_plafon");

        builder.HasIndex(application => application.Tenor)
            .HasDatabaseName("ix_pengajuan_kredit_tenor");
    }
}
