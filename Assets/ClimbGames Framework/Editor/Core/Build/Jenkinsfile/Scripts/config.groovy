import groovy.transform.Field

@Field buildTarget
@Field buildType
@Field buildVersion
@Field versionCode
@Field unityHome
@Field projectPath

def setup() {
    
    buildTarget = params.BUILD_TARGET
    buildType = params.BRANCH_NAME
    buildVersion = params.BUILD_VERSION
    versionCode = params.VERSION_CODE
    unityHome = tool name: "${env.UNITY_NAME}", type: 'org.jenkinsci.plugins.unity3d.Unity3dInstallation'
    projectPath = "${env.WORKSPACE}"

    println "================================="
    println " BuildTarget: ${buildTarget}"
    println " BuildType: ${buildType}"
    println " BuildVersion: ${buildVersion}"
    println " VersionCode: ${versionCode}"
    println " UnityHome: ${unityHome}"
    println " ProjectPath: ${projectPath}"
    println "================================="
}

return this