using System;
using UnityEngine;

namespace Unity.Behavior.GraphFramework
{
    [Serializable]
    internal class SerializableType : ISerializationCallbackReceiver
    {
        public static implicit operator SerializableType(Type value)
        {
            return new SerializableType(value);
        }

        public static implicit operator Type(SerializableType value)
        {
            return !ReferenceEquals(value, null) ? value.m_Type : null;
        }

        private SerializableType() { }
        
        public SerializableType(Type type)
        {
            m_Type = type;
            AssignTypeToString();
        }

        public SerializableType(string typeText)
        {
            m_SerializableType = typeText;
            ReadTypeFromString();
        }

        public virtual void OnBeforeSerialize()
        {
            AssignTypeToString();
        }


        public virtual void OnAfterDeserialize()
        {
            ReadTypeFromString();
        }


        private void AssignTypeToString()
        {
            if (m_Type != null)
            {
                m_SerializableType = m_Type.AssemblyQualifiedName;
            }
        }

        private void ReadTypeFromString()
        {
            m_Type = Type.GetType(m_SerializableType);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
                return true;

            Type otherType = null;
            if (obj is SerializableType otherSerializableType)
            {
                otherType = otherSerializableType.Type;
            }
            else if (obj is Type sysType)
            {
                otherType = sysType;
            }
            else if (!ReferenceEquals(obj, null))
            {
                return false;
            }

            return Type == otherType;
        }

        public static bool operator ==(SerializableType left, SerializableType right)
        {
            if (!ReferenceEquals(left, null))
            {
                return left.Equals(right);
            }
            if (!ReferenceEquals(right, null))
            {
                return right.Equals(left);
            }
            return true; // both null
        }

        public static bool operator !=(SerializableType left, SerializableType right)
        {
            return !(left == right);
        }

        public override int GetHashCode()
        {
            if (m_Type == null && !string.IsNullOrEmpty(m_SerializableType))
            {
                ReadTypeFromString();
            }
            return m_Type != null ? m_Type.GetHashCode() : 0;
        }

        public override string ToString() => text;
        public string text
        {
            get
            {
                if (string.IsNullOrEmpty(m_SerializableType) && m_Type != null)
                {
                    AssignTypeToString();
                }
                return m_SerializableType;
            }
        }

        public Type Type
        {
            get
            {
                if (m_Type == null && !string.IsNullOrEmpty(m_SerializableType))
                {
                    ReadTypeFromString();
                }
                return m_Type;
            }
            private set
            {
                m_Type = value;
                AssignTypeToString();
            }
        }

        [NonSerialized]
        private Type m_Type;
        [SerializeField]
        private string m_SerializableType;
    }
}