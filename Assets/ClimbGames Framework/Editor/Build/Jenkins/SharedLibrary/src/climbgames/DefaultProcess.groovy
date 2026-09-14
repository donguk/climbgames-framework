package climbgames

class DefaultProcess implements IBuildProcess {

    transient protected def script

    void init(def script) {

        this.script = script
    }

    @Override 
    void build(IBuildSettings settings) {
        
        PipelineConfig config = settings.config
        
        script.bat """
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
    def deploy(IBuildSettings settings) {

        return "http://download_link"
    }    

    void deleteFile(String filePath) {

        if (script.fileExists(filePath)) {
            
            def deleteFilePath = filePath.replace('/', '\\')
            def cmd = "del /f /q ${deleteFilePath}"

            if (script.isUnix()) {
                script.sh(script: cmd, returnStatus: true)
            } else {
                script.bat(script: cmd, returnStatus: true)
            }
        }
    }
}