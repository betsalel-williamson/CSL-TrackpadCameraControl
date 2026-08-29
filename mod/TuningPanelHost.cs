#if HAS_CITIES
using System;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl
{
    /// <summary>
    /// Floating in-game Assist / tuning panel host (ColossalUI).
    /// </summary>
    internal static class TuningPanelHost
    {
        private static UIPanel _root;
        private static UIButton _reopen;
        private static UILabel _presetDesc;
        private static float _nextY;

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
            _root.width = 440f;
            _root.height = 640f;
            _root.relativePosition = new Vector3(40f, 80f);
            _root.canFocus = true;
            _root.isInteractive = true;
            _root.eventMouseDown += (c, e) =>
            {
                _root.BringToFront();
            };

            MakeDraggable(_root);

            UILabel title = AddLabel(_root, "Trackpad Camera Control", 12f, 8f, true);
            title.textScale = 1.1f;

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

            AddSection("Preset");
            AddDropdownLikeButtons(s);
            _presetDesc = AddLabel(
                _root,
                ModOptions.PresetDescription(s.GesturePreset),
                12f,
                _nextY,
                false
            );
            _presetDesc.width = _root.width - 24f;
            _presetDesc.wordWrap = true;
            _presetDesc.autoHeight = true;
            _nextY += 48f;

            AddButton(
                "Reset to factory default",
                () =>
                {
                    ModOptions.ResetToFactory(s);
                    Destroy();
                    EnsureCreated();
                    ApplyVisibility();
                }
            );

            BuildPanSection(s);
            BuildZoomSection(s);
            BuildYawSection(s);
            BuildOrbitSection(s);

            // Scroll by clipping height; keep content stacked.
            _root.height = Mathf.Min(700f, _nextY + 16f);

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

        private static void AddDropdownLikeButtons(ModSettings s)
        {
            UIButton maps = _root.AddUIComponent<UIButton>();
            maps.text = "Maps+";
            maps.width = 100f;
            maps.height = 28f;
            maps.relativePosition = new Vector3(12f, _nextY);
            maps.normalBgSprite = "ButtonMenu";
            maps.hoveredBgSprite = "ButtonMenuHovered";
            maps.pressedBgSprite = "ButtonMenuPressed";
            maps.eventClick += (c, e) =>
            {
                ModOptions.ApplyGesturePresetIndex(s, 0);
                if (_presetDesc != null)
                {
                    _presetDesc.text = ModOptions.MapsPlusDescription;
                }
            };

            UIButton cad = _root.AddUIComponent<UIButton>();
            cad.text = "CAD";
            cad.width = 100f;
            cad.height = 28f;
            cad.relativePosition = new Vector3(120f, _nextY);
            cad.normalBgSprite = "ButtonMenu";
            cad.hoveredBgSprite = "ButtonMenuHovered";
            cad.pressedBgSprite = "ButtonMenuPressed";
            cad.eventClick += (c, e) =>
            {
                ModOptions.ApplyGesturePresetIndex(s, 1);
                if (_presetDesc != null)
                {
                    _presetDesc.text = ModOptions.CadDescription;
                }
            };
            _nextY += 32f;
        }

        private static void BuildPanSection(ModSettings s)
        {
            AddSection("Pan");
            AddEnableReverse(s, () => s.PanEnabled, v => s.PanEnabled = v, "Enable");
            AddEnableReverse(s, () => s.InvertPanX, v => s.InvertPanX = v, "Reverse X");
            AddEnableReverse(s, () => s.InvertPanY, v => s.InvertPanY = v, "Reverse Y");
            AddFloatRow(s, "Drag X", () => s.PanSensitivityX, ModOptions.ApplyPanSensitivityX);
            AddFloatRow(s, "Drag Y", () => s.PanSensitivityY, ModOptions.ApplyPanSensitivityY);
            AddFloatRow(s, "Btn X", () => s.PanButtonScaleX, ModOptions.ApplyPanButtonScaleX);
            AddFloatRow(s, "Btn Y", () => s.PanButtonScaleY, ModOptions.ApplyPanButtonScaleY);
            AddEnableReverse(
                s,
                () => s.PanLowPassEnabled,
                v => s.PanLowPassEnabled = v,
                "Low-pass"
            );
            AddFloatRow(s, "LP α", () => s.PanLowPassAlpha, ModOptions.ApplyPanLowPassAlpha);

            AddChromeButtons(
                new string[] { "Pad", "N", "S", "E", "W" },
                new Action[]
                {
                    () => Nudge(CameraOp.Pan, 0.02f, 0f, 0f, 0f, true),
                    () => NudgeButton(CameraOp.Pan, 0f, 1f, 0f, 0f),
                    () => NudgeButton(CameraOp.Pan, 0f, -1f, 0f, 0f),
                    () => NudgeButton(CameraOp.Pan, 1f, 0f, 0f, 0f),
                    () => NudgeButton(CameraOp.Pan, -1f, 0f, 0f, 0f),
                }
            );
        }

        private static void BuildZoomSection(ModSettings s)
        {
            AddSection("Zoom");
            AddEnableReverse(s, () => s.ZoomEnabled, v => s.ZoomEnabled = v, "Enable");
            AddEnableReverse(s, () => s.InvertZoom, v => s.InvertZoom = v, "Reverse");
            AddFloatRow(s, "Drag", () => s.ZoomSensitivity, ModOptions.ApplyZoomSensitivity);
            AddFloatRow(s, "Btn", () => s.ZoomButtonScale, ModOptions.ApplyZoomButtonScale);
            AddEnableReverse(
                s,
                () => s.ZoomLowPassEnabled,
                v => s.ZoomLowPassEnabled = v,
                "Low-pass"
            );
            AddFloatRow(s, "LP α", () => s.ZoomLowPassAlpha, ModOptions.ApplyZoomLowPassAlpha);
            AddChromeButtons(
                new string[] { "+", "−" },
                new Action[]
                {
                    () => NudgeButton(CameraOp.Zoom, 0f, 0f, 1f, 0f),
                    () => NudgeButton(CameraOp.Zoom, 0f, 0f, -1f, 0f),
                }
            );
        }

        private static void BuildYawSection(ModSettings s)
        {
            AddSection("Rotate (yaw)");
            AddEnableReverse(s, () => s.YawEnabled, v => s.YawEnabled = v, "Enable");
            AddEnableReverse(s, () => s.InvertYawRotate, v => s.InvertYawRotate = v, "Reverse");
            AddFloatRow(s, "Drag", () => s.YawRotateSensitivity, ModOptions.ApplyYawRotateSensitivity);
            AddFloatRow(s, "Btn", () => s.YawRotateButtonScale, ModOptions.ApplyYawRotateButtonScale);
            AddEnableReverse(
                s,
                () => s.YawLowPassEnabled,
                v => s.YawLowPassEnabled = v,
                "Low-pass"
            );
            AddFloatRow(s, "LP α", () => s.YawLowPassAlpha, ModOptions.ApplyYawLowPassAlpha);
            AddChromeButtons(
                new string[] { "◀", "▶" },
                new Action[]
                {
                    () => NudgeButton(CameraOp.Yaw, 0f, 0f, 0f, -1f),
                    () => NudgeButton(CameraOp.Yaw, 0f, 0f, 0f, 1f),
                }
            );
        }

        private static void BuildOrbitSection(ModSettings s)
        {
            AddSection("Orbit");
            AddEnableReverse(s, () => s.OrbitEnabled, v => s.OrbitEnabled = v, "Enable");
            AddEnableReverse(s, () => s.InvertOrbitYaw, v => s.InvertOrbitYaw = v, "Reverse yaw");
            AddEnableReverse(
                s,
                () => s.InvertOrbitPitch,
                v => s.InvertOrbitPitch = v,
                "Reverse pitch"
            );
            AddFloatRow(s, "Drag yaw", () => s.OrbitYawSensitivity, ModOptions.ApplyOrbitYawSensitivity);
            AddFloatRow(
                s,
                "Drag pitch",
                () => s.OrbitPitchSensitivity,
                ModOptions.ApplyOrbitPitchSensitivity
            );
            AddFloatRow(s, "Btn yaw", () => s.OrbitYawButtonScale, ModOptions.ApplyOrbitYawButtonScale);
            AddFloatRow(
                s,
                "Btn pitch",
                () => s.OrbitPitchButtonScale,
                ModOptions.ApplyOrbitPitchButtonScale
            );
            AddEnableReverse(
                s,
                () => s.OrbitLowPassEnabled,
                v => s.OrbitLowPassEnabled = v,
                "Low-pass"
            );
            AddFloatRow(s, "LP α", () => s.OrbitLowPassAlpha, ModOptions.ApplyOrbitLowPassAlpha);
            AddChromeButtons(
                new string[] { "Yaw+", "Yaw−", "Pitch+", "Pitch−" },
                new Action[]
                {
                    () => NudgeButton(CameraOp.Orbit, 1f, 0f, 0f, 0f),
                    () => NudgeButton(CameraOp.Orbit, -1f, 0f, 0f, 0f),
                    () => NudgeButton(CameraOp.Orbit, 0f, 1f, 0f, 0f),
                    () => NudgeButton(CameraOp.Orbit, 0f, -1f, 0f, 0f),
                }
            );
        }

        private static void NudgeButton(
            CameraOp op,
            float dxSign,
            float dySign,
            float pinchSign,
            float rotateSign
        )
        {
            ModSettings s = Mod.Settings;
            GesturePipeline pipe = Mod.Pipeline;
            if (s == null || pipe == null || pipe.Camera == null)
            {
                return;
            }

            if (op == CameraOp.Pan && !s.PanEnabled)
            {
                return;
            }

            if (op == CameraOp.Zoom && !s.ZoomEnabled)
            {
                return;
            }

            if (op == CameraOp.Yaw && !s.YawEnabled)
            {
                return;
            }

            if (op == CameraOp.Orbit && !s.OrbitEnabled)
            {
                return;
            }

            CameraApplicator.ApplyButton(
                op,
                dxSign,
                dySign,
                pinchSign,
                rotateSign,
                s,
                pipe.Camera
            );
        }

        private static void Nudge(
            CameraOp op,
            float dx,
            float dy,
            float pinch,
            float rotate,
            bool drag
        )
        {
            ModSettings s = Mod.Settings;
            GesturePipeline pipe = Mod.Pipeline;
            if (s == null || pipe == null || pipe.Camera == null)
            {
                return;
            }

            CameraApplicator.Apply(
                op,
                dx,
                dy,
                pinch,
                rotate,
                s,
                pipe.Camera,
                drag ? CameraApplicator.InputModality.Drag : CameraApplicator.InputModality.Button
            );
        }

        private static void AddSection(string title)
        {
            UILabel label = AddLabel(_root, "— " + title + " —", 12f, _nextY, true);
            _nextY += 22f;
        }

        private static void AddEnableReverse(
            ModSettings s,
            Func<bool> get,
            Action<bool> set,
            string label
        )
        {
            UICheckBox box = _root.AddUIComponent<UICheckBox>();
            box.width = _root.width - 24f;
            box.height = 20f;
            box.relativePosition = new Vector3(12f, _nextY);
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
                    x =>
                    {
                        set(v);
                    }
                );
            _nextY += 22f;
        }

        private static void AddFloatRow(
            ModSettings s,
            string label,
            Func<float> get,
            Action<ModSettings, float> apply
        )
        {
            UILabel lbl = AddLabel(_root, label, 12f, _nextY, false);
            lbl.width = 90f;

            UITextField field = _root.AddUIComponent<UITextField>();
            field.width = 100f;
            field.height = 22f;
            field.relativePosition = new Vector3(110f, _nextY);
            field.normalBgSprite = "TextFieldPanel";
            field.hoveredBgSprite = "TextFieldPanelHovered";
            field.focusedBgSprite = "TextFieldPanel";
            field.selectionSprite = "EmptySprite";
            field.text = ModOptions.FormatFloat(get());
            field.numericalOnly = false;
            field.allowFloats = true;
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
            _nextY += 26f;
        }

        private static UIButton AddButton(string text, Action onClick)
        {
            UIButton btn = _root.AddUIComponent<UIButton>();
            btn.text = text;
            btn.width = Mathf.Min(280f, _root.width - 24f);
            btn.height = 28f;
            btn.relativePosition = new Vector3(12f, _nextY);
            btn.normalBgSprite = "ButtonMenu";
            btn.hoveredBgSprite = "ButtonMenuHovered";
            btn.pressedBgSprite = "ButtonMenuPressed";
            btn.eventClick += (c, e) =>
            {
                if (onClick != null)
                {
                    onClick();
                }
            };
            _nextY += 32f;
            return btn;
        }

        private static void AddChromeButtons(string[] labels, Action[] actions)
        {
            if (labels == null || actions == null)
            {
                return;
            }

            float x = 12f;
            int n = Math.Min(labels.Length, actions.Length);
            for (int i = 0; i < n; i++)
            {
                string label = labels[i];
                Action action = actions[i];
                UIButton btn = _root.AddUIComponent<UIButton>();
                btn.text = label ?? "?";
                btn.width = 70f;
                btn.height = 26f;
                btn.relativePosition = new Vector3(x, _nextY);
                btn.normalBgSprite = "ButtonMenu";
                btn.hoveredBgSprite = "ButtonMenuHovered";
                btn.pressedBgSprite = "ButtonMenuPressed";
                Action captured = action;
                btn.eventClick += (c, e) =>
                {
                    if (captured != null)
                    {
                        captured();
                    }
                };
                x += 76f;
            }

            _nextY += 30f;
        }

        private static UILabel AddLabel(UIPanel parent, string text, float x, float y, bool bold)
        {
            UILabel label = parent.AddUIComponent<UILabel>();
            label.text = text;
            label.relativePosition = new Vector3(x, y);
            label.textColor = Color.white;
            label.autoSize = true;
            return label;
        }

        private static void MakeDraggable(UIPanel panel)
        {
            bool dragging = false;
            Vector3 panelStart = Vector3.zero;
            Vector3 mouseStart = Vector3.zero;

            panel.eventMouseDown += (c, e) =>
            {
                dragging = true;
                panelStart = panel.absolutePosition;
                mouseStart = Input.mousePosition;
                panel.BringToFront();
            };
            panel.eventMouseUp += (c, e) =>
            {
                dragging = false;
            };
            panel.eventMouseMove += (c, e) =>
            {
                if (!dragging)
                {
                    return;
                }

                Vector3 delta = Input.mousePosition - mouseStart;
                // Unity screen Y is up; UI Y is down.
                panel.absolutePosition =
                    panelStart + new Vector3(delta.x, -delta.y, 0f);
            };
        }
    }
}
#endif
