using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Ingame
{
    public class EditorEndlessGameManager : EndlessGameManager
    {
        public StandardMenuButton continueButton;

        public WeightedItemObject[] items;

        public override void Initialize()
        {
            levelObject = new LevelGenerationParameters(); // fuck
            levelObject.potentialItems = items;
            base.Initialize();
        }

        public override void LoadNextLevel()
        {
            Singleton<EditorPlayModeManager>.Instance.Win();
        }

        public override void RestartLevel()
        {
            continueButton.OnPress = new UnityEngine.Events.UnityEvent();
            continueButton.OnPress.AddListener(ReturnToMenu);
            base.RestartLevel();
        }
    }
}
