using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace InventoryEngine
{
    public class EventCaller<Arg1>
    {
        private List<Action<Arg1>> m_listeners = new List<Action<Arg1>>();
        private bool m_callingEvents;

        public void AddListener(Action<Arg1> listener)
        {
            m_listeners.Add(listener);
        }

        public void RemoveListener(Action<Arg1> listener)
        {
            if (m_callingEvents)
            {
                for (int i = 0; i < m_listeners.Count; ++i)
                {
                    if (m_listeners[i] == listener)
                    {
                        m_listeners[i] = null;
                        break;
                    }
                }
            }
            else
            {
                m_listeners.Remove(listener);
            }
        }

        public void Invoke(Arg1 arg1)
        {
            m_callingEvents = true;

            bool hasRemoved = false;

            for(int i = 0; i < m_listeners.Count; ++i)
            {
                if (m_listeners[i] != null)
                    m_listeners[i].Invoke(arg1);
                else
                    hasRemoved = true;
            }

            m_callingEvents = false;

            if (hasRemoved)
            {
                for (int i = 0; i < m_listeners.Count; ++i)
                {
                    if (m_listeners[i] == null)
                    {
                        m_listeners.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        public void ClearListeners()
        {
            m_listeners.Clear();
        }
    }

    public class EventCaller<Arg1, Arg2>
    {
        private List<Action<Arg1, Arg2>> m_listeners = new List<Action<Arg1, Arg2>>();
        private bool m_callingEvents;

        public void AddListener(Action<Arg1, Arg2> listener)
        {
            m_listeners.Add(listener);
        }

        public void RemoveListener(Action<Arg1, Arg2> listener)
        {
            if (m_callingEvents)
            {
                for (int i = 0; i < m_listeners.Count; ++i)
                {
                    if (m_listeners[i] == listener)
                    {
                        m_listeners[i] = null;
                        break;
                    }
                }
            }
            else
            {
                m_listeners.Remove(listener);
            }
        }

        public void Invoke(Arg1 arg1, Arg2 arg2)
        {
            m_callingEvents = true;

            bool hasRemoved = false;

            for (int i = 0; i < m_listeners.Count; ++i)
            {
                if (m_listeners[i] != null)
                    m_listeners[i].Invoke(arg1, arg2);
                else
                    hasRemoved = true;
            }

            m_callingEvents = false;

            if (hasRemoved)
            {
                for (int i = 0; i < m_listeners.Count; ++i)
                {
                    if (m_listeners[i] == null)
                    {
                        m_listeners.RemoveAt(i);
                        --i;
                    }
                }
            }
        }

        public void ClearListeners()
        {
            m_listeners.Clear();
        }
    }
}
