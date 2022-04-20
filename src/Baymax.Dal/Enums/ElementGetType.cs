using System.ComponentModel;
using Baymax.Dal.Attributes;

namespace Baymax.Dal.Enums
{
    [TypeConverter(typeof(EnumDescriptionTypeConverter))]
    public enum ElementGetType
    {
        [ActionName("Id")]
        ById,

        [ActionName("Name")]
        ByName,

        [ActionName("ClassName")]
        ByClassName,

        [ActionName("LinkText")]
        ByLinkText,

        [ActionName("XPath")]
        ByXPath,

        [ActionName("PartialLinkText")]
        ByPartialLinkText,

        [ActionName("TagName")]
        ByTagName,

        [ActionName("CssSelector")]
        ByCssSelector
    }
}