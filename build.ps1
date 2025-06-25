dotnet clean -c Release
$FolderName = $((Get-Date).ToString('yyyy_MM_dd_hh_mm'))
dotnet publish WPFUI -c Release -p:PublishSingleFile=true -o $FolderName --self-contained
$Filename = $FolderName + ".zip"
$Argument = "-afzip" + " " + $Filename + " " + $FolderName
Start-Process -FilePath winrar -ArgumentList "a", $Argument -Wait
Remove-Item $FolderName -Recurse





