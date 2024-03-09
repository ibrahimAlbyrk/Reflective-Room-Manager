namespace REFLECTIVE.Runtime.NETWORK.Room.Scenes
{
    internal interface ISceneChangeManager
    {
        public void ChangeScene(Room room, string sceneName, bool keepClientObjects);
    }
}