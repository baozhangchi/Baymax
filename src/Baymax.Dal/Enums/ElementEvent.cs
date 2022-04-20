using System.ComponentModel;

namespace Baymax.Dal.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ElementEvent
    {
        [Description("输入")]
        Enter,
        [Description("点击")]
        Click,
        [Description("验证")]
        Verification
    }
}