namespace TrackpadCameraControl.Rewrite
{
    /// <summary>
    /// Unit-testable projection of one FeelCatalog row bound to live settings.
    /// Options and Debug hosts share the same descriptor inventory; this captures values.
    /// </summary>
    public sealed class FeelPanelEntry
    {
        public string Section { get; set; }
        public string Id { get; set; }
        public string Label { get; set; }
        public FeelControlKind CatalogKind { get; set; }
        public FeelControlKind ToolkitKind { get; set; }
        public string ValueKind { get; set; }
        public float? NumericValue { get; set; }
        public bool? BoolValue { get; set; }
        public string TextValue { get; set; }
    }
}
