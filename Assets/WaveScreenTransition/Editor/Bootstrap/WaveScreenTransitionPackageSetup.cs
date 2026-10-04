using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace Wave.ScreenTransition.Editor
{
    public static class WaveScreenTransitionPackageSetup
    {
        private const string ReadyDefine = "WAVE_SCREEN_TRANSITION_READY";
        private const string PackageRoot = "Assets/WaveScreenTransition";
        private const string ExportVersion = "0.1.3";
        private const double RequestTimeoutSeconds = 600d;

        private static readonly PackageRequirement[] Requirements =
        {
            new(
                "com.harumak.unityscreennavigator",
                "https://github.com/Haruma-K/UnityScreenNavigator.git?path=/Assets/UnityScreenNavigator#v1.8.0",
                "1.8.0"),
            new(
                "com.cysharp.unitask",
                "https://github.com/Cysharp/UniTask.git?path=src/UniTask/Assets/Plugins/UniTask#2.5.11",
                "2.5.11"),
            new("com.unity.timeline", "com.unity.timeline@1.8.10", "1.8.10"),
            new("com.unity.test-framework", "com.unity.test-framework@1.6.0", "1.6.0")
        };

        private static AddAndRemoveRequest _installRequest;
        private static ListRequest _listRequest;
        private static double _requestDeadline;
        private static InstallPhase _installPhase;

        [MenuItem("Tools/Wave Screen Transition/Install Dependencies")]
        public static void InstallDependencies()
        {
            if (HasActiveRequest())
            {
                Debug.LogWarning("A Wave Screen Transition package operation is already running.");
                return;
            }

            try
            {
                SetReadyDefine(false);
                AssetDatabase.Refresh();
                _installPhase = InstallPhase.Inspecting;
                _listRequest = Client.List(offlineMode: false, includeIndirectDependencies: true);
                SetRequestDeadline();
                EditorApplication.update += PollPrepareInstallRequest;
                EditorUtility.DisplayProgressBar(
                    "Wave Screen Transition",
                    "Checking existing Unity package revisions...",
                    0.1f);
            }
            catch (Exception exception)
            {
                EditorUtility.ClearProgressBar();
                Debug.LogException(exception);
            }
        }

        [MenuItem("Tools/Wave Screen Transition/Verify Dependencies")]
        public static void VerifyDependencies()
        {
            if (HasActiveRequest())
            {
                Debug.LogWarning("A Wave Screen Transition package operation is already running.");
                return;
            }

            SetReadyDefine(false);
            AssetDatabase.Refresh();
            _listRequest = Client.List(offlineMode: false, includeIndirectDependencies: true);
            _requestDeadline = EditorApplication.timeSinceStartup + RequestTimeoutSeconds;
            EditorApplication.update += PollListRequest;
            EditorUtility.DisplayProgressBar(
                "Wave Screen Transition",
                "Checking installed Unity packages...",
                0.1f);
        }

        [MenuItem("Tools/Wave Screen Transition/Enable Runtime Assembly")]
        public static void EnableRuntimeAssembly()
        {
            SetReadyDefine(true);
            AssetDatabase.Refresh();
            Debug.Log($"Enabled {ReadyDefine} for the active build target.");
        }

        [MenuItem("Tools/Wave Screen Transition/Disable Runtime Assembly")]
        public static void DisableRuntimeAssembly()
        {
            SetReadyDefine(false);
            AssetDatabase.Refresh();
            Debug.Log($"Disabled {ReadyDefine} for the active build target.");
        }

        [MenuItem("Tools/Wave Screen Transition/Validate Package")]
        public static void ValidatePackageMenu()
        {
            ValidatePackage(showDialog: true);
        }

        public static void ValidatePackageForCi()
        {
            var valid = ValidatePackage(showDialog: false);
            EditorApplication.Exit(valid ? 0 : 1);
        }

        [MenuItem("Tools/Wave Screen Transition/Export UnityPackage")]
        public static void ExportUnityPackage()
        {
            if (!ValidatePackage(showDialog: true)) return;

            var artifactsDirectory = GetArtifactsDirectory();
            Directory.CreateDirectory(artifactsDirectory);

            var outputPath = EditorUtility.SaveFilePanel(
                "Export Wave Screen Transition",
                artifactsDirectory,
                $"WaveScreenTransition_{ExportVersion}",
                "unitypackage");

            if (string.IsNullOrWhiteSpace(outputPath)) return;

            AssetDatabase.ExportPackage(
                GetPackageRoots(),
                outputPath,
                ExportPackageOptions.Recurse);

            Debug.Log($"Exported Wave Screen Transition package to: {outputPath}");
            EditorUtility.RevealInFinder(outputPath);
        }

        public static void ExportUnityPackageForCi()
        {
            if (!ValidatePackage(showDialog: false))
            {
                EditorApplication.Exit(1);
                return;
            }

            var artifactsDirectory = GetArtifactsDirectory();
            Directory.CreateDirectory(artifactsDirectory);

            var outputPath = Path.Combine(
                artifactsDirectory,
                $"WaveScreenTransition_{ExportVersion}.unitypackage");

            AssetDatabase.ExportPackage(
                GetPackageRoots(),
                outputPath,
                ExportPackageOptions.Recurse);

            Debug.Log($"Exported Wave Screen Transition package to: {outputPath}");
            EditorApplication.Exit(File.Exists(outputPath) ? 0 : 1);
        }

        private static void PollInstallRequest()
        {
            if (_installRequest == null) return;

            if (!_installRequest.IsCompleted)
            {
                if (EditorApplication.timeSinceStartup > _requestDeadline)
                {
                    FinishInstallRequest();
                    Debug.LogError("Timed out while installing Wave Screen Transition dependencies.");
                }

                return;
            }

            FinishInstallRequest();

            if (_installRequest.Status != StatusCode.Success)
            {
                SetReadyDefine(false);
                AssetDatabase.Refresh();
                Debug.LogError($"Failed to install dependencies: {_installRequest.Error?.message}");
                return;
            }

            if (_installPhase == InstallPhase.RemovingOldDependencies)
            {
                Debug.Log("[WaveScreenTransition] Old dependency revisions removed. Installing the required revisions...");
                StartAddRequest();
                return;
            }

            var resolved = _installRequest.Result == null
                ? string.Empty
                : string.Join(", ", _installRequest.Result.Select(package => $"{package.name}@{package.version}"));
            Debug.Log($"Wave Screen Transition dependencies installed: {resolved}");
            VerifyDependencies();
        }

        private static void PollPrepareInstallRequest()
        {
            if (_listRequest == null) return;

            if (!_listRequest.IsCompleted)
            {
                if (EditorApplication.timeSinceStartup > _requestDeadline)
                {
                    FinishListRequest(PollPrepareInstallRequest);
                    SetReadyDefine(false);
                    AssetDatabase.Refresh();
                    Debug.LogError("Timed out while checking existing Wave Screen Transition dependencies.");
                }

                return;
            }

            FinishListRequest(PollPrepareInstallRequest);

            if (_listRequest.Status != StatusCode.Success)
            {
                SetReadyDefine(false);
                AssetDatabase.Refresh();
                Debug.LogError($"Failed to inspect installed packages: {_listRequest.Error?.message}");
                return;
            }

            var installed = _listRequest.Result.ToDictionary(
                package => package.name,
                StringComparer.OrdinalIgnoreCase);
            var staleDirectDependencies = Requirements
                .Where(requirement => installed.TryGetValue(requirement.Name, out var package) &&
                                      package.isDirectDependency &&
                                      !IsExpectedVersion(package, requirement))
                .Select(requirement => requirement.Name)
                .ToArray();

            if (staleDirectDependencies.Length == 0)
            {
                StartAddRequest();
                return;
            }

            _installPhase = InstallPhase.RemovingOldDependencies;
            _installRequest = Client.AddAndRemove(
                Array.Empty<string>(),
                staleDirectDependencies);
            SetRequestDeadline();
            EditorApplication.update += PollInstallRequest;
            EditorUtility.DisplayProgressBar(
                "Wave Screen Transition",
                $"Removing old dependency revisions: {string.Join(", ", staleDirectDependencies)}",
                0.25f);
            Debug.LogWarning(
                $"[WaveScreenTransition] Replacing old direct dependency revisions: {string.Join(", ", staleDirectDependencies)}");
        }

        private static void StartAddRequest()
        {
            _installPhase = InstallPhase.AddingRequiredDependencies;
            var packageSpecs = Requirements.Select(requirement => requirement.Spec).ToArray();
            _installRequest = Client.AddAndRemove(packageSpecs, Array.Empty<string>());
            SetRequestDeadline();
            EditorApplication.update += PollInstallRequest;
            EditorUtility.DisplayProgressBar(
                "Wave Screen Transition",
                "Installing required Unity packages...",
                0.5f);
        }

        private static void PollListRequest()
        {
            if (_listRequest == null) return;

            if (!_listRequest.IsCompleted)
            {
                if (EditorApplication.timeSinceStartup > _requestDeadline)
                {
                    FinishListRequest(PollListRequest);
                    SetReadyDefine(false);
                    AssetDatabase.Refresh();
                    Debug.LogError("Timed out while checking Wave Screen Transition dependencies.");
                }

                return;
            }

            FinishListRequest(PollListRequest);

            if (_listRequest.Status != StatusCode.Success)
            {
                SetReadyDefine(false);
                AssetDatabase.Refresh();
                Debug.LogError($"Failed to list installed packages: {_listRequest.Error?.message}");
                return;
            }

            var installed = _listRequest.Result.ToDictionary(
                package => package.name,
                StringComparer.OrdinalIgnoreCase);
            var missing = new List<string>();

            foreach (var requirement in Requirements)
            {
                if (installed.TryGetValue(requirement.Name, out var package) &&
                    string.Equals(package.version, requirement.ExpectedVersion, StringComparison.OrdinalIgnoreCase))
                {
                    Debug.Log($"[WaveScreenTransition] OK {package.name}@{package.version}");
                }
                else
                {
                    var installedVersion = installed.TryGetValue(requirement.Name, out var installedPackage)
                        ? installedPackage.version
                        : "not installed";
                    missing.Add($"{requirement.Name} (required {requirement.ExpectedVersion}, found {installedVersion})");
                    Debug.LogWarning(
                        $"[WaveScreenTransition] {requirement.Name} requires {requirement.ExpectedVersion}, found {installedVersion}");
                }
            }

            if (missing.Count == 0)
            {
                SetReadyDefine(true);
                AssetDatabase.Refresh();
                Debug.Log("All Wave Screen Transition dependencies are installed.");
            }
            else
            {
                SetReadyDefine(false);
                AssetDatabase.Refresh();
                Debug.LogWarning(
                    $"Run Tools/Wave Screen Transition/Install Dependencies to install the required revisions: {string.Join(", ", missing)}");
            }
        }

        private static bool ValidatePackage(bool showDialog)
        {
            var errors = new List<string>();
            var warnings = new List<string>();

            var requiredPaths = new[]
            {
                $"{PackageRoot}/Runtime",
                $"{PackageRoot}/Samples",
                $"{PackageRoot}/Editor",
                $"{PackageRoot}/Tests",
                $"{PackageRoot}/README.md"
            };

            foreach (var path in requiredPaths)
            {
                if (!AssetDatabase.IsValidFolder(path) && !File.Exists(ToAbsolutePath(path)))
                    errors.Add($"Missing package path: {path}");
            }

            if (!IsReadyDefineEnabled())
                warnings.Add($"{ReadyDefine} is not enabled for the active build target.");

            foreach (var assetGuid in AssetDatabase.FindAssets(string.Empty, new[] { PackageRoot }))
            {
                var assetPath = AssetDatabase.GUIDToAssetPath(assetGuid);
                if (string.IsNullOrEmpty(assetPath) || AssetDatabase.IsValidFolder(assetPath)) continue;

                var metaPath = assetPath + ".meta";
                if (!File.Exists(ToAbsolutePath(metaPath))) errors.Add($"Missing meta file: {metaPath}");

                foreach (var dependency in AssetDatabase.GetDependencies(assetPath, recursive: true))
                {
                    if (dependency.StartsWith(PackageRoot + "/", StringComparison.OrdinalIgnoreCase)) continue;
                    if (dependency.StartsWith("Packages/", StringComparison.OrdinalIgnoreCase)) continue;

                    if (dependency.StartsWith("Assets/_Wave/", StringComparison.OrdinalIgnoreCase) ||
                        dependency.StartsWith("Assets/ThirdParty/", StringComparison.OrdinalIgnoreCase))
                    {
                        errors.Add($"Project-specific dependency detected: {assetPath} -> {dependency}");
                    }
                    else if (dependency.StartsWith("Assets/", StringComparison.OrdinalIgnoreCase))
                    {
                        warnings.Add($"External Assets dependency: {assetPath} -> {dependency}");
                    }
                }
            }

            foreach (var error in errors) Debug.LogError($"[WaveScreenTransition] {error}");
            foreach (var warning in warnings) Debug.LogWarning($"[WaveScreenTransition] {warning}");

            var valid = errors.Count == 0;
            if (showDialog)
            {
                var message = valid
                    ? $"Validation passed with {warnings.Count} warning(s)."
                    : $"Validation failed with {errors.Count} error(s) and {warnings.Count} warning(s).";
                EditorUtility.DisplayDialog("Wave Screen Transition", message, "OK");
            }

            return valid;
        }

        private static string GetArtifactsDirectory()
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", "Artifacts"));
        }

        private static string ToAbsolutePath(string assetPath)
        {
            return Path.GetFullPath(Path.Combine(Application.dataPath, "..", assetPath));
        }

        private static string[] GetPackageRoots()
        {
            return new[]
            {
                $"{PackageRoot}/Runtime",
                $"{PackageRoot}/Samples",
                $"{PackageRoot}/Editor",
                $"{PackageRoot}/Tests",
                $"{PackageRoot}/README.md"
            };
        }

        private static bool HasActiveRequest()
        {
            return (_installRequest != null && !_installRequest.IsCompleted) ||
                   (_listRequest != null && !_listRequest.IsCompleted);
        }

        private static void SetRequestDeadline()
        {
            _requestDeadline = EditorApplication.timeSinceStartup + RequestTimeoutSeconds;
        }

        private static void FinishInstallRequest()
        {
            EditorApplication.update -= PollInstallRequest;
            EditorUtility.ClearProgressBar();
        }

        private static void FinishListRequest(EditorApplication.CallbackFunction callback)
        {
            EditorApplication.update -= callback;
            EditorUtility.ClearProgressBar();
        }

        private static bool IsExpectedVersion(
            UnityEditor.PackageManager.PackageInfo package,
            PackageRequirement requirement)
        {
            return string.Equals(
                package.version,
                requirement.ExpectedVersion,
                StringComparison.OrdinalIgnoreCase);
        }

        private enum InstallPhase
        {
            Inspecting,
            RemovingOldDependencies,
            AddingRequiredDependencies
        }

        private static bool IsReadyDefineEnabled()
        {
            return GetDefines().Contains(ReadyDefine);
        }

        private static void SetReadyDefine(bool enabled)
        {
            var defines = GetDefines();
            if (enabled) defines.Add(ReadyDefine);
            else defines.RemoveAll(value => value == ReadyDefine);

            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            PlayerSettings.SetScriptingDefineSymbolsForGroup(
                buildTargetGroup,
                string.Join(";", defines));
        }

        private static List<string> GetDefines()
        {
            var buildTargetGroup = EditorUserBuildSettings.selectedBuildTargetGroup;
            var current = PlayerSettings.GetScriptingDefineSymbolsForGroup(buildTargetGroup);

            return current
                .Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Distinct(StringComparer.Ordinal)
                .ToList();
        }

        private sealed class PackageRequirement
        {
            public PackageRequirement(string name, string spec, string expectedVersion)
            {
                Name = name;
                Spec = spec;
                ExpectedVersion = expectedVersion;
            }

            public string Name { get; }
            public string Spec { get; }
            public string ExpectedVersion { get; }
        }
    }
}
