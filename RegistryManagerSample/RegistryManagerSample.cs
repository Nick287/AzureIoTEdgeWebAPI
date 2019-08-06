// Copyright (c) Microsoft. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for full license information.

using Microsoft.Azure.Devices.Shared;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.Azure.Devices.Samples
{
    public class RegistryManagerSample
    {
        private const string DeviceId = "RegistryManagerSample_Device";
        // Either set the IOTHUB_PFX_X509_THUMBPRINT and IOTHUB_PFX_X509_THUMBPRINT2 environment variables 
        // or within launchSettings.json:
        private static string s_primaryThumbprint = Environment.GetEnvironmentVariable("IOTHUB_PFX_X509_THUMBPRINT");
        private static string s_secondaryThumbprint = Environment.GetEnvironmentVariable("IOTHUB_PFX_X509_THUMBPRINT2");

        private readonly RegistryManager _registryManager;

        public RegistryManagerSample(RegistryManager registryManager)
        {
            _registryManager = registryManager ?? throw new ArgumentNullException(nameof(registryManager));
        }

        public async Task RunSampleAsync()
        {
            await EnumerateTwinsAsync().ConfigureAwait(false);

            return;

            try
            {
                await AddDeviceAsync(DeviceId).ConfigureAwait(false);
            }
            finally
            {
                await RemoveDeviceAsync(DeviceId).ConfigureAwait(false);
            }

            try
            {
                await AddDeviceWithSelfSignedCertificateAsync(
                    DeviceId,
                    s_primaryThumbprint,
                    s_secondaryThumbprint).ConfigureAwait(false);
            }
            finally
            {
                await RemoveDeviceAsync(DeviceId).ConfigureAwait(false);
            }

            try
            {
                await AddDeviceWithCertificateAuthorityAuthenticationAsync(DeviceId).ConfigureAwait(false);
                await UpdateDesiredProperties(DeviceId).ConfigureAwait(false);
            }
            finally
            {
                await RemoveDeviceAsync(DeviceId).ConfigureAwait(false);
            }
        }

        public async Task EnumerateTwinsAsync()
        {
            Console.WriteLine("Querying devices:");

            #region only for demo iot hub

            var query = _registryManager.CreateQuery("select * from devices");

            while (query.HasMoreResults)
            {
                IEnumerable<Twin> twins = await query.GetNextAsTwinAsync().ConfigureAwait(false);

                foreach (Twin twin in twins)
                {
                    Console.WriteLine(
                        " DeviceID: "+ twin.DeviceId + "\r\n" +
                        " Is Edge device: " + twin.Capabilities.IotEdge +"\r\n" +
                        " Status:" + twin.Status +"\r\n " + "\r\n" );

                    var twin1 = await _registryManager.GetTwinAsync(twin.DeviceId);
                }
            }

            var device = await _registryManager.GetDeviceAsync("WinVM001");

            var modules = await _registryManager.GetModulesOnDeviceAsync("WinVM001");

            var module = await _registryManager.GetModuleAsync("WinVM001", "webapi");

            var moduletwin = await _registryManager.GetTwinAsync("WinVM002", "webapi");

            #endregion
            
            var desiredproperties =
            @"{
                properties: {
                    desired: {
                        ""CloudStorageAccount"":""key"",
                        ""ContainerName"" : ""test"",
                        ""PathAndFileName"":""2617789e-d3db-3c23-bbad-4848cd8d9898.json""
                    }
                }
            }";

            await _registryManager.UpdateTwinAsync(moduletwin.DeviceId, moduletwin.ModuleId, desiredproperties, moduletwin.ETag);

            moduletwin = await _registryManager.GetTwinAsync("WinVM002", "webapi");

            return;

            //var query = _registryManager.CreateQuery("select * from devices");

            //while (query.HasMoreResults)
            //{
            //    IEnumerable<Twin> twins = await query.GetNextAsTwinAsync().ConfigureAwait(false);

            //    foreach (Twin twin in twins)
            //    {
            //        Console.WriteLine(
            //            "\t{0, -50} : {1, 10} : Last seen: {2, -10}",
            //            twin.DeviceId,
            //            twin.ConnectionState,
            //            twin.LastActivityTime);

            //        var twin1 = await _registryManager.GetTwinAsync(twin.DeviceId);
            //    }
            //}
        }

        public async Task AddDeviceAsync(string deviceId)
        {
            Console.Write($"Adding device '{deviceId}' with default authentication . . . ");
            await _registryManager.AddDeviceAsync(new Device(deviceId)).ConfigureAwait(false);
            Console.WriteLine("DONE");
        }

        public async Task AddDeviceWithSelfSignedCertificateAsync(
            string deviceId,
            string primaryThumbprint,
            string secondaryThumbprint)
        {
            var device = new Device(deviceId)
            {
                Authentication = new AuthenticationMechanism
                {
                    Type = AuthenticationType.SelfSigned,
                    X509Thumbprint = new X509Thumbprint
                    {
                        PrimaryThumbprint = primaryThumbprint,
                        SecondaryThumbprint = secondaryThumbprint
                    }
                }
            };

            Console.Write($"Adding device '{deviceId}' with self signed certificate auth . . . ");
            await _registryManager.AddDeviceAsync(device).ConfigureAwait(false);
            Console.WriteLine("DONE");
        }

        public async Task AddDeviceWithCertificateAuthorityAuthenticationAsync(string deviceId)
        {
            var device = new Device(deviceId)
            {
                Authentication = new AuthenticationMechanism
                {
                    Type = AuthenticationType.CertificateAuthority
                }
            };

            Console.Write($"Adding device '{deviceId}' with CA authentication . . . ");
            await _registryManager.AddDeviceAsync(device).ConfigureAwait(false);
            Console.WriteLine("DONE");
        }

        public async Task RemoveDeviceAsync(string deviceId)
        {
            Console.Write($"Remove device '{deviceId}' . . . ");
            await _registryManager.RemoveDeviceAsync(deviceId);
            Console.WriteLine("Done");
        }

        public async Task UpdateDesiredProperties(string deviceId)
        {
            var twin = await _registryManager.GetTwinAsync(deviceId);

            var patch =
                @"{
                properties: {
                    desired: {
                      customKey: 'customValue'
                    }
                }
            }";

            await _registryManager.UpdateTwinAsync(twin.DeviceId, patch, twin.ETag);
        }
    }
}
