using PlusLevelStudio.UI;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

namespace PlusLevelStudio.Editor
{
    public class EditorPopupExchangeHandler : UIExchangeHandler
    {
        public Action OnYes;
        public Action OnNo;
        public override bool GetStateBoolean(string key)
        {
            throw new NotImplementedException();
        }

        public override void OnElementsCreated()
        {
            
        }

        public override void SendInteractionMessage(string message, object data = null)
        {
            Debug.Log(message);
            if (message == "yes")
            {
                OnYes();
            }
            else
            {
                if (OnNo != null)
                {
                    OnNo();
                }
            }
            EditorController.Instance.RemoveUI(gameObject);
        }
    }
}
