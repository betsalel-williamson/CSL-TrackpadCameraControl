#if HAS_CITIES
using System;
using ColossalFramework.UI;
using UnityEngine;

namespace TrackpadCameraControl
{
    /// <summary>Small Confirm name dialog for Feel Save as… (Debug and Options).</summary>
    internal static class FeelSaveAsDialog
    {
        private const float DialogWidth = 320f;
        private const float DialogHeight = 120f;

        private static UIPanel _dialog;
        private static UITextField _nameField;
        private static Action _onClosed;

        /// <summary>Show dialog; OK runs <see cref="ModOptions.SaveNamedFeelPreset"/> then closes.</summary>
        public static void Show(ModSettings settings, Action onClosed = null)
        {
            if (settings == null)
            {
                return;
            }

            Close();
            _onClosed = onClosed;

            UIView view = UIView.GetAView();
            if (view == null)
            {
                return;
            }

            _dialog = view.AddUIComponent(typeof(UIPanel)) as UIPanel;
            if (_dialog == null)
            {
                return;
            }

            _dialog.name = "TrackpadCameraFeelSaveAsDialog";
            _dialog.backgroundSprite = "MenuPanel2";
            _dialog.width = DialogWidth;
            _dialog.height = DialogHeight;
            // Colossal UI coords use UIView size — not Unity Screen pixels (HiDPI off-screen).
            float uiW = view.fixedWidth > 1f ? view.fixedWidth : Screen.width;
            float uiH = view.fixedHeight > 1f ? view.fixedHeight : Screen.height;
            _dialog.relativePosition = new Vector3(
                Mathf.Floor((uiW - DialogWidth) * 0.5f),
                Mathf.Floor((uiH - DialogHeight) * 0.5f)
            );
            _dialog.canFocus = true;
            _dialog.isInteractive = true;
            _dialog.isVisible = true;
            _dialog.opacity = 1f;

            UILabel title = _dialog.AddUIComponent<UILabel>();
            title.text = "Save feel preset as…";
            title.relativePosition = new Vector3(12f, 10f);
            title.textColor = Color.white;
            title.autoSize = true;

            _nameField = _dialog.AddUIComponent<UITextField>();
            _nameField.width = DialogWidth - 24f;
            _nameField.height = 24f;
            _nameField.relativePosition = new Vector3(12f, 36f);
            _nameField.normalBgSprite = "TextFieldPanel";
            _nameField.hoveredBgSprite = "TextFieldPanelHovered";
            _nameField.focusedBgSprite = "TextFieldPanel";
            _nameField.selectionSprite = "EmptySprite";
            _nameField.horizontalAlignment = UIHorizontalAlignment.Left;
            _nameField.text = SuggestName(settings);
            _nameField.selectOnFocus = true;
            _nameField.isInteractive = true;
            _nameField.eventTextSubmitted += (c, t) => TrySave(settings);

            UIButton cancel = _dialog.AddUIComponent<UIButton>();
            StyleDialogButton(cancel, "Cancel", 12f, 76f, 90f);
            cancel.eventClick += (c, e) =>
            {
                e.Use();
                Close();
            };

            UIButton ok = _dialog.AddUIComponent<UIButton>();
            StyleDialogButton(ok, "OK", DialogWidth - 12f - 90f, 76f, 90f);
            ok.eventClick += (c, e) =>
            {
                e.Use();
                TrySave(settings);
            };

            _dialog.BringToFront();
            _dialog.Focus();
            _nameField.Focus();
        }

        private static string SuggestName(ModSettings settings)
        {
            if (settings == null || string.IsNullOrEmpty(settings.ActiveFeelPresetName))
            {
                return string.Empty;
            }

            if (
                FeelProfiles.IsBuiltInName(settings.ActiveFeelPresetName)
                || string.Equals(
                    settings.ActiveFeelPresetName,
                    FeelProfiles.NameNewPreset,
                    StringComparison.Ordinal
                )
            )
            {
                return string.Empty;
            }

            return settings.ActiveFeelPresetName;
        }

        private static void TrySave(ModSettings settings)
        {
            string name = _nameField != null ? _nameField.text : string.Empty;
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(name.Trim()))
            {
                return;
            }

            name = name.Trim();
            if (FeelProfiles.IsBuiltInName(name))
            {
                return;
            }

            if (!ModOptions.SaveNamedFeelPreset(settings, name))
            {
                return;
            }

            Close();
        }

        private static void StyleDialogButton(
            UIButton button,
            string text,
            float x,
            float y,
            float w
        )
        {
            button.text = text;
            button.width = w;
            button.height = 28f;
            button.relativePosition = new Vector3(x, y);
            button.normalBgSprite = "ButtonMenu";
            button.hoveredBgSprite = "ButtonMenuHovered";
            button.pressedBgSprite = "ButtonMenuPressed";
            button.disabledBgSprite = "ButtonMenuDisabled";
            button.textScale = 0.85f;
        }

        public static void Close()
        {
            if (_dialog != null)
            {
                UnityEngine.Object.Destroy(_dialog.gameObject);
                _dialog = null;
            }

            _nameField = null;
            Action closed = _onClosed;
            _onClosed = null;
            if (closed != null)
            {
                closed();
            }
        }
    }
}
#endif
