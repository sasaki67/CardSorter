using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;

namespace Wave.ScreenTransition.Samples
{
    public sealed class SampleModal : Modal
    {
        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(64, 220, 240, 120), GUI.skin.window);
            GUILayout.Label("Sample Modal");

            if (GUILayout.Button("Close Modal"))
                ServiceLocator.Resolve<TransitionService>()?.CloseModal();

            GUILayout.EndArea();
        }
    }
}
