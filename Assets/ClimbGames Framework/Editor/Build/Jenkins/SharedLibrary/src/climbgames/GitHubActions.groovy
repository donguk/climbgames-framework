package climbgames

class GitHubActions implements Serializable {

    transient protected def script

    GitHubActions(def script) {

        this.script = script
    }

    void exportIPA(xcodeUrl, userAuth, teamID, p12Base64, p12Password, provisionBase64, buildType, buildVersion) {

        script.withCredentials([script.string(credentialsId: 'github-access-token', variable: 'GITHUB_TOKEN')]) {

            def githubToken = "${script.env.GITHUB_TOKEN}"
            def repoOwner   = 'donguk'
            def repoName    = 'workflow-build-pipeline'
            def uri = "https://api.github.com/repos/${repoOwner}/${repoName}/actions/workflows/ios-export-ipa.yml/dispatches"

            script.echo """
                        =================================
                         githubToken: ${githubToken}
                         repoOwner: ${repoOwner}
                         repoName: ${repoName}
                         uri: ${uri}
                         xcode: ${xcodeUrl}
                         buildType: ${buildType}
                         buildVersion: ${buildVersion}
                        =================================
            """.stripIndent()
            
            script.powershell '''
                $headers = @{
                    "Authorization" = "Bearer $env:GITHUB_TOKEN"
                    "Accept"        = "application/vnd.github+json"
                    "X-GitHub-Api-Version" = "2022-11-28"
                }
                
                $body = @{
                    ref = "main"
                    inputs = @{
                        xcode_download_url = "''' + xcodeUrl + '''"
                        file_server_user_auth = "''' + userAuth + '''"
                        team_id = "''' + teamID + '''"
                        p12_base64 = "''' + p12Base64 + '''"
                        p12_password = "''' + p12Password + '''"
                        provision_base64 = "''' + provisionBase64 + '''"
                        build_type = "''' + buildType + '''"
                        build_version = "''' + buildVersion + '''"
                        build_number = "$env:BUILD_NUMBER"
                    }
                } | ConvertTo-Json -Depth 3

                Invoke-RestMethod -Uri "''' + uri + '''" `
                    -Method Post `
                    -Headers $headers `
                    -ContentType "application/json" `
                    -Body $body
                '''
        }
    }
}