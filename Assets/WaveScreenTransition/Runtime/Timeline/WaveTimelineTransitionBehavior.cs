using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.Timeline;
using UnityScreenNavigator.Runtime.Core.Shared;

namespace Wave.ScreenTransition
{
    public sealed class WaveTimelineTransitionBehavior : TransitionAnimationBehaviour
    {
        [SerializeField] private PlayableDirector _director;
        [SerializeField] private TimelineAsset _timelineAsset;

        public float TimelineDuration => _timelineAsset == null ? 0f : (float)_timelineAsset.duration;
        public override float Duration => TimelineDuration;

        public void SetTimelineAsset(TimelineAsset timelineAsset)
        {
            _timelineAsset = timelineAsset;
        }

        public override void Setup()
        {
            if (_director == null) throw new MissingReferenceException("A PlayableDirector is required.");
            if (_timelineAsset == null) throw new MissingReferenceException("A TimelineAsset is required.");

            _director.playableAsset = _timelineAsset;
            _director.time = 0;
            _director.initialTime = 0;
            _director.playOnAwake = false;
            _director.timeUpdateMode = DirectorUpdateMode.Manual;
            _director.extrapolationMode = DirectorWrapMode.None;
        }

        public override void SetTime(float time)
        {
            if (_director == null) return;

            _director.time = time;
            _director.Evaluate();
        }
    }
}
