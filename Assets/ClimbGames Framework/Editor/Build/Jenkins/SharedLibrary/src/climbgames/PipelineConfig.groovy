package climbgames

class PipelineConfig implements Serializable {

    transient Object script

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
    String buildFileName

    PipelineConfig(Object script) {

        this.script = script
    }

    void init() {

        buildTarget = script.params.BUILD_TARGET                
        branchName = script.params.BRANCH_NAME
        buildVersion = script.params.BUILD_VERSION
        versionCode = script.params.VERSION_CODE
        buildNumber = script.env.BUILD_NUMBER

        switch (branchName)
        {
            case 'develop': buildType = 'dev'; break
            default: buildType = branchName; break
        }
        profileName = script.params.PROFILE_NAME
        isContentUpdates = script.params.IS_CONTENT_UPDATES as boolean
        unityHome = script.tool(name: script.env.UNITY_NAME, type: 'org.jenkinsci.plugins.unity3d.Unity3dInstallation')

        projectPath = script.env.WORKSPACE
        relativeBuildPath = "Build"
        buildPath = "${projectPath}/${relativeBuildPath}/${buildTarget}"

        String productName = script.params.PRODUCT_NAME
        if (productName) {
            buildFileName = "${productName}_${buildType}_${buildVersion}(${versionCode})_${buildNumber}"
        } else {
            buildFileName = "Application_${buildType}_${buildVersion}(${versionCode})_${buildNumber}"
        }
    }
}