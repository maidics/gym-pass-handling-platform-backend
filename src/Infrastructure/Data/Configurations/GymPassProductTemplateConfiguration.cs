using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;
public class GymPassProductTemplateConfiguration : IEntityTypeConfiguration<GymPassProductTemplate>
{
    public void Configure(EntityTypeBuilder<GymPassProductTemplate> builder)
    {
        builder.Property(gppt => gppt.EurPrice).HasPrecision(18, 2);
    }
}
