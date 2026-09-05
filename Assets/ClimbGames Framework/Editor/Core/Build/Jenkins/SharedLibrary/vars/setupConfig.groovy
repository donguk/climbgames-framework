import climbgames.PipelineConfig

def call(steps) {

    def config = new PipelineConfig(steps)

    config.init()
    return config
}