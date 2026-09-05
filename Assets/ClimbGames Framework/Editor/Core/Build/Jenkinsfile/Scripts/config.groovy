import groovy.transform.Field

@Field buildTarget
@Field profileName
@Field branchName
@Field buildVersion
@Field versionCode
@Field buildNumber
@Field isContentUpdates
@Field unityHome
@Field projectPath
@Field relativeBuildPath
@Field buildPath
@Field buildFileName

def setup() {
    
    buildTarget = params.BUILD_TARGET
    branchName = params.BRANCH_NAME
    buildVersion = params.BUILD_VERSION
    versionCode = params.VERSION_CODE
    buildNumber = env.BUILD_NUMBER
    isContentUpdates = params.IS_CONTENT_UPDATES
    unityHome = tool name: "${env.UNITY_NAME}", type: 'org.jenkinsci.plugins.unity3d.Unity3dInstallation'
    projectPath = "${env.WORKSPACE}"
    relativeBuildPath = "Build"
    buildPath = "${projectPath}/${relativeBuildPath}/${buildTarget}"

    if (params.PRODUCT_NAME != null)
        buildFileName = "${params.PRODUCT_NAME}_${branchName}_${buildVersion}(${versionCode})_${buildNumber}"
    else
        buildFileName = "Application_${branchName}_${buildVersion}(${versionCode})_${buildNumber}"

    println "================================="
    println " BuildTarget: ${buildTarget}"
    println " BranchName: ${branchName}"
    println " BuildVersion: ${buildVersion}"
    println " VersionCode: ${versionCode}"
    println " BuildNumber: ${buildNumber}"
    println " IsContentUpdates: ${isContentUpdates}"
    println " UnityHome: ${unityHome}"
    println " ProjectPath: ${projectPath}"
    println " relativeBuildPath: ${relativeBuildPath}"
    println " BuildFileName: ${buildFileName}"
    println "================================="

    def scriptPath = "Jenkins/Scripts/${buildTarget}_build.groovy"
    if (!fileExists(scriptPath))
        error "Unsupported BUILD_TARGET '${buildTarget}'. Please check parameter or add '${scriptPath}'."
    
    return load (scriptPath)
}

return this