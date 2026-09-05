package climbgames

class DefaultProcess implements IBuildProcess {

    @Override 
    void build(IBuildSettings settings) {
        
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

        def steps = config.steps
        steps.bat """
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

    @Override 
    void deploy(IBuildSettings settings) {

        println "[${this.class.simpleName}] deploy: ${settings.config.buildTarget}"
    }
}