using LottotryDataRecoveryApp.Models;
using static LottotryDataRecoveryApp.BusinessModels.Constants;

namespace LottotryDataRecoveryApp.Lib
{
    public class NewBC49Gen : DataGenBase
    {
        
        public NewBC49Gen(LottoDb lottoDb) : base(lottoDb)
        {
        }
           


        public void ParseCsv()
        {
            var path = GetDataPath("BC49.csv");

            List<BC49> rows = [];

            int drawNumber = (int)db.BC49
                .OrderBy(d => d.DrawNumber)
                .ToList()
                .Last().DrawNumber;

            int lottoTypesNumber = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.BC49)
                .OrderBy(d => d.DrawNumber)
                .ToList()
                .Last().DrawNumber;
            
            using (StreamReader reader = new StreamReader(path))
            {
                string? line;
                //reader.ReadLine(); // skip first line

                while ((line = reader.ReadLine()) != null)
                {
                    string[] arr = line.Split(',');
                    if (int.Parse(arr[1]) <= lottoTypesNumber) return;
                    var entity = new BC49()
                    {
                        DrawNumber = ++drawNumber,
                        DrawDate = DateTime.Parse(arr[2].Trim('"')),
                        Number1 = int.Parse(arr[3]),
                        Number2 = int.Parse(arr[4]),
                        Number3 = int.Parse(arr[5]),
                        Number4 = int.Parse(arr[6]),
                        Number5 = int.Parse(arr[7]),
                        Number6 = int.Parse(arr[8]),
                        Bonus = int.Parse(arr[9]),
                    };
                    rows.Add(entity);                  
                }
                InsertDb(rows);
            }
        }

        private void InsertDb(List<BC49> rows)
        {         
            db.BC49.AddRange(rows);
            db.SaveChanges();            
        }

        public override void InsertLottTypeTable()
        {
            var lastLottoType = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.BC49)
                .OrderByDescending(d => d.DrawNumber)
                .FirstOrDefault();

            int lastLottoTypeDrawNumber = lastLottoType?.DrawNumber ?? 0;
            int maxDrawNumber = db.BC49.Max(x => x.DrawNumber);

            if (lastLottoTypeDrawNumber == maxDrawNumber) return;

            for (int draw = lastLottoTypeDrawNumber + 1; draw <= maxDrawNumber; draw++)
            {

                var lotto = db.BC49.FirstOrDefault(x => x.DrawNumber == draw);
                if (lotto == null) continue;

                var prevLottoType = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.BC49)
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
                    LottoName = (int)LottoNames.BC49,
                    DrawNumber = lotto.DrawNumber,
                    DrawDate = lotto.DrawDate,
                    NumberRange = (int)LottoNumberRange.BC49,
                };
                db.LottoTypes.Add(lottoType);

                //Store to Numbers table
                List<Number> numbers = new List<Number>();
                for (int i = 1; i <= (int)LottoNumberRange.BC49; i++)
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
                                    lotto.Number5 != i &&
                                    lotto.Number6 != i &&
                                    lotto.Bonus != i) ? prevDraw[i - 1].Distance + 1 : 0,

                        IsHit = (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ||
                                    lotto.Number6 == i ||
                                    lotto.Bonus == i) ? true : false,


                        NumberofDrawsWhenHit =
                                    (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ||
                                    lotto.Number6 == i ||
                                    lotto.Bonus == i) ? prevDraw[i - 1].Distance + 1 : 0,

                        IsBonusNumber = lotto.Bonus == i ? true : false,
                        TotalHits = (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ||
                                    lotto.Number6 == i ||
                                    lotto.Bonus == i) ? prevDraw[i - 1].TotalHits + 1 : prevDraw[i - 1].TotalHits,

                        // probability
                        Probability = CalculateProbability(LottoNames.BC49, i)?.Result ?? 0,
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
