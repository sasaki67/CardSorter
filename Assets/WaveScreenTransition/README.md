# Wave Screen Transition

Reusable screen navigation infrastructure for Unity projects using UnityScreenNavigator.

## Contents

- Runtime transition service for Page, Modal, and no-return Modal containers.
- `ServiceLocator` for registering the transition service and application adapters.
- MVP-style Page/Modal base classes with lifecycle hooks.
- Type-checked parameter injection before `Initialize`.
- Optional Timeline transition behavior.
- A minimal `MainEntry` sample and sample asset generator.
- Editor tools for dependency installation, validation, and `.unitypackage` export.

## Requirements

- Unity 6000.5 or newer is required for the dependency set included in this package
  (Unity 6.6 is supported).
- UnityScreenNavigator 1.8.0, which includes the Unity 6.5+ `EntityId` compatibility fix.
- UniTask 2.5.11, which includes the Unity 6.2+ generic TreeView compatibility fix.
- Unity Timeline 1.8.10 when the Timeline extension is used.
- Unity Test Framework 1.6.0 when the included editor tests are used.

uGUI is supplied by Unity and is not installed by the setup tool.

## Initial setup

1. Import this `.unitypackage` into the project.
2. Open `Tools/Wave Screen Transition/Install Dependencies`. The installer detects old
   direct revisions, removes them, and then adds the required revisions.
3. Wait for Package Manager resolution and script compilation to finish.
4. Open `Tools/Wave Screen Transition/Verify Dependencies`. This also confirms the
   expected dependency versions before enabling the runtime assemblies.
5. Run `Tools/Wave Screen Transition/Create Sample Assets` if a sample scene is needed.
6. Open `Assets/WaveScreenTransition/Samples/SampleScene.unity` and press Play.

The setup tool uses Unity's asynchronous Package Manager Client API. It does not edit
`Packages/manifest.json` directly. The `WAVE_SCREEN_TRANSITION_READY` define is enabled
only after dependency installation succeeds, so the Runtime and Samples assemblies do not
compile against missing external assemblies during the first import.

## Export

Run `Tools/Wave Screen Transition/Validate Package`, then
`Tools/Wave Screen Transition/Export UnityPackage`.

The exporter includes only the `Assets/WaveScreenTransition` runtime, sample, editor, test,
and README assets. Project-specific `_Wave`, `ThirdParty`, Mapbox, and game screen assets are
not included.

## Production entry point

The sample `Wave.ScreenTransition.Samples.MainEntry` is intentionally minimal. The current
application-specific `MainEntry` should remain in the application project and initialize its
own managers before showing a production screen.
