using System;
using System.Collections.Generic;
using System.Text;

namespace PlusLevelStudio.Ingame
{
    public class Structure_LevelBoxEditor : Structure_LevelBox
    {
        public void OnLoadingFinished(LevelLoader loader)
        {
            base.OnGenerationFinished(loader);
        }
    }
}
