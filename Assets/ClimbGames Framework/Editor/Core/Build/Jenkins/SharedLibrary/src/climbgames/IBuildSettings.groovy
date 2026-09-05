package climbgames

interface IBuildSettings extends Serializable {

    PipelineConfig getConfig()
    String getExecuteMethod()
    String getCustomArgs()
}