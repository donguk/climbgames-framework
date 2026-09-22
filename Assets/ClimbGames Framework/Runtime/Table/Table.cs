using UnityEngine;

namespace ClimbGames
{
    public interface ITable
    {
        void Initialize();
    }

    public abstract class Table : ScriptableObject
    {
        protected virtual void OnInitialized()
        {

        }
    }
}