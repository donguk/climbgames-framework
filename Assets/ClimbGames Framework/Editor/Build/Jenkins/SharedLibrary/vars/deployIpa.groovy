def call(script, ipaUrl, buildType, buildVersion, buildNumber) {

    script.echo """
                ipaUrl: ${ipaUrl}
                buildType: ${buildType}
                buildVersion: ${buildVersion}
                buildNumber: ${buildNumber}
    """.stripIndent()
}