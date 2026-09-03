#if HAS_CITIES
using System;
using ColossalFramework.UI;
using ICities;
using UnityEngine;

namespace TrackpadCameraControl.Rewrite
{
    public static partial class DebugHost
    {
        private const float PanelWidth = 480f;
        private const float TitleBarHeight = 32f;
        private const float HeaderButtonSize = 32f;
        private const float HeaderButtonRestOpacity = 0.55f;

        private static UIPanel _root;
        private static UIPanel _titleBar;
        private static UIButton _closeButton;
        private static UIButton _reopenButton;
        private static UILabel _titleLabel;
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
            BuildCatalogContent();
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
            _closeButton.BringToFront();
        }

        private static void BuildCatalogContent()
        {
            UIPanel content = _root.AddUIComponent<UIPanel>();
            content.name = "TrackpadCameraRewriteDebugContent";
            content.relativePosition = new Vector3(8f, TitleBarHeight + 8f);
            content.width = PanelWidth - 16f;
            content.height = _root.height - TitleBarHeight - 16f;
            content.autoLayout = true;
            content.autoLayoutDirection = LayoutDirection.Vertical;
            content.autoLayoutPadding = new RectOffset(4, 4, 4, 4);

            var helper = new UIHelper(content);
            FeelHostBinder.BindCatalog(helper, _editor);
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
            _reopenButton.text = Title;
            _reopenButton.width = 220f;
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

            button.opacity = HeaderButtonRestOpacity;
            button.hoveredOpacity = 1f;
            button.pressedOpacity = 1f;
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
            _titleLabel = null;
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
