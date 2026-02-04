namespace REFLECTIVE.Runtime.NETWORK.Room.GUI.Modules
{
    using Structs;

    /// <summary>
    /// Interface for HUD tab modules.
    /// </summary>
    public interface IHUDModule
    {
        string TabName { get; }
        void RegisterEvents();
        void UnregisterEvents();
        void DrawTab(RoomInfo room);
        void ClearData();
    }
}
