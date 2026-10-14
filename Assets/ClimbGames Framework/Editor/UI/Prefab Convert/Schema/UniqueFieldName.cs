using System.Collections.Generic;

namespace ClimbGames.Editor.UI
{
    public interface IUniquableName
    {
        IUniquableName Parent { get; }
        string UniqueName { get; }
    }

    public class UniqueNameGenerator
    {
        private Dictionary<string, int> nameCounts;

        public UniqueNameGenerator()
        {
            nameCounts = new Dictionary<string, int>();
        }

        public void Clear()
        {
            nameCounts.Clear();
        }

        public string GetName(IUniquableName uniquable)
        {
            string uniqueName = uniquable.UniqueName;

            bool isDuplicated = nameCounts.TryGetValue(uniqueName, out var duplicatedCount);
            while (isDuplicated)
            {
                IUniquableName parent = uniquable.Parent;
                if (parent == null || string.IsNullOrEmpty(parent.UniqueName))
                    break;

                uniqueName = $"{parent.UniqueName}_{uniqueName}";
                isDuplicated = nameCounts.TryGetValue(uniqueName, out duplicatedCount);
                uniquable = parent;
            }

            nameCounts[uniqueName] = duplicatedCount + 1;

            if (isDuplicated)
                uniqueName += $"_{duplicatedCount}";

            return uniqueName;
        }
    }
}