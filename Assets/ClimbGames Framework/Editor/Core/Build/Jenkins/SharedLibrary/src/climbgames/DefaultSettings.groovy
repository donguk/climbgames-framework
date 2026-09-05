package climbgames

class DefaultSettings implements IBuildSettings {

    PipelineConfig config
    String executeMethod = "BuildScript.BuildApp"
    protected Map<String, Object> customArgMap = [:]

    DefaultSettings(PipelineConfig config) {
        this.config = config
    }

    DefaultSettings addCustomArg(String key, Object value) {
        if (key) {
            this.customArgMap[key.trim()] = value
        }
        return this
    }

    @Override
    String getCustomArgs() {
        if (!customArgMap) {
            return ""
        }

        return customArgMap.collect { key, value -> "$key:${value != null ? value : ''}"}
                            .join(',')
    }
}