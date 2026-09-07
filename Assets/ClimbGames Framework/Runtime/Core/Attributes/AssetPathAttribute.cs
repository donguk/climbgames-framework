using System;

namespace ClimbGames
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AssetPathAttribute : Attribute
    {
        public string Value { get; private set; }
        public bool IsPreloaded { get; private set; }

        public AssetPathAttribute(string value, bool isPreloaded = false)
        {
            Value = value;
            IsPreloaded = isPreloaded;
        }
    }
}