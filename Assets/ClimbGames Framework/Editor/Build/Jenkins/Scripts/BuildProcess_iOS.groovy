import climbgames.DefaultProcess
import climbgames.IBuildSettings

class BuildProcess_iOS extends DefaultProcess implements Serializable {

    @Override 
    void build(IBuildSettings settings) {

        settings.executeMethod = "ClimbGames.Editor.CommandLineBuilder.BuildiOS"
        //settings.addCustomArg("automaticSigning", true)

        super.build(settings)
    }

    @Override 
    void deploy(IBuildSettings settings) {

        def config = settings.config
        def buildPath = "${config.buildPath}"
        
        config.script?.echo "buildPath: ${addressablePath}"
        config.script?.echo "Please implement the Deploy() method..."
    }       
}

return new BuildProcess_iOS()