$tempFile = [System.IO.Path]::GetTempFileName() + ".js"
$initScript = 'rs.initiate({ _id: "rs0", members: [{ _id: 0, host: "localhost:27017" }] })'
Set-Content -Path $tempFile -Value $initScript
& "C:\Users\adnan.mansuri\AppData\Local\Programs\mongosh\mongosh.exe" --file $tempFile
Remove-Item $tempFile -Force

