using MediatR;
using Microsoft.EntityFrameworkCore;
using SmartWalletAI.Application.Common.Interfaces;
using SmartWalletAI.Domain.Entities;
using SmartWalletAI.Domain.Enums; 
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Portfolios.Command.BuyAsset
{
    public class BuyAssetCommandHandler : IRequestHandler<BuyAssetCommand, bool>
    {
        private readonly IRepository<Asset> _assetRepository;
        private readonly IRepository<MarketPrice> _marketRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<TransactionHistory> _transactionHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public BuyAssetCommandHandler(
            IRepository<Asset> assetRepository,
            IRepository<MarketPrice> marketRepository,
            IRepository<Wallet> walletRepository,
            IRepository<TransactionHistory> transactionHistoryRepository,
            IUnitOfWork unitOfWork)
        {
            _assetRepository = assetRepository;
            _marketRepository = marketRepository;
            _walletRepository = walletRepository;
            _transactionHistoryRepository = transactionHistoryRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(BuyAssetCommand request, CancellationToken cancellationToken)
        {
            var marketPrice = await _marketRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(m => m.Type == request.AssetType, cancellationToken);

            if (marketPrice == null)
                throw new Exception("Piyasa fiyatı bulunamadı.");

            var unitPrice = marketPrice.CurrentSellPrice;

            decimal assetAmount = 0m;
            decimal totalCost = 0m;

            
            if (request.IsFiatAmount)
            {                
                totalCost = request.Amount;
                assetAmount = totalCost / unitPrice; 
            }
            else
            {             
                assetAmount = request.Amount;
                totalCost = assetAmount * unitPrice; 
            }

            var userWallet = await _walletRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (userWallet == null)
                throw new Exception("Kullanıcı cüzdanı bulunamadı.");
          
            if (userWallet.Balance < totalCost)
                throw new Exception("Yetersiz bakiye.");

            userWallet.Withdraw(totalCost);
            await _walletRepository.UpdateAsync(userWallet);

            var existingAsset = await _assetRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.Type == request.AssetType, cancellationToken);

            if (existingAsset == null)
            {
                var newAsset = new Asset(request.UserId, request.AssetType);
                newAsset.AddAmount(assetAmount, unitPrice); 
                await _assetRepository.AddAsync(newAsset);
            }
            else
            {
                existingAsset.AddAmount(assetAmount, unitPrice);
                await _assetRepository.UpdateAsync(existingAsset);
            }

            var transaction = new TransactionHistory(
                userId: request.UserId,
                assetType: request.AssetType,
                transactionType: TransactionType.Buy,
                amount: assetAmount, 
                unitPrice: unitPrice
            );
            await _transactionHistoryRepository.AddAsync(transaction);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
    }