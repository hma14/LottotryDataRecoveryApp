using LottotryDataRecoveryApp.Models;
using static LottotryDataRecoveryApp.BusinessModels.Constants;

namespace LottotryDataRecoveryApp.Lib
{
    public class NewLottoMaxGen : DataGenBase
    {
        
        public NewLottoMaxGen(LottoDb lottoDb) : base(lottoDb)
        {
        }

        public void ParseCsv()
        {
            var path = GetDataPath("LOTTOMAX.csv");
            List<LottoMax> rows = [];


            int drawNumber = db.LottoMax
                            .OrderBy(d => d.DrawNumber)
                            .ToList()
                            .LastOrDefault()?.DrawNumber ?? 0;

            if (drawNumber == db.LottoMax.Max(x => x.DrawNumber)) return;

            int lottoTypesNumber = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.LottoMax)
                .OrderBy(d => d.DrawNumber)
                .ToList()
                .LastOrDefault()?.DrawNumber ?? 0;

            using (StreamReader reader = new StreamReader(path))
            {
                string? line;

                while ((line = reader.ReadLine()) != null && line != string.Empty)
                {
                    string[] arr = line.Split(',');
                    if (int.Parse(arr[11]) == 0)  continue; // skip if bonus number is 0, which means the draw is not completed yet

                    if (int.Parse(arr[1]) <= lottoTypesNumber) return;
                    var entity = new LottotryDataRecoveryApp.LottoMax()
                    {
                        DrawNumber = ++drawNumber,
                        DrawDate = DateTime.Parse(arr[3].Trim('"')),
                        Number1 = int.Parse(arr[4]),
                        Number2 = int.Parse(arr[5]),
                        Number3 = int.Parse(arr[6]),
                        Number4 = int.Parse(arr[7]),
                        Number5 = int.Parse(arr[8]),
                        Number6 = int.Parse(arr[9]),
                        Number7 = int.Parse(arr[10]),
                        Bonus = int.Parse(arr[11]),
                    };
                    rows.Add(entity);
                }
                InsertDb(rows);
            }
        }

        private void InsertDb(List<LottotryDataRecoveryApp.LottoMax> rows)
        {         
            db.LottoMax.AddRange(rows);
            db.SaveChanges();            
        }

        public override void InsertLottTypeTable()
        {
            var lastLottoType = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.LottoMax)
                .OrderByDescending(d => d.DrawNumber)
                .FirstOrDefault();

            int lastLottoTypeDrawNumber = lastLottoType?.DrawNumber ?? 0;
            int maxDrawNumber = db.LottoMax.Max(x => x.DrawNumber);

            if (lastLottoTypeDrawNumber == maxDrawNumber) return;

            for (int draw = lastLottoTypeDrawNumber + 1; draw <= maxDrawNumber; draw++)
            {

                var lotto = db.LottoMax.FirstOrDefault(x => x.DrawNumber == draw);
                if (lotto == null) continue;

                var prevLottoType = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.LottoMax)
                .OrderBy(d => d.DrawNumber)
                .ToList()
                .LastOrDefault();

                //if (prevLottoType == null) continue;

                var prevDraw = prevLottoType != null ? (db.Numbers
                    .Where(x => x.LottoTypeId == prevLottoType.Id)
                    .OrderBy(n => n.Value)
                    .ToList()) : null;

#if false
                if (prevDraw.Count == 50)
                {
                    prevDraw.Add(new Number { Id = Guid.NewGuid(), Value = 51 });
                    prevDraw.Add(new Number { Id = Guid.NewGuid(), Value = 52 });
                }
#endif

                // Store to LottoType table
                LottoType lottoType = new LottoType
                {
                    Id = Guid.NewGuid(),
                    LottoName = (int)LottoNames.LottoMax,
                    DrawNumber = lotto.DrawNumber,
                    DrawDate = lotto.DrawDate,
                    NumberRange = (int)LottoNumberRange.LottoMax,
                };
                db.LottoTypes.Add(lottoType);

                //Store to Numbers table
                List<Number> numbers = new List<Number>();
                for (int i = 1; i <= (int)LottoNumberRange.LottoMax; i++)
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
                                    lotto.Number7 != i &&
                                    lotto.Bonus != i) ? (prevDraw != null && prevDraw[i - 1] != null ? (prevDraw[i - 1].Distance + 1) : 1) : 0,

                        IsHit = (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ||
                                    lotto.Number6 == i ||
                                    lotto.Number7 == i ||
                                    lotto.Bonus == i) ? true : false,


                        NumberofDrawsWhenHit =
                                    (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ||
                                    lotto.Number6 == i ||
                                    lotto.Number7 == i ||
                                    lotto.Bonus == i) ? (prevDraw != null && prevDraw[i - 1] != null ? (prevDraw[i - 1].Distance + 1) : 1) : 0,

                        IsBonusNumber = lotto.Bonus == i ? true : false,
                        TotalHits = (lotto.Number1 == i ||
                                    lotto.Number2 == i ||
                                    lotto.Number3 == i ||
                                    lotto.Number4 == i ||
                                    lotto.Number5 == i ||
                                    lotto.Number6 == i ||
                                    lotto.Number7 == i ||
                                    lotto.Bonus == i) ? (prevDraw != null && prevDraw[i - 1] != null ? (prevDraw[i - 1].TotalHits + 1) : 1) : (prevDraw != null && prevDraw[i - 1] != null ? prevDraw[i - 1].TotalHits : 0),

                        // probability
                        Probability = CalculateProbability(LottoNames.LottoMax, i)?.Result ?? 0,
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
