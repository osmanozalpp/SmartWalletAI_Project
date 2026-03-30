using Microsoft.EntityFrameworkCore.Storage;
using SmartWalletAI.Application.Common.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Infrastructure.Persistence.Configurations
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly ApplicationDbContext _context ;

        public UnitOfWork(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task<IDbContextTransaction>BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
           return await _context.Database.BeginTransactionAsync(cancellationToken);
        }

        public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
    }
}
