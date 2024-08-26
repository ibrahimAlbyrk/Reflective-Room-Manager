namespace REFLECTIVE.Runtime.NETWORK.Matchmaker
{
    public static class RoomMatchFactory
    {
        private static NoneRoomMatchStrategy _noneRoomMatchStrategy;
        private static MapBasedRoomMatchStrategy _mapBasedRoomMatchStrategy;
        private static LevelBasedRoomMatchStrategy _levelBasedRoomMatchStrategy;
        
        public static RoomMatchStrategy Create(RoomMatchType roomMatchType)
        {
            RoomMatchStrategy simulator = roomMatchType switch
            {
                RoomMatchType.None => _noneRoomMatchStrategy ??= new NoneRoomMatchStrategy(),
                RoomMatchType.MapBased => _mapBasedRoomMatchStrategy ??= new MapBasedRoomMatchStrategy(),
                RoomMatchType.LevelBased => _levelBasedRoomMatchStrategy ??= new LevelBasedRoomMatchStrategy(),
                _ => throw new System.ArgumentOutOfRangeException(nameof(roomMatchType), roomMatchType, null)
            };
            return simulator;
        }
    }
}