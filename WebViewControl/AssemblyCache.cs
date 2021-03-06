﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;

namespace WebViewControl {

    internal class AssemblyCache {

        private object SyncRoot { get; } = new object();
        private Dictionary<string, Assembly> assemblies;
        private bool newAssembliesLoaded = true;

        internal Assembly ResolveResourceAssembly(Uri resourceUrl, bool failOnMissingAssembly) {
            if (assemblies == null) {
                lock (SyncRoot) {
                    if (assemblies == null) {
                        assemblies = new Dictionary<string, Assembly>();
                        AppDomain.CurrentDomain.AssemblyLoad += OnAssemblyLoaded;
                    }
                }
            }

            var assemblyName = ResourceUrl.GetEmbeddedResourceAssemblyName(resourceUrl);
            var assembly = GetAssemblyByName(assemblyName);

            if (assembly == null) {
                if (newAssembliesLoaded) {
                    lock (SyncRoot) {
                        if (newAssembliesLoaded) {
                            // add loaded assemblies to cache
                            newAssembliesLoaded = false;
                            foreach (var domainAssembly in AppDomain.CurrentDomain.GetAssemblies()) {
                                // replace if duplicated (can happen)
                                assemblies[domainAssembly.GetName().Name] = domainAssembly;
                            }
                        }
                    }
                }

                assembly = GetAssemblyByName(assemblyName);
                if (assembly == null) {
                    try {
                        // try load assembly from its name
                        assembly = AppDomain.CurrentDomain.Load(new AssemblyName(assemblyName));
                    } catch (IOException) { 
                        // ignore
                    }

                    if (assembly != null) {
                        assemblies[assembly.GetName().Name] = assembly;
                    }
                }
            }

            if (failOnMissingAssembly && assembly == null) {
                throw new InvalidOperationException("Could not find assembly for: " + resourceUrl);
            }
            return assembly;
        }

        private Assembly GetAssemblyByName(string assemblyName) {
            Assembly assembly;
            assemblies.TryGetValue(assemblyName, out assembly);
            return assembly;
        }

        private void OnAssemblyLoaded(object sender, AssemblyLoadEventArgs args) {
            newAssembliesLoaded = true;
        }
    }
}
