import climbgames.PipelineConfig
import climbgames.IBuildProcess
import climbgames.DefaultProcess

def call(config) {

    String scriptPath = "Jenkins/Scripts/BuildProcess_${config.buildType}.groovy"
    IBuildProcess buildProcess

    if (fileExists(scriptPath)) {
        buildProcess = load(scriptPath) as IBuildProcess
    } else {
        buildProcess = new DefaultProcess()
    }

    return buildProcess
}