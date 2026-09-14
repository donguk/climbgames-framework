package climbgames

class PipelineConfig implements Serializable {

    transient def script

    String buildTarget    
    String branchName
    String buildVersion
    String versionCode
    String buildNumber
    String buildType
    String profileName
    Boolean isContentUpdates
    String unityHome
    String projectPath
    String relativeBuildPath
    String buildPath
    String productName

    PipelineConfig(def script) {

        this.script = script
    }

    void init() {

        buildTarget = script.params.BUILD_TARGET                
        branchName = script.params.BRANCH_NAME
        buildVersion = script.params.BUILD_VERSION
        versionCode = script.params.VERSION_CODE
        buildNumber = script.env.BUILD_NUMBER
        productName = script.params.PRODUCT_NAME

        switch (branchName)
        {
            case 'qa': buildType = 'QA'; break;
            case 'live': buildType = 'Live'; break;
            default: buildType = 'Dev'; break
        }
        profileName = script.params.PROFILE_NAME
        isContentUpdates = script.params.IS_CONTENT_UPDATES as boolean
        unityHome = script.tool(name: script.env.UNITY_NAME, type: 'org.jenkinsci.plugins.unity3d.Unity3dInstallation')

        projectPath = script.env.WORKSPACE
        relativeBuildPath = "Build"
        buildPath = "${projectPath}/${relativeBuildPath}/${buildTarget}/${buildType}"
    }

    String getBuildFileName() {

        if (productName) {
            return "${productName}_${buildType}_${buildVersion}(${versionCode})_${buildNumber}"
        } 
        return "Application_${buildType}_${buildVersion}(${versionCode})_${buildNumber}"
    }
}