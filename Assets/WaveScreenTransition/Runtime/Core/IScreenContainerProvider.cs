using UnityScreenNavigator.Runtime.Core.Modal;
using UnityScreenNavigator.Runtime.Core.Page;
using UnityScreenNavigator.Runtime.Core.Sheet;

namespace Wave.ScreenTransition
{
    public interface IScreenContainerProvider
    {
        PageContainer RootPageContainer { get; }
        ModalContainer RootModalContainer { get; }
        ModalContainer RootNoReturnModalContainer { get; }
        SheetContainer RootSheetContainer { get; }
    }
}
