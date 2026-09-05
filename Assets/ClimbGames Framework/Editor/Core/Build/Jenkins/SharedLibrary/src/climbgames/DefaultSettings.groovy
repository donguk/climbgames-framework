package climbgames

class DefaultSettings implements IBuildSettings {

    PipelineConfig config
    String executeMethod = "BuildScript.BuildApp"
    protected Map<String, Object> customArgMap = [:]

    DefaultSettings(PipelineConfig config) {
        
        this.config = config
    }

    void init() {
        
        println "================================="
        println " BuildTarget: ${config.buildTarget}"
        println " ProfileName: ${config.profileName}"
        println " BranchName: ${config.branchName}"
        println " BuildVersion: ${config.buildVersion}"
        println " VersionCode: ${config.versionCode}"
        println " BuildNumber: ${config.buildNumber}"
        println " IsContentUpdates: ${config.isContentUpdates}"
        println " UnityHome: ${config.unityHome}"
        println " ProjectPath: ${config.projectPath}"
        println " RelativeBuildPath: ${config.relativeBuildPath}"
        println " BuildFileName: ${config.buildFileName}"
        println "================================="

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