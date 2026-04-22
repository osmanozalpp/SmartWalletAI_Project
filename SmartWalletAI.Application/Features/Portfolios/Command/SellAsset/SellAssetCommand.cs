using MediatR;
using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Command.SellAsset
{
    public class SellAssetCommand : IRequest<bool>
    {
        [JsonIgnore]
        public Guid UserId { get; set; }
        public AssetType AssetType { get; set; }
        public decimal Amount { get; set; }
        public bool IsFiatAmount { get; set; } // True ise Amount TL'dir, False ise Gramdır.

    }
}
