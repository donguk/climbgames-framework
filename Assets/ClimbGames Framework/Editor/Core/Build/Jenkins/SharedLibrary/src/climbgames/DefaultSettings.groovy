package climbgames

class DefaultSettings implements IBuildSettings {

    PipelineConfig config
    String executeMethod = "BuildScript.BuildApp"
    protected Map<String, Object> customArgMap = [:]

    DefaultSettings(PipelineConfig config) {
        
        this.config = config
    }

    void init() {
        
        config.script?.echo """
                            =================================
                             BuildTarget: ${config.buildTarget}
                             ProfileName: ${config.profileName}
                             BranchName: ${config.branchName}
                             BuildVersion: ${config.buildVersion}
                             VersionCode: ${config.versionCode}
                             BuildNumber: ${config.buildNumber}
                             IsContentUpdates: ${config.isContentUpdates}
                             UnityHome: ${config.unityHome}
                             ProjectPath: ${config.projectPath}
                             RelativeBuildPath: ${config.relativeBuildPath}
                             BuildFileName: ${config.buildFileName}
                             CustomArgs: ${getCustomArgs()}
                            =================================
        """.stripIndent()

        executeMethod = "ClimbGames.Editor.CommandLineBuilder.BuildAndroid"

        addCustomArg("profileName", config.profileName)
        addCustomArg("branchName", config.branchName)
        addCustomArg("buildVersion", config.buildVersion)
        addCustomArg("versionCode", config.versionCode)
        addCustomArg("buildNumber", config.buildNumber)
        addCustomArg("isContentUpdates", config.isContentUpdates)
        addCustomArg("relativeBuildPath", config.relativeBuildPath)
        addCustomArg("buildFileName", config.buildFileName)
    }

    DefaultSettings addCustomArg(String key, Object value) {
        if (key) {
            this.customArgMap[key.trim()] = value
        }
        return this
    }

    @Override
    String getCustomArgs() {

        if (!customArgMap) {
            return ""
        }
        return customArgMap.collect { key, value -> "$key:${value != null ? value : ''}"}
                            .join(',')
    }
}