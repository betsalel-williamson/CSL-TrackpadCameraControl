#if HAS_CITIES
using System;
using System.Collections.Generic;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl.Rewrite
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
        private static UILabel _title;
        private static UIDropDown _feelDropdown;
        private static UIButton _feelSaveAsButton;
        private static UIButton _feelDeleteButton;
        private static string[] _feelDropdownItems;
        private static float _nextY;
        private static bool _handlingSettingsChanged;
        private static bool _rebuildQueued;

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
            VanillaCameraKeyLabelsWatch.LabelsChanged -= OnKeymappingLabelsChanged;
            VanillaCameraKeyLabelsWatch.LabelsChanged += OnKeymappingLabelsChanged;
            VanillaCameraKeyLabelsWatch.EnsureHooked();

            _root = view.AddUIComponent(typeof(UIPanel)) as UIPanel;
            if (_root == null)
            {
                return;
            }

            ModSettings s = Mod.EnsureSettings();

            ClearRefreshBindings();

            _root.name = "TrackpadCameraDebugPanel";
            _root.backgroundSprite = "MenuPanel2";
            _root.width = PanelWidth;
            _root.height = 420f;
            _root.relativePosition = new Vector3(s.DebugPanelPosX, s.DebugPanelPosY, 0f);
            _root.canFocus = true;
            _root.isInteractive = true;
            _root.eventMouseDown += (c, e) => OnPanelMouseDown();

            BuildTitleBar();
            ResetPanelFocusVisual();
            ApplyPanelFocusVisual();

            _nextY = TitleBarHeight + 8f;

            AddSection("Feel presets");
            AddFeelPresetRow(s);

            // Tab order (product fields): Zoom sens/dead → Pan X/Y/dead → Rotate sens/dead →
            // Orbit yaw/pitch/dead → Include system info. Feel presets stay click-focus only.
            ResetTabOrder();
            // Shipped section order: Zoom → Pan → Rotate → Orbit (feel presets = General above).
            BuildZoomSection(s);
            BuildPanSection(s);
            BuildRotateSection(s);
            BuildOrbitSection(s);

            AddBuildInfoFooter();

            _root.height = _nextY + 16f;
            ClampPanelIntoView();
            // Re-apply values after all fields are wired (numericalOnly / layout can clear text).
            RefreshFloatFields();
            RefreshChecks();

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

            ApplyPanelStackOrder();
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

            ApplyPanelStackOrder();
        }

        public static void Destroy()
        {
            ModOptions.SettingsChanged -= OnSettingsChanged;
            VanillaCameraKeyLabelsWatch.LabelsChanged -= OnKeymappingLabelsChanged;

            if (_root != null)
            {
                SavePanelPosition();
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
            _title = null;
            _feelSaveAsButton = null;
            _feelDeleteButton = null;
            _feelDropdown = null;
            _feelDropdownItems = null;
            ClearRefreshBindings();
            FeelSaveAsDialog.Close();
        }

        private static void OnSettingsChanged()
        {
            if (_handlingSettingsChanged)
            {
                return;
            }

            if (_root == null)
            {
                ApplyVisibility();
                return;
            }

            _rebuildQueued = true;
        }

        private static void OnKeymappingLabelsChanged()
        {
            if (_root == null)
            {
                return;
            }

            _rebuildQueued = true;
        }

        /// <summary>
        /// Prefer in-place control/label updates after Reset or external settings edits;
        /// fall back to Destroy/EnsureCreated only when structure cannot be refreshed.
        /// </summary>
        public static void ProcessPendingUiRebuild()
        {
            if (!_rebuildQueued || _handlingSettingsChanged)
            {
                return;
            }

            _rebuildQueued = false;
            if (_root == null)
            {
                ApplyVisibility();
                return;
            }

            if (TryRefreshInPlace())
            {
                return;
            }

            _handlingSettingsChanged = true;
            try
            {
                Vector3 pos = _root.relativePosition;
                Destroy();
                EnsureCreated();
                if (_root != null)
                {
                    _root.relativePosition = pos;
                }

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
            _titleBar.isInteractive = true;

            UIDragHandle drag = _titleBar.AddUIComponent<UIDragHandle>();
            drag.target = _root;
            drag.constrainToScreen = true;
            // Mouse-up is handled by UIDragHandle, not the title-bar UIPanel.
            drag.eventMouseUp += (c, e) => SavePanelPosition();

            _title = AddLabel(_titleBar, Mod.DebugPanelTitle, 0f, 0f);
            _title.textScale = 1.0f;
            _title.autoSize = false;
            _title.width = PanelWidth - (HeaderButtonSize * 2f) - 8f;
            _title.height = TitleBarHeight;
            _title.textAlignment = UIHorizontalAlignment.Center;
            _title.verticalAlignment = UIVerticalAlignment.Middle;
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
                OptionsPanelNavigation.OpenModOptions();
            };
            _optionsButton.eventMouseDown += (c, e) =>
            {
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

            // ColossalUI UIDropDown only opens its popup when triggerButton is set; arrow
            // sprites belong on that button (AlgernonCommons / Skyve pattern), not on the DD.
            _feelDropdown = _root.AddUIComponent<UIDropDown>();
            _feelDropdown.tabIndex = -1;
            _feelDropdown.width = 200f;
            _feelDropdown.height = 28f;
            _feelDropdown.relativePosition = new Vector3(Col0, _nextY);
            _feelDropdown.listWidth = 200;
            _feelDropdown.listHeight = 500;
            _feelDropdown.itemHeight = 24;
            _feelDropdown.normalBgSprite = "ButtonMenu";
            _feelDropdown.hoveredBgSprite = "ButtonMenuHovered";
            _feelDropdown.focusedBgSprite = "ButtonMenu";
            _feelDropdown.listBackground = "GenericPanelLight";
            _feelDropdown.itemHover = "ListItemHover";
            _feelDropdown.itemHighlight = "ListItemHighlight";
            _feelDropdown.foregroundSpriteMode = UIForegroundSpriteMode.Stretch;
            _feelDropdown.textScale = 0.85f;
            _feelDropdown.verticalAlignment = UIVerticalAlignment.Middle;
            _feelDropdown.horizontalAlignment = UIHorizontalAlignment.Left;
            _feelDropdown.textFieldPadding = new RectOffset(8, 8, 6, 0);
            _feelDropdown.itemPadding = new RectOffset(14, 0, 4, 0);
            _feelDropdown.listPosition = UIDropDown.PopupListPosition.Automatic;
            _feelDropdown.clampListToScreen = true;
            _feelDropdown.popupColor = new Color32(45, 52, 61, 255);
            _feelDropdown.popupTextColor = new Color32(170, 170, 170, 255);
            _feelDropdown.items = _feelDropdownItems;
            _feelDropdown.selectedIndex = ModOptions.IndexOfFeelPresetDropdownItem(
                _feelDropdownItems,
                s.ActiveFeelPresetName
            );

            UIButton trigger = _feelDropdown.AddUIComponent<UIButton>();
            _feelDropdown.triggerButton = trigger;
            trigger.size = _feelDropdown.size;
            trigger.text = string.Empty;
            trigger.relativePosition = Vector3.zero;
            trigger.normalFgSprite = "IconDownArrow";
            trigger.hoveredFgSprite = "IconDownArrowHovered";
            trigger.pressedFgSprite = "IconDownArrowPressed";
            trigger.focusedFgSprite = "IconDownArrowFocused";
            trigger.disabledFgSprite = "IconDownArrowDisabled";
            trigger.foregroundSpriteMode = UIForegroundSpriteMode.Fill;
            trigger.horizontalAlignment = UIHorizontalAlignment.Right;
            trigger.verticalAlignment = UIVerticalAlignment.Middle;
            trigger.zOrder = 0;

            // Subscribe after selectedIndex so init does not treat it as a user choice.
            _feelDropdown.eventSelectedIndexChanged += OnFeelDropdownSelected;

            UIButton reset = MakeMenuButton("Reset", Col0 + 208f, _nextY, 64f);
            reset.tabIndex = -1;
            reset.eventClick += (c, e) =>
            {
                ModOptions.ApplyFeelDefault(s);
            };

            _feelSaveAsButton = MakeMenuButton("Save as…", Col0 + 280f, _nextY, 88f);
            _feelSaveAsButton.tabIndex = -1;
            _feelSaveAsButton.isEnabled = ModOptions.IsFeelDirtyNewPreset(s);
            _feelSaveAsButton.eventClick += (c, e) =>
            {
                e.Use();
                FeelSaveAsDialog.Show(Mod.EnsureSettings(), RefreshFeelPresetButtons);
            };

            _feelDeleteButton = MakeMenuButton("Delete", Col0 + 376f, _nextY, 64f);
            _feelDeleteButton.tabIndex = -1;
            _feelDeleteButton.isEnabled = ModOptions.IsNamedUserFeelPreset(s);
            _feelDeleteButton.eventClick += (c, e) =>
            {
                e.Use();
                ModOptions.DeleteNamedFeelPreset(Mod.EnsureSettings());
            };
            _nextY += 32f;
        }

        private static void RefreshFeelPresetButtons()
        {
            if (_feelSaveAsButton != null)
            {
                _feelSaveAsButton.isEnabled = ModOptions.IsFeelDirtyNewPreset(Mod.Settings);
            }

            if (_feelDeleteButton != null)
            {
                _feelDeleteButton.isEnabled = ModOptions.IsNamedUserFeelPreset(Mod.Settings);
            }
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
            ModOptions.ApplyFeelPresetDropdownChoice(s, _feelDropdownItems[index]);
        }

        private static void BuildPanSection(ModSettings s)
        {
            AddOpHeading(() => ModOptions.OpHeadingPan);
            AddFloatPair(
                s,
                "Sensitivity X",
                () => s.PanGainX,
                ModOptions.ApplyPanGainX,
                "Sensitivity Y",
                () => s.PanGainY,
                ModOptions.ApplyPanGainY
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
            AddFloatPair(
                s,
                "Deadband",
                () => s.MotionDeadband,
                ModOptions.ApplyMotionDeadband,
                null,
                null,
                null
            );
        }

        private static void BuildZoomSection(ModSettings s)
        {
            AddOpHeading(() => ModOptions.OpHeadingZoom);
#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.ZoomGain,
                ModOptions.ApplyZoomGain,
                "Btn",
                () => s.ZoomStep,
                ModOptions.ApplyZoomStep
            );
#else
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.ZoomGain,
                ModOptions.ApplyZoomGain,
                null,
                null,
                null
            );
#endif
            AddFloatPair(
                s,
                "Deadband",
                () => s.PinchDeadband,
                ModOptions.ApplyPinchDeadband,
                null,
                null,
                null
            );
        }

        private static void BuildRotateSection(ModSettings s)
        {
            AddOpHeading(() => ModOptions.OpHeadingRotate);
#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.RotateGain,
                ModOptions.ApplyRotateGain,
                "Btn",
                () => s.RotateStep,
                ModOptions.ApplyRotateStep
            );
#else
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.RotateGain,
                ModOptions.ApplyRotateGain,
                null,
                null,
                null
            );
#endif
            AddFloatPair(
                s,
                "Deadband",
                () => s.RotateDeadband,
                ModOptions.ApplyRotateDeadband,
                null,
                null,
                null
            );
        }

        private static void BuildOrbitSection(ModSettings s)
        {
            AddOpHeading(() => ModOptions.OpHeadingOrbit);
            AddFloatPair(
                s,
                "Sensitivity yaw",
                () => s.OrbitYawGain,
                ModOptions.ApplyOrbitYawGain,
                "Sensitivity pitch",
                () => s.OrbitPitchGain,
                ModOptions.ApplyOrbitPitchGain
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
            AddFloatPair(
                s,
                "Deadband",
                () => s.MotionDeadband,
                ModOptions.ApplyMotionDeadband,
                null,
                null,
                null
            );
        }

        private static void AddBuildInfoFooter()
        {
            string builtLine = Mod.GetBuildInfoPanelDisplay();
            if (string.IsNullOrEmpty(builtLine))
            {
                // Still show Include + Copy when the stamp is missing so QA can paste product info.
                builtLine = "Built (local): ?";
            }

            _nextY += 8f;
            float actionsY = _nextY;

            // Row 1: Include system info, then Copy.
            const float includeBoxW = 180f;
            UICheckBox includeBox = _root.AddUIComponent<UICheckBox>();
            includeBox.width = includeBoxW;
            includeBox.height = 20f;
            includeBox.relativePosition = new Vector3(
                Col0,
                actionsY + (FooterCopyButtonHeight - 20f) * 0.5f
            );
            UISprite uncheckedSprite = includeBox.AddUIComponent<UISprite>();
            uncheckedSprite.spriteName = "check-unchecked";
            uncheckedSprite.size = new Vector2(16f, 16f);
            uncheckedSprite.relativePosition = Vector3.zero;
            includeBox.checkedBoxObject = includeBox.AddUIComponent<UISprite>();
            ((UISprite)includeBox.checkedBoxObject).spriteName = "check-checked";
            includeBox.checkedBoxObject.size = new Vector2(16f, 16f);
            includeBox.checkedBoxObject.relativePosition = Vector3.zero;
            UILabel includeLabel = includeBox.AddUIComponent<UILabel>();
            includeLabel.text = "Include system info";
            includeLabel.tooltip = "Include OS, devices, and assembly versions when copying";
            includeLabel.relativePosition = new Vector3(22f, 2f);
            includeBox.label = includeLabel;
            includeBox.tooltip = includeLabel.tooltip;
            includeBox.isChecked = Mod.Settings == null || Mod.Settings.IncludeSystemInfoInCopy;
            includeBox.eventCheckChanged += (c, v) =>
            {
                if (_handlingSettingsChanged)
                {
                    return;
                }

                ModSettings copySettings = Mod.EnsureSettings();
                ModOptions.ApplyBool(copySettings, x => x.IncludeSystemInfoInCopy = v);
            };
            AssignTabOrder(includeBox);
            NumericTextFieldUi.WireTabStop(includeBox, tabScope: _root);
            RegisterCheck(
                includeBox,
                () =>
                {
                    ModSettings copySettings = Mod.Settings;
                    return copySettings == null || copySettings.IncludeSystemInfoInCopy;
                }
            );

            float copyX = Col0 + includeBoxW + 8f;
            UIButton copy = MakeMenuButton("Copy", copyX, actionsY, FooterCopyButtonWidth);
            copy.height = FooterCopyButtonHeight;
            copy.tooltip = "Copy build info (UTC) and optional system info";
            copy.eventClick += (c, e) =>
            {
                ModSettings copySettings = Mod.Settings;
                bool includeSystem = copySettings == null || copySettings.IncludeSystemInfoInCopy;
                GUIUtility.systemCopyBuffer = QaClipboardReport.Format(includeSystem);
            };
            copy.eventMouseDown += (c, e) =>
            {
                e.Use();
            };

            _nextY = actionsY + FooterCopyButtonHeight + 4f;

            // Row 2: build stamp in local time (single line; clipboard keeps UTC + asm).
            UILabel label = AddLabel(_root, builtLine, Col0, _nextY);
            label.textColor = new Color(1f, 1f, 1f, 0.75f);
            label.autoSize = true;
            label.wordWrap = false;
            label.isInteractive = false;
            label.PerformLayout();
            _nextY += Mathf.Max(18f, label.height + 4f);
        }

        /// <summary>Keep the Debug panel (including footer) inside the game UI view.</summary>
        private static void ClampPanelIntoView()
        {
            if (_root == null)
            {
                return;
            }

            const float margin = 8f;
            Vector3 p = _root.relativePosition;
            float maxX = Mathf.Max(margin, Screen.width - _root.width - margin);
            float maxY = Mathf.Max(margin, Screen.height - _root.height - margin);
            p.x = Mathf.Clamp(p.x, margin, maxX);
            p.y = Mathf.Clamp(p.y, margin, maxY);
            _root.relativePosition = p;
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
            box.eventCheckChanged += (c, v) =>
            {
                if (_handlingSettingsChanged)
                {
                    return;
                }

                set(v);
            };
            AssignTabOrder(box);
            NumericTextFieldUi.WireTabStop(box, tabScope: _root);
            RegisterCheck(box, get);
            _nextY += 22f;
        }

        private static void AddSection(string title)
        {
            AddLabel(_root, "— " + title + " —", Col0, _nextY);
            _nextY += 22f;
        }

        private static void AddOpHeading(Func<string> getHeading)
        {
            if (getHeading == null)
            {
                return;
            }

            string text = getHeading();
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            string[] lines = DebugPanelRefresh.SplitHeadingLines(text);
            if (lines.Length == 0)
            {
                return;
            }

            var labels = new List<UILabel>(lines.Length);
            for (int i = 0; i < lines.Length; i++)
            {
                string line = lines[i];
                if (string.IsNullOrEmpty(line))
                {
                    continue;
                }

                if (labels.Count == 0)
                {
                    UILabel titleLabel = _root.AddUIComponent<UILabel>();
                    titleLabel.text = line;
                    titleLabel.relativePosition = new Vector3(Col0, _nextY);
                    titleLabel.textColor = Color.white;
                    titleLabel.autoSize = true;
                    titleLabel.PerformLayout();
                    labels.Add(titleLabel);
                    _nextY += titleLabel.height + 2f;
                    continue;
                }

                labels.Add(AddOpHeadingBodyLine(line));
            }

            if (labels.Count == 0)
            {
                return;
            }

            RegisterOpHeading(labels.ToArray(), getHeading);
            _nextY += 6f;
        }

        private static UILabel AddOpHeadingBodyLine(string line)
        {
            UILabel bodyLabel = _root.AddUIComponent<UILabel>();
            bodyLabel.text = line;
            bodyLabel.relativePosition = new Vector3(Col0, _nextY);
            bodyLabel.textColor = Color.white;
            bodyLabel.textScale = 0.85f;
            bodyLabel.autoSize = true;
            bodyLabel.PerformLayout();
            _nextY += Mathf.Max(16f, bodyLabel.height + 4f);
            return bodyLabel;
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
            {
                if (_handlingSettingsChanged)
                {
                    return;
                }

                ModOptions.ApplyBool(
                    s,
                    xSettings =>
                    {
                        set(v);
                    }
                );
            };
            RegisterCheck(box, get);
        }

        private static void AddFloatPair(
            ModSettings s,
            string labelL,
            Func<float> getL,
            Action<ModSettings, float> applyL,
            string labelR,
            Func<float> getR,
            Action<ModSettings, float> applyR
        )
        {
            AddFloatAt(s, Col0, labelL, getL, applyL);
            if (getR != null && applyR != null && !string.IsNullOrEmpty(labelR))
            {
                AddFloatAt(s, Col1, labelR, getR, applyR);
            }

            _nextY += 26f;
        }

        private static void AddFloatAt(
            ModSettings s,
            float x,
            string label,
            Func<float> get,
            Action<ModSettings, float> apply
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
            field.selectOnFocus = true;
            field.isInteractive = true;
            // Wire first: enabling numericalOnly after assigning text can clear the field
            // (seen empty Rotate/Deadband values until Reset refreshed bindings).
            WireFloatTextFieldSubmit(field, () => SubmitFloatField(field, s, get, apply));
            RegisterFloatField(field, get);
            field.text = FormatFieldValue(get());
        }

        private static string FormatFieldValue(float value)
        {
            return ModOptions.FormatGain(value);
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
            btn.disabledBgSprite = "ButtonMenuDisabled";
            btn.textColor = Color.white;
            btn.disabledTextColor = new Color32(128, 128, 128, 255);
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

        private static void SavePanelPosition()
        {
            if (_root == null)
            {
                return;
            }

            ModSettings s = Mod.EnsureSettings();
            if (s == null)
            {
                return;
            }

            _root.MakePixelPerfect();
            ClampPanelIntoView();
            Vector3 p = _root.relativePosition;
            ModOptions.ApplyPanelPosition(s, p.x, p.y);
        }
    }
}
#endif
