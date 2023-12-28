using System;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace REFLECTIVE.Runtime.NETWORK.Connection.Data
{
    public class ConnectionEvent<T1, T2>
    {
        protected Action<T1, T2> _action;
        
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
            LogIfDebug();
            
            _action?.Invoke(t1, t2);
        }

        public ConnectionEvent(bool isDebug = false, [CallerMemberName] string logString = "")
        {
            _isDebug = isDebug;
            
            _logString = logString;
        }

        protected void LogIfDebug()
        {
            #if UNITY_EDITOR
            if (_isDebug)
                Debug.Log(_logString);
            #endif
        }
    }
    
    public class ConnectionEvent<T> : ConnectionEvent<T, object>
    {
        protected new Action<T> _action;
        
        public ConnectionEvent(bool isDebug = false, [CallerMemberName] string logString = "")
            : base(isDebug, logString)
        {
        }
        
        internal void Call(T t)
        {
            LogIfDebug();
            _action?.Invoke(t);
        }
        
        public void AddListener(Action<T> action)
        {
            _action += action;
        }

        public void RemoveListener(Action<T> action)
        {
            _action -= action;
        }
    }

    public class ConnectionEvent : ConnectionEvent<object, object>
    {
        protected new Action _action;
        
        public ConnectionEvent(bool isDebug = false, [CallerMemberName] string logString = "")
            : base(isDebug, logString)
        {
        }
        
        internal void Call()
        {
            LogIfDebug();
            _action?.Invoke();
        }
        
        public void AddListener(Action action)
        {
            _action += action;
        }

        public void RemoveListener(Action action)
        {
            _action -= action;
        }
    }
}