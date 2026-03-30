using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Wallets.Commands.Queries.GetMyWallet
{
    public class GetMyWalletQuery : IRequest<WalletSummaryDto>
    {
        public Guid UserId { get; set; }
    }
}
