using System.ComponentModel;

namespace Baymax.Dal.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ActionType
    {
        [Description("地址跳转")]
        JumpToAddress,
        [Description("获取元素")]
        GetElement,
        [Description("获取窗体")]
        GetWindow
    }
}