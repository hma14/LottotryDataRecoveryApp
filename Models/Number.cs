using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace LottotryDataRecoveryApp.Models
{
    public class Number
    {
        [Key]
        public Guid Id { get; set; }

        public int Value { get; set; }

        [JsonIgnore]
        [IgnoreDataMember]
        [ForeignKey("LottoType")]
        public Guid? LottoTypeId { get; set; }
        public LottoType LottoType { get; set; }

        public int Distance { get; set; }

        public bool IsHit { get; set; }

        public int NumberofDrawsWhenHit { get; set; }

        public bool IsBonusNumber { get; set; }
        public int TotalHits { get; set; }

        //  1 - 5
        public int Probability { get; set; }
    }
}
