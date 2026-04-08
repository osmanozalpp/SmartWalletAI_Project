using MediatR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Command.SellAsset
{
    public class SellAssetCommandHandler : IRequestHandler<SellAssetCommand, bool>
    {
        private readonly IRepository<Asset> _assetRepository;
        private readonly IRepository<MarketPrice> _marketRrepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<TransactionHistory> _transactionHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SellAssetCommandHandler(IRepository<Asset> assetRepository , IRepository<MarketPrice> marketRrepository , IRepository<Wallet> walletRepository , IRepository<TransactionHistory> transactionHistoryRepository, IUnitOfWork unitOfWork)
        {
            _assetRepository = assetRepository;
            _marketRrepository = marketRrepository;
            _walletRepository = walletRepository;
            _transactionHistoryRepository = transactionHistoryRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(SellAssetCommand request, CancellationToken cancellationToken)
        {
            var existingAsset = await _assetRepository.GetAllAsQueryable().FirstOrDefaultAsync(a => a.UserId == request.UserId && a.Type == request.AssetType , cancellationToken);

            if(existingAsset == null || existingAsset.Amount < request.Amount)
            {
                throw new Exception("Yetersiz varlık bakiyesi . Satmak istediğiniz kadar varlığa sahip değilsiniz.");
            }

            var marketPrice = await _marketRrepository.GetAllAsQueryable().FirstOrDefaultAsync(m => m.Type == request.AssetType, cancellationToken);

            if(marketPrice == null)
            {
                throw new Exception("Piyasa fiyatı bulunamadı.Lütfen daha sonra tekrar deneyin.");
            }

            //banka alırken alış fiyatından alır
            var unitPrice = marketPrice.CurrentBuyPrice;
            var totalRevenue = unitPrice * request.Amount;

            var userWallet = await _walletRepository.GetAllAsQueryable().FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if(userWallet == null)
            {
                throw new Exception("Kullanıcı cüzdanı bulunamadı.Lütfen daha sonra tekrar deneyin.");
            }

            //varlıktan satılan miktarı düş
            existingAsset.RemoveAmount(request.Amount);

            //kullanıcıya parayı yatır
            userWallet.Deposit(totalRevenue);

            var transactionLog = new TransactionHistory
            (
            userId: request.UserId,
            assetType: request.AssetType,
            transactionType: TransactionType.Sell, 
            amount: request.Amount,
            unitPrice: unitPrice
            );

            await _transactionHistoryRepository.AddAsync(transactionLog);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
}
