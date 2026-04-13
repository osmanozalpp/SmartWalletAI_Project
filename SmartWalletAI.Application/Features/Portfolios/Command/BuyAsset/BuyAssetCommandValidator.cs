using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Command.BuyAsset
{
    public class BuyAssetCommandValidator : AbstractValidator<BuyAssetCommand>
    {
        public BuyAssetCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0)
                .WithMessage("Alım miktarı sıfırdan büyük olmalıdır.");
            

            RuleFor(x => x.AssetType)
                .IsInEnum()
                .WithMessage("Geçersiz varlık tipi.");
        }
    }
}
