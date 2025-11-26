using System.ComponentModel.DataAnnotations;

namespace LottotryDataRecoveryApp.Models
{
    public class LottoType
    {
        [Key]
        public Guid Id { get; set; }
        public int LottoName { get; set; }

        public int DrawNumber { get; set; }

        public DateTime DrawDate { get; set; }

        public int NumberRange { get; set; }

        public ICollection<Number> Numbers { get; set; } = new List<Number>();
    }
}
