namespace ClimbGames.Editor
{
    public abstract class Schema
    {
        public string Namespace { get; private set; }
        public abstract string ScriptName { get; }

        public Schema()
        {
            Namespace = FrameworkEditorSettings.Instance.ProjectNamesapce;
            if (string.IsNullOrEmpty(Namespace))
                Namespace = "ClimbGames";
        }
    }
}