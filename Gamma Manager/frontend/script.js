document.addEventListener('DOMContentLoaded', () => {
    // Menu navigation
    const menuItems = document.querySelectorAll('.menu-item');
    const sections = document.querySelectorAll('.section');

    menuItems.forEach(item => {
        item.addEventListener('click', () => {
            const targetSection = item.dataset.section;

            // Update active menu item
            menuItems.forEach(mi => mi.classList.remove('active'));
            item.classList.add('active');

            // Show target section
            sections.forEach(section => section.classList.remove('active'));
            document.getElementById(`${targetSection}-section`).classList.add('active');
        });
    });

    // Monitor selection
    const monitorSelect = document.getElementById('monitor-select');
    monitorSelect.addEventListener('change', (e) => {
        const monitorIndex = parseInt(e.target.value);
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.hostObjects.bridge.SetMonitor(monitorIndex);
        }
    });

    // Populate monitor dropdown from C#
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.hostObjects.bridge.GetMonitorList().then(jsonString => {
            try {
                const monitors = JSON.parse(jsonString);
                monitorSelect.innerHTML = '';
                monitors.forEach((monitor, index) => {
                    const option = document.createElement('option');
                    option.value = index;
                    option.textContent = monitor;
                    monitorSelect.appendChild(option);
                });
            } catch (e) {
                console.error('Failed to parse monitor list:', e);
            }
        });
    }

    // Slider configuration matching C# logic
    const sliderConfig = {
        gamma: {
            sliders: ['gamma-all', 'gamma-red', 'gamma-green', 'gamma-blue'],
            format: (val) => (val / 100).toFixed(2),
            bridgeMethod: 'SetGamma'
        },
        contrast: {
            sliders: ['contrast-all', 'contrast-red', 'contrast-green', 'contrast-blue'],
            format: (val) => (val / 100).toFixed(2),
            bridgeMethod: 'SetContrast'
        },
        brightness: {
            sliders: ['brightness-all', 'brightness-red', 'brightness-green', 'brightness-blue'],
            format: (val) => (val / 100).toFixed(2),
            bridgeMethod: 'SetBrightness'
        }
    };

    // Setup all sliders
    Object.entries(sliderConfig).forEach(([type, config]) => {
        config.sliders.forEach(sliderId => {
            const slider = document.getElementById(sliderId);
            const display = document.getElementById(`val-${sliderId}`);

            if (!slider || !display) return;

            slider.addEventListener('input', (e) => {
                const val = parseInt(e.target.value);
                display.textContent = config.format(val);

                // Determine channel (all, red, green, blue)
                const channel = sliderId.split('-')[1];

                // Send to C# backend
                if (window.chrome && window.chrome.webview) {
                    window.chrome.webview.hostObjects.bridge[config.bridgeMethod](channel, val);
                }
            });
        });
    });

    // Hotkey assignment
    let currentPresetForHotkey = null;

    function showHotkeyDialog(presetName) {
        currentPresetForHotkey = presetName;
        const dialog = document.getElementById('hotkey-dialog');
        const presetNameSpan = document.getElementById('hotkey-preset-name');
        presetNameSpan.textContent = presetName;
        dialog.style.display = 'flex';
        document.getElementById('hotkey-capture').focus();
    }

    function closeHotkeyDialog() {
        document.getElementById('hotkey-dialog').style.display = 'none';
        currentPresetForHotkey = null;
    }

    document.getElementById('hotkey-cancel').addEventListener('click', closeHotkeyDialog);

    document.getElementById('hotkey-capture').addEventListener('keydown', (e) => {
        e.preventDefault();

        let modifiers = 0;
        if (e.ctrlKey) modifiers |= 2;  // MOD_CONTROL
        if (e.altKey) modifiers |= 1;   // MOD_ALT
        if (e.shiftKey) modifiers |= 4; // MOD_SHIFT

        const key = e.keyCode;

        // Display the combination
        let combo = [];
        if (e.ctrlKey) combo.push('Ctrl');
        if (e.altKey) combo.push('Alt');
        if (e.shiftKey) combo.push('Shift');
        combo.push(e.key.toUpperCase());

        document.getElementById('hotkey-display').textContent = combo.join('+');

        // Save button
        document.getElementById('hotkey-save').onclick = () => {
            if (window.chrome && window.chrome.webview && currentPresetForHotkey) {
                window.chrome.webview.hostObjects.bridge.SetHotkey(currentPresetForHotkey, modifiers, key);
                closeHotkeyDialog();
                loadPresetList();
            }
        };
    });

    // Preset Management
    function loadPresetList() {
        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.hostObjects.bridge.GetPresetList().then(jsonString => {
                try {
                    const presets = JSON.parse(jsonString);
                    const presetList = document.getElementById('preset-list');

                    if (presets.length === 0) {
                        presetList.innerHTML = '<p class="empty-message">No presets saved yet</p>';
                    } else {
                        presetList.innerHTML = '';
                        presets.forEach((preset, index) => {
                            // Get hotkey for this preset
                            window.chrome.webview.hostObjects.bridge.GetHotkeyForPreset(preset).then(hotkeyStr => {
                                const presetItem = document.createElement('div');
                                presetItem.className = 'preset-item';
                                presetItem.innerHTML = `
                                    <div class="preset-info">
                                        <span class="preset-number">${index + 1}</span>
                                        <span class="preset-name">${preset}</span>
                                        <span class="preset-hotkey">${hotkeyStr || 'No hotkey'}</span>
                                    </div>
                                    <div class="preset-buttons">
                                        <button class="btn-hotkey" data-preset="${preset}">Set Hotkey</button>
                                        <button class="btn-load" data-preset="${preset}">Load</button>
                                        <button class="btn-delete" data-preset="${preset}">Delete</button>
                                    </div>
                                `;
                                presetList.appendChild(presetItem);
                            });
                        });

                        // Add event listeners after all items are added
                        setTimeout(() => {
                            document.querySelectorAll('.btn-hotkey').forEach(btn => {
                                btn.addEventListener('click', () => {
                                    showHotkeyDialog(btn.dataset.preset);
                                });
                            });

                            document.querySelectorAll('.btn-load').forEach(btn => {
                                btn.addEventListener('click', () => {
                                    const presetName = btn.dataset.preset;
                                    window.chrome.webview.hostObjects.bridge.LoadPreset(presetName);
                                });
                            });

                            document.querySelectorAll('.btn-delete').forEach(btn => {
                                btn.addEventListener('click', () => {
                                    const presetName = btn.dataset.preset;
                                    if (confirm(`Delete preset "${presetName}"?`)) {
                                        window.chrome.webview.hostObjects.bridge.DeletePreset(presetName);
                                        loadPresetList();
                                    }
                                });
                            });
                        }, 100);
                    }
                } catch (e) {
                    console.error('Failed to parse preset list:', e);
                }
            });
        }
    }

    // Save preset button
    document.getElementById('btn-save-preset').addEventListener('click', () => {
        const nameInput = document.getElementById('preset-name-input');
        const presetName = nameInput.value.trim();

        if (!presetName) {
            alert('Please enter a preset name');
            return;
        }

        if (window.chrome && window.chrome.webview) {
            window.chrome.webview.hostObjects.bridge.SavePreset(presetName);
            nameInput.value = '';
            loadPresetList();
        }
    });

    // Load presets when switching to preset section
    menuItems.forEach(item => {
        if (item.dataset.section === 'presets') {
            item.addEventListener('click', loadPresetList);
        }
    });

    // Reset button
    document.getElementById('btn-reset').addEventListener('click', () => {
        // Reset gamma to 1.0 (value = 100)
        sliderConfig.gamma.sliders.forEach(sliderId => {
            const slider = document.getElementById(sliderId);
            const display = document.getElementById(`val-${sliderId}`);
            slider.value = 100;
            display.textContent = '1.00';

            const channel = sliderId.split('-')[1];
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.hostObjects.bridge.SetGamma(channel, 100);
            }
        });

        // Reset contrast to 1.0 (value = 100)
        sliderConfig.contrast.sliders.forEach(sliderId => {
            const slider = document.getElementById(sliderId);
            const display = document.getElementById(`val-${sliderId}`);
            slider.value = 100;
            display.textContent = '1.00';

            const channel = sliderId.split('-')[1];
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.hostObjects.bridge.SetContrast(channel, 100);
            }
        });

        // Reset brightness to 0.0 (value = 0)
        sliderConfig.brightness.sliders.forEach(sliderId => {
            const slider = document.getElementById(sliderId);
            const display = document.getElementById(`val-${sliderId}`);
            slider.value = 0;
            display.textContent = '0.00';

            const channel = sliderId.split('-')[1];
            if (window.chrome && window.chrome.webview) {
                window.chrome.webview.hostObjects.bridge.SetBrightness(channel, 0);
            }
        });
    });

    // Initialize from backend
    if (window.chrome && window.chrome.webview) {
        window.chrome.webview.hostObjects.bridge.GetInitialState().then(jsonString => {
            try {
                const state = JSON.parse(jsonString);
                // Update UI with initial values if needed
            } catch (e) {
                console.error('Failed to parse initial state:', e);
            }
        });
    }

    // Hide loading screen after everything is initialized
    // Add a small delay to ensure smooth transition
    setTimeout(() => {
        const loadingScreen = document.getElementById('loading-screen');
        if (loadingScreen) {
            loadingScreen.classList.add('hidden');
            // Remove from DOM after transition completes
            setTimeout(() => {
                loadingScreen.remove();
            }, 500);
        }
    }, 300);
});
