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
        private readonly IRepository<MarketPrice> _marketRepository;
        private readonly IRepository<Wallet> _walletRepository;
        private readonly IRepository<TransactionHistory> _transactionHistoryRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SellAssetCommandHandler(IRepository<Asset> assetRepository, IRepository<MarketPrice> marketRepository, IRepository<Wallet> walletRepository, IRepository<TransactionHistory> transactionHistoryRepository, IUnitOfWork unitOfWork)
        {
            _assetRepository = assetRepository;
            _marketRepository = marketRepository;
            _walletRepository = walletRepository;
            _transactionHistoryRepository = transactionHistoryRepository;
            _unitOfWork = unitOfWork;
        }
        public async Task<bool> Handle(SellAssetCommand request, CancellationToken cancellationToken)
        {
            var marketPrice = await _marketRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(m => m.Type == request.AssetType, cancellationToken);

            if (marketPrice == null)
                throw new Exception("Piyasa fiyatı bulunamadı.");

            var unitPrice = marketPrice.CurrentBuyPrice;

            decimal assetAmountToSell = 0m;
            decimal totalToReceive = 0m;

            if (request.IsFiatAmount)
            {
                
                totalToReceive = request.Amount;
                assetAmountToSell = totalToReceive / unitPrice;
            }
            else
            {
                assetAmountToSell = request.Amount;
                totalToReceive = assetAmountToSell * unitPrice;
            }

            var existingAsset = await _assetRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(a => a.UserId == request.UserId && a.Type == request.AssetType, cancellationToken);

            if (existingAsset == null || existingAsset.Amount < assetAmountToSell)
                throw new Exception("Yetersiz varlık bakiyesi.");

            var userWallet = await _walletRepository.GetAllAsQueryable()
                .FirstOrDefaultAsync(w => w.UserId == request.UserId, cancellationToken);

            if (userWallet == null)
                throw new Exception("Kullanıcı cüzdanı bulunamadı.");

            userWallet.Deposit(totalToReceive); 
            await _walletRepository.UpdateAsync(userWallet);

            existingAsset.RemoveAmount(assetAmountToSell); 
            await _assetRepository.UpdateAsync(existingAsset);

            var transaction = new TransactionHistory(
                userId: request.UserId,
                assetType: request.AssetType,
                transactionType: TransactionType.Sell,
                amount: assetAmountToSell,
                unitPrice: unitPrice
            );
            await _transactionHistoryRepository.AddAsync(transaction);

            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return true;
        }
    }
    }
