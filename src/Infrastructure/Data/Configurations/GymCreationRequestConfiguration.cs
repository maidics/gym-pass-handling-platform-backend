using Azure.Core;
using FitPass.Application.Requests.DTOs;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class GymCreationRequestConfiguration : IEntityTypeConfiguration<Request<CreateGymDTO>>
{
    public void Configure(EntityTypeBuilder<Request<CreateGymDTO>> builder)
    {
        builder.ToTable("GymCreationRequests");

        builder.Property(r => r.RequestDto).HasColumnType("jsonb");
    }
}