using System;

namespace Baymax.Dal.Attributes
{
    public class ActionNameAttribute : Attribute
    {
        public string ActionName { get; }

        public ActionNameAttribute(string actionName)
        {
            ActionName = actionName;
        }

        public static implicit operator string(ActionNameAttribute attribute) => attribute.ActionName;

        public static explicit operator ActionNameAttribute(string actionName) => new ActionNameAttribute(actionName);
    }
}