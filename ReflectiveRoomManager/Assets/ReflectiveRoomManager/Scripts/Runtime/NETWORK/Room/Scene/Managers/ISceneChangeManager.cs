namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal interface ISceneChangeManager
    {
        /// <summary>
        /// Performs the action of changing the scene of the specified room
        /// </summary>
        /// <param name="room">The room that will change its scene.</param>
        /// <param name="sceneName">The name of the new scene.</param>
        /// <param name="keepClientObjects">Determines whether to keep client objects in the scene or not.</param>
        public void ChangeScene(Room room, string sceneName, bool keepClientObjects);
    }
}