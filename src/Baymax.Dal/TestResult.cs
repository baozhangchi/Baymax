using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Baymax.Dal
{
    public class TestResult
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int HistoryId { get; set; }

        [ForeignKey(nameof(HistoryId))]
        public virtual TestHistory History
        {
            get;
            set;
        }

        public bool VerificationResult { get; set; }

        public string Message { get; set; }

        public string ScreenshotPath { get; set; }
    }
}