using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Wave.ScreenTransition.Samples
{
    public sealed class SamplePage : Page
    {
        private void OnGUI()
        {
            if (!Application.isPlaying) return;

            GUILayout.BeginArea(new Rect(24, 24, 320, 180), GUI.skin.window);
            GUILayout.Label("Wave Screen Transition");
            GUILayout.Label("Sample Page");

            if (GUILayout.Button("Open Modal"))
                ServiceLocator.Resolve<TransitionService>()?.ShowModal<SampleModal>();

            if (GUILayout.Button("Close Page"))
                ServiceLocator.Resolve<TransitionService>()?.ClosePage();

            GUILayout.EndArea();
        }
    }
}
