package climbgames

class PipelineConfig implements Serializable {

    final String buildTarget
    final String profileName
    final String branchName
    final String buildVersion
    final String versionCode
    final String buildNumber
    final boolean isContentUpdates
    final String unityHome
    final String projectPath
    final String relativeBuildPath
    final String buildPath
    final String buildFileName

    PipelineConfig(Object steps) {
        
        buildTarget = steps.params.BUILD_TARGET
        profileName = steps.params.PROFILE_NAME
        branchName = steps.params.BRANCH_NAME
        buildVersion = steps.params.BUILD_VERSION
        versionCode = steps.params.VERSION_CODE
        buildNumber = steps.env.BUILD_NUMBER
        isContentUpdates = steps.params.IS_CONTENT_UPDATES as boolean
        unityHome = steps.tool(name: steps.env.UNITY_NAME, type: 'org.jenkinsci.plugins.unity3d.Unity3dInstallation')

        projectPath = steps.env.WORKSPACE
        relativeBuildPath = "Build"
        buildPath = "${projectPath}/${relativeBuildPath}/${buildTarget}"

        String productName = steps.params.PRODUCT_NAME
        if (productName) {
            buildFileName = "${productName}_${branchName}_${buildVersion}(${versionCode})_${buildNumber}"
        } else {
            buildFileName = "Application_${branchName}_${buildVersion}(${versionCode})_${buildNumber}"
        }
        
        
        steps.echo "================================="
        steps.echo " BuildTarget: ${buildTarget}"
        steps.echo " ProfileName: ${profileName}"
        steps.echo " BranchName: ${branchName}"
        steps.echo " BuildVersion: ${buildVersion}"
        steps.echo " VersionCode: ${versionCode}"
        steps.echo " BuildNumber: ${buildNumber}"
        steps.echo " IsContentUpdates: ${isContentUpdates}"
        steps.echo " UnityHome: ${unityHome}"
        steps.echo " ProjectPath: ${projectPath}"
        steps.echo " RelativeBuildPath: ${relativeBuildPath}"
        steps.echo " BuildFileName: ${buildFileName}"
        steps.echo "================================="
    }
}