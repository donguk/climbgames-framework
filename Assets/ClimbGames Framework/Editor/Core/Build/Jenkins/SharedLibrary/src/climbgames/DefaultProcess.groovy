package climbgames

class DefaultBuildProcess implements IBuildProcess {

    @override 
    void build(IBuildSettings settings) {
        
        println "[${this.class.simpleName}] start: ${settings.config.buildTarget}"

        PipelineConfig config = settings.config
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
            -executeMethod ${settings.executeMethod} ^
            -customArgs:"${customArgs}" ^
            -logFile -
        """
    }

    @override 
    void deploy(IBuildSettings settings) {

        println "[${this.class.simpleName}] deploy: ${settings.config.buildTarget}"
    }
}