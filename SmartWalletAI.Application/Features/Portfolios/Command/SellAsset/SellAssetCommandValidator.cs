using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Command.SellAsset
{
    public class SellAssetCommandValidator : AbstractValidator<SellAssetCommand>
    {
        public SellAssetCommandValidator()
        {
            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Satış miktarı sıfırdan büyük olmalıdır.");

            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("Kullanıcı bilgisi eksik");

            RuleFor(x=>x.AssetType)
                .IsInEnum().WithMessage("Geçersiz varlık türü. (Sadece Altın, Gümüş, USD veya EUR satılabilir)");
        }
    }
}
