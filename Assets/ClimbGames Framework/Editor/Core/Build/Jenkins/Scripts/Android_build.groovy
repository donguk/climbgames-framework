def start(config) {

    def arguments = [        
        profileName: config.profileName,
        branchName: config.branchName,
        buildVersion: config.buildVersion,
        versionCode: config.versionCode,
        buildNumber: config.buildNumber,
        isContentUpdates: config.isContentUpdates,
        relativeBuildPath: config.relativeBuildPath,
        buildFileName: config.buildFileName,
    ]
    def customArgs = arguments
                    .collect { k, v -> "$k:${v != null ? v : ''}" }
                    .join(',')

    bat """
        "${config.unityHome}\\Unity.exe" ^
        -batchmode ^
        -quit ^
        -nographics ^
        -buildTarget ${config.buildTarget} ^
        -projectPath "${config.projectPath}" ^
        -executeMethod ClimbGames.Editor.CommandLineBuilder.BuildAndroid ^
        -customArgs:"${customArgs}" ^
        -logFile -
    """
}

def deploy(config) {

    def addressablePath = "${config.buildPath}/Addressables/ServerData/${config.buildVersion}"
    def apkFile = "${config.buildPath}/${config.buildFileName}.apk"

    echo "addressablePath: ${addressablePath}"
    echo "apkFile: ${apkFile}"
    echo "Please implement the Deploy() method..."
}

return this