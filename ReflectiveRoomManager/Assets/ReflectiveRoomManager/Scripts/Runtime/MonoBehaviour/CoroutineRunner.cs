using UnityEngine;

namespace REFLECTIVE.Runtime.MonoBehavior
{
    public class CoroutineRunner : MonoBehaviour
    {
        private static CoroutineRunner _instance;

        public static CoroutineRunner Instance
        {
            get
            {
                if (_instance == null)
                {
                    var go = new GameObject("[ReflectiveRM] CoroutineRunner");
                    DontDestroyOnLoad(go);
                    _instance = go.AddComponent<CoroutineRunner>();
                }

                return _instance;
            }
        }

        public static void Cleanup()
        {
            if (_instance == null) return;

            _instance.StopAllCoroutines();
            Destroy(_instance.gameObject);
            _instance = null;
        }

        private void OnDestroy()
        {
            if (_instance == this)
                _instance = null;
        }
    }
}
