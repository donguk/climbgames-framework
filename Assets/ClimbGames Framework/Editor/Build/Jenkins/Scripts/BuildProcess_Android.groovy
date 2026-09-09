import climbgames.DefaultProcess
import climbgames.IBuildSettings

class BuildProcess_Android extends DefaultProcess implements Serializable {

    @Override 
    void build(IBuildSettings settings) {

        super.build(settings)

        // Please implement it if necessary.
        //
        //
    }

    @Override 
    void deploy(IBuildSettings settings) {

        def config = settings.config
        def buildPath = "${config.buildPath}"
        
        config.script?.echo "buildPath: ${addressablePath}"
        config.script?.echo "Please implement the Deploy() method..."
    }
}

return new BuildProcess_Android()