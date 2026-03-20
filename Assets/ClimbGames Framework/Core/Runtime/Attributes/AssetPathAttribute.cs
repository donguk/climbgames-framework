using System;

namespace ClimbGames.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class AssetPathAttribute : Attribute
    {
        public string Value { get; private set; }

        public AssetPathAttribute(string value)
        {
            Value = value;
        }
    }
}