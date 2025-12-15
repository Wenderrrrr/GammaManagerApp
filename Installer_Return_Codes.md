# Installer Return Codes (Inno Setup)

This application uses Inno Setup for installation. The following return codes (exit codes) may be returned by the installer:

| Return Code | Description |
| :--- | :--- |
| **0** | **Success.** The installation was successful. |
| **1** | **Initialization Error.** The setup failed to initialize (e.g., not enough permissions or corrupted file). |
| **2** | **Cancelled.** The user clicked Cancel or closed the installer during the process. |
| **3** | **Fatal Error.** A fatal error occurred during installation. |
| **4** | **Restart Required.** The installation was successful, but a system restart is needed to complete the installation tasks. |
| **5** | **File Error.** A file error occurred (e.g., disk full or access denied). |
| **6** | **Uninstall Error.** Errors occurred during the uninstallation phase. |

## Notes
- The installer runs silently with `/VERYSILENT` and `/SUPPRESSMSGBOXES` flags.
- If a restart is required, code `4` is returned, and the Store wrapper handles the prompt.
