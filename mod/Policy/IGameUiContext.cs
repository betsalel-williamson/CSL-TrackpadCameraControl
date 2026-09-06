namespace TrackpadCameraControl.Rewrite
{
    /// <summary>Per-frame game UI / focus queries for <see cref="InputGates"/>.</summary>
    public interface IGameUiContext
    {
        bool IsMenuOrOptionsOpen();

        bool IsPointerOverUi();

        bool IsGameFocused();
    }
}
