$content = Get-Content 'Gamma Manager\Window.cs' -Raw
$newMethod = @'
        public string GetStateJson()
        {
            // Simple manual JSON construction
            return string.Format(@"{{ 
                ""all"": {0}, 
                ""red"": {1},
                ""green"": {2},
                ""blue"": {3} 
            }}", 
                (int)(currDisplay.rGamma * 100),
                (int)(currDisplay.rGamma * 100),
                (int)(currDisplay.gGamma * 100),
                (int)(currDisplay.bGamma * 100));
        }
'@

# Find and replace the method
$pattern = '(?s)public string GetStateJson\(\).*?\r?\n\s*\}'
$content = $content -replace $pattern, $newMethod
$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
