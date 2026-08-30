#if HAS_CITIES
using System;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Floating in-game Assist / tuning panel host (ColossalUI).
    /// Product-surface feel controls; gated chrome / CAD / Contacts via ENABLE_* compile symbols.
    /// </summary>
    internal static class TuningPanelHost
    {
        private const float PanelWidth = 560f;
        private const float Col0 = 12f;
        private const float Col1 = 286f;
        private const float ColWidth = 260f;
        private const float FieldLabelW = 90f;
        private const float FieldInputW = 72f;

        private static UIPanel _root;
        private static UIButton _reopen;
        private static UILabel _presetDesc;
        private static UILabel _title;
        private static UITextField _feelNameField;
        private static float _nextY;
        private static bool _dragging;
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

            _root = view.AddUIComponent(typeof(UIPanel)) as UIPanel;
            if (_root == null)
            {
                return;
            }

            _root.name = "TrackpadCameraTuningPanel";
            _root.backgroundSprite = "MenuPanel2";
            _root.width = PanelWidth;
            _root.height = 520f;
            _root.relativePosition = new Vector3(40f, 60f);
            _root.canFocus = true;
            _root.isInteractive = true;
            _root.eventMouseDown += (c, e) =>
            {
                _root.BringToFront();
            };

            UILabel title = AddLabel(_root, "Trackpad Camera Control", Col0, 8f);
            title.textScale = 1.1f;
            _title = title;
            // Drag only from the title label so text fields keep focus and clicks.
            title.eventMouseDown += OnTitleMouseDown;
            title.eventMouseUp += OnTitleMouseUp;
            title.eventMouseMove += OnTitleMouseMove;
            _root.eventMouseUp += OnTitleMouseUp;

            UIButton close = _root.AddUIComponent<UIButton>();
            close.text = "X";
            close.width = 28f;
            close.height = 24f;
            close.relativePosition = new Vector3(_root.width - 36f, 8f);
            close.normalBgSprite = "ButtonMenu";
            close.hoveredBgSprite = "ButtonMenuHovered";
            close.pressedBgSprite = "ButtonMenuPressed";
            close.eventClick += (c, e) => HidePanel();

            _nextY = 36f;
            ModSettings s = Mod.EnsureSettings();

            AddSection("Feel presets");
            AddFeelPresetRow(s);

            _presetDesc = _root.AddUIComponent<UILabel>();
            _presetDesc.textColor = Color.white;
            _presetDesc.relativePosition = new Vector3(Col0, _nextY);
            _presetDesc.width = PanelWidth - 24f;
            _presetDesc.autoSize = false;
            _presetDesc.autoHeight = true;
            _presetDesc.wordWrap = true;
            _presetDesc.text =
#if ENABLE_CAD_GESTURE_STYLE
                ModOptions.PresetDescription(s.GesturePreset);
#else
                ModOptions.MapsPlusDescription;
#endif
            _presetDesc.PerformLayout();
            float descH = Mathf.Max(36f, _presetDesc.height + 4f);
            _nextY += descH;

#if ENABLE_CAD_GESTURE_STYLE
            AddSection("Gesture style");
            AddCadStyleButtons(s);
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddSection("Capture");
            AddCaptureBackendButtons(s);
#endif

            BuildPanSection(s);
            BuildZoomSection(s);
            BuildYawSection(s);
            BuildOrbitSection(s);

            _root.height = Mathf.Min(720f, _nextY + 16f);

            _reopen = view.AddUIComponent(typeof(UIButton)) as UIButton;
            if (_reopen != null)
            {
                _reopen.name = "TrackpadCameraTuningReopen";
                _reopen.text = "Trackpad";
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
            bool on = s != null && s.AssistUiEnabled;
            if (_root != null)
            {
                _root.isVisible = on;
            }

            if (_reopen != null)
            {
                _reopen.isVisible = !on || (_root != null && !_root.isVisible);
            }
        }

        public static void Destroy()
        {
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

            _presetDesc = null;
            _title = null;
            _feelNameField = null;
            _dragging = false;
        }

        private static void ShowPanel()
        {
            ModSettings s = Mod.EnsureSettings();
            s.AssistUiEnabled = true;
            ModOptions.NotifyChanged();
            if (_root == null)
            {
                EnsureCreated();
            }

            if (_root != null)
            {
                _root.isVisible = true;
            }

            if (_reopen != null)
            {
                _reopen.isVisible = false;
            }
        }

        private static void HidePanel()
        {
            if (_root != null)
            {
                _root.isVisible = false;
            }

            if (_reopen != null)
            {
                _reopen.isVisible = true;
            }

            ModOptions.FlushStore(true);
        }

        private static void RebuildPanel()
        {
            Destroy();
            EnsureCreated();
            ApplyVisibility();
        }

        private static void AddFeelPresetRow(ModSettings s)
        {
            float y = _nextY;
            float bw = 72f;
            float gap = 6f;
            float x = Col0;

            UIButton slow = MakeMenuButton("Slow", x, y, bw);
            slow.eventClick += (c, e) =>
            {
                ModOptions.ApplyFeelSlow(s);
                RebuildPanel();
            };
            x += bw + gap;

            UIButton def = MakeMenuButton("Default", x, y, bw);
            def.eventClick += (c, e) =>
            {
                ModOptions.ApplyFeelDefault(s);
                RebuildPanel();
            };
            x += bw + gap;

            UIButton fast = MakeMenuButton("Fast", x, y, bw);
            fast.eventClick += (c, e) =>
            {
                ModOptions.ApplyFeelFast(s);
                RebuildPanel();
            };
            x += bw + gap;

            UIButton reset = MakeMenuButton("Reset", x, y, bw);
            reset.eventClick += (c, e) =>
            {
                ModOptions.ResetToFactory(s);
                RebuildPanel();
            };
            _nextY += 32f;

            UILabel nameLbl = AddLabel(_root, "Name", Col0, _nextY + 2f);
            nameLbl.width = 40f;
            nameLbl.autoSize = false;

            _feelNameField = _root.AddUIComponent<UITextField>();
            _feelNameField.width = 180f;
            _feelNameField.height = 22f;
            _feelNameField.relativePosition = new Vector3(Col0 + 44f, _nextY);
            _feelNameField.normalBgSprite = "TextFieldPanel";
            _feelNameField.hoveredBgSprite = "TextFieldPanelHovered";
            _feelNameField.focusedBgSprite = "TextFieldPanel";
            _feelNameField.selectionSprite = "EmptySprite";
            _feelNameField.text = "";
            _feelNameField.selectOnFocus = true;
            _feelNameField.isInteractive = true;
            _feelNameField.builtinKeyNavigation = true;

            UIButton save = MakeMenuButton("Save as…", Col0 + 232f, _nextY - 2f, 90f);
            save.eventClick += (c, e) =>
            {
                string name = _feelNameField != null ? _feelNameField.text : "";
                ModOptions.SaveNamedFeelPreset(s, name);
            };

            UIButton load = MakeMenuButton("Load", Col0 + 328f, _nextY - 2f, 70f);
            load.eventClick += (c, e) =>
            {
                string name = _feelNameField != null ? _feelNameField.text : "";
                if (ModOptions.LoadNamedFeelPreset(s, name))
                {
                    RebuildPanel();
                }
            };
            _nextY += 30f;
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
            AddCheckRow(
                s,
                () => s.PanEnabled,
                v => s.PanEnabled = v,
                "Enable",
                () => s.InvertPanX,
                v => s.InvertPanX = v,
                "Reverse X"
            );
            AddCheckRow(
                s,
                () => s.InvertPanY,
                v => s.InvertPanY = v,
                "Reverse Y",
#if ENABLE_CONTACTS_CAPTURE
                () => s.PanLowPassEnabled,
                v => s.PanLowPassEnabled = v,
                "Low-pass"
#else
                null,
                null,
                null
#endif
            );
            AddFloatPair(
                s,
                "Sensitivity X",
                () => s.PanSensitivityX,
                ModOptions.ApplyPanSensitivityX,
                "Sensitivity Y",
                () => s.PanSensitivityY,
                ModOptions.ApplyPanSensitivityY
            );
#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Btn X",
                () => s.PanButtonScaleX,
                ModOptions.ApplyPanButtonScaleX,
                "Btn Y",
                () => s.PanButtonScaleY,
                ModOptions.ApplyPanButtonScaleY
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddFloatPair(
                s,
                "LP α",
                () => s.PanLowPassAlpha,
                ModOptions.ApplyPanLowPassAlpha,
                null,
                null,
                null
            );
#endif
        }

        private static void BuildZoomSection(ModSettings s)
        {
            AddOpHeading(ModOptions.OpHeadingZoom);
            AddCheckRow(
                s,
                () => s.ZoomEnabled,
                v => s.ZoomEnabled = v,
                "Enable",
                () => s.InvertZoom,
                v => s.InvertZoom = v,
                "Reverse"
            );
#if ENABLE_CONTACTS_CAPTURE
            AddCheckRow(
                s,
                () => s.ZoomLowPassEnabled,
                v => s.ZoomLowPassEnabled = v,
                "Low-pass",
                null,
                null,
                null
            );
#endif

#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.ZoomSensitivity,
                ModOptions.ApplyZoomSensitivity,
                "Btn",
                () => s.ZoomButtonScale,
                ModOptions.ApplyZoomButtonScale
            );
#else
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.ZoomSensitivity,
                ModOptions.ApplyZoomSensitivity,
                null,
                null,
                null
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddFloatPair(
                s,
                "LP α",
                () => s.ZoomLowPassAlpha,
                ModOptions.ApplyZoomLowPassAlpha,
                null,
                null,
                null
            );
#endif
        }

        private static void BuildYawSection(ModSettings s)
        {
            AddOpHeading(ModOptions.OpHeadingRotate);
            AddCheckRow(
                s,
                () => s.YawEnabled,
                v => s.YawEnabled = v,
                "Enable",
                () => s.InvertYawRotate,
                v => s.InvertYawRotate = v,
                "Reverse"
            );
#if ENABLE_CONTACTS_CAPTURE
            AddCheckRow(
                s,
                () => s.YawLowPassEnabled,
                v => s.YawLowPassEnabled = v,
                "Low-pass",
                null,
                null,
                null
            );
#endif

#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.YawRotateSensitivity,
                ModOptions.ApplyYawRotateSensitivity,
                "Btn",
                () => s.YawRotateButtonScale,
                ModOptions.ApplyYawRotateButtonScale
            );
#else
            AddFloatPair(
                s,
                "Sensitivity",
                () => s.YawRotateSensitivity,
                ModOptions.ApplyYawRotateSensitivity,
                null,
                null,
                null
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddFloatPair(
                s,
                "LP α",
                () => s.YawLowPassAlpha,
                ModOptions.ApplyYawLowPassAlpha,
                null,
                null,
                null
            );
#endif
        }

        private static void BuildOrbitSection(ModSettings s)
        {
            AddOpHeading(ModOptions.OpHeadingOrbit);
            AddCheckRow(
                s,
                () => s.OrbitEnabled,
                v => s.OrbitEnabled = v,
                "Enable",
                () => s.InvertOrbitYaw,
                v => s.InvertOrbitYaw = v,
                "Reverse yaw"
            );
            AddCheckRow(
                s,
                () => s.InvertOrbitPitch,
                v => s.InvertOrbitPitch = v,
                "Reverse pitch",
#if ENABLE_CONTACTS_CAPTURE
                () => s.OrbitLowPassEnabled,
                v => s.OrbitLowPassEnabled = v,
                "Low-pass"
#else
                null,
                null,
                null
#endif
            );
            AddFloatPair(
                s,
                "Sensitivity yaw",
                () => s.OrbitYawSensitivity,
                ModOptions.ApplyOrbitYawSensitivity,
                "Sensitivity pitch",
                () => s.OrbitPitchSensitivity,
                ModOptions.ApplyOrbitPitchSensitivity
            );
            AddFloatPair(
                s,
                "Pitch min",
                () => s.OrbitPitchMin,
                ModOptions.ApplyOrbitPitchMin,
                "Pitch max",
                () => s.OrbitPitchMax,
                ModOptions.ApplyOrbitPitchMax
            );
#if ENABLE_ASSIST_CHROME
            AddFloatPair(
                s,
                "Btn yaw",
                () => s.OrbitYawButtonScale,
                ModOptions.ApplyOrbitYawButtonScale,
                "Btn pitch",
                () => s.OrbitPitchButtonScale,
                ModOptions.ApplyOrbitPitchButtonScale
            );
#endif

#if ENABLE_CONTACTS_CAPTURE
            AddFloatPair(
                s,
                "LP α",
                () => s.OrbitLowPassAlpha,
                ModOptions.ApplyOrbitLowPassAlpha,
                null,
                null,
                null
            );
#endif
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
            label.width = PanelWidth - 24f;
            label.autoSize = false;
            label.autoHeight = true;
            label.wordWrap = true;
            label.PerformLayout();
            _nextY += Mathf.Max(22f, label.height + 4f);
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
            UILabel lbl = AddLabel(_root, label, x, _nextY + 2f);
            lbl.width = FieldLabelW;
            lbl.autoSize = false;

            UITextField field = _root.AddUIComponent<UITextField>();
            field.width = FieldInputW;
            field.height = 22f;
            field.relativePosition = new Vector3(x + FieldLabelW + 4f, _nextY);
            field.normalBgSprite = "TextFieldPanel";
            field.hoveredBgSprite = "TextFieldPanelHovered";
            field.focusedBgSprite = "TextFieldPanel";
            field.selectionSprite = "EmptySprite";
            field.text = ModOptions.FormatFloat(get());
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
                    field.text = ModOptions.FormatFloat(get());
                }
                else
                {
                    field.text = ModOptions.FormatFloat(get());
                }
            };
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
