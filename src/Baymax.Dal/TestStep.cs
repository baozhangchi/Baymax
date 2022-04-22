using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Runtime.CompilerServices;
using Baymax.Dal.Enums;

namespace Baymax.Dal
{
    public class TestStep : INotifyPropertyChanged
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int SortIndex { get; set; }

        [DisplayName("操作类型")]
        public ActionType ActionType { get; set; }

        [DisplayName("跳转地址")]
        public string JumpAddress { get; set; }

        [DisplayName("元素获取方式")]
        public ElementGetType? ElementGetType { get; set; }

        [DisplayName("元素获取值")]
        public string ElementGetValue { get; set; }

        [DisplayName("获取多个元素")]
        public bool GetMultipleElements { get; set; }

        [DisplayName("元素操作类型")]
        public ElementEvent? ElementEvent { get; set; }

        [DisplayName("是否要清除值")]
        public bool ClearValue { get; set; }

        [DisplayName("输入值")]
        public string EnterValue { get; set; }

        [DisplayName("验证类型")]
        public VerificationType VerificationType { get; set; }

        [DisplayName("验证值")]
        public string VerificationValue { get; set; }

        [DisplayName("是否输出截图")]
        public bool OutputScreenhot { get; set; }

        [DisplayName("截图父级级数(截图自身时为0)")]
        public uint ParentLevel { get; set; }

        public List<TestHistory> TestHistories { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}