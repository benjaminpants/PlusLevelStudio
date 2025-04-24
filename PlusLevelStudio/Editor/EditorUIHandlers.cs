using System;
using System.Collections.Generic;
using System.Text;
using PlusLevelStudio.UI;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

namespace PlusLevelStudio.Editor
{
    public class EditorUIMainHandler : UIExchangeHandler
    {
        public override void SendInteractionMessage(string message)
        {
            switch (message)
            {
                case "exit":
                    Destroy(Singleton<EditorController>.Instance.camera.gameObject);
                    Singleton<InputManager>.Instance.ActivateActionSet("Interface");
                    Singleton<GlobalCam>.Instance.ChangeType(CameraRenderType.Base);
                    SceneManager.LoadScene("MainMenu");
                    break;
            }
        }
    }
}
