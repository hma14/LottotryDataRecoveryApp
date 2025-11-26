using LottotryDataRecoveryApp.Models;
using SeleniumLottoDataApp;
using SeleniumLottoDataApp.Models;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
using static LottotryDataRecoveryApp.BusinessModels.Constants;

namespace LottotryDataRecoveryApp.Lib
{
    public class NewDailyGrandGen : DataGenBase
    {
        
        public NewDailyGrandGen(LottoDb lottoDb) : base(lottoDb)
        {
        }
           


        public void ParseCsv()
        {
            var path = GetDataPath("DailyGrand.csv");

            List<DailyGrand> rows = [];
            List<DailyGrand_GrandNumber> rows_grand = [];

            int drawNumber =  (int) db.DailyGrand
                .OrderBy(d => d.DrawNumber)
                .ToList()
                .Last().DrawNumber;

            int lottoTypesNumber = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.DailyGrand)
                .OrderBy(d => d.DrawNumber)
                .ToList()
                .Last().DrawNumber;

            using (StreamReader reader = new (path))
            {
                string? line;
                //reader.ReadLine(); // skip first line

                while ((line = reader.ReadLine()) != null)
                {
                    string[] arr = line.Split(',');
                    if (int.Parse(arr[1]) <= lottoTypesNumber) return;

                    var currentDrawDate = DateTime.Parse(arr[2].Trim('"'));
                    var entity = new DailyGrand()
                    {
                        Id = Guid.NewGuid(),
                        DrawNumber = ++drawNumber,
                        DrawDate = currentDrawDate,
                        Number1 = int.Parse(arr[5]),
                        Number2 = int.Parse(arr[6]),
                        Number3 = int.Parse(arr[7]),
                        Number4 = int.Parse(arr[8]),
                        Number5 = int.Parse(arr[9]),
                    };
                    rows.Add(entity);

                    // DailyGrand_GrandNumber table
                    var grand = new DailyGrand_GrandNumber
                    {
                        Id = Guid.NewGuid(),
                        DrawNumber = drawNumber,
                        DrawDate = currentDrawDate,
                        GrandNumber = int.Parse(arr[10]),
                    };
                    rows_grand.Add(grand);
                }
                InsertDb(rows, rows_grand);
            }
        }

        private void InsertDb(List<DailyGrand> rows, List<DailyGrand_GrandNumber> grand)
        {         
            db.DailyGrand.AddRange(rows);
            db.DailyGrand_GrandNumber.AddRange(grand);
            db.SaveChanges();            
        }

        public override void InsertLottTypeTable()
        {
            var lastLottoType = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.DailyGrand)
                .OrderByDescending(d => d.DrawNumber)
                .FirstOrDefault();

            int lastLottoTypeDrawNumber = lastLottoType?.DrawNumber ?? 0;
            int maxDrawNumber = db.DailyGrand.Max(x => x.DrawNumber);

            if (lastLottoTypeDrawNumber == maxDrawNumber) return;

            for (int draw = lastLottoTypeDrawNumber + 1; draw <= maxDrawNumber; draw++)
            {

                var lotto = db.DailyGrand.FirstOrDefault(x => x.DrawNumber == draw);
                if (lotto == null) continue;

                var prevLottoType = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.DailyGrand)
                .OrderByDescending(d => d.DrawNumber)
                .FirstOrDefault();

                if (prevLottoType == null) continue;

                var prevDraw = db.Numbers
                    .Where(x => x.LottoTypeId == prevLottoType.Id)
                    .OrderBy(n => n.Value).ToArray();

                // Store to LottoType table
                LottoType lottoType = new LottoType
                {
                    Id = Guid.NewGuid(),
                    LottoName = (int)LottoNames.DailyGrand,
                    DrawNumber = lotto.DrawNumber,
                    DrawDate = lotto.DrawDate,
                    NumberRange = (int)LottoNumberRange.DailyGrand,
                };
                db.LottoTypes.Add(lottoType);

                //Store to Numbers table
                List<Number> numbers = new List<Number>();
                for (int i = 1; i <= (int)LottoNumberRange.DailyGrand; i++)
                {
                    Number number = new Number
                    {
                        Id = Guid.NewGuid(),
                        Value = i,
                        LottoTypeId = lottoType.Id,
                        Distance = (lotto.Number1 != i &&
                                    lotto.Number2 != i &&
                                    lotto.Number3 != i &&
                                    lotto.Number4 != i &&
                                    lotto.Number5 != i ) ? prevDraw[i - 1].Distance + 1 : 0,

                        IsHit = (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ) ? true : false,


                        NumberofDrawsWhenHit =
                                    (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ) ? prevDraw[i - 1].Distance + 1 : 0,

                        TotalHits = (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ) ? prevDraw[i - 1].TotalHits + 1 : prevDraw[i - 1].TotalHits,

                        // probability
                        Probability = CalculateProbability(LottoNames.DailyGrand, i)?.Result ?? 0,
                    };
                    numbers.Add(number);
                }
                db.Numbers.AddRange(numbers);
                try
                {
                    db.SaveChanges();
                }
                catch (Exception ex)
                {
                    var error = ex.Message;
                }
            }
        }
    }
}
