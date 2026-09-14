def call(script, buildType, buildVersion, buildNumber, ipaUrl, bundleID, appTitle) {

    def userAuth = 'climbgames-admin:climbgames2@'

    URL url = new URL(ipaUrl)
    def basePath = "${url.path}"
    def baseUrl = "${url.protocol}://${url.authority}${basePath.replaceAll(/\/[^\/]+$/, '')}"
    def ipaName = basePath.substring(basePath.lastIndexOf('/') + 1)
    
    script.echo """
                =================================
                 buildType: ${buildType}
                 buildVersion: ${buildVersion}
                 buildNumber: ${buildNumber}
                 ipaUrl: ${ipaUrl}
                 baseUrl: ${baseUrl}
                 ipaName: ${ipaName}
                =================================
    """.stripIndent()

    // 파일이동
    def remotePath = "${baseUrl}/build/${buildVersion}_${buildNumber}"
    script.sh """
        curl -u "${userAuth}" -i -X MKCOL "${baseUrl}/build"
        curl -u "${userAuth}" -i -X MKCOL "${baseUrl}/build/${buildVersion}_${buildNumber}"
        curl -u "${userAuth}" -i -X MOVE "${ipaUrl}" -H "Destination:${remotePath}/${ipaName}"
    """
                                        
    def plistContent = """
<?xml version="1.0" encoding="UTF-8"?>
<!DOCTYPE plist PUBLIC "-//Apple//DTD PLIST 1.0//EN" "http://www.apple.com/DTDs/PropertyList-1.0.dtd">
<plist version="1.0">
<dict>
	<key>items</key>
	<array>
		<dict>
			<key>assets</key>
			<array>
				<dict>
					<key>kind</key>
					<string>software-package</string>
					<key>url</key>
					<string>${remotePath}/${ipaName}</string>
				</dict>
			</array>
			<key>metadata</key>
			<dict>
				<key>bundle-identifier</key>
				<string>${bundleID}</string>
				<key>bundle-version</key>
				<string>${buildVersion}</string>
				<key>kind</key>
				<string>software</string>
				<key>platform-identifier</key>
				<string>com.apple.platform.iphoneos</string>
				<key>title</key>
				<string>${appTitle}</string>
			</dict>
		</dict>
	</array>
</dict>
</plist>
    """
                    
    script.writeFile file: 'manifest.plist', text: plistContent
    script.sh """
        curl -u "${userAuth}" -X PUT -T manifest.plist "${remotePath}/manifest.plist"
    """
    
    
    
    def htmlContent = """
<!DOCTYPE html>
<html lang="ko">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>BuildLab</title>
    <style>
        * {
            box-sizing: border-box;
        }

        html, body {
            margin: 0;
            width: 100%;
            height: 100vh;
            background: #111111;
            color: #a8a8a8;
            font-family: Arial, "Noto Sans KR", sans-serif;
        }

        body {
            display: flex;
            align-items: center;
            justify-content: center;
        }

        .container {
            text-align: center;
            line-height: 1.5;
            transform: translateY(-40px);
        }

        .title {
            margin: 0 0 18px;
            font-size: 34px;
            font-weight: 700;
            color: #b0b0b0;
        }

        .version,
        .number {
            margin: 3px 0;
            font-size: 18px;
            font-weight: 600;
            color: #8f8f8f;
        }

        .download {
            display: inline-block;
            margin-top: 22px;
            font-size: 19px;
            font-weight: 700;
            color: #4c8dcb;
            text-decoration: underline;
            text-underline-offset: 3px;
        }

        .download:hover {
            color: #72abe0;
        }
    </style>
</head>
<body>
    <div class="container">
        <div class="title">${appTitle}</div>
        <div class="version">version: ${buildVersion}</div>
        <div class="number">number: ${buildNumber}</div>
        <a class="download" href="${remotePath}/manifest.plist">download</a>
    </div>
</body>
</html>
    """
    
    
    script.writeFile file: 'index.html', text: htmlContent
    script.sh """
        curl -u "${userAuth}" -X PUT -T index.html "${remotePath}/index.html"
    """

    return "${remotePath}/index.html"
}