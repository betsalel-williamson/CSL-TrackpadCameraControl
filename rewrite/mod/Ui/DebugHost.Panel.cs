#if HAS_CITIES
using System;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl.Rewrite
{
    public static partial class DebugHost
    {
        private const float PanelWidth = 560f;
        private const float TitleBarHeight = 32f;
        private const float HeaderButtonSize = 32f;
        private const float HeaderButtonRestOpacity = 0.55f;
        private const float FieldGutter = 12f;
        private const float Col0 = FieldGutter;
        private const float FieldLabelW = 132f;
        private const float FieldInputW = 72f;
        private const float FieldLabelGap = 4f;
        private const float FieldColumnW = FieldLabelW + FieldLabelGap + FieldInputW;
        private const float Col1 = PanelWidth - FieldGutter - FieldColumnW;
        private const float FooterCopyButtonWidth = 64f;
        private const float FooterCopyButtonHeight = 28f;

        private static UIPanel _root;
        private static UIPanel _titleBar;
        private static UIButton _closeButton;
        private static UIButton _optionsButton;
        private static UIButton _reopenButton;
        private static UILabel _titleLabel;
        private static UIDropDown _feelDropdown;
        private static UIButton _feelSaveAsButton;
        private static UIButton _feelDeleteButton;
        private static string[] _feelDropdownItems;
        private static float _nextY;
        private static bool _handlingSettingsChanged;

        private static void EnsurePanelBuilt()
        {
            if (_root != null)
            {
                return;
            }

            UIView view = UIView.GetAView();
            if (view == null || _editor == null)
            {
                return;
            }

            ModSettings settings = _editor.Settings;
            if (settings == null)
            {
                return;
            }

            ClearRefreshBindings();

            _root = view.AddUIComponent(typeof(UIPanel)) as UIPanel;
            if (_root == null)
            {
                return;
            }

            _root.name = "TrackpadCameraRewriteDebugPanel";
            _root.backgroundSprite = "MenuPanel2";
            _root.width = PanelWidth;
            _root.height = 420f;
            _root.relativePosition = new Vector3(
                settings.DebugPanelPosX,
                settings.DebugPanelPosY,
                0f
            );
            _root.canFocus = true;
            _root.isInteractive = true;

            BuildTitleBar();
            _nextY = TitleBarHeight + 8f;
            AddSection("Feel presets");
            AddFeelPresetRow(settings);
            BuildZoomSection(settings);
            BuildPanSection(settings);
            BuildRotateSection(settings);
            BuildOrbitSection(settings);
            AddBuildInfoFooter();
            _root.height = _nextY + 16f;
            ClampPanelIntoView();
            RefreshFloatFields();
            RefreshChecks();
            EnsureReopenChip(view);
            ApplyPanelVisibility();
        }

        private static void BuildTitleBar()
        {
            _titleBar = _root.AddUIComponent<UIPanel>();
            _titleBar.name = "TrackpadCameraRewriteDebugTitleBar";
            _titleBar.width = PanelWidth;
            _titleBar.height = TitleBarHeight;
            _titleBar.relativePosition = Vector3.zero;
            _titleBar.backgroundSprite = "GenericPanel";
            _titleBar.isInteractive = true;

            UIDragHandle drag = _titleBar.AddUIComponent<UIDragHandle>();
            drag.target = _root;
            drag.constrainToScreen = true;
            drag.eventMouseUp += (c, e) => SavePanelPosition();

            _titleLabel = _root.AddUIComponent<UILabel>();
            _titleLabel.text = Title;
            _titleLabel.textScale = 1.0f;
            _titleLabel.autoSize = false;
            _titleLabel.width = PanelWidth - (HeaderButtonSize * 2f) - 8f;
            _titleLabel.height = TitleBarHeight;
            _titleLabel.textAlignment = UIHorizontalAlignment.Center;
            _titleLabel.verticalAlignment = UIVerticalAlignment.Middle;
            _titleLabel.relativePosition = new Vector3(0f, 0f);
            _titleLabel.isInteractive = false;

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
            StyleHeaderButton(_optionsButton);
            _optionsButton.eventClick += (c, e) => OptionsPanelNavigation.OpenModOptions();
            _optionsButton.eventMouseDown += (c, e) => e.Use();

            _closeButton = _root.AddUIComponent<UIButton>();
            _closeButton.text = string.Empty;
            _closeButton.width = HeaderButtonSize;
            _closeButton.height = HeaderButtonSize;
            _closeButton.relativePosition = new Vector3(closeX, 0f);
            _closeButton.normalBgSprite = "buttonclose";
            _closeButton.hoveredBgSprite = "buttonclosehover";
            _closeButton.pressedBgSprite = "buttonclosepressed";
            StyleHeaderButton(_closeButton);
            _closeButton.eventClick += (c, e) => DismissPanel();
            _closeButton.eventMouseDown += (c, e) => e.Use();
            _optionsButton.BringToFront();
            _closeButton.BringToFront();
        }

        private static void AddFeelPresetRow(ModSettings s)
        {
            _feelDropdownItems = FeelHostBinder.BuildFeelPresetItems(_editor);
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
            _feelDropdown.selectedIndex = FeelHostBinder.IndexOfFeelPreset(
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

            _feelDropdown.eventSelectedIndexChanged += OnFeelDropdownSelected;

            UIButton reset = MakeMenuButton("Reset", Col0 + 208f, _nextY, 64f);
            reset.tabIndex = -1;
            reset.eventClick += (c, e) => _editor.ResetToFactory();

            _feelSaveAsButton = MakeMenuButton("Save as…", Col0 + 280f, _nextY, 88f);
            _feelSaveAsButton.tabIndex = -1;
            _feelSaveAsButton.isEnabled = OptionsHost.IsFeelDirtyNewPreset(s);
            _feelSaveAsButton.eventClick += (c, e) =>
            {
                e.Use();
                FeelSaveAsDialog.Show(_editor, RefreshFeelPresetButtons);
            };

            _feelDeleteButton = MakeMenuButton("Delete", Col0 + 376f, _nextY, 64f);
            _feelDeleteButton.tabIndex = -1;
            _feelDeleteButton.isEnabled = OptionsHost.IsNamedUserFeelPreset(s);
            _feelDeleteButton.eventClick += (c, e) =>
            {
                e.Use();
                if (OptionsHost.IsNamedUserFeelPreset(_editor.Settings))
                {
                    _editor.DeleteNamedPreset(_editor.Settings.ActiveFeelPresetName);
                }
            };
            _nextY += 32f;
        }

        private static void RefreshFeelPresetButtons()
        {
            if (_feelSaveAsButton != null)
            {
                _feelSaveAsButton.isEnabled = OptionsHost.IsFeelDirtyNewPreset(
                    _editor != null ? _editor.Settings : null
                );
            }

            if (_feelDeleteButton != null)
            {
                _feelDeleteButton.isEnabled = OptionsHost.IsNamedUserFeelPreset(
                    _editor != null ? _editor.Settings : null
                );
            }
        }

        private static void OnFeelDropdownSelected(UIComponent component, int index)
        {
            if (_handlingSettingsChanged || _feelDropdownItems == null || _editor == null)
            {
                return;
            }

            if (index < 0 || index >= _feelDropdownItems.Length)
            {
                return;
            }

            _editor.LoadPreset(_feelDropdownItems[index]);
        }

        private static void BuildZoomSection(ModSettings s)
        {
            AddSection("Zoom");
            AddFloatPair(
                "Sensitivity",
                () => s.ZoomGain,
                v => _editor.ApplyGain((x, g) => x.ZoomGain = g, v),
                null,
                null,
                null
            );
            AddFloatPair(
                "Deadband",
                () => s.PinchDeadband,
                v => _editor.ApplyGain((x, g) => x.PinchDeadband = g, v),
                null,
                null,
                null
            );
        }

        private static void BuildPanSection(ModSettings s)
        {
            AddSection("Pan");
            AddFloatPair(
                "Sensitivity X",
                () => s.PanGainX,
                v => _editor.ApplyGain((x, g) => x.PanGainX = g, v),
                "Sensitivity Y",
                () => s.PanGainY,
                v => _editor.ApplyGain((x, g) => x.PanGainY = g, v)
            );
            AddFloatPair(
                "Deadband",
                () => s.MotionDeadband,
                v => _editor.ApplyGain((x, g) => x.MotionDeadband = g, v),
                null,
                null,
                null
            );
        }

        private static void BuildRotateSection(ModSettings s)
        {
            AddSection("Rotate");
            AddFloatPair(
                "Sensitivity",
                () => s.RotateGain,
                v => _editor.ApplyGain((x, g) => x.RotateGain = g, v),
                null,
                null,
                null
            );
            AddFloatPair(
                "Deadband",
                () => s.RotateDeadband,
                v => _editor.ApplyGain((x, g) => x.RotateDeadband = g, v),
                null,
                null,
                null
            );
        }

        private static void BuildOrbitSection(ModSettings s)
        {
            AddSection("Orbit");
            AddFloatPair(
                "Sensitivity yaw",
                () => s.OrbitYawGain,
                v => _editor.ApplyGain((x, g) => x.OrbitYawGain = g, v),
                "Sensitivity pitch",
                () => s.OrbitPitchGain,
                v => _editor.ApplyGain((x, g) => x.OrbitPitchGain = g, v)
            );
            AddFloatPair(
                "Deadband",
                () => s.MotionDeadband,
                v => _editor.ApplyGain((x, g) => x.MotionDeadband = g, v),
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
                builtLine = "Built (local): ?";
            }

            _nextY += 8f;
            float actionsY = _nextY;

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
            ModSettings settings = _editor != null ? _editor.Settings : null;
            includeBox.isChecked = settings == null || settings.IncludeSystemInfoInCopy;
            includeBox.eventCheckChanged += (c, v) =>
            {
                if (_handlingSettingsChanged || _editor == null || _editor.Settings == null)
                {
                    return;
                }

                _editor.Settings.IncludeSystemInfoInCopy = v;
                FeelEditor.FlushStore(false);
                FeelEditor.NotifyChanged();
            };
            AssignTabOrder(includeBox);
            NumericTextFieldUi.WireTabStop(includeBox, tabScope: _root);
            RegisterCheck(
                includeBox,
                () =>
                {
                    ModSettings copySettings = _editor != null ? _editor.Settings : null;
                    return copySettings == null || copySettings.IncludeSystemInfoInCopy;
                }
            );

            float copyX = Col0 + includeBoxW + 8f;
            UIButton copy = MakeMenuButton("Copy", copyX, actionsY, FooterCopyButtonWidth);
            copy.height = FooterCopyButtonHeight;
            copy.tooltip = "Copy build info (UTC) and optional system info";
            copy.eventClick += (c, e) =>
            {
                ModSettings copySettings = _editor != null ? _editor.Settings : null;
                bool includeSystem = copySettings == null || copySettings.IncludeSystemInfoInCopy;
                GUIUtility.systemCopyBuffer = QaClipboardReport.Format(includeSystem);
            };
            copy.eventMouseDown += (c, e) => e.Use();

            _nextY = actionsY + FooterCopyButtonHeight + 4f;

            UILabel label = AddLabel(_root, builtLine, Col0, _nextY);
            label.textColor = new Color(1f, 1f, 1f, 0.75f);
            label.autoSize = true;
            label.wordWrap = false;
            label.isInteractive = false;
            label.PerformLayout();
            _nextY += Mathf.Max(18f, label.height + 4f);
        }

        private static void AddSection(string title)
        {
            AddLabel(_root, "— " + title + " —", Col0, _nextY);
            _nextY += 22f;
        }

        private static void AddFloatPair(
            string labelL,
            Func<float> getL,
            Action<float> applyL,
            string labelR,
            Func<float> getR,
            Action<float> applyR
        )
        {
            AddFloatAt(Col0, labelL, getL, applyL);
            if (getR != null && applyR != null && !string.IsNullOrEmpty(labelR))
            {
                AddFloatAt(Col1, labelR, getR, applyR);
            }

            _nextY += 26f;
        }

        private static void AddFloatAt(float x, string label, Func<float> get, Action<float> apply)
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
            // Wire first: enabling numericalOnly after assigning text can clear the field.
            WireFloatTextFieldSubmit(field, () => SubmitFloatField(field, get, apply));
            RegisterFloatField(field, get);
            field.text = FormatFieldValue(get());
        }

        private static string FormatFieldValue(float value)
        {
            return NumericFieldInput.FormatGain(value);
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

        private static void EnsureReopenChip(UIView view)
        {
            if (_reopenButton != null)
            {
                return;
            }

            _reopenButton = view.AddUIComponent(typeof(UIButton)) as UIButton;
            if (_reopenButton == null)
            {
                return;
            }

            _reopenButton.name = "TrackpadCameraRewriteDebugReopen";
            _reopenButton.text = "Debug";
            _reopenButton.width = 90f;
            _reopenButton.height = 28f;
            _reopenButton.relativePosition = new Vector3(40f, 40f);
            _reopenButton.normalBgSprite = "ButtonMenu";
            _reopenButton.hoveredBgSprite = "ButtonMenuHovered";
            _reopenButton.pressedBgSprite = "ButtonMenuPressed";
            _reopenButton.eventClick += (c, e) => ReopenPanel();
            _reopenButton.isVisible = false;
        }

        private static void StyleHeaderButton(UIButton button)
        {
            if (button == null)
            {
                return;
            }

            SetHeaderButtonOpacity(button, HeaderButtonRestOpacity);
            button.eventMouseEnter += (c, e) => SetHeaderButtonOpacity(button, 1f);
            button.eventMouseLeave += (c, e) =>
                SetHeaderButtonOpacity(button, HeaderButtonRestOpacity);
            button.eventMouseDown += (c, e) => SetHeaderButtonOpacity(button, 1f);
        }

        private static void SetHeaderButtonOpacity(UIButton button, float opacity)
        {
            if (button == null)
            {
                return;
            }

            button.opacity = opacity;
            Color32 color = button.color;
            color.a = (byte)Mathf.Clamp(Mathf.RoundToInt(opacity * 255f), 0, 255);
            button.color = color;
        }

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

        private static void ApplyPanelVisibility()
        {
            ModSettings settings = _editor != null ? _editor.Settings : null;
            bool assistEnabled = settings != null && settings.AssistUiEnabled;
            bool dismissed = settings != null && settings.DebugPanelDismissed;
            bool showRoot = ShouldShowRoot(assistEnabled, dismissed);
            bool showReopen = ShouldShowReopen(assistEnabled, dismissed);

            if (_root != null)
            {
                _root.isVisible = showRoot;
            }

            if (_reopenButton != null)
            {
                _reopenButton.isVisible = showReopen;
            }
        }

        private static void DestroyPanel()
        {
            FeelSaveAsDialog.Close();
            if (_root != null)
            {
                SavePanelPosition();
                UnityEngine.Object.Destroy(_root.gameObject);
                _root = null;
            }

            if (_reopenButton != null)
            {
                UnityEngine.Object.Destroy(_reopenButton.gameObject);
                _reopenButton = null;
            }

            _titleBar = null;
            _closeButton = null;
            _optionsButton = null;
            _titleLabel = null;
            _feelDropdown = null;
            _feelSaveAsButton = null;
            _feelDeleteButton = null;
            _feelDropdownItems = null;
            ClearRefreshBindings();
        }

        private static void DismissPanel()
        {
            if (_editor != null)
            {
                _editor.DismissDebugPanel();
            }

            ApplyPanelVisibility();
        }

        private static void ReopenPanel()
        {
            if (_editor != null)
            {
                _editor.SetShowDebugPanel(true);
            }

            if (_root == null)
            {
                EnsurePanelBuilt();
            }

            ApplyPanelVisibility();
        }

        private static void SavePanelPosition()
        {
            if (_root == null || _editor == null || _editor.Settings == null)
            {
                return;
            }

            _root.MakePixelPerfect();
            ClampPanelIntoView();
            Vector3 p = _root.relativePosition;
            _editor.SaveDebugPanelPosition(p.x, p.y);
        }

        private static void OnPanelSettingsChanged()
        {
            if (_handlingSettingsChanged)
            {
                return;
            }

            ApplyPanelVisibility();
            if (_root == null)
            {
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
                DestroyPanel();
                EnsurePanelBuilt();
                if (_root != null)
                {
                    _root.relativePosition = pos;
                }

                ApplyPanelVisibility();
            }
            finally
            {
                _handlingSettingsChanged = false;
            }
        }
    }
}
#endif
