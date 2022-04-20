using System.ComponentModel;

namespace Baymax.Dal
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum VerificationType
    {
        [Description("等于")]
        Equal,
        [Description("不等于")]
        NotEqual,
        [Description("包含")]
        Contain,
        [Description("不包含")]
        NotContain
    }
}