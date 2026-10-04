using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Wave.ScreenTransition
{
    /// <summary>
    /// Small, explicit service registry used by the transition base classes and samples.
    /// The registry is intentionally independent from any game-specific service type.
    /// </summary>
    public static class ServiceLocator
    {
        private static readonly Dictionary<Type, object> Services = new();

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetForPlayModeWithoutDomainReload()
        {
            Clear();
        }

        public static bool TryRegister<TService>(TService instance) where TService : class
        {
            return TryRegister<TService, TService>(instance);
        }

        public static bool TryRegister<TService, TImplementation>(TImplementation instance)
            where TService : class
            where TImplementation : class, TService
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));

            var serviceType = typeof(TService);
            if (Services.ContainsKey(serviceType)) return false;

            Services.Add(serviceType, instance);
            return true;
        }

        public static void Register<TService>(TService instance) where TService : class
        {
            if (!TryRegister(instance))
                throw new InvalidOperationException($"A service of type '{typeof(TService).FullName}' is already registered.");
        }

        public static bool TryResolve<TService>(out TService service) where TService : class
        {
            if (Services.TryGetValue(typeof(TService), out var value) && value is TService resolved)
            {
                service = resolved;
                return true;
            }

            service = null;
            return false;
        }

        public static TService Resolve<TService>() where TService : class
        {
            TryResolve(out TService service);
            return service;
        }

        public static bool TryUnregister<TService>(TService expectedInstance = null) where TService : class
        {
            if (!Services.TryGetValue(typeof(TService), out var value)) return false;
            if (expectedInstance != null && !ReferenceEquals(value, expectedInstance)) return false;

            Services.Remove(typeof(TService));
            DisposeIfNeeded(value);
            return true;
        }

        public static bool TryUnregister<TService, TImplementation>(TImplementation expectedInstance)
            where TService : class
            where TImplementation : class, TService
        {
            return TryUnregister<TService>(expectedInstance);
        }

        public static void Clear()
        {
            var values = Services.Values.Distinct().ToArray();
            Services.Clear();

            foreach (var value in values) DisposeIfNeeded(value);
        }

        private static void DisposeIfNeeded(object value)
        {
            if (value is not IDisposable disposable) return;

            try
            {
                disposable.Dispose();
            }
            catch (Exception exception)
            {
                Debug.LogException(exception);
            }
        }
    }
}
