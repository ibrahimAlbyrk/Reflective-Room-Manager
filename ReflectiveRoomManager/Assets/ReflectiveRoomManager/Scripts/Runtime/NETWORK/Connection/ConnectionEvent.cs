using System;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace REFLECTIVE.Runtime.NETWORK.Connection.Data
{
    public class ConnectionEvent
    {
        private Action _action;

        private readonly string _logString;

        private readonly bool _isDebug;

        public void AddListener(Action action)
        {
            _action += action;
        }

        public void RemoveListener(Action action)
        {
            _action -= action;
        }

        internal void Call()
        {
            #if UNITY_EDITOR
            if(_isDebug)
                Debug.Log(_logString);
            #endif
            
            _action?.Invoke();
        }

        public ConnectionEvent(bool isDebug = false, [CallerMemberName]string logString = "")
        {
            _isDebug = isDebug;
            
            _logString = logString;
        }
    }
    
    public class ConnectionEvent<T1>
    {
        private Action<T1> _action;

        private readonly string _logString;
        
        private readonly bool _isDebug;
        
        public void AddListener(Action<T1> action)
        {
            _action += action;
        }

        public void RemoveListener(Action<T1> action)
        {
            _action -= action;
        }

        internal void Call(T1 t1)
        {
            #if UNITY_EDITOR
            if(_isDebug)
                Debug.Log(_logString);
            #endif
            
            _action?.Invoke(t1);
        }
        
        public ConnectionEvent(bool isDebug = false, [CallerMemberName]string logString = "")
        {
            _isDebug = isDebug;
            
            _logString = logString;
        }
    }

    public class ConnectionEvent<T1, T2>
    {
        private Action<T1, T2> _action;
        
        private readonly string _logString;
        
        private readonly bool _isDebug;

        public void AddListener(Action<T1, T2> action)
        {
            _action += action;
        }

        public void RemoveListener(Action<T1, T2> action)
        {
            _action -= action;
        }

        internal void Call(T1 t1, T2 t2)
        {
            #if UNITY_EDITOR
            if(_isDebug)
                Debug.Log(_logString);
            #endif
            
            _action?.Invoke(t1, t2);
        }
        
        public ConnectionEvent(bool isDebug = false, [CallerMemberName]string logString = "")
        {
            _isDebug = isDebug;
            
            _logString = logString;
        }
    }
}