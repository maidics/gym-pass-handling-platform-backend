using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using FitPass.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FitPass.Infrastructure.Data.Configurations;

public class UserPaymentProfileConfiguration : IEntityTypeConfiguration<UserPaymentProfile>
{
    public void Configure(EntityTypeBuilder<UserPaymentProfile> builder)
    {
        builder.HasOne(upp => upp.ApplicationUser).WithOne(au => au.PaymentProfile).HasForeignKey<UserPaymentProfile>(upp => upp.ApplicationUserId);
    }
}
