using System;
using System.Collections;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Page;

namespace Wave.ScreenTransition
{
    public class PageScreen<TPresenter, TView, TModel> : Page
        where TPresenter : IScreenPresenter<TView, TModel>, new()
        where TView : MonoBehaviour
        where TModel : new()
    {
        private readonly List<IDisposable> _disposables = new();
        private TPresenter _presenter;
        private TView _rootView;
        private TModel _model;

        protected TransitionService TransitionService { get; private set; }
        protected TModel Model => _model;
        protected TView View => _rootView;

        public override IEnumerator Initialize()
        {
            CreateMvp();
            _presenter.Initialize(_rootView, _model);
            yield return base.Initialize();
        }

        public override IEnumerator WillPushEnter()
        {
            yield return _presenter.WillPushEnter(_rootView, _model).ToCoroutine();
        }

        public override void DidPushEnter()
        {
            _presenter.DidPushEnter(_rootView, _model).Forget();
        }

        public override void DidPopEnter()
        {
            _presenter.DidPopEnter(_rootView, _model);
        }

        public override void DidPopExit()
        {
            _presenter.DidPopExit(_rootView, _model);
        }

        public override IEnumerator Cleanup()
        {
            _presenter?.Cleanup(_rootView, _model);
            yield return base.Cleanup();
        }

        protected virtual IEnumerable<IDisposable> OnBindTransitions(TPresenter presenter)
        {
            yield break;
        }

        protected virtual TModel CreateModel()
        {
            return new TModel();
        }

        private void CreateMvp()
        {
            if (_presenter != null) return;

            _model = CreateModel();
            _rootView = GetComponent<TView>();
            if (_rootView == null)
                throw new MissingComponentException($"{GetType().Name} requires a {typeof(TView).Name} component.");

            _presenter = new TPresenter();
            TransitionService = ServiceLocator.Resolve<TransitionService>();
            if (TransitionService == null)
                throw new InvalidOperationException("TransitionService is not registered in ServiceLocator.");

            _disposables.AddRange(OnBindTransitions(_presenter));
            _disposables.AddRange(_presenter.Bind(_rootView, _model));
        }

        private void OnDestroy()
        {
            foreach (var disposable in _disposables) disposable?.Dispose();
            _disposables.Clear();
        }
    }
}
