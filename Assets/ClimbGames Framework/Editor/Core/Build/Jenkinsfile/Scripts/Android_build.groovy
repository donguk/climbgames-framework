def start(config) {

    def customArgs = """
        profileName: ,
        branchName:${config.branchName},
        buildVersion:${config.buildVersion},
        versionCode:${config.versionCode},
        isContentUpdates:${config.isContentUpdates}
        """

    bat """
        "${config.unityHome}\\Unity.exe" ^
        -batchmode ^
        -quit ^
        -nographics ^
        -buildTarget ${config.buildTarget} ^
        -projectPath "${config.projectPath}" ^
        -executeMethod ClimbGames.Editor.CommandLineBuilder.BuildAndroid() ^
        -customArgs:"${customArgs}" ^
        -logFile -
    """
}

return this