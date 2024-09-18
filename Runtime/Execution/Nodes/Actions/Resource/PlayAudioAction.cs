using System;
using System.Collections.Generic;
using Unity.Properties;
using UnityEngine;
using UnityEngine.Audio;

namespace Unity.Behavior
{
    [Serializable, GeneratePropertyBag]
    [NodeDescription(
        name: "Play Audio",
        story: "Play [AudioResource] on [Target]",
        category: "Action/Resource",
        id: "b5b0abd81036c6b81089944eb166dc54",
        description: "Plays an AudioResource at the Target location. " +
        "\nIt is possible to choose to spawn the AudioSource on either the Target itself or as a new empty GameObject." +
        "\nResource are internally pooled and shared across all [Play Audio] nodes."
        )]
    internal partial class PlayAudioAction : Action
    {
        private static Stack<AudioSource> s_SharedPool = null;

        [SerializeReference] public BlackboardVariable<AudioClip> AudioResource;
        [Tooltip("If target is not assigned, instantiate the AudioSource at the origin of the world.")]
        [SerializeReference] public BlackboardVariable<GameObject> Target;
        [SerializeReference] public BlackboardVariable<bool> LoopClip = new BlackboardVariable<bool>(false);
        [Tooltip("Should the AudioSource be spawn at the root of the scene or as a child of the target?" +
            "\nSet to true instantiate the AudioSource as a child of the target transform." +
            "\nSet to false when emitting a sound on an object that is going to be disabled (like a projectile).")]
        [SerializeReference] public BlackboardVariable<bool> SetTargetAsParent = new BlackboardVariable<bool>(false);
        [Tooltip("Prefab or scene reference use as model to configure the instantiated AudioSource.")]
        [SerializeReference] public BlackboardVariable<AudioSource> AudioSourceReference;

        protected override Status OnStart()
        {
            if (AudioResource.Value == null)
            {
                LogFailure("No AudioResource assigned.");
                return Status.Failure;
            }

            AudioSource audioSource = GetOrCreateAudioSource();

            if (AudioSourceReference.Value != null)
            {
                CopyAudioSourceReference(audioSource, AudioSourceReference.Value);
            }

            audioSource.loop = LoopClip.Value;
            audioSource.clip = AudioResource.Value;
            audioSource.Play();

            if (!LoopClip.Value)
            {
                AudioClip clip = AudioResource.Value as AudioClip;
                if (clip != null)
                {
                    Awaitable_ReleaseAudioClip(audioSource, clip.length);
                }
                else
                {
                    Awaitable_ReleaseAudioResource(audioSource);
                }
            }

            return Status.Success;
        }

        [RuntimeInitializeOnLoadMethod]
        private static void ResetStaticInstance()
        {
            s_SharedPool = new Stack<AudioSource>();
        }

        private static void CopyAudioSourceReference(AudioSource target, AudioSource reference)
        {
            target.outputAudioMixerGroup = reference.outputAudioMixerGroup;
            target.bypassEffects = reference.bypassEffects;
            target.bypassListenerEffects = reference.bypassListenerEffects;
            target.bypassReverbZones = reference.bypassReverbZones;
            target.priority = reference.priority;
            target.volume = reference.volume;
            target.pitch = reference.pitch;
            target.panStereo = reference.panStereo;
            target.spatialize = reference.spatialize;
            target.spatialBlend = reference.spatialBlend;
            target.reverbZoneMix = reference.reverbZoneMix;

            target.dopplerLevel = reference.dopplerLevel;
            target.spread = reference.spread;
            target.rolloffMode = reference.rolloffMode;
            target.minDistance = reference.minDistance;
            target.maxDistance = reference.maxDistance;
            target.spatializePostEffects = reference.spatializePostEffects;
        }

        private AudioSource GetOrCreateAudioSource()
        {
            GameObject gao = null;
            AudioSource audioSource = null;
            if (s_SharedPool.Count > 0)
            {
                do
                {
                    audioSource = s_SharedPool.Pop();
                } // Skip audio sources we lost reference to.
                while (audioSource == null && s_SharedPool.Count > 0);

                if (audioSource == null)
                {
                    return GetOrCreateAudioSource();
                }

                audioSource.enabled = true;

                gao = audioSource.gameObject;
                gao.SetActive(true);
#if UNITY_EDITOR
                if (audioSource.clip != AudioResource.Value)
                {
                    gao.name = "SFX: " + AudioResource.Value.name;
                }
#endif
            }
            else
            {
                gao = new GameObject("SFX: " + AudioResource.Value.name, typeof(AudioSource));
                audioSource = gao.GetComponent<AudioSource>();
                audioSource.playOnAwake = false;
            }

            if (Target.Value != null)
            {
                if (SetTargetAsParent.Value == false)
                {
                    gao.transform.position = Target.Value.transform.position;
                }
                else
                {
                    gao.transform.parent = Target.Value.transform;
                    gao.transform.localPosition = Vector3.zero;
                }
            }
            else
            {
                gao.transform.position = Vector3.zero;
            }

            return audioSource;
        }

        private void ReleaseAudio(AudioSource source)
        {
            source.enabled = false;
            source.gameObject.SetActive(false);
            s_SharedPool.Push(source);
        }

        private async void Awaitable_ReleaseAudioClip(AudioSource source, float delay)
        {
            // await Awaitable.WaitForSecondsAsync(delay);
            ReleaseAudio(source);
        }

        private async void Awaitable_ReleaseAudioResource(AudioSource source)
        {
            do
            {
                // await Awaitable.WaitForSecondsAsync(1f);
            }
            while (source.isPlaying);

            ReleaseAudio(source);
        }
    }
}
