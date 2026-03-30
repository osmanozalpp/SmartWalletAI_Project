using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SmartWalletAI.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Infrastructure.Persistence.Configurations
{
    public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
    {
        public void Configure(EntityTypeBuilder<Wallet> builder)
        {
            builder.HasKey(w=>w.Id);

            builder.Property(w => w.IBAN).IsRequired().HasMaxLength(16);
            builder.HasIndex(w => w.IBAN).IsUnique();

            builder.Property(w => w.Balance).HasColumnType("decimal(18,2)");

        }
    }
}
