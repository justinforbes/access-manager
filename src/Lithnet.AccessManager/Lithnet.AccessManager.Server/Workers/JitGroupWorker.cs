﻿using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lithnet.AccessManager.Server.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Lithnet.AccessManager.Server.Workers
{
    public class JitGroupWorker : BackgroundService
    {
        private readonly ILogger logger;
        private readonly JitConfigurationOptions options;
        private readonly IJitAccessGroupResolver groupResolver;
        private readonly IActiveDirectory directory;
        private readonly IDiscoveryServices discoveryServices;
        private readonly Dictionary<string, SearchParameters> deltaInformation = new Dictionary<string, SearchParameters>(StringComparer.CurrentCultureIgnoreCase);

        private readonly int fullSyncInterval;

        private readonly int deltaSyncInterval;

        private readonly int timerInterval;

        public JitGroupWorker(ILogger<JitGroupWorker> logger, IOptions<JitConfigurationOptions> options, IJitAccessGroupResolver groupResolver, IActiveDirectory directory, IDiscoveryServices discoveryServices)
        {
            this.logger = logger;
            this.options = options.Value;
            this.groupResolver = groupResolver;
            this.directory = directory;
            this.discoveryServices = discoveryServices;
            this.fullSyncInterval = Math.Max(1, this.options.FullSyncInterval ?? 60);
            this.deltaSyncInterval = Math.Max(0, this.options.DeltaSyncInterval ?? 1);

            if (this.deltaSyncInterval >= this.fullSyncInterval)
            {
                this.deltaSyncInterval = 0;
            }

            this.timerInterval = this.deltaSyncInterval <= 0 ? this.fullSyncInterval : this.deltaSyncInterval;
        }

        public override Task StartAsync(CancellationToken cancellationToken)
        {
            this.logger.LogTrace("Starting JIT group thread");
            return base.StartAsync(cancellationToken);
        }

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            this.logger.LogTrace("Stopping JIT group thread");
            return base.StopAsync(cancellationToken);
        }

        protected override async Task ExecuteAsync(CancellationToken cancellationToken)
        {
            try
            {
                if (!this.options.EnableJitGroupCreation)
                {
                    return;
                }

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                await WorkLoop(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
        }

        private async Task WorkLoop(CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                this.ProcessJitGroups(cancellationToken);
                await Task.Delay(TimeSpan.FromMinutes(this.timerInterval), cancellationToken);
            }
        }

        private void ProcessJitGroups(CancellationToken cancellationToken)
        {
            foreach (var mapping in this.options.JitGroupMappings)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    this.PerformSync(mapping, cancellationToken);
                }
                catch (OperationCanceledException)
                { }
                catch (Exception ex)
                {
                    this.logger.LogError(EventIDs.JitWorkerUnexpectedError, ex, $"There was an unexpected error in the JIT group processing thread for the target {mapping.ComputerOU}");
                }
            }
        }

        private void ThrowOnMappingConfigurationError(JitGroupMapping mapping)
        {
            if (string.IsNullOrWhiteSpace(mapping.ComputerOU))
            {
                throw new ConfigurationException("A JIT group mapping contains a null ComputerOU field");
            }

            if (string.IsNullOrWhiteSpace(mapping.GroupOU))
            {
                throw new ConfigurationException("A JIT group mapping contains a null GroupOU field");
            }

            if (string.IsNullOrWhiteSpace(mapping.GroupNameTemplate))
            {
                throw new ConfigurationException("A JIT group mapping had a null GroupNameTemplate field");
            }

            if (!this.groupResolver.IsTemplatedName(mapping.GroupNameTemplate))
            {
                throw new ConfigurationException(
                    $"The mapping for computers in OU '{mapping.ComputerOU}' contains a template without the %computerName% placeholder and cannot be processed");
            }
        }

        private void PerformSync(JitGroupMapping mapping, CancellationToken cancellationToken)
        {
            this.ThrowOnMappingConfigurationError(mapping);

            if (!this.deltaInformation.TryGetValue(mapping.ComputerOU, out SearchParameters usnData))
            {
                usnData = new SearchParameters
                {
                    Server = mapping.PreferredDC,
                    DnsDomain = this.discoveryServices.GetDomainNameDns(mapping.ComputerOU),
                    LastUsn = 0
                };

                this.deltaInformation.Add(mapping.ComputerOU, usnData);
            }

            cancellationToken.ThrowIfCancellationRequested();

            if (this.deltaSyncInterval <= 0 || DateTime.UtcNow > usnData.LastFullSync.AddMinutes(this.fullSyncInterval))
            {
                this.logger.LogTrace("Resetting for a full sync");
                usnData.Server = mapping.PreferredDC;
                usnData.LastUsn = 0;
            }

            this.PopulateUsnDataWithFallback(usnData);
            cancellationToken.ThrowIfCancellationRequested();

            if (usnData.LastUsn == usnData.HighestUsn)
            {
                this.logger.LogTrace($"No directory changes detected on {usnData.Server}");
                return;
            }

            if (usnData.LastUsn == 0)
            {
                this.PerformFullSync(mapping, usnData, cancellationToken);
                usnData.LastFullSync = DateTime.UtcNow;
            }
            else
            {
                this.PerformPartialSync(mapping, usnData, cancellationToken);
            }

            usnData.LastUsn = usnData.HighestUsn;
        }

        private void PerformPartialSync(JitGroupMapping mapping, SearchParameters data, CancellationToken cancellationToken)
        {
            this.logger.LogTrace($"Performing delta JIT group synchronization for domain {data.DnsDomain} against server {data.Server}");
            cancellationToken.ThrowIfCancellationRequested();

            var computers = GetComputers(mapping, data);
            cancellationToken.ThrowIfCancellationRequested();

            var expectedGroupNames = GetExpectedGroupNames(computers, mapping.GroupNameTemplate).ToList();
            cancellationToken.ThrowIfCancellationRequested();

            if (expectedGroupNames.Count > 0)
            {
                this.logger.LogTrace($"{expectedGroupNames.Count} groups to create");
                this.CreateGroups(expectedGroupNames, mapping.GroupOU, mapping.GroupDescription, mapping.GroupType, cancellationToken);
            }
            else
            {
                this.logger.LogTrace("No groups need to be created");
            }
        }

        private void PerformFullSync(JitGroupMapping mapping, SearchParameters data, CancellationToken cancellationToken)
        {
            this.logger.LogTrace($"Performing full JIT group synchronization for domain {data.DnsDomain} against server {data.Server}");
            cancellationToken.ThrowIfCancellationRequested();

            var computers = GetComputers(mapping, data);
            cancellationToken.ThrowIfCancellationRequested();

            var groups = GetGroups(mapping);
            cancellationToken.ThrowIfCancellationRequested();

            var expectedGroupNames = GetExpectedGroupNames(computers, mapping.GroupNameTemplate).ToList();
            var currentGroupNames = groups.Select(t => t.GetPropertyString("cn")).ToList();
            var groupsToCreate = expectedGroupNames.Except(currentGroupNames, StringComparer.CurrentCultureIgnoreCase).ToList();

            if (groupsToCreate.Count > 0)
            {
                this.logger.LogTrace($"{groupsToCreate.Count} groups to create");
                this.CreateGroups(groupsToCreate, mapping.GroupOU, mapping.GroupDescription, mapping.GroupType, cancellationToken);
            }
            else
            {
                this.logger.LogTrace("No groups need to be created");
            }

            if (mapping.EnableJitGroupDeletion)
            {
                var groupsToDelete = groups
                    .Where(t => !expectedGroupNames.Contains(t.GetPropertyString("cn"), StringComparer.CurrentCultureIgnoreCase))
                    .Select(t => t.GetPropertyString("msDS-PrincipalName")).ToList();

                if (groupsToDelete.Count > 0)
                {
                    this.logger.LogTrace($"{groupsToDelete.Count} groups to delete");
                    this.DeleteGroups(groupsToDelete, cancellationToken);
                }
                else
                {
                    this.logger.LogTrace("No groups need to be deleted");
                }
            }
        }

        private void CreateGroups(IEnumerable<string> groupsToCreate, string groupOU, string groupDescription, GroupType groupType, CancellationToken cancellationToken)
        {
            foreach (var group in groupsToCreate)
            {
                cancellationToken.ThrowIfCancellationRequested();

                try
                {
                    this.logger.LogTrace($"Creating JIT group {group} in OU {groupOU}");
                    this.directory.CreateGroup(group, groupDescription ?? "JIT access group created by Lithnet Access Manager", groupType, groupOU, true);
                    this.logger.LogInformation(EventIDs.JitWorkerGroupCreated, $"Created JIT group {group} in OU {groupOU}");
                }
                catch (Exception ex)
                {
                    this.logger.LogError(EventIDs.JitWorkerGroupCreateError, ex, $"The JIT group {group} could not be created in OU {groupOU}");
                }
            }
        }

        private void DeleteGroups(IEnumerable<string> groupsToDelete, CancellationToken cancellationToken)
        {
            foreach (var group in groupsToDelete)
            {
                cancellationToken.ThrowIfCancellationRequested();

                string groupName = group;

                try
                {
                    this.logger.LogTrace($"Deleting JIT group {groupName}");
                    this.directory.DeleteGroup(groupName);
                    this.logger.LogInformation(EventIDs.JitWorkerGroupDeleted, $"Deleted JIT group {groupName}");
                }
                catch (Exception ex)
                {
                    this.logger.LogError(EventIDs.JitWorkerGroupDeleteError, ex, $"The JIT group {groupName} could not be deleted");
                }
            }
        }

        private IList<SearchResult> GetComputers(JitGroupMapping mapping, SearchParameters searchParams)
        {
            return this.GetObjects(searchParams.Server, mapping.ComputerOU, "computer",
                mapping.Subtree ? SearchScope.Subtree : SearchScope.OneLevel, searchParams.LastUsn, searchParams.HighestUsn);
        }

        private IList<SearchResult> GetGroups(JitGroupMapping mapping)
        {
            string filter = $"(&(objectCategory=group))";
            string path = $"LDAP://{mapping.GroupOU.EscapeAdsiComponent()}";
            return this.GetObjects(path, SearchScope.OneLevel, filter);
        }

        private IList<SearchResult> GetObjects(string path, SearchScope scope, string filter)
        {
            logger.LogTrace($"Searching in {path} ({scope}) with filter '{filter}'");

            using DirectorySearcher d = new DirectorySearcher
            {
                SearchRoot = new DirectoryEntry(path),
                SearchScope = scope,
                Filter = filter,
                PageSize = 1000
            };

            d.PropertiesToLoad.Add("cn");
            d.PropertiesToLoad.Add("samAccountName");
            d.PropertiesToLoad.Add("msDS-PrincipalName");

            using SearchResultCollection result = d.FindAll();

            logger.LogTrace($"{result.Count} items found");
            return result.OfType<SearchResult>().ToList();
        }

        private IList<SearchResult> GetObjects(string server, string ou, string objectCategory, SearchScope scope, long lowUsn, long highUsn)
        {
            string filter = $"(&(objectCategory={objectCategory})(uSNCreated<={highUsn})(uSNCreated>={lowUsn + 1}))";
            string path = $"LDAP://{server}/{ou.EscapeAdsiComponent()}";
            return this.GetObjects(path, scope, filter);
        }

        private IEnumerable<string> GetExpectedGroupNames(IList<SearchResult> results, string template)
        {
            foreach (var result in results)
            {
                string cn = result.GetPropertyString("cn");

                if (cn.Contains("\0CNF:", StringComparison.Ordinal))
                {
                    continue;
                }

                string samAccountName = result.GetPropertyString("samAccountName").TrimEnd('$');
                
                var name = this.groupResolver.BuildGroupName(template, null, samAccountName);
                yield return name.TrimStart('\\');
            }
        }

        private void PopulateUsnDataWithFallback(SearchParameters search)
        {
            search.Server ??= search.DnsDomain;

            try
            {
                this.PopulateUsnData(search);
            }
            catch (Exception ex)
            {
                logger.LogWarning(EventIDs.JitWorkerUsnFallback, ex, $"Could not contact {search.Server}. Resetting USN data and attempting against domain {search.DnsDomain}");
                search.LastUsn = 0;
                search.Server = search.DnsDomain;
                this.PopulateUsnData(search);
            }
        }

        private void PopulateUsnData(SearchParameters search)
        {
            var rootDse = new DirectoryEntry($"LDAP://{search.Server}/rootDSE");
            search.Server = rootDse.GetPropertyString("dnsHostName");
            search.HighestUsn = long.Parse(rootDse.GetPropertyString("highestCommittedUSN"));
        }
    }
}