using UnityEngine;
using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;
using UnityScreenNavigator.Runtime.Core.Sheet;

namespace Wave.ScreenTransition
{
    [DisallowMultipleComponent]
    public sealed class ScreenNavigatorRoot : MonoBehaviour, IScreenContainerProvider
    {
        [SerializeField] private PageContainer _rootPageContainer;
        [SerializeField] private ModalContainer _rootModalContainer;
        [SerializeField] private ModalContainer _rootNoReturnModalContainer;
        [SerializeField] private SheetContainer _rootSheetContainer;

        public PageContainer RootPageContainer => _rootPageContainer;
        public ModalContainer RootModalContainer => _rootModalContainer;
        public ModalContainer RootNoReturnModalContainer => _rootNoReturnModalContainer;
        public SheetContainer RootSheetContainer => _rootSheetContainer;
        public TransitionService TransitionService { get; private set; }

        public TransitionService Initialize(bool registerToServiceLocator = true)
        {
            ValidateConfiguration();

            if (TransitionService == null)
            {
                TransitionService = new TransitionService(this);
            }

            if (registerToServiceLocator)
            {
                ServiceLocator.TryRegister<IScreenContainerProvider>(this);
                ServiceLocator.TryRegister(TransitionService);
            }

            return TransitionService;
        }

        public void Configure(
            PageContainer pageContainer,
            ModalContainer modalContainer,
            ModalContainer noReturnModalContainer = null,
            SheetContainer sheetContainer = null)
        {
            _rootPageContainer = pageContainer;
            _rootModalContainer = modalContainer;
            _rootNoReturnModalContainer = noReturnModalContainer;
            _rootSheetContainer = sheetContainer;
        }

        private void ValidateConfiguration()
        {
            if (_rootPageContainer == null)
                throw new MissingReferenceException($"{nameof(ScreenNavigatorRoot)} requires a PageContainer.");

            if (_rootModalContainer == null)
                throw new MissingReferenceException($"{nameof(ScreenNavigatorRoot)} requires a ModalContainer.");
        }

        private void OnDestroy()
        {
            ServiceLocator.TryUnregister<IScreenContainerProvider>(this);
            if (TransitionService != null)
                ServiceLocator.TryUnregister(TransitionService);
        }
    }
}
