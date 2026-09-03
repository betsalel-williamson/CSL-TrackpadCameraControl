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
        private static bool _modalPushed;

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

            // ClipCast only accepts hits under the top modal (IsChildOf). A sibling panel on
            // UIView is invisible to mouse while Options is modal. Parent under the current
            // modal so clicks work without a nested PushModal (nested PushModal + BringToFront
            // parks panelsLibraryModalEffect over Options → permanent "blur" after close).
            UIComponent host = UIView.GetModalComponent();
            if (host != null)
            {
                _dialog = host.AddUIComponent<UIPanel>();
                _modalPushed = false;
            }
            else
            {
                _dialog = view.AddUIComponent(typeof(UIPanel)) as UIPanel;
                if (_dialog == null)
                {
                    return;
                }

                UIView.PushModal(_dialog);
                _modalPushed = true;
            }

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
            // absolutePosition is UIView space whether parented under Options or the root view.
            _dialog.absolutePosition = new Vector3(
                Mathf.Floor((uiW - DialogWidth) * 0.5f),
                Mathf.Floor((uiH - DialogHeight) * 0.5f)
            );
            _dialog.canFocus = true;
            _dialog.isInteractive = true;
            _dialog.isVisible = true;
            _dialog.opacity = 1f;
            _dialog.BringToFront();

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
            _nameField.selectionBackgroundColor = new Color32(0, 105, 210, 255);
            _nameField.cursorBlinkTime = 0.45f;
            _nameField.cursorWidth = 1;
            _nameField.padding = new RectOffset(6, 6, 3, 3);
            _nameField.horizontalAlignment = UIHorizontalAlignment.Left;
            _nameField.verticalAlignment = UIVerticalAlignment.Middle;
            _nameField.textColor = Color.white;
            _nameField.disabledTextColor = new Color32(128, 128, 128, 255);
            _nameField.text = ModOptions.SuggestFeelSaveAsName(settings);
            _nameField.selectOnFocus = true;
            _nameField.isInteractive = true;
            _nameField.canFocus = true;
            _nameField.readOnly = false;
            // Programmatic UITextField defaults m_BuiltinKeyNavigation=false; OnKeyPress inserts
            // only when this is true. Template fields (Options UIHelper) get it from the prefab.
            _nameField.builtinKeyNavigation = true;
            // Default true: a focus flicker on open would Submit → TrySave and close the dialog.
            _nameField.submitOnFocusLost = false;
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

            _dialog.Focus();
            _nameField.Focus();
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
                if (_modalPushed && UIView.GetModalComponent() == _dialog)
                {
                    UIView.PopModal();
                }

                _modalPushed = false;
                UnityEngine.Object.Destroy(_dialog.gameObject);
                _dialog = null;
            }
            else
            {
                _modalPushed = false;
            }

            _nameField = null;
            // If we nested PushModal (Debug path), PopModal + BringToFront re-slots the
            // library modal effect under Options. Options-only path never pushed, so Options
            // stayed top modal and never needed this — still safe to run.
            RestoreOptionsChromeAfterClose();

            Action closed = _onClosed;
            _onClosed = null;
            if (closed != null)
            {
                closed();
            }
        }

        private static void RestoreOptionsChromeAfterClose()
        {
            try
            {
                if (UIView.library == null)
                {
                    return;
                }

                OptionsMainPanel options = UIView.library.Get<OptionsMainPanel>("OptionsPanel");
                if (options == null || options.component == null || !options.component.isVisible)
                {
                    return;
                }

                // BringToFront while Options is GetModalComponent() places panelsLibraryModalEffect
                // at zOrder = Options.zOrder - 1 (see UIView.BringToFront). Focus alone does not.
                options.component.BringToFront();
                UIView.SetFocus(options.component);
                options.component.Focus();

                UIComponent modalEffect = null;
                UIView view = options.component.GetUIView();
                if (view != null)
                {
                    modalEffect = view.panelsLibraryModalEffect;
                }

                if (modalEffect != null && UIView.GetModalComponent() == options.component)
                {
                    modalEffect.zOrder = options.component.zOrder - 1;
                    modalEffect.Show(false);
                    modalEffect.opacity = 1f;
                }
            }
            catch
            {
                // fail soft — Debug-only Save as has no Options to restore
            }
        }
    }
}
#endif
