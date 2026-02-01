using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    /// <summary>
    /// The base class for all editor tools.
    /// </summary>
    public abstract class EditorTool
    {
        /// <summary>
        /// The ID for this tool, used during the save/load process.
        /// </summary>
        public abstract string id { get; }

        public virtual string titleKey => "Ed_Tool_" + id + "_Title";
        public virtual string descKey => "Ed_Tool_" + id + "_Desc";

        /// <summary>
        /// The sprite for this tool in the editor.
        /// </summary>
        public Sprite sprite;

        /// <summary>
        /// Called when the tool is selected/picked up
        /// </summary>
        public abstract void Begin();

        /// <summary>
        /// Called when the tool is put away/closed
        /// </summary>
        public abstract void Exit();


        /// <summary>
        /// Called when the tool gets cancelled for any reason/the user attempts to cancel the tool.
        /// </summary>
        /// <returns>If the tool should be cancelled/returned when the cancel is performed.</returns>
        public abstract bool Cancelled();

        /// <summary>
        /// Called when the mouse is pressed.
        /// </summary>
        /// <returns>If the tool should be returned.</returns>
        public abstract bool MousePressed();
        /// <summary>
        /// Called when the mouse is released.
        /// </summary>
        /// <returns>If the tool should be returned.</returns>
        public abstract bool MouseReleased();

        /// <summary>
        /// Called when the controller does its standard update
        /// </summary>
        public abstract void Update();

        protected void SoundPlayOneshot(SoundObject obj, float volume = 1f)
        {
            EditorController.Instance.audMan.PlaySingle(obj, volume);
        }

        protected void SoundStopOneshot()
        {
            EditorController.Instance.audMan.FlushQueue(true);
        }

        protected void SoundPlayOneshot(string sound, float volume = 1f)
        {
            SoundPlayOneshot(LevelStudioPlugin.Instance.sounds[sound], volume);
        }

        protected void SoundPlayLooping(SoundObject obj)
        {
            EditorController.Instance.loopingAudMan.QueueAudio(obj, true);
            EditorController.Instance.loopingAudMan.SetLoop(true);
        }

        protected void SoundPitch(float pitch)
        {
            EditorController.Instance.audMan.pitchModifier = pitch;
        }

        protected void SoundPitchLooping(float pitch)
        {
            EditorController.Instance.loopingAudMan.pitchModifier = pitch;
        }

        protected void SoundStopLooping()
        {
            EditorController.Instance.loopingAudMan.pitchModifier = 1f;
            EditorController.Instance.loopingAudMan.FlushQueue(true);
        }

        protected void SoundPlayLooping(string sound)
        {
            SoundPlayLooping(LevelStudioPlugin.Instance.sounds[sound]);
        }
    }
}
