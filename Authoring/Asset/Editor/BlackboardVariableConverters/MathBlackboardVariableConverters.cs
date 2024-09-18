using System;
using UnityEngine;

namespace Unity.Behavior
{
    internal class Vector2ToVector3BlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(Vector2) && toType == typeof(Vector3);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new Vector2ToVector3BlackboardVariable(variable as BlackboardVariable<Vector2>);
        }
    }

    internal class Vector2ToVector4BlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(Vector2) && toType == typeof(Vector4);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new Vector2ToVector4BlackboardVariable(variable as BlackboardVariable<Vector2>);
        }
    }

    internal class Vector3ToVector4BlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(Vector3) && toType == typeof(Vector4);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new Vector3ToVector4BlackboardVariable(variable as BlackboardVariable<Vector3>);
        }
    }

    internal class Vector3ToVector2BlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(Vector3) && toType == typeof(Vector2);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new Vector3ToVector2BlackboardVariable(variable as BlackboardVariable<Vector3>);
        }
    }

    internal class Vector4ToVector2BlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(Vector4) && toType == typeof(Vector2);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new Vector4ToVector2BlackboardVariable(variable as BlackboardVariable<Vector4>);
        }
    }

    internal class Vector4ToVector3BlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(Vector4) && toType == typeof(Vector3);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new Vector4ToVector3BlackboardVariable(variable as BlackboardVariable<Vector4>);
        }
    }

    internal class FloatToDoubleBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(float) && toType == typeof(double);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new FloatToDoubleBlackboardVariable(variable as BlackboardVariable<float>);
        }
    }

    internal class FloatToIntBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(float) && toType == typeof(int);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new FloatToIntBlackboardVariable(variable as BlackboardVariable<float>);
        }
    }

    internal class IntToFloatBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(int) && toType == typeof(float);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new IntToFloatBlackboardVariable(variable as BlackboardVariable<int>);
        }
    }

    internal class IntToDoubleBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(int) && toType == typeof(double);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new IntToDoubleBlackboardVariable(variable as BlackboardVariable<int>);
        }
    }

    internal class DoubleToIntBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(double) && toType == typeof(int);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new DoubleToIntBlackboardVariable(variable as BlackboardVariable<double>);
        }
    }

    internal class DoubleToFloatBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(double) && toType == typeof(float);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new DoubleToFloatBlackboardVariable(variable as BlackboardVariable<double>);
        }
    }
}