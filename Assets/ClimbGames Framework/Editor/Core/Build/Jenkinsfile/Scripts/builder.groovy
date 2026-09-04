def platformBuild() {

    def config = load 'Jenkins/Scripts/config.groovy'
    config.setup()

    switch(config.buildTarget)
    {
        case 'Android':
        case 'iOS':
            def build = load "Jenkins/Scripts/${config.buildTarget}_build.groovy"
            build.start(config)
            break;

        default:
            error "Unsupported BUILD_TARGET '${config.buildTarget}'. Please check the parameter value or add the required Groovy script under 'Jenkins/Scripts/'."
            break;
    }
}

return this