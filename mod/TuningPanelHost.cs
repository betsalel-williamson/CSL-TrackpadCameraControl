#if HAS_CITIES
using System;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Floating in-game Debug / tuning panel host (ColossalUI).
    /// Product-surface feel controls; gated chrome / CAD / Contacts via ENABLE_* compile symbols.
    /// </summary>
    internal static partial class TuningPanelHost
    {
        private const float PanelWidth = 560f;
        private const float TitleBarHeight = 32f;
        private const float FieldGutter = 12f;
        private const float Col0 = FieldGutter;
        private const float FieldLabelW = 132f;
        private const float FieldInputW = 72f;
        private const float FieldLabelGap = 4f;
        private const float FieldColumnW = FieldLabelW + FieldLabelGap + FieldInputW;

        // Symmetric left/right gutters: right column starts so its input ends at PanelWidth - FieldGutter.
        private const float Col1 = PanelWidth - FieldGutter - FieldColumnW;
        private const float ColWidth = FieldColumnW;
        private const float HeaderButtonSize = 32f;
        private const float HeaderButtonRestOpacity = 0.55f;
        private const float FooterCopyButtonWidth = 64f;
        private const float FooterCopyButtonHeight = 28f;

        private static UIPanel _root;
        private static UIPanel _titleBar;
        private static UIButton _closeButton;
        private static UIButton _optionsButton;
        private static UIButton _reopen;
        private static UILabel _presetDesc;
        private static UILabel _title;
        private static UITextField _feelNameField;
        private static UIDropDown _feelDropdown;
        private static string[] _feelDropdownItems;
        private static float _nextY;
        private static bool _dragging;
        private static bool _handlingSettingsChanged;
        private static Vector3 _dragPanelStart;
        private static Vector3 _dragMouseStart;

        public static void EnsureCreated()
        {
            if (_root != null)
            {
                return;
            }

            UIView view = UIView.GetAView();
            if (view == null)
            {
                return;
            }

            ModOptions.SettingsChanged -= OnSettingsChanged;
            ModOptions.SettingsChanged += OnSettingsChanged;

            _root = view.AddUIComponent(typeof(UIPanel)) as UIPanel;
            if (_root == null)
            {
                return;
            }

            _root.name = "TrackpadCameraDebugPanel";
            _root.backgroundSprite = "MenuPanel2";
            _root.width = PanelWidth;
            _root.height = 420f;
            _root.relativePosition = new Vector3(40f, 60f);
            _root.canFocus = true;
            _root.isInteractive = true;
            _root.eventMouseDown += (c, e) =>
            {
                _root.BringToFront();
            };
            _root.eventMouseUp += OnTitleMouseUp;

            BuildTitleBar();

            _nextY = TitleBarHeight + 8f;
            ModSettings s = Mod.EnsureSettings();

            AddSection("Feel presets");
            AddFeelPresetRow(s);

#if ENABLE_CAD_GESTURE_STYLE
            _presetDesc = _root.AddUIComponent<UILabel>();
            _presetDesc.textColor = Color.white;
            _presetDesc.relativePosition = new Vector3(Col0, _nextY);
            _presetDesc.width = PanelWidth - 24f;
            _presetDesc.autoSize = false;
            _presetDesc.autoHeight = true;
            _presetDesc.wordWrap = true;
            _presetDesc.text = ModOptions.PresetDescription(s.GesturePreset);
            _presetDesc.PerformLayout();
            float descH = Mathf.Max(36f, _presetDesc.height + 4f);
            _nextY += descH;

            AddSection("Gesture style");
            AddCadStyleButtons(s);
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddSection("Capture");
            AddCaptureBackendButtons(s);
#endif

            // Shipped section order: Zoom → Pan → Rotate → Orbit (feel presets = General above).
            BuildZoomSection(s);
            BuildPanSection(s);
            BuildYawSection(s);
            BuildOrbitSection(s);

            AddBuildInfoFooter();

            _root.height = Mathf.Min(720f, _nextY + 16f);

            _reopen = view.AddUIComponent(typeof(UIButton)) as UIButton;
            if (_reopen != null)
            {
                _reopen.name = "TrackpadCameraDebugReopen";
                _reopen.text = "Debug";
                _reopen.width = 90f;
                _reopen.height = 28f;
                _reopen.relativePosition = new Vector3(40f, 40f);
                _reopen.normalBgSprite = "ButtonMenu";
                _reopen.hoveredBgSprite = "ButtonMenuHovered";
                _reopen.pressedBgSprite = "ButtonMenuPressed";
                _reopen.eventClick += (c, e) => ShowPanel();
                _reopen.isVisible = false;
            }
        }

        public static void ApplyVisibility()
        {
            ModSettings s = Mod.Settings;
            bool assistEnabled = s != null && s.AssistUiEnabled;
            bool dismissed = s != null && s.DebugPanelDismissed;
            bool showRoot = ShouldShowRoot(assistEnabled, dismissed);
            bool showReopen = ShouldShowReopen(assistEnabled, dismissed);
            if (_root != null)
            {
                _root.isVisible = showRoot;
            }

            if (_reopen != null)
            {
                _reopen.isVisible = showReopen;
            }
        }

        public static void Destroy()
        {
            ModOptions.SettingsChanged -= OnSettingsChanged;

            if (_root != null)
            {
                UnityEngine.Object.Destroy(_root.gameObject);
                _root = null;
            }

            if (_reopen != null)
            {
                UnityEngine.Object.Destroy(_reopen.gameObject);
                _reopen = null;
            }

            _titleBar = null;
            _closeButton = null;
            _optionsButton = null;
            _presetDesc = null;
            _title = null;
            _feelNameField = null;
            _feelDropdown = null;
            _feelDropdownItems = null;
            _dragging = false;
        }

        private static void OnSettingsChanged()
        {
            if (_handlingSettingsChanged)
            {
                return;
            }

            _handlingSettingsChanged = true;
            try
            {
                if (_root == null)
                {
                    ApplyVisibility();
                    return;
                }

                Vector3 pos = _root.relativePosition;
                Destroy();
                EnsureCreated();
                if (_root != null)
                {
                    _root.relativePosition = pos;
                }

                // Do not restore prior isVisible — ApplyVisibility owns root + reopen from
                // AssistUiEnabled and dismiss state (OPTIONS off must hide the Debug chip).
                ApplyVisibility();
            }
            finally
            {
                _handlingSettingsChanged = false;
            }
        }

        private static void ShowPanel()
        {
            ModSettings s = Mod.EnsureSettings();
            ModOptions.ApplyBool(s, x => x.DebugPanelDismissed = false);
            s.AssistUiEnabled = true;
            ModOptions.NotifyChanged();
            if (_root == null)
            {
                EnsureCreated();
            }

            ApplyVisibility();
        }

        private static void HidePanel()
        {
            _dragging = false;
            ModSettings s = Mod.EnsureSettings();
            ModOptions.ApplyBool(s, x => x.DebugPanelDismissed = true);
            ApplyVisibility();
            ModOptions.FlushStore(true);
        }

        private static void BuildTitleBar()
        {
            _titleBar = _root.AddUIComponent<UIPanel>();
            _titleBar.name = "TrackpadCameraDebugTitleBar";
            _titleBar.width = PanelWidth;
            _titleBar.height = TitleBarHeight;
            _titleBar.relativePosition = Vector3.zero;
            _titleBar.backgroundSprite = "GenericPanel";
            // Soft translucent strip over MenuPanel2 (not opaque punch-out).
            _titleBar.color = new Color32(40, 40, 40, 160);
            _titleBar.isInteractive = true;
            _titleBar.eventMouseDown += OnTitleMouseDown;
            _titleBar.eventMouseUp += OnTitleMouseUp;
            _titleBar.eventMouseMove += OnTitleMouseMove;

            _title = AddLabel(_titleBar, Mod.OptionsTitle, Col0, 8f);
            _title.textScale = 1.1f;
            // Title text is decorative; drag comes from the title-bar strip.
            _title.isInteractive = false;

            float closeX = _root.width - HeaderButtonSize - 2f;
            float optionsX = closeX - HeaderButtonSize - 4f;

            _optionsButton = _root.AddUIComponent<UIButton>();
            _optionsButton.text = string.Empty;
            _optionsButton.width = HeaderButtonSize;
            _optionsButton.height = HeaderButtonSize;
            _optionsButton.relativePosition = new Vector3(optionsX, 0f);
            _optionsButton.normalBgSprite = "Options";
            _optionsButton.hoveredBgSprite = "OptionsHovered";
            _optionsButton.pressedBgSprite = "OptionsPressed";
            StyleNativeHeaderButton(_optionsButton);
            _optionsButton.eventClick += (c, e) =>
            {
                if (UIView.library != null)
                {
                    UIView.library.ShowModal<OptionsMainPanel>("OptionsPanel");
                }
            };
            _optionsButton.eventMouseDown += (c, e) =>
            {
                _dragging = false;
                e.Use();
            };

            _closeButton = _root.AddUIComponent<UIButton>();
            _closeButton.text = string.Empty;
            _closeButton.width = HeaderButtonSize;
            _closeButton.height = HeaderButtonSize;
            _closeButton.relativePosition = new Vector3(closeX, 0f);
            _closeButton.normalBgSprite = "buttonclose";
            _closeButton.hoveredBgSprite = "buttonclosehover";
            _closeButton.pressedBgSprite = "buttonclosepressed";
            StyleNativeHeaderButton(_closeButton);
            _closeButton.eventClick += (c, e) => HidePanel();
            _closeButton.eventMouseDown += (c, e) =>
            {
                // Close must not start a title-bar drag.
                _dragging = false;
                e.Use();
            };
            _optionsButton.BringToFront();
            _closeButton.BringToFront();
        }

        /// <summary>
        /// City Vitals–style header chrome: soft at rest, full opacity on hover/press.
        /// </summary>
        private static void StyleNativeHeaderButton(UIButton button)
        {
            if (button == null)
            {
                return;
            }

            SetButtonOpacity(button, HeaderButtonRestOpacity);
            button.eventMouseEnter += (c, e) => SetButtonOpacity(button, 1f);
            button.eventMouseLeave += (c, e) => SetButtonOpacity(button, HeaderButtonRestOpacity);
            button.eventMouseDown += (c, e) => SetButtonOpacity(button, 1f);
        }

        private static void SetButtonOpacity(UIButton button, float opacity)
        {
            if (button == null)
            {
                return;
            }

            // ColossalUI respects UIComponent.opacity; keep color.a in sync as fallback.
            button.opacity = opacity;
            Color32 color = button.color;
            color.a = (byte)Mathf.Clamp(Mathf.RoundToInt(opacity * 255f), 0, 255);
            button.color = color;
        }

        private static void AddFeelPresetRow(ModSettings s)
        {
            _feelDropdownItems = ModOptions.GetFeelPresetDropdownItems(s);

            _feelDropdown = _root.AddUIComponent<UIDropDown>();
            _feelDropdown.width = 220f;
            _feelDropdown.height = 28f;
            _feelDropdown.relativePosition = new Vector3(Col0, _nextY);
            _feelDropdown.listWidth = 220;
            _feelDropdown.itemHeight = 24;
            _feelDropdown.normalBgSprite = "ButtonMenu";
            _feelDropdown.hoveredBgSprite = "ButtonMenuHovered";
            _feelDropdown.focusedBgSprite = "ButtonMenu";
            _feelDropdown.listBackground = "GenericPanelLight";
            _feelDropdown.itemHover = "ListItemHover";
            _feelDropdown.itemHighlight = "ListItemHighlight";
            _feelDropdown.normalFgSprite = "IconDownArrow";
            _feelDropdown.hoveredFgSprite = "IconDownArrowHovered";
            _feelDropdown.focusedFgSprite = "IconDownArrowFocused";
            _feelDropdown.textScale = 0.85f;
            _feelDropdown.textFieldPadding = new RectOffset(8, 8, 6, 0);
            _feelDropdown.listPosition = UIDropDown.PopupListPosition.Automatic;
            _feelDropdown.items = _feelDropdownItems;
            _feelDropdown.selectedIndex = ModOptions.IndexOfFeelPresetDropdownItem(
                _feelDropdownItems,
                s.ActiveFeelPresetName
            );
            // Subscribe after selectedIndex so init does not treat it as a user choice.
            _feelDropdown.eventSelectedIndexChanged += OnFeelDropdownSelected;

            UIButton reset = MakeMenuButton("Reset", Col0 + 228f, _nextY, 72f);
            reset.eventClick += (c, e) =>
            {
                // SettingsChanged rebuilds the panel (New Preset / field sync).
                ModOptions.ResetToFactory(s);
            };
            _nextY += 32f;

            UILabel nameLbl = AddLabel(_root, "Name", Col0, _nextY);
            nameLbl.width = FieldLabelW;
            nameLbl.autoSize = false;

            _feelNameField = _root.AddUIComponent<UITextField>();
            _feelNameField.width = 180f;
            _feelNameField.height = 22f;
            _feelNameField.relativePosition = new Vector3(
                Col0 + FieldLabelW + FieldLabelGap,
                _nextY
            );
            _feelNameField.normalBgSprite = "TextFieldPanel";
            _feelNameField.hoveredBgSprite = "TextFieldPanelHovered";
            _feelNameField.focusedBgSprite = "TextFieldPanel";
            _feelNameField.selectionSprite = "EmptySprite";
            _feelNameField.text = "";
            _feelNameField.selectOnFocus = true;
            _feelNameField.isInteractive = true;
            _feelNameField.builtinKeyNavigation = true;
            _nextY += 30f;
        }

        private static void OnFeelDropdownSelected(UIComponent component, int index)
        {
            if (_handlingSettingsChanged || _feelDropdownItems == null)
            {
                return;
            }

            if (index < 0 || index >= _feelDropdownItems.Length)
            {
                return;
            }

            ModSettings s = Mod.EnsureSettings();
            string label = _feelDropdownItems[index];
            if (string.Equals(label, ModOptions.FeelPresetSaveAsLabel, StringComparison.Ordinal))
            {
                string name = _feelNameField != null ? _feelNameField.text : "";
                if (!ModOptions.SaveNamedFeelPreset(s, name))
                {
                    if (_feelDropdown != null)
                    {
                        _feelDropdown.selectedIndex = ModOptions.IndexOfFeelPresetDropdownItem(
                            _feelDropdownItems,
                            s.ActiveFeelPresetName
                        );
                    }
                }

                return;
            }

            ModOptions.ApplyFeelPresetDropdownChoice(s, label);
        }

#if ENABLE_CAD_GESTURE_STYLE
        private static void AddCadStyleButtons(ModSettings s)
        {
            UIButton maps = MakeMenuButton("Maps+", Col0, _nextY, 120f);
            maps.eventClick += (c, e) =>
            {
                ModOptions.ApplyGesturePresetIndex(s, 0);
                UpdatePresetDesc(ModOptions.MapsPlusDescription);
            };

            UIButton cad = MakeMenuButton("CAD", Col0 + 128f, _nextY, 120f);
            cad.eventClick += (c, e) =>
            {
                ModOptions.ApplyGesturePresetIndex(s, 1);
                UpdatePresetDesc(ModOptions.CadDescription);
            };
            _nextY += 32f;
        }
#endif

#if ENABLE_CONTACTS_CAPTURE
        private static void AddCaptureBackendButtons(ModSettings s)
        {
            UIButton apple = MakeMenuButton("AppKit", Col0, _nextY, 120f);
            apple.eventClick += (c, e) => ModOptions.ApplyCaptureBackendIndex(s, 0);

            UIButton contacts = MakeMenuButton("Contacts", Col0 + 128f, _nextY, 120f);
            contacts.eventClick += (c, e) => ModOptions.ApplyCaptureBackendIndex(s, 1);
            _nextY += 32f;
        }
#endif

        private static void UpdatePresetDesc(string text)
        {
            if (_presetDesc == null)
            {
                return;
            }

            _presetDesc.text = text;
            _presetDesc.PerformLayout();
        }

        private static void BuildPanSection(ModSettings s)
        {
            AddOpHeading(ModOptions.OpHeadingPan);
            AddFloatPair(
                s,
                "Sensitivity X",
                () => s.PanGainX,
                ModOptions.ApplyPanGainX,
                "Sensitivity Y",
                () => s.PanGainY,
                ModOptions.ApplyPanGainY,
                gainFormatL: true,
                gainFormatR: true
            );
#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Btn X",
                () => s.PanStepX,
                ModOptions.ApplyPanStepX,
                "Btn Y",
                () => s.PanStepY,
                ModOptions.ApplyPanStepY
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddCheckRow(
                s,
                () => s.PanFilterEnabled,
                v => s.PanFilterEnabled = v,
                "Low-pass",
                null,
                null,
                null
            );
            AddFloatPair(
                s,
                "LP α",
                () => s.PanFilterAlpha,
                ModOptions.ApplyPanFilterAlpha,
                null,
                null,
                null
            );
#endif
        }

        private static void BuildZoomSection(ModSettings s)
        {
            AddOpHeading(ModOptions.OpHeadingZoom);
#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.ZoomGain,
                ModOptions.ApplyZoomGain,
                "Btn",
                () => s.ZoomStep,
                ModOptions.ApplyZoomStep,
                gainFormatL: true
            );
#else
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.ZoomGain,
                ModOptions.ApplyZoomGain,
                null,
                null,
                null,
                gainFormatL: true
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddCheckRow(
                s,
                () => s.ZoomFilterEnabled,
                v => s.ZoomFilterEnabled = v,
                "Low-pass",
                null,
                null,
                null
            );
            AddFloatPair(
                s,
                "LP α",
                () => s.ZoomFilterAlpha,
                ModOptions.ApplyZoomFilterAlpha,
                null,
                null,
                null
            );
#endif
        }

        private static void BuildYawSection(ModSettings s)
        {
            AddOpHeading(ModOptions.OpHeadingRotate);
#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.YawRotateGain,
                ModOptions.ApplyYawRotateGain,
                "Btn",
                () => s.YawRotateStep,
                ModOptions.ApplyYawRotateStep,
                gainFormatL: true
            );
#else
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.YawRotateGain,
                ModOptions.ApplyYawRotateGain,
                null,
                null,
                null,
                gainFormatL: true
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddCheckRow(
                s,
                () => s.YawFilterEnabled,
                v => s.YawFilterEnabled = v,
                "Low-pass",
                null,
                null,
                null
            );
            AddFloatPair(
                s,
                "LP α",
                () => s.YawFilterAlpha,
                ModOptions.ApplyYawFilterAlpha,
                null,
                null,
                null
            );
#endif
        }

        private static void BuildOrbitSection(ModSettings s)
        {
            AddOpHeading(ModOptions.OpHeadingOrbit);
            AddFloatPair(
                s,
                "Sensitivity yaw",
                () => s.OrbitYawGain,
                ModOptions.ApplyOrbitYawGain,
                "Sensitivity pitch",
                () => s.OrbitPitchGain,
                ModOptions.ApplyOrbitPitchGain,
                gainFormatL: true,
                gainFormatR: true
            );
#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Btn yaw",
                () => s.OrbitYawStep,
                ModOptions.ApplyOrbitYawStep,
                "Btn pitch",
                () => s.OrbitPitchStep,
                ModOptions.ApplyOrbitPitchStep
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddCheckRow(
                s,
                () => s.OrbitFilterEnabled,
                v => s.OrbitFilterEnabled = v,
                "Low-pass",
                null,
                null,
                null
            );
            AddFloatPair(
                s,
                "LP α",
                () => s.OrbitFilterAlpha,
                ModOptions.ApplyOrbitFilterAlpha,
                null,
                null,
                null
            );
#endif
        }

        private static void AddBuildInfoFooter()
        {
            string line = Mod.GetBuildInfoFooterDisplay();
            if (string.IsNullOrEmpty(line))
            {
                return;
            }

            _nextY += 8f;
            float rowY = _nextY;
            float copyX = PanelWidth - FieldGutter - FooterCopyButtonWidth;
            float labelWidth = copyX - Col0 - 4f;

            UILabel label = AddLabel(_root, line, Col0, rowY);
            label.textColor = new Color(1f, 1f, 1f, 0.75f);
            label.width = labelWidth;
            label.autoSize = false;
            label.autoHeight = true;
            label.wordWrap = true;
            label.isInteractive = false;
            label.PerformLayout();

            // Labelled "Copy" — Cities UI fonts do not render clipboard glyphs (blank square).
            UIButton copy = MakeMenuButton("Copy", copyX, rowY, FooterCopyButtonWidth);
            copy.height = FooterCopyButtonHeight;
            copy.tooltip = "Copy build info (and system info when checked)";
            copy.eventClick += (c, e) =>
            {
                ModSettings copySettings = Mod.Settings;
                bool includeSystem = copySettings == null || copySettings.IncludeSystemInfoInCopy;
                GUIUtility.systemCopyBuffer = QaClipboardReport.Format(includeSystem);
            };
            copy.eventMouseDown += (c, e) =>
            {
                _dragging = false;
                e.Use();
            };

            _nextY += Mathf.Max(FooterCopyButtonHeight, label.height + 4f);
            AddLocalCheckRow(
                "Include system info (OS, devices)",
                () =>
                {
                    ModSettings copySettings = Mod.Settings;
                    return copySettings == null || copySettings.IncludeSystemInfoInCopy;
                },
                v =>
                {
                    ModSettings copySettings = Mod.EnsureSettings();
                    ModOptions.ApplyBool(copySettings, x => x.IncludeSystemInfoInCopy = v);
                }
            );
        }

        private static void AddLocalCheckRow(string label, Func<bool> get, Action<bool> set)
        {
            UICheckBox box = _root.AddUIComponent<UICheckBox>();
            box.width = PanelWidth - (FieldGutter * 2f);
            box.height = 20f;
            box.relativePosition = new Vector3(Col0, _nextY);
            UISprite uncheckedSprite = box.AddUIComponent<UISprite>();
            uncheckedSprite.spriteName = "check-unchecked";
            uncheckedSprite.size = new Vector2(16f, 16f);
            uncheckedSprite.relativePosition = Vector3.zero;
            box.checkedBoxObject = box.AddUIComponent<UISprite>();
            ((UISprite)box.checkedBoxObject).spriteName = "check-checked";
            box.checkedBoxObject.size = new Vector2(16f, 16f);
            box.checkedBoxObject.relativePosition = Vector3.zero;
            UILabel boxLabel = box.AddUIComponent<UILabel>();
            boxLabel.text = label;
            boxLabel.relativePosition = new Vector3(22f, 2f);
            box.label = boxLabel;
            box.isChecked = get();
            box.eventCheckChanged += (c, v) => set(v);
            _nextY += 22f;
        }

        private static void AddSection(string title)
        {
            AddLabel(_root, "— " + title + " —", Col0, _nextY);
            _nextY += 22f;
        }

        private static void AddOpHeading(string text)
        {
            UILabel label = _root.AddUIComponent<UILabel>();
            label.text = text;
            label.relativePosition = new Vector3(Col0, _nextY);
            label.textColor = Color.white;
            // Full content width so long op copy does not collide with Sensitivity fields.
            label.width = PanelWidth - (FieldGutter * 2f);
            label.height = 36f;
            label.autoSize = false;
            label.autoHeight = true;
            label.wordWrap = true;
            label.PerformLayout();
            _nextY += Mathf.Max(28f, label.height + 8f);
        }

        private static void AddCheckRow(
            ModSettings s,
            Func<bool> getL,
            Action<bool> setL,
            string labelL,
            Func<bool> getR,
            Action<bool> setR,
            string labelR
        )
        {
            AddCheckAt(s, Col0, getL, setL, labelL);
            if (getR != null && setR != null && !string.IsNullOrEmpty(labelR))
            {
                AddCheckAt(s, Col1, getR, setR, labelR);
            }

            _nextY += 22f;
        }

        private static void AddCheckAt(
            ModSettings s,
            float x,
            Func<bool> get,
            Action<bool> set,
            string label
        )
        {
            UICheckBox box = _root.AddUIComponent<UICheckBox>();
            box.width = ColWidth;
            box.height = 20f;
            box.relativePosition = new Vector3(x, _nextY);
            UISprite uncheckedSprite = box.AddUIComponent<UISprite>();
            uncheckedSprite.spriteName = "check-unchecked";
            uncheckedSprite.size = new Vector2(16f, 16f);
            uncheckedSprite.relativePosition = Vector3.zero;
            box.checkedBoxObject = box.AddUIComponent<UISprite>();
            ((UISprite)box.checkedBoxObject).spriteName = "check-checked";
            box.checkedBoxObject.size = new Vector2(16f, 16f);
            box.checkedBoxObject.relativePosition = Vector3.zero;
            UILabel boxLabel = box.AddUIComponent<UILabel>();
            boxLabel.text = label;
            boxLabel.relativePosition = new Vector3(22f, 2f);
            box.label = boxLabel;
            box.isChecked = get();
            box.eventCheckChanged += (c, v) =>
                ModOptions.ApplyBool(
                    s,
                    xSettings =>
                    {
                        set(v);
                    }
                );
        }

        private static void AddFloatPair(
            ModSettings s,
            string labelL,
            Func<float> getL,
            Action<ModSettings, float> applyL,
            string labelR,
            Func<float> getR,
            Action<ModSettings, float> applyR,
            bool gainFormatL = false,
            bool gainFormatR = false
        )
        {
            AddFloatAt(s, Col0, labelL, getL, applyL, gainFormatL);
            if (getR != null && applyR != null && !string.IsNullOrEmpty(labelR))
            {
                AddFloatAt(s, Col1, labelR, getR, applyR, gainFormatR);
            }

            _nextY += 26f;
        }

        private static void AddFloatAt(
            ModSettings s,
            float x,
            string label,
            Func<float> get,
            Action<ModSettings, float> apply,
            bool useGainFormat = false
        )
        {
            UILabel lbl = AddLabel(_root, label, x, _nextY);
            lbl.width = FieldLabelW;
            lbl.autoSize = false;

            UITextField field = _root.AddUIComponent<UITextField>();
            field.width = FieldInputW;
            field.height = 22f;
            field.relativePosition = new Vector3(x + FieldLabelW + FieldLabelGap, _nextY);
            field.normalBgSprite = "TextFieldPanel";
            field.hoveredBgSprite = "TextFieldPanelHovered";
            field.focusedBgSprite = "TextFieldPanel";
            field.selectionSprite = "EmptySprite";
            field.text = FormatFieldValue(get(), useGainFormat);
            field.numericalOnly = false;
            field.allowFloats = true;
            field.selectOnFocus = true;
            field.submitOnFocusLost = true;
            field.isInteractive = true;
            field.builtinKeyNavigation = true;
            field.eventTextSubmitted += (c, text) =>
            {
                if (!ModOptions.TryApplyFloat(s, text, apply))
                {
                    field.text = FormatFieldValue(get(), useGainFormat);
                }
                else
                {
                    field.text = FormatFieldValue(get(), useGainFormat);
                }
            };
        }

        private static string FormatFieldValue(float value, bool useGainFormat)
        {
            return useGainFormat ? ModOptions.FormatGain(value) : ModOptions.FormatFloat(value);
        }

        private static UIButton MakeMenuButton(string text, float x, float y, float width)
        {
            UIButton btn = _root.AddUIComponent<UIButton>();
            btn.text = text;
            btn.width = width;
            btn.height = 28f;
            btn.relativePosition = new Vector3(x, y);
            btn.normalBgSprite = "ButtonMenu";
            btn.hoveredBgSprite = "ButtonMenuHovered";
            btn.pressedBgSprite = "ButtonMenuPressed";
            return btn;
        }

        private static UILabel AddLabel(UIPanel parent, string text, float x, float y)
        {
            UILabel label = parent.AddUIComponent<UILabel>();
            label.text = text;
            label.relativePosition = new Vector3(x, y);
            label.textColor = Color.white;
            label.autoSize = true;
            return label;
        }

        private static void OnTitleMouseDown(UIComponent c, UIMouseEventParameter e)
        {
            if (_closeButton != null && e.source == _closeButton)
            {
                return;
            }

            if (_optionsButton != null && e.source == _optionsButton)
            {
                return;
            }

            _dragging = true;
            if (_root != null)
            {
                _dragPanelStart = _root.absolutePosition;
                _root.BringToFront();
            }

            _dragMouseStart = Input.mousePosition;
            e.Use();
        }

        private static void OnTitleMouseUp(UIComponent c, UIMouseEventParameter e)
        {
            _dragging = false;
        }

        private static void OnTitleMouseMove(UIComponent c, UIMouseEventParameter e)
        {
            if (!_dragging || _root == null)
            {
                return;
            }

            Vector3 delta = Input.mousePosition - _dragMouseStart;
            _root.absolutePosition = _dragPanelStart + new Vector3(delta.x, -delta.y, 0f);
            e.Use();
        }
    }
}
#endif
