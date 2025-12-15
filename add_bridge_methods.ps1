$content = Get-Content 'Gamma Manager\Window.cs' -Raw

# Add the new methods after UpdateGammaFromWeb
$pattern = '(?s)(Gamma\.SetGammaRamp\(currDisplay\.displayLink,.*?currDisplay\.bBright\)\);.*?\}\)\);.*?}\s+)(public string GetStateJson)'
$newMethods = @'
$1
        public void UpdateContrastFromWeb(string channel, int value)
        {
            this.Invoke(new Action(() => {
                float floatVal = value / 100f;
                
                switch(channel)
                {
                    case "all":
                         currDisplay.rContrast = floatVal;
                         currDisplay.gContrast = floatVal;
                         currDisplay.bContrast = floatVal;
                         break;
                    case "red":
                         currDisplay.rContrast = floatVal;
                         break;
                    case "green":
                         currDisplay.gContrast = floatVal;
                         break;
                    case "blue":
                         currDisplay.bContrast = floatVal;
                         break;
                }
                
                Gamma.SetGammaRamp(currDisplay.displayLink,
                        Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma, currDisplay.rContrast,
                        currDisplay.gContrast, currDisplay.bContrast, currDisplay.rBright, currDisplay.gBright, currDisplay.bBright));
            }));
        }

        public void UpdateBrightnessFromWeb(string channel, int value)
        {
            this.Invoke(new Action(() => {
                float floatVal = value / 100f;
                
                switch(channel)
                {
                    case "all":
                         currDisplay.rBright = floatVal;
                         currDisplay.gBright = floatVal;
                         currDisplay.bBright = floatVal;
                         break;
                    case "red":
                         currDisplay.rBright = floatVal;
                         break;
                    case "green":
                         currDisplay.gBright = floatVal;
                         break;
                    case "blue":
                         currDisplay.bBright = floatVal;
                         break;
                }
                
                Gamma.SetGammaRamp(currDisplay.displayLink,
                        Gamma.CreateGammaRamp(currDisplay.rGamma, currDisplay.gGamma, currDisplay.bGamma, currDisplay.rContrast,
                        currDisplay.gContrast, currDisplay.bContrast, currDisplay.rBright, currDisplay.gBright, currDisplay.bBright));
            }));
        }

        $2
'@

$content = $content -replace $pattern, $newMethods
$content | Set-Content 'Gamma Manager\Window.cs' -NoNewline
