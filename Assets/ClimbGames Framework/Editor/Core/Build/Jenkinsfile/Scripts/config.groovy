import groovy.transform.Field

@Field buildTarget
@Field branchName
@Field buildVersion
@Field versionCode
@Field isContentUpdates 
@Field unityHome
@Field projectPath

def setup() {
    
    buildTarget = params.BUILD_TARGET
    branchName = params.BRANCH_NAME
    buildVersion = params.BUILD_VERSION
    versionCode = params.VERSION_CODE
    isContentUpdates = params.IS_CONTENT_UPDATES
    unityHome = tool name: "${env.UNITY_NAME}", type: 'org.jenkinsci.plugins.unity3d.Unity3dInstallation'
    projectPath = "${env.WORKSPACE}"

    println "================================="
    println " BuildTarget: ${buildTarget}"
    println " BranchName: ${branchName}"
    println " BuildVersion: ${buildVersion}"
    println " VersionCode: ${versionCode}"
    println " IsContentUpdates: ${isContentUpdates}"
    println " UnityHome: ${unityHome}"
    println " ProjectPath: ${projectPath}"
    println "================================="
}

return this