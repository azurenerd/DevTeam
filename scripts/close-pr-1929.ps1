Push-Location src\AgentSquad.Runner
$secretOutput = dotnet user-secrets list 2>&1
Pop-Location
$tok = ($secretOutput | Select-String "GitHubToken" | Out-String).Split('=')[1].Trim()
$hdr = @{ Authorization = "Bearer $tok"; Accept = "application/vnd.github+json"; "X-GitHub-Api-Version" = "2022-11-28" }
$body = @{ state = "closed" } | ConvertTo-Json
$r = Invoke-RestMethod -Uri "https://api.github.com/repos/azurenerd/ReportingDashboard/pulls/1929" -Method Patch -Headers $hdr -Body $body
$r | Select-Object number, state, title
