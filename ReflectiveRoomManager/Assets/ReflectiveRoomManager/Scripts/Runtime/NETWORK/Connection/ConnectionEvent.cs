using System;
using UnityEngine;
using System.Runtime.CompilerServices;

namespace REFLECTIVE.Runtime.NETWORK.Connection.Data
{
    public abstract class ConnectionEventBase
    {
        private readonly string _logString;
        private readonly bool _isDebug;

        protected ConnectionEventBase(bool isDebug, string logString)
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

    public class ConnectionEvent<T1, T2> : ConnectionEventBase
    {
        private Action<T1, T2> _action;

        public ConnectionEvent(bool isDebug = false, [CallerMemberName] string logString = "")
            : base(isDebug, logString) { }

        public void AddListener(Action<T1, T2> action) => _action += action;
        public void RemoveListener(Action<T1, T2> action) => _action -= action;

        internal void Call(T1 t1, T2 t2)
        {
            LogIfDebug();

            if (_action == null) return;

            foreach (var handler in _action.GetInvocationList())
            {
                try
                {
                    ((Action<T1, T2>)handler)(t1, t2);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }

    public class ConnectionEvent<T1, T2, T3> : ConnectionEventBase
    {
        private Action<T1, T2, T3> _action;

        public ConnectionEvent(bool isDebug = false, [CallerMemberName] string logString = "")
            : base(isDebug, logString) { }

        public void AddListener(Action<T1, T2, T3> action) => _action += action;
        public void RemoveListener(Action<T1, T2, T3> action) => _action -= action;

        internal void Call(T1 t1, T2 t2, T3 t3)
        {
            LogIfDebug();

            if (_action == null) return;

            foreach (var handler in _action.GetInvocationList())
            {
                try
                {
                    ((Action<T1, T2, T3>)handler)(t1, t2, t3);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }

    public class ConnectionEvent<T> : ConnectionEventBase
    {
        private Action<T> _action;

        public ConnectionEvent(bool isDebug = false, [CallerMemberName] string logString = "")
            : base(isDebug, logString) { }

        public void AddListener(Action<T> action) => _action += action;
        public void RemoveListener(Action<T> action) => _action -= action;

        internal void Call(T t)
        {
            LogIfDebug();

            if (_action == null) return;

            foreach (var handler in _action.GetInvocationList())
            {
                try
                {
                    ((Action<T>)handler)(t);
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }

    public class ConnectionEvent : ConnectionEventBase
    {
        private Action _action;

        public ConnectionEvent(bool isDebug = false, [CallerMemberName] string logString = "")
            : base(isDebug, logString) { }

        public void AddListener(Action action) => _action += action;
        public void RemoveListener(Action action) => _action -= action;

        internal void Call()
        {
            LogIfDebug();

            if (_action == null) return;

            foreach (var handler in _action.GetInvocationList())
            {
                try
                {
                    ((Action)handler)();
                }
                catch (Exception e)
                {
                    Debug.LogException(e);
                }
            }
        }
    }
}
