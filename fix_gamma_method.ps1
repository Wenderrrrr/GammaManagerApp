$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Find and replace the UpdateGammaFromWeb method
$pattern = '(?s)// Bridge Methods\s+public void UpdateGammaFromWeb\(string channel, int value\).*?}\s+}\)'
$newMethod = @'
// Bridge Methods
        public void UpdateGammaFromWeb(string channel, int value)
        {
            // Run on UI thread
            this.Invoke(new Action(() => {
                // Value comes from web slider (30-440 range, matching original trackBarGamma)
                // Convert to float: value / 100f (so 100 = 1.0 gamma)
                float floatVal = value / 100f;
                
                switch(channel)
                {
                    case "all":
                         currDisplay.rGamma = floatVal;
                         currDisplay.gGamma = floatVal;
                         currDisplay.bGamma = floatVal;
                         break;
                    case "red":
                         currDisplay.rGamma = floatVal;
                         break;
                    case "green":
                         currDisplay.gGamma = floatVal;
                         break;
                    case "blue":
                         currDisplay.bGamma = floatVal;
                         break;
                }
                
                // Apply using the original Gamma.SetGammaRamp logic
                Gamma.SetGammaRamp(currDisplay.displayLink,
                        Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma, currDisplay.rContrast,
                        currDisplay.gContrast, currDisplay.bContrast, currDisplay.rBright, currDisplay.gBright, currDisplay.bBright));
            }))
'@

$content = $content -replace $pattern, $newMethod
$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
