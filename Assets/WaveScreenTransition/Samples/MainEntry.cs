using UnityEngine;

namespace Wave.ScreenTransition.Samples
{
    /// <summary>
    /// Minimal composition root for the package sample.
    /// The production application's MainEntry remains outside this package.
    /// </summary>
    [DefaultExecutionOrder(-1000)]
    public sealed class MainEntry : MonoBehaviour
    {
        [SerializeField] private ScreenNavigatorRoot _screenNavigatorRoot;
        [SerializeField] private bool _showSamplePageOnStart = true;

        private TransitionService _transitionService;

        private void Awake()
        {
            if (_screenNavigatorRoot == null)
                _screenNavigatorRoot = FindObjectOfType<ScreenNavigatorRoot>();

            if (_screenNavigatorRoot == null)
            {
                Debug.LogError("Wave Screen Transition sample requires a ScreenNavigatorRoot.", this);
                return;
            }

            _transitionService = _screenNavigatorRoot.Initialize();
        }

        private void Start()
        {
            if (_showSamplePageOnStart && _transitionService != null)
                _transitionService.ShowPage<SamplePage>(playAnimation: false);
        }
    }
}
