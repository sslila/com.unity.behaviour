using UnityEngine;
using UnityEngine.Audio;
using System;

namespace Unity.Behavior
{
    internal class AudioResourceToAudioClipBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(AudioClip) && toType == typeof(AudioClip);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new UnityObjectToUnityObjectBlackboardVariable<AudioClip, AudioClip>(variable as BlackboardVariable<AudioClip>);
        }
    }

    internal class AudioClipToAudioResourceBlackboardVariableConverter : IBlackboardVariableConverter
    {
        public bool CanConvert(Type fromType, Type toType)
        {
            return fromType == typeof(AudioClip) && toType == typeof(AudioClip);
        }

        public BlackboardVariable Convert(Type fromType, Type toType, BlackboardVariable variable)
        {
            return new UnityObjectToUnityObjectBlackboardVariable<AudioClip, AudioClip>(variable as BlackboardVariable<AudioClip>);
        }
    }
}