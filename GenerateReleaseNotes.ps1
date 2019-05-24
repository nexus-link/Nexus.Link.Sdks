$file = "$env:Build_ArtifactStagingDirectory\$env:Filename";
$title = "$env:Title";

Set-Content $file -Value "<!doctype html><html><head><meta charset='UTF-8'><title>$title</title>"
Add-Content $file "<style type='text/css'>body { margin: 2rem; font-family: sans-serif; }</style>"
Add-Content $file "</head><body><h1>$title</h1>"

$today = Get-Date -Format "yyyy-MM-dd"
Add-Content $file "<p><em>Generated $today</em></p>"

Get-Childitem . -Include *.csproj -Recurse -ErrorAction SilentlyContinue | ForEach-Object {
    $packageIdNode = Select-Xml -Path $_ -XPath "//PackageId";
    $packageId = $packageIdNode.Node.'#text';
    if (-not [string]::IsNullOrEmpty($packageId)) {

        $releaseNotes = Select-Xml -Path $_ -XPath "//PackageReleaseNotes";
        $releaseNotesFinal = $releaseNotes.Node."#text".Trim()
        $releaseNotes | Select-String -Pattern '(\d+\.\d+\.\d+)' -AllMatches | Foreach { $_.Matches } | Foreach {
            $version = $_.Value
            # http://fulcrum-nuget.azurewebsites.net/nuget/Packages(Id='Nexus.Link.BusinessEvents.Sdk',Version='1.0.8')
            $url = 'http://fulcrum-nuget.azurewebsites.net/nuget/Packages(Id=''' + $packageId + ''',Version=''' + $version + ''')'
            Try {
                $response = Invoke-RestMethod "$url"
                $releaseDate = $response.ChildNodes.updated[2]
                $releaseNotesFinal = $releaseNotesFinal.Replace($version, $version + " (" + $releaseDate.Substring(0, 10) + ")")
            } Catch {
                # It's ok, we skip the date for this version
                # Write-Error Error: $_.Exception.Message;
            }
        }
        Add-Content $file "<h2>$packageId</h2>"
        Add-Content $file "<pre>      $releaseNotesFinal</pre>"
    }
}

Add-Content $file '</body></html>'
