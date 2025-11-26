namespace LottotryDataRecoveryApp
{
    using LottotryDataRecoveryApp.BusinessModels;
    using LottotryDataRecoveryApp.Models;
    using SeleniumLottoDataApp.Models;
    using SeleniumLottoDataApp;
    using System.Data.Entity;

    public partial class LottoDb : DbContext
    {
        public LottoDb()
            : base("name=LottoDbContext")
        {
        }

        public virtual DbSet<BC49> BC49 { get; set; }
        public virtual DbSet<Lotto649> Lotto649 { get; set; }
        public virtual DbSet<LottoMax> LottoMax { get; set; }
        public virtual DbSet<DailyGrand> DailyGrand { get; set; }
        public virtual DbSet<DailyGrand_GrandNumber> DailyGrand_GrandNumber { get; set; }
        public virtual DbSet<LottoNumber> LottoNumber { get; set; }
        public virtual DbSet<Number> Numbers { get; set; }
        public virtual DbSet<LottoType> LottoTypes { get; set; }
        public virtual DbSet<LottoName> LottoNames { get; set; }


        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {

        }
    }
}
