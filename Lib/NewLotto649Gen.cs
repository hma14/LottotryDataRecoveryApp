using LottotryDataRecoveryApp.Models;
using static LottotryDataRecoveryApp.BusinessModels.Constants;

namespace LottotryDataRecoveryApp.Lib
{
    public class NewLotto649Gen : DataGenBase
    {
        
        public NewLotto649Gen(LottoDb lottoDb) : base(lottoDb)
        {
        }
           


        public void ParseCsv()
        {
            var path = GetDataPath("649.csv");
            List<Lotto649> rows = [];

            int drawNumber =  (int) db.Lotto649.ToList().Last().DrawNumber;
            int lottoTypesNumber = db.Lotto649.ToList().Last().DrawNumber;
            
            using (StreamReader reader = new StreamReader(path))
            {
                string? line;
                //reader.ReadLine(); // skip first line

                while ((line = reader.ReadLine()) != null)
                {
                    string[] arr = line.Split(',');
                    if (int.Parse(arr[1]) <= lottoTypesNumber) return;
                    var entity = new LottotryDataRecoveryApp.Lotto649()
                    {
                        DrawNumber = ++drawNumber,
                        DrawDate = DateTime.Parse(arr[3].Trim('"')),
                        Number1 = int.Parse(arr[4]),
                        Number2 = int.Parse(arr[5]),
                        Number3 = int.Parse(arr[6]),
                        Number4 = int.Parse(arr[7]),
                        Number5 = int.Parse(arr[8]),
                        Number6 = int.Parse(arr[9]),
                        Bonus = int.Parse(arr[10]),
                    };
                    rows.Add(entity);                  
                }
                InsertDb(rows);
            }
        }

        private void InsertDb(List<LottotryDataRecoveryApp.Lotto649> rows)
        {         
            db.Lotto649.AddRange(rows);
            db.SaveChanges();            
        }

        public override void InsertLottTypeTable()
        {
            var lastLottoType = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.Lotto649)
                .OrderByDescending(d => d.DrawNumber)
                .FirstOrDefault();

            int lastLottoTypeDrawNumber = lastLottoType?.DrawNumber ?? 0;
            int maxDrawNumber = db.Lotto649.Max(x => x.DrawNumber);

            if (lastLottoTypeDrawNumber == maxDrawNumber) return;

            for (int draw = lastLottoTypeDrawNumber + 1; draw <= maxDrawNumber; draw++)
            {

                var lotto = db.Lotto649.FirstOrDefault(x => x.DrawNumber == draw);
                if (lotto == null) continue;

                var prevLottoType = db.LottoTypes
                .Where(x => x.LottoName == (int)LottoNames.Lotto649)
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
                    LottoName = (int)LottoNames.Lotto649,
                    DrawNumber = lotto.DrawNumber,
                    DrawDate = lotto.DrawDate,
                    NumberRange = (int)LottoNumberRange.Lotto649,
                };
                db.LottoTypes.Add(lottoType);

                //Store to Numbers table
                List<Number> numbers = new List<Number>();
                for (int i = 1; i <= (int)LottoNumberRange.Lotto649; i++)
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
                        Probability = CalculateProbability(LottoNames.Lotto649, i)?.Result ?? 0,
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
