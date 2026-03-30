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
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
            builder.HasKey(x => x.Id);

            builder.HasIndex(u=>u.Email).IsUnique();
            builder.Property(u=>u.Email).IsRequired().HasMaxLength(100);

            builder.Property(u => u.PasswordHash).IsRequired();

            //1-1
            builder.HasOne(u => u.Wallet)
            .WithOne(w=>w.User)
            .HasForeignKey<Wallet>(w=>w.UserId)
            .OnDelete(DeleteBehavior.Cascade);//kullanıcı silinirse cüzdanı da silinsin
                
            
        }
    }
}
