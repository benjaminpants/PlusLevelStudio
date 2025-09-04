using HarmonyLib;
using System.Reflection;
using UnityEngine;
using UnityEngine.Audio;

namespace PlusLevelStudio.Ingame
{
    public class EditorWormholeController : WormholeController
    {
        public AudioClip ambienceClip;
        public AudioMixerGroup mixerGroup;
        static FieldInfo _source = AccessTools.Field(typeof(AmbienceRoomFunction), "source");
        public override void LoadingFinished()
        {
            RoomController room = ec.CellFromPosition(transform.position).room;
            if (room == null)
            {
                Destroy(gameObject);
                return;
            }
            Initialize(ec, room);
            // now that we've initialized, add some finishing touches

            // dont add ambience if it already was added
            if (room.functionObject.TryGetComponent<WormholeAmbienceRoomFunction>(out _))
            {
                return;
            }
            WormholeAmbienceRoomFunction ambience = room.functionObject.AddComponent<WormholeAmbienceRoomFunction>();
            AudioSource ambienceSource = room.functionObject.AddComponent<AudioSource>();
            ambienceSource.clip = ambienceClip;
            ambienceSource.outputAudioMixerGroup = mixerGroup;
            ambienceSource.panStereo = 0f;
            ambienceSource.volume = 0f;
            ambienceSource.dopplerLevel = 0f;
            ambienceSource.loop = true;
            _source.SetValue(ambience, ambienceSource);
            room.functions.AddFunction(ambience);
            ambience.Initialize(room);
            ambienceSource.Play(); // play on awake doesnt work
        }
    }

    /// <summary>
    /// Identical to AmbienceRoomFunction, but can be checked for, so multiple wormholes dont add unnecessary extra ambiences.
    /// </summary>
    public class WormholeAmbienceRoomFunction : AmbienceRoomFunction
    {

    }

    public class EditorCoverRoomFunction : CoverRoomFunction
    {
        public override void OnGenerationFinished()
        {
            Build(FindObjectOfType<LevelBuilder>(), new System.Random());
        }
    }
}
