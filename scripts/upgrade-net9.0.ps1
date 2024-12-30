$projectsPath = "." # 替换为你的解决方案路径

# cd $projectsPath

# $gitStatus = git status --porcelain
# if ($gitStatus) {
#     Write-Host "检测到未提交的更改，请先提交或暂存以下文件：" -ForegroundColor Red
#     Write-Host $gitStatus
#     exit 1
# }
# else {
#     Write-Host "没有未提交的 Git 更改，继续执行脚本。" -ForegroundColor Green
# }





# 1. 查找并更新项目版本号
Write-Host "正在查找目录：$projectsPath" -ForegroundColor Cyan



Get-ChildItem -Path $projectsPath -Recurse -Filter *.csproj | ForEach-Object {
    $file = $_.FullName
    # 更新 TargetFramework 到 net9.0
    (Get-Content $file) -replace '<TargetFramework>.*<\/TargetFramework>', '<TargetFramework>net9.0</TargetFramework>' | Set-Content $file
    (Get-Content $file) -replace '<TargetFrameworks>.*<\/TargetFrameworks>', '<TargetFrameworks>net9.0</TargetFrameworks>' | Set-Content $file
   
}




Write-Host "脚本执行完成！" -ForegroundColor Cyan
