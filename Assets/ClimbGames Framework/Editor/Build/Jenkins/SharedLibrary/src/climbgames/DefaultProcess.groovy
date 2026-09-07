package climbgames

class DefaultProcess implements IBuildProcess {

    @Override 
    void build(IBuildSettings settings) {
        
        PipelineConfig config = settings.config
        
        config.script.bat """
            "${config.unityHome}\\Unity.exe" ^
            -batchmode ^
            -quit ^
            -nographics ^
            -buildTarget ${config.buildTarget} ^
            -projectPath "${config.projectPath}" ^
            -executeMethod ${settings.executeMethod} ^
            -customArgs:"${settings.getCustomArgs()}" ^
            -logFile -
        """
    }

    @Override 
    void deploy(IBuildSettings settings) {

        println "[${this.class.simpleName}] deploy: ${settings.config.buildTarget}"
    }
}