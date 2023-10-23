using System;
using UnityEngine;

namespace REFLECTIVE.Runtime.NETWORK.Room
{
    using Loader;
    
    public abstract partial class RoomManagerBase
    {
        private bool InitializeSingleton()
        {
            if (_singleton != null && _singleton == this)
                return true;

            if (!_dontDestroyOnLoad)
            {
                _singleton = this;
                return true;
            }

            if (_singleton != null)
            {
                Debug.LogWarning(
                    "Multiple RoomManagers detected in the scene. Only one RoomManager can exist at a time.The duplicate RoomManager will be destroyed.");
                Destroy(gameObject);

                // Return false to not allow collision-destroyed second instance to continue.
                return false;
            }

            _singleton = this;
            if (!Application.isPlaying) return true;

            // Force the object to scene root, in case user made it a child of something
            // in the scene since DDOL is only allowed for scene root objects
            transform.SetParent(null);
            DontDestroyOnLoad(gameObject);

            return true;
        }

        private void InitializeRoomLoader()
        {
            _roomLoader = _defaultRoomData.RoomLoaderType switch
            {
                RoomLoaderType.NoneScene => new NoneSceneRoomLoader(),
                RoomLoaderType.AdditiveScene => new SceneRoomLoader(),
                RoomLoaderType.SingleScene => new SceneRoomLoader(),
                _ => throw new ArgumentOutOfRangeException()
            };
        }
    }
}