# Code Signing Guide for Gamma Manager

Your app needs to be digitally signed for Microsoft Store submission. Here are your options:

## Issue
- Package: `GammaSetup.exe`
- Status: **Unsigned**
- Requirement: Must be signed with SHA256 or higher algorithm

---

## ✅ Recommended: Convert to MSIX (Easiest Path)

### Why MSIX?
- ✅ **Free code signing** by Microsoft Store
- ✅ **Free hosting** on Microsoft Store
- ✅ Automatic updates
- ✅ Better Windows 10/11 integration
- ✅ No certificate purchase needed

### Steps to Create MSIX:

#### Method 1: Using MSIX Packaging Tool (GUI - Easiest)

1. **Download MSIX Packaging Tool**
   - Get it from Microsoft Store
   - Or download: https://www.microsoft.com/store/productId/9N5LW3JBCXKF

2. **Run the Packaging Tool**
   - Open MSIX Packaging Tool
   - Select "Application package"
   - Choose "Create package on this computer"

3. **Package Your App**
   - Browse to: `Output/GammaSetup.exe`
   - Follow the wizard
   - Set app name: `Gamma Manager`
   - Set publisher: (Use your Microsoft Partner Center publisher ID)
   - Set version: `1.2.0.0`

4. **Configure**
   - Include all files from `bin/Release/`
   - Set entry point: `Gamma Manager.exe`
   - Add capabilities: "runFullTrust" (for full desktop access)

5. **Create Package**
   - Build the MSIX
   - Test the package
   - Submit to Microsoft Store

#### Method 2: Using MakeAppx Command Line

1. **Prepare folder structure:**
   ```
   MSIX_Package/
   ├── Gamma Manager.exe
   ├── *.dll (all dependencies)
   ├── frontend/
   ├── Package.appxmanifest (already created)
   └── Assets/
       ├── Square150x150Logo.png
       ├── Square44x44Logo.png
       ├── Wide310x150Logo.png
       └── SplashScreen.png
   ```

2. **Create MSIX:**
   ```powershell
   # From Windows SDK (usually at C:\Program Files (x86)\Windows Kits\10\bin\<version>\x64)
   makeappx pack /d "MSIX_Package" /p "GammaManager.msix"
   ```

3. **For testing (self-sign):**
   ```powershell
   # Create test certificate
   New-SelfSignedCertificate -Type Custom -Subject "CN=Test" -KeyUsage DigitalSignature -FriendlyName "Test Cert" -CertStoreLocation "Cert:\CurrentUser\My"
   
   # Sign package
   SignTool sign /fd SHA256 /a /f TestCert.pfx /p password GammaManager.msix
   ```

---

## Alternative: Use Azure Trusted Signing

If you want to keep the EXE/MSI format:

### Steps:

1. **Sign up for Azure Trusted Signing**
   - Go to: https://aka.ms/TrustedSigning
   - Create an Azure account (if needed)
   - Set up Trusted Signing service

2. **Install Azure Code Signing Tools**
   ```powershell
   # Install via Visual Studio or
   winget install Microsoft.AzureCodeSigning
   ```

3. **Sign Your Executable**
   ```powershell
   # After setup, sign with:
   AzureSignTool sign -kvu "https://your-vault.vault.azure.net/" `
                      -kvi "your-client-id" `
                      -kvt "your-tenant-id" `
                      -kvc "your-certificate-name" `
                      -fd sha256 `
                      -v "Output/GammaSetup.exe"
   ```

### Cost:
- ~$9.99/month for basic tier
- Includes signing operations

---

## Purchase Standard Code Signing Certificate

If you prefer traditional code signing:

### Providers:
- DigiCert (~$474/year)
- Sectigo (~$199/year)  
- GlobalSign (~$249/year)

### After Purchase:
```powershell
# Sign with SignTool (from Windows SDK)
signtool sign /f "YourCert.pfx" /p "password" /fd SHA256 /tr http://timestamp.digicert.com /td SHA256 "Output/GammaSetup.exe"
```

---

## 📝 What You Need to Do Now

**For Microsoft Store - Choose ONE:**

### ✅ Option A: MSIX (FREE - Recommended)
1. Delete current app from Partner Center
2. Use MSIX Packaging Tool on `Output/GammaSetup.exe`
3. Create new MSIX package
4. Submit MSIX to Store (Store will sign it)

### Option B: Azure Trusted Signing ($9.99/month)
1. Set up Azure Trusted Signing
2. Sign `Output/GammaSetup.exe` 
3. Re-submit signed EXE to Store

### Option C: Buy Certificate (~$199-474/year)
1. Purchase code signing cert
2. Sign executable with SignTool
3. Re-submit signed EXE to Store

---

## 🎯 Quick Recommendation

**Use MSIX** - It's free, and Microsoft Store handles signing automatically. You get all the benefits (updates, hosting, signing) at no cost.

## Next Steps:
1. Review the `Package.appxmanifest` file created
2. Download MSIX Packaging Tool from Microsoft Store
3. Package your app
4. Submit to Microsoft Store

## Resources:
- MSIX Packaging Tool: https://www.microsoft.com/store/productId/9N5LW3JBCXKF
- MSIX Documentation: https://docs.microsoft.com/windows/msix/
- Trusted Signing: https://aka.ms/TrustedSigning
- Partner Center: https://partner.microsoft.com/dashboard
