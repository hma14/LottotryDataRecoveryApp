using LottotryDataRecoveryApp.BusinessModels;
using LottotryDataRecoveryApp.Dto;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using static LottotryDataRecoveryApp.BusinessModels.Constants;

namespace LottotryDataRecoveryApp.Lib
{
    public class DataGenBase
    {
        public LottoDb db { get; set; }


        public DataGenBase(LottoDb _db)
        {
            db = _db;
        }

        public virtual void InsertLottTypeTable() { }

        public string GetDataPath(string dataFileName)
        {
            var currentDirectory = Directory.GetCurrentDirectory();
            var parentDirectory = Directory.GetParent(currentDirectory)?.Parent?.Parent?.FullName;

            if (parentDirectory == null)
            {
                throw new InvalidOperationException("Parent directory could not be determined.");
            }

            return Path.Combine(parentDirectory, $"Lotto.Data/{dataFileName}");
        }

        private async Task<IEnumerable<LottoTypeDto>> GetLottoTypesAsync(int lottoName)
        {
            // Define the base URL and API endpoint
            string apiUrl = $"http://api.lottotry.com/api/lottotypes?lottoName={lottoName}";

            // Create an instance of HttpClient
            using (HttpClient client = new HttpClient())
            {
                try
                {
                    // Send a GET request to the API endpoint
                    HttpResponseMessage response = await client.GetAsync(apiUrl);

                    // Ensure the response was successful (status code 200)
                    response.EnsureSuccessStatusCode();

                    // Read the response content as a string
                    string responseBody = await response.Content.ReadAsStringAsync();

                    // Deserialize the JSON response into IEnumerable<LottoTypeDto>
                    var lottoTypes = JsonSerializer.Deserialize<IEnumerable<LottoTypeDto>>(responseBody);

                    return lottoTypes.ToList();
                }
                catch (HttpRequestException e)
                {
                    // Handle any errors that occur during the request
                    Console.WriteLine($"Request error: {e.Message}");
                    return null;
                }
            }
        }

        private bool IsInRange(int num, int start, int end)
        {
            return num >= start && num <= end;
        }

        public async Task<int> CalculateProbability(LottoNames lottoName, int num)
        {
            // get range of this number in history
            var list = await GetLottoTypesAsync((int)lottoName);
            var sortedList = list.OrderByDescending(x => x.DrawDate);
            int probability = 0;


            List<NumberDto> numbers = [];
            foreach (var item in sortedList)
            {
                var n = item.Numbers.Where(x => x.Value == num).FirstOrDefault();
                if (n == null) continue;

                n.LottoName = item.LottoName;
                n.DrawNumber = item.DrawNumber;
                n.DrawDate = item.DrawDate;
                n.NumberRange = item.NumberRange;
                numbers.Add(n);
            }

            var hits = numbers.Where(x => x.IsHit == true).ToList();

            if ((hits[0].NumberofDrawsWhenHit > Constants.COLD_POINT ||
                hits[1].NumberofDrawsWhenHit > Constants.COLD_POINT) &&
                numbers[0].Distance < Constants.NORMAL_RANGE &&
                numbers[0].Distance >= Constants.HOT_POINT) probability++;

            if ((hits[0].NumberofDrawsWhenHit > Constants.COLD_POINT &&
                hits[1].NumberofDrawsWhenHit > Constants.NORMAL_RANGE) &&
                numbers[0].Distance < Constants.NORMAL_RANGE &&
                numbers[0].Distance >= Constants.HOT_POINT - 2) probability++;

            if (hits[0].NumberofDrawsWhenHit > Constants.COLD_POINT &&
                numbers[0].Distance > Constants.NORMAL_RANGE) probability++;

            if (hits[0].DrawNumber == hits[1].DrawNumber + 1 &&
                numbers[0].Distance < Constants.NORMAL_RANGE) probability++;

            if ((hits[0].DrawNumber == hits[1].DrawNumber + 2 ||
                hits[0].DrawNumber == hits[1].DrawNumber + 3) &&
                numbers[0].IsHit == false &&
                numbers[0].Distance < Constants.NORMAL_RANGE) probability++;

            if (hits[0].Distance >= Constants.COLD_POINT &&
                numbers[0].Distance > Constants.NORMAL_RANGE) probability++;

            if (hits[1].NumberofDrawsWhenHit > Constants.COLD_POINT &&
                hits[0].DrawNumber < hits[1].DrawNumber + Constants.NORMAL_RANGE &&
                numbers[0].Distance < Constants.NORMAL_RANGE &&
                numbers[0].IsHit == false) probability++;

            if (hits[0].NumberofDrawsWhenHit == hits[1].NumberofDrawsWhenHit &&
                numbers[0].IsHit == false)
            {
                if (numbers[0].Distance + 1 == hits[0].NumberofDrawsWhenHit ||
                   numbers[0].Distance == hits[0].NumberofDrawsWhenHit)
                {
                    probability++;
                }
                else if (IsInRange(numbers[0].Distance, hits[0].NumberofDrawsWhenHit, Constants.NORMAL_RANGE))
                {
                    probability++;
                }
            }

            if (hits[0].NumberofDrawsWhenHit <= Constants.HOT_POINT &&
                hits[1].NumberofDrawsWhenHit <= Constants.HOT_POINT &&
                hits[2].NumberofDrawsWhenHit <= Constants.HOT_POINT &&
                hits[3].NumberofDrawsWhenHit <= Constants.NORMAL_RANGE &&
                numbers[0].IsHit == false) probability++;

            if (numbers[0].Distance + 1 == hits[0].NumberofDrawsWhenHit)
                probability++;


            if (numbers[0].Distance >= Constants.COLD_POINT &&
                hits[0].DrawNumber == hits[1].DrawNumber + 1)
                probability++;





            return probability;
        }

    }
}
