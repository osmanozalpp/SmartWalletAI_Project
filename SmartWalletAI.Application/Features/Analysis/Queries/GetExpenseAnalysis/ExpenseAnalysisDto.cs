using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SmartWalletAI.Application.Features.Analysis.Queries.GetExpenseAnalysis
{

    // Analiz sonucunun tamamı (Döneceğimiz ana paket)
    public class ExpenseAnalysisDto
    {
        public decimal TotalMonthlyExpense { get; set; }
        public decimal DailyAverageExpense { get; set; }
        public string TopSendingCategory { get; set; }
        public List<CategoryExpenseDetail> CategoryDetails { get; set; }

        public string AiAnalysisAdvice { get; set; }
    }

    //pie chart için
    public class CategoryExpenseDetail
    {
        public string CategoryName { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal Percentage { get; set; }
    }
}
