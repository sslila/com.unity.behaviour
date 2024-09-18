using System;
using UnityEngine;

namespace Unity.Behavior
{
    [Serializable] //TODO, Generic SerializeReference doesnt work 2022
    internal abstract class BaseCastBlackboardVariable<OriginalType, CastType> : BlackboardVariable<CastType>
    {
        [SerializeReference]
        protected BlackboardVariable<OriginalType> m_SourceVariable;
        protected CastType m_CachedValue;
        private bool m_CallbackRegistered = false;
        public BaseCastBlackboardVariable(BlackboardVariable<OriginalType> linkedVariable)
        {
            m_SourceVariable = linkedVariable;
        }

        public override CastType Value
        {
            get
            {
                if (!m_CallbackRegistered)
                {
                    m_SourceVariable.OnValueChanged += OnSourceValueChanged;
                    m_CallbackRegistered = true;
                    OnSourceValueChanged();
                }
                return m_CachedValue;
            }
            set
            {
                SetOriginalValueFromCastValue(value);
            }
        }

        // This is called when the users sets the cast value and we want to update the original variable from the cast value.
        protected abstract void SetOriginalValueFromCastValue(CastType value);
        // This is called when the source variable value has changed and we can use it to cast the value and cache it.
        protected abstract void OnSourceValueChanged();
    }

    internal class Vector2ToVector3BlackboardVariable : BaseCastBlackboardVariable<Vector2, Vector3>
    {
        public Vector2ToVector3BlackboardVariable(BlackboardVariable<Vector2> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(Vector3 value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class Vector2ToVector4BlackboardVariable : BaseCastBlackboardVariable<Vector2, Vector4>
    {
        public Vector2ToVector4BlackboardVariable(BlackboardVariable<Vector2> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(Vector4 value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class Vector3ToVector2BlackboardVariable : BaseCastBlackboardVariable<Vector3, Vector2>
    {
        public Vector3ToVector2BlackboardVariable(BlackboardVariable<Vector3> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(Vector2 value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class Vector3ToVector4BlackboardVariable : BaseCastBlackboardVariable<Vector3, Vector4>
    {
        public Vector3ToVector4BlackboardVariable(BlackboardVariable<Vector3> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(Vector4 value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class Vector4ToVector2BlackboardVariable : BaseCastBlackboardVariable<Vector4, Vector2>
    {
        public Vector4ToVector2BlackboardVariable(BlackboardVariable<Vector4> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(Vector2 value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class Vector4ToVector3BlackboardVariable : BaseCastBlackboardVariable<Vector4, Vector3>
    {
        public Vector4ToVector3BlackboardVariable(BlackboardVariable<Vector4> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(Vector3 value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class FloatToDoubleBlackboardVariable : BaseCastBlackboardVariable<float, double>
    {
        public FloatToDoubleBlackboardVariable(BlackboardVariable<float> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(double value)
        {
            m_SourceVariable.Value = (float)value;
        }
    }

    internal class FloatToIntBlackboardVariable : BaseCastBlackboardVariable<float, int>
    {
        public FloatToIntBlackboardVariable(BlackboardVariable<float> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = Convert.ToInt32(m_SourceVariable.Value);
        }

        protected override void SetOriginalValueFromCastValue(int value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class DoubleToFloatBlackboardVariable : BaseCastBlackboardVariable<double, float>
    {
        public DoubleToFloatBlackboardVariable(BlackboardVariable<double> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = (float)m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(float value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class DoubleToIntBlackboardVariable : BaseCastBlackboardVariable<double, int>
    {
        public DoubleToIntBlackboardVariable(BlackboardVariable<double> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = (int)m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(int value)
        {
            m_SourceVariable.Value = value;
        }
    }

    internal class IntToFloatBlackboardVariable : BaseCastBlackboardVariable<int, float>
    {
        public IntToFloatBlackboardVariable(BlackboardVariable<int> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(float value)
        {
            m_SourceVariable.Value = (int)value;
        }
    }

    internal class IntToDoubleBlackboardVariable : BaseCastBlackboardVariable<int, double>
    {
        public IntToDoubleBlackboardVariable(BlackboardVariable<int> linkedVariable) : base(linkedVariable)
        {
        }

        protected override void OnSourceValueChanged()
        {
            m_CachedValue = m_SourceVariable.Value;
        }

        protected override void SetOriginalValueFromCastValue(double value)
        {
            m_SourceVariable.Value = (int)value;
        }
    }
}