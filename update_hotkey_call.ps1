$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Update hotkey initialization to use new RegisterHotkeys method
$content = $content -replace 'hotkeyManager\.RegisterDefaultHotkeys\(\);', 'hotkeyManager.RegisterHotkeys();'

$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
