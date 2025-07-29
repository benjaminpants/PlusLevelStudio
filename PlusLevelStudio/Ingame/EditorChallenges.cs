using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Ingame
{
    public class EditorGrappleChallengeManager : GrappleChallengeManager
    {
        public override void LoadNextLevel()
        {
            Singleton<CoreGameManager>.Instance.Quit();
        }
    }

    public class EditorStealthyChallengeManager : StealthyChallengeManager
    {
        public override void LoadNextLevel()
        {
            Singleton<CoreGameManager>.Instance.Quit();
        }
    }
}
