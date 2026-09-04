def start(config) {

    def arguments = [
        profileName: config.profileName,
        branchName: config.branchName,
        buildVersion: config.buildVersion,
        versionCode: config.versionCode,
        buildNumber: config.buildNumber,
        isContentUpdates: config.isContentUpdates
    ]
    def customArgs = arguments.collect { k, v -> "$k:$v" }.join(',')

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

return this