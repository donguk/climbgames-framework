using System;

namespace ClimbGames.Core
{
    [AttributeUsage(AttributeTargets.Class, Inherited = true)]
    public class SingletonConfigAttribute : AssetPathAttribute
    {
        public bool DontDestroy { get; private set; }

        public SingletonConfigAttribute(bool dontDestroy) : base(null)
        {
            DontDestroy = dontDestroy;
        }

        public SingletonConfigAttribute(string assetPath, bool dontDestroy = true) : base(assetPath)
        {
            DontDestroy = dontDestroy;
        }
    }
}