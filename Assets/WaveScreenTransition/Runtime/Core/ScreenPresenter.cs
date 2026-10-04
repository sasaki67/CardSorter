using System;
using System.Collections.Generic;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Wave.ScreenTransition
{
    public interface IScreenPresenter<in TView, in TModel>
    {
        IEnumerable<IDisposable> Bind(TView view, TModel model);
        void Initialize(TView view, TModel model);
        UniTask WillPushEnter(TView view, TModel model);
        UniTask DidPushEnter(TView view, TModel model);
        void DidPopEnter(TView view, TModel model);
        void DidPopExit(TView view, TModel model);
        void Cleanup(TView view, TModel model);
    }

    public class ScreenPresenter<TView, TModel> : IScreenPresenter<TView, TModel>
    {
        public virtual IEnumerable<IDisposable> Bind(TView view, TModel model)
        {
            yield break;
        }

        public virtual void Initialize(TView view, TModel model)
        {
        }

        public virtual UniTask WillPushEnter(TView view, TModel model)
        {
            return UniTask.CompletedTask;
        }

        public virtual UniTask DidPushEnter(TView view, TModel model)
        {
            return UniTask.CompletedTask;
        }

        public virtual void DidPopEnter(TView view, TModel model)
        {
        }

        public virtual void DidPopExit(TView view, TModel model)
        {
        }

        public virtual void Cleanup(TView view, TModel model)
        {
            if (model is IDisposable disposable) disposable.Dispose();
        }
    }
}
