import climbgames.PipelineConfig
import climbgames.DefaultSettings

class CustomSettings extends DefaultSettings implements Serializable  {

    CustomSettings(PipelineConfig config) {
        super(config)
    }

    @Override
    void init() {

        super.init()

        // 
        //BuildTarget: ${config.buildTarget}
        //ProfileName: ${config.profileName}
        //BranchName: ${config.branchName}
        //BuildVersion: ${config.buildVersion}
        //VersionCode: ${config.versionCode}
        //BuildNumber: ${config.buildNumber}
        //IsContentUpdates: ${config.isContentUpdates}
        //UnityHome: ${config.unityHome}
        //ProjectPath: ${config.projectPath}
        //RelativeBuildPath: ${config.relativeBuildPath}
        //BuildFileName: ${config.buildFileName}
        //CustomArgs: ${getCustomArgs()}

        // example
        //executeMethod = "ClimbGames.Editor.CommandLineBuilder.BuildAndroid"

        //addCustomArg("profileName", config.profileName)
        //addCustomArg("branchName", config.branchName)
        //addCustomArg("buildVersion", config.buildVersion)
        //addCustomArg("versionCode", config.versionCode)
        //addCustomArg("buildNumber", config.buildNumber)
        //addCustomArg("isContentUpdates", config.isContentUpdates)
        //addCustomArg("relativeBuildPath", config.relativeBuildPath)
        //addCustomArg("buildFileName", config.buildFileName)
        

        // Please implement it if necessary.
        //
        //
    }
}

return { PipelineConfig config ->
    def settings = new CustomSettings(config)
    settings.init()
    return settings
}