using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Baymax.Dal.Enums;

namespace Baymax.Dal
{
    public class TestHistory
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int StepId { get; set; }

        public long TimeStamp { get; set; }

        public int SortIndex { get; set; }

        [DisplayName("验证值")]
        public string VerificationValue { get; set; }

        [ForeignKey(nameof(StepId))]
        public virtual TestStep Step { get; set; }

        public List<TestResult> Results { get; set; }

        public static explicit operator TestHistory(TestStep step) => new TestHistory()
        {
            StepId = step.Id,
            SortIndex = step.SortIndex,
            VerificationValue = step.VerificationValue
        };
    }
}