
namespace ClimbGames
{
    public abstract class Singleton<T> where T : class, new()
    {
        private static readonly System.Lazy<T> _instance = new System.Lazy<T>(() => new T());

        public static T Instance => _instance.Value;
    }
}