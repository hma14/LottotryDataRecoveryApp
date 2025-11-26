using LottotryDataRecoveryApp.Lib;

namespace LottotryDataRecoveryApp
{
    class Program
    {
        static void Main(string[] args)
        {
            LottoDb dbContext = new ();


            var obj = new NewDailyGrandGen(dbContext);
            //var obj = new NewBC49Gen(dbContext);
            obj.ParseCsv();
            obj.InsertLottTypeTable();

        }
    }
}
