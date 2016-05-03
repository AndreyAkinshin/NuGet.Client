﻿using System;
using System.IO;
using System.Threading.Tasks;
using NuGet.Configuration;
using NuGet.Protocol;
using NuGet.Protocol.Core.Types;

namespace NuGet.Commands
{
    /// <summary>
    /// Helper functions for shared command runners (push, delete, etc)
    /// </summary>
    internal static class CommandRunnerUtility
    {
        public static string ResolveSource(IPackageSourceProvider sourceProvider, string currentDirectory, string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                source = sourceProvider.DefaultPushSource;
            }

            if (!string.IsNullOrEmpty(source))
            {
                source = sourceProvider.ResolveAndValidateSource(currentDirectory, source);
            }

            if (string.IsNullOrEmpty(source))
            {
                throw new ArgumentException(Strings.Error_MissingSourceParameter);
            }

            return source;
        }

        public static string GetApiKey(ISettings settings, string source, string apiKey)
        {
            if (string.IsNullOrEmpty(apiKey))
            {
                apiKey = SettingsUtility.GetDecryptedValue(settings, ConfigurationConstants.ApiKeys, source);
            }

            return apiKey;
        }

        public static async Task<PackageUpdateResource> GetPackageUpdateResource(IPackageSourceProvider sourceProvider, string source)
        {
            // Use a loaded PackageSource if possible since it contains credential info
            PackageSource packageSource = null;
            foreach (var loadedPackageSource in sourceProvider.LoadPackageSources())
            {
                if (loadedPackageSource.IsEnabled && source == loadedPackageSource.Source)
                {
                    packageSource = loadedPackageSource;
                    break;
                }
            }

            if (packageSource == null)
            {
                packageSource = new PackageSource(source);
            }

            var sourceRepositoryProvider = new CachingSourceProvider(sourceProvider);
            var sourceRepository = sourceRepositoryProvider.CreateRepository(packageSource);

            return await sourceRepository.GetResourceAsync<PackageUpdateResource>();
        }
    }
}
