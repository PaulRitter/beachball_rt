using Robust.Client.Graphics;
using Robust.Client.ResourceManagement;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Client.UserInterface.CustomControls;
using Robust.Shared.IoC;
using Robust.Shared.Maths;

namespace Content.Client.UserInterface;

/// <summary>
///     Creates a stylesheet for UI.
///     This is required so text and controls show up.
///     (Please note that this stylesheet is simple and incomplete.)
/// </summary>
public sealed class StyleSheetManager
{
    [Dependency] private readonly IUserInterfaceManager _userInterfaceManager = default!;
    [Dependency] private readonly IResourceCache _resourceCache = default!;

    public const string StyleClassLabelGame = "LabelGame";
        
    public void Initialize()
    {
        var fontRes = _resourceCache.GetResource<FontResource>("/Fonts/afrikas.woff");
        var font = new VectorFont(fontRes, 10);
        var bigFont = new VectorFont(fontRes, 32);

        var panelTex = _resourceCache.GetResource<TextureResource>("/Textures/Interface/panel.png");
        var panel = new StyleBoxTexture { Texture = panelTex };
        panel.SetPatchMargin(StyleBox.Margin.All, 2);
        panel.SetExpandMargin(StyleBox.Margin.All, 2);

        var panelDarkTex = _resourceCache.GetResource<TextureResource>("/Textures/Interface/panelDark.png");
        var panelDark = new StyleBoxTexture { Texture = panelDarkTex };
        panelDark.SetPatchMargin(StyleBox.Margin.All, 2);
        panelDark.SetExpandMargin(StyleBox.Margin.All, 2);

        var panelDarkHighlightTex = _resourceCache.GetResource<TextureResource>("/Textures/Interface/panelDarkHighlight.png");
        var panelDarkHighlight = new StyleBoxTexture { Texture = panelDarkHighlightTex };
        panelDarkHighlight.SetPatchMargin(StyleBox.Margin.All, 2);
        panelDarkHighlight.SetExpandMargin(StyleBox.Margin.All, 2);

        var tabContainerPanelTex = _resourceCache.GetResource<TextureResource>("/Textures/Interface/tabPanel.png");
        var tabContainerPanel = new StyleBoxTexture
        {
            Texture = tabContainerPanelTex.Texture,
        };
        tabContainerPanel.SetPatchMargin(StyleBox.Margin.All, 2);

        var tabContainerBoxActive = new StyleBoxFlat {BackgroundColor = new Color(64, 64, 64)};
        tabContainerBoxActive.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);
        var tabContainerBoxInactive = new StyleBoxFlat {BackgroundColor = new Color(32, 32, 32)};
        tabContainerBoxInactive.SetContentMarginOverride(StyleBox.Margin.Horizontal, 5);
            
        var textureCloseButton = _resourceCache.GetResource<TextureResource>("/Textures/Interface/cross.png").Texture;

        _userInterfaceManager.Stylesheet = new Stylesheet(new[]
        {
            new StyleRule(
                new SelectorElement(null, null, null, null),
                new[]
                {
                    new StyleProperty("font", font),
                    new StyleProperty("font-color", Color.FromHex("#3A3836")),
                    new StyleProperty(PanelContainer.StylePropertyPanel, panel),
                    new StyleProperty(LineEdit.StylePropertyStyleBox, panel),
                    new StyleProperty(LineEdit.StylePropertyCursorColor, Color.FromHex("#6A6764")),
                }),
            // TabContainer
            new StyleRule(new SelectorElement(typeof(TabContainer), null, null, null),
                new[]
                {
                    new StyleProperty(TabContainer.StylePropertyPanelStyleBox, tabContainerPanel),
                    new StyleProperty(TabContainer.StylePropertyTabStyleBox, tabContainerBoxActive),
                    new StyleProperty(TabContainer.StylePropertyTabStyleBoxInactive, tabContainerBoxInactive),
                }),
            // LineEdit Placeholder
            new StyleRule(
                new SelectorElement(typeof(LineEdit), null, null, new[] {LineEdit.StylePseudoClassPlaceholder}), new[]
                {
                    new StyleProperty("font-color", Color.FromHex("#6A6764")),
                }),
            // Buttons
            new StyleRule(
                new SelectorElement(typeof(Button), null, null, null), new[]
                {
                    new StyleProperty("font-color", Color.FromHex("#3D2403")),
                    new StyleProperty(Button.StylePropertyStyleBox, panelDark)
                }),
            new StyleRule(
                new SelectorElement(typeof(Button), null, null, new[] {Button.StylePseudoClassHover}), new[]
                {
                    new StyleProperty("font-color", Color.FromHex("#6B3F05")),
                    new StyleProperty(Button.StylePropertyStyleBox, panelDarkHighlight)
                }),
            // Game label.
            new StyleRule(
                new SelectorElement(typeof(Label), new [] {StyleClassLabelGame}, null, null), new []
                {
                    new StyleProperty(Label.StylePropertyFont, bigFont)
                }),
             // Window close button base texture.
            new StyleRule(
                new SelectorElement(typeof(TextureButton), new[] {DefaultWindow.StyleClassWindowCloseButton}, null,
                    null),
                new[]
                {
                    new StyleProperty(TextureButton.StylePropertyTexture, textureCloseButton),
                    new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#BB88BB")),
                }),
            // Window close button hover.
            new StyleRule(
                new SelectorElement(typeof(TextureButton), new[] {DefaultWindow.StyleClassWindowCloseButton}, null,
                    new[] {TextureButton.StylePseudoClassHover}),
                new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#DD88DD")),
                }),
            // Window close button pressed.
            new StyleRule(
                new SelectorElement(typeof(TextureButton), new[] {DefaultWindow.StyleClassWindowCloseButton}, null,
                    new[] {TextureButton.StylePseudoClassPressed}),
                new[]
                {
                    new StyleProperty(Control.StylePropertyModulateSelf, Color.FromHex("#FFCCFF")),
                }),
        });
    }
}