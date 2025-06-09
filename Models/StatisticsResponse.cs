// Models/StatisticsResponse.cs

namespace ServiPuntosUyAdmin.Models
{
    public class StatisticsResponse
    {
        public bool Error { get; set; }
        public StatisticsData Data { get; set; }
        public string Message { get; set; }
    }

    public class StatisticsData
    {
        public UsersStatistics Users { get; set; }
        public TransactionsStatistics Transactions { get; set; }
        public PromotionsStatistics Promotions { get; set; }
    }

    public class UsersStatistics
    {
        public int Total { get; set; }
        public UsersByType ByType { get; set; }
    }

    public class UsersByType
    {
        public int Central { get; set; }
        public int Tenant { get; set; }
        public int Branch { get; set; }
        public int EndUser { get; set; }
    }

    public class TransactionsStatistics
    {
        public int Total { get; set; }
    }

    public class PromotionsStatistics
    {
        public int Total { get; set; }
        public int TenantPromotions { get; set; }
        public int BranchPromotions { get; set; }
    }

}

