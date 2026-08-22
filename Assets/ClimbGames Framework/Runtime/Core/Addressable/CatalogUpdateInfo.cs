using System.Collections.Generic;

namespace ClimbGames
{
    public struct CatalogUpdateInfo
    {
        public List<string> locators;
        public float downloadSize; // mb

        public bool IsExist => locators != null && locators.Count > 0;

        public override string ToString()
        {
            string text = string.Empty;
            if (locators != null)
            {
                for (int i = 0; i < locators.Count; ++i)
                {
                    if (i > 0)
                        text += ", \n";
                    text += locators[i];
                }
            }

            return @$"locators({text})
                downloadSize({downloadSize} MB)";
        }
    }
}