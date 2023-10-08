using System;

namespace REFLECTIVE.Runtime.SceneManagement.Processor.Factory
{
    using NETWORK.Room.Loader;
    
    public static class SceneProcessorFactory
    {
        public static SceneProcessor Create(RoomLoaderType roomLoaderType)
        {
            return roomLoaderType switch
            {
                RoomLoaderType.NoneScene => null,
                RoomLoaderType.SingleScene => new SingleSceneProcessor(),
                RoomLoaderType.AdditiveScene => new AdditiveSceneProcessor(),
                _ => throw new ArgumentOutOfRangeException($"Scene Processor Factory", "Room Loader Type is undefined")
            };
        }
    }
}