import climbgames.PipelineConfig

def call(script) {

    def config = new PipelineConfig(script)

    config.init()
    return config
}