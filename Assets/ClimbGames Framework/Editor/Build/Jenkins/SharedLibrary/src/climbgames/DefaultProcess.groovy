package climbgames

class DefaultProcess implements IBuildProcess {

    transient protected def script

    void init(def script) {

        this.script = script
    }

    @Override 
    void build(IBuildSettings settings) {
        
        PipelineConfig config = settings.config
        
        config.script.bat """
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
    void deploy(IBuildSettings settings) {

        script.echo "[${this.class.simpleName}] deploy: ${settings.config.buildTarget}"
    }

    void uploadToHfs(String sourcePath, String remoteUrl, String userAuth = '') {

        // 문자열 끝에 붙어 있는 슬래시(/)를 모두 제거
        remoteUrl = remoteUrl.replaceAll('/+$', '')

        // dir 블록으로 대상 폴더 진입 후 탐색
        script.dir(sourcePath) {
            def files = script.findFiles(glob: '**/*')

            // 1. 필요한 디렉터리 경로만 Set으로 중복 제거 수집
            Set<String> folderUrlsToCreate = []

            for (def file in files) {
                if (file.directory) continue

                def relativePath = file.path.replace('\\', '/')
                if (relativePath.contains('/')) {
                    def parentPath = relativePath.substring(0, relativePath.lastIndexOf('/'))
                    folderUrlsToCreate.add("${remoteUrl}/${parentPath}")
                }
            }

            // 2. 디렉터리 선제 생성 (HFS가 상위 폴더를 자동 생성해 주므로 최하위 폴더만 호출)
            for (String folderUrl in folderUrlsToCreate) {
                createRemoteFolder(folderUrl, userAuth)
            }

            for (def file in files) {
                if (file.directory) continue

                // dir 블록 안에서 findFiles 실행 시 file.path는 상대 경로로 나옵니다 (예: "catalog.json", "aa/test.bundle")
                def relativePath = file.path.replace('\\', '/')
                def fileRemoteUrl = "${remoteUrl}/${relativePath}"

                // curl PUT 업로드 (file.path는 현재 dir 기준 상대 경로)
                if (script.isUnix()) {
                    script.sh "curl -s -f -u '${userAuth}' -X PUT --data-binary '@${relativePath}' '${fileRemoteUrl}'"
                } else {
                    script.bat "curl -s -f -u \"${userAuth}\" -X PUT --data-binary \"@${relativePath}\" \"${fileRemoteUrl}\""
                }
            }
        }      
    }

    void uploadFileToHfs(String filePath, String remoteUrl, String userAuth = '') {

        // dir 블록 안에서 findFiles 실행 시 file.path는 상대 경로로 나옵니다 (예: "catalog.json", "aa/test.bundle")
        def normalizedPath = filePath.replace('\\', '/')

        // 경로에서 파일명만 추출 (예: "a/b/catalog.json" -> "catalog.json")
        def fileName = normalizedPath.contains('/') 
                        ? normalizedPath.substring(normalizedPath.lastIndexOf('/') + 1) 
                        : normalizedPath

        def fileRemoteUrl = "${remoteUrl}/${fileName}"

        // curl PUT 업로드 (file.path는 현재 dir 기준 상대 경로)
        if (script.isUnix()) {
            script.sh "curl -s -f -u '${userAuth}' -X PUT --data-binary '@${filePath}' '${fileRemoteUrl}'"
        } else {
            script.bat "curl -s -f -u \"${userAuth}\" -X PUT --data-binary \"@${filePath}\" \"${fileRemoteUrl}\""
        }
    }

    void createRemoteFolder(String folderUrl, String userAuth = '') {

        def cmd = "curl -s -o /dev/null -w \"%{http_code}\" -u \"${userAuth}\" -X MKCOL \"${folderUrl}\""
        
        if (script.isUnix()) {
            script.sh(script: cmd, returnStatus: true)
        } else {
            script.bat(script: cmd, returnStatus: true)
        }
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

    void githubAction_ExportIpa(xcode_download_url, team_id, p12_base64, p12_password, provision_base64) {
        
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
                         xcode: ${xcode_download_url}
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
                        xcode_download_url = "''' + xcode_download_url + '''"
                        team_id = "''' + team_id + '''"
                        p12_base64 = "''' + p12_base64 + '''"
                        p12_password = "''' + p12_password + '''"
                        provision_base64 = "''' + provision_base64 + '''"
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