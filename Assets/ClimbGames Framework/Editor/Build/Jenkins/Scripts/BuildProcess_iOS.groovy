import climbgames.DefaultProcess
import climbgames.IBuildSettings

class BuildProcess_iOS extends DefaultProcess implements Serializable {

    @Override 
    void build(IBuildSettings settings) {

        settings.executeMethod = "ClimbGames.Editor.CommandLineBuilder.BuildiOS"
            
        super.build(settings)

        // Please implement it if necessary.
        //
        //
    }

    @Override 
    def deploy(IBuildSettings settings) {

        def config = settings.config
        def buildPath = "${config.buildPath}"
        
        config.script?.echo "buildPath: ${addressablePath}"
        config.script?.echo "Please implement the Deploy() method..."

        return "http://download_link"
    }       
}

return new BuildProcess_iOS()