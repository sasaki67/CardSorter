using System;
using System.Linq;
using Cysharp.Threading.Tasks;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;
using UnityScreenNavigator.Runtime.Foundation.Coroutine;

namespace Wave.ScreenTransition
{
    public sealed class TransitionService
    {
        private readonly IScreenContainerProvider _containers;

        public TransitionService(IScreenContainerProvider containers)
        {
            _containers = containers ?? throw new ArgumentNullException(nameof(containers));
        }

        public AsyncProcessHandle ShowPage<TPage>(bool playAnimation = true, bool stack = true)
            where TPage : Page
        {
            return ShowPage(typeof(TPage).Name, playAnimation, stack);
        }

        public AsyncProcessHandle ShowPage(
            string resourceKey,
            bool playAnimation = true,
            bool stack = true)
        {
            return _containers.RootPageContainer.Push(
                RequireResourceKey(resourceKey), playAnimation, stack: stack);
        }

        public AsyncProcessHandle ShowPage<TPage, TParameter>(
            TParameter parameter,
            bool playAnimation = true,
            bool stack = true)
            where TPage : Page
        {
            return ShowPage(
                typeof(TPage).Name,
                parameter,
                playAnimation,
                stack);
        }

        public AsyncProcessHandle ShowPage<TParameter>(
            string resourceKey,
            TParameter parameter,
            bool playAnimation = true,
            bool stack = true)
        {
            return _containers.RootPageContainer.Push(
                RequireResourceKey(resourceKey),
                playAnimation,
                stack: stack,
                onLoad: pageObject => SetParameter(pageObject.page, parameter, resourceKey));
        }

        public AsyncProcessHandle ClosePage(bool playAnimation = true)
        {
            if (_containers.RootPageContainer.OrderedPagesIds.Count == 0)
                throw new InvalidOperationException("Cannot close a page because the page stack is empty.");

            return _containers.RootPageContainer.Pop(playAnimation);
        }

        public bool HasPage<TPage>() where TPage : Page
        {
            return HasPage(typeof(TPage).Name);
        }

        public bool HasPage(string resourceKey)
        {
            return _containers.RootPageContainer.Pages.Values.Any(
                page => page.Identifier == RequireResourceKey(resourceKey));
        }

        public AsyncProcessHandle ShowModal<TModal>(bool playAnimation = true)
            where TModal : Modal
        {
            return ShowModal(typeof(TModal).Name, playAnimation);
        }

        public AsyncProcessHandle ShowModal(string resourceKey, bool playAnimation = true)
        {
            return _containers.RootModalContainer.Push(
                RequireResourceKey(resourceKey), playAnimation);
        }

        public AsyncProcessHandle ShowModal<TModal, TParameter>(
            TParameter parameter,
            bool playAnimation = true)
            where TModal : Modal
        {
            return ShowModal(typeof(TModal).Name, parameter, playAnimation);
        }

        public AsyncProcessHandle ShowModal<TParameter>(
            string resourceKey,
            TParameter parameter,
            bool playAnimation = true)
        {
            return _containers.RootModalContainer.Push(
                RequireResourceKey(resourceKey),
                playAnimation,
                onLoad: modalObject => SetParameter(modalObject.modal, parameter, resourceKey));
        }

        public AsyncProcessHandle CloseModal(bool playAnimation = true)
        {
            if (_containers.RootModalContainer.OrderedModalIds.Count == 0)
                throw new InvalidOperationException("Cannot close a modal because the modal stack is empty.");

            return _containers.RootModalContainer.Pop(playAnimation);
        }

        public async UniTask CloseModalAsync(bool playAnimation = true)
        {
            await CloseModal(playAnimation).Task;
        }

        public AsyncProcessHandle ShowNoReturnModal<TModal>(bool playAnimation = true)
            where TModal : Modal
        {
            return ShowNoReturnModal(typeof(TModal).Name, playAnimation);
        }

        public AsyncProcessHandle ShowNoReturnModal(string resourceKey, bool playAnimation = true)
        {
            return GetNoReturnModalContainer().Push(
                RequireResourceKey(resourceKey), playAnimation);
        }

        public AsyncProcessHandle ShowNoReturnModal<TModal, TParameter>(
            TParameter parameter,
            bool playAnimation = true)
            where TModal : Modal
        {
            return ShowNoReturnModal(typeof(TModal).Name, parameter, playAnimation);
        }

        public AsyncProcessHandle ShowNoReturnModal<TParameter>(
            string resourceKey,
            TParameter parameter,
            bool playAnimation = true)
        {
            return GetNoReturnModalContainer().Push(
                RequireResourceKey(resourceKey),
                playAnimation,
                onLoad: modalObject => SetParameter(modalObject.modal, parameter, resourceKey));
        }

        public AsyncProcessHandle CloseNoReturnModal(bool playAnimation = true)
        {
            var container = GetNoReturnModalContainer();
            if (container.OrderedModalIds.Count == 0)
                throw new InvalidOperationException("Cannot close a no-return modal because the modal stack is empty.");

            return container.Pop(playAnimation);
        }

        public async UniTask CloseNoReturnModalAsync(bool playAnimation = true)
        {
            await CloseNoReturnModal(playAnimation).Task;
        }

        private ModalContainer GetNoReturnModalContainer()
        {
            if (_containers.RootNoReturnModalContainer == null)
                throw new InvalidOperationException("A no-return ModalContainer is not configured.");

            return _containers.RootNoReturnModalContainer;
        }

        private static string RequireResourceKey(string resourceKey)
        {
            if (string.IsNullOrWhiteSpace(resourceKey))
                throw new ArgumentException("A screen resource key is required.", nameof(resourceKey));

            return resourceKey;
        }

        private static void SetParameter<TParameter>(object screen, TParameter parameter, string resourceKey)
        {
            if (screen is IParameterized<TParameter> parameterized)
            {
                parameterized.SetParameter(parameter);
                return;
            }

            throw new InvalidOperationException(
                $"Screen '{resourceKey}' does not implement IParameterized<{typeof(TParameter).Name}>.");
        }
    }
}
