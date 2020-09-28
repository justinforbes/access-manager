﻿using System;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Lithnet.AccessManager.Server.Configuration;
using Lithnet.AccessManager.Server.UI.AuthorizationRuleImport;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Extensions.Logging;
using PropertyChanged;
using Stylet;

namespace Lithnet.AccessManager.Server.UI
{
    public class ImportWizardWindowViewModel : Conductor<PropertyChangedBase>.Collection.OneActive
    {
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly ILogger<ImportWizardWindowViewModel> logger;
        private readonly ImportWizardCsvSettingsViewModel csvSettingsVm;
        private readonly ImportWizardImportContainerViewModel containerVm;
        private readonly ImportWizardImportTypeViewModel importTypeVm;
        private readonly ImportWizardRuleSettingsViewModel ruleVm;
        private readonly ImportWizardLapsWebSettingsViewModel lapsWebVm;
        private readonly ImportWizardImportReadyViewModel importReadyVm;
        private readonly IImportProviderFactory importProviderFactory;
        private readonly IImportResultsViewModelFactory resultsFactory;
        private readonly AuditOptions auditOptions;
        private readonly IEventAggregator eventAggregator;

        private ProgressDialogController progress;
        private int progressCurrent;
        private int progressMaximum;
        private CancellationTokenSource progressCts;
        private bool isSaved;

        public bool NextButtonIsDefault { get; set; } = false;

        public bool CancelButtonVisible { get; set; } = true;

        public bool NextButtonVisible { get; set; } = true;

        public bool BackButtonVisible { get; set; } = true;

        public bool ImportButtonVisible { get; set; } = false;

        public bool ImportButtonIsDefault { get; set; } = false;

        public bool DoDiscoveryButtonVisible { get; set; } = false;

        public bool DoDiscoveryButtonIsDefault { get; set; } = false;

        public AuthorizationViewModel AuthorizationViewModel { get; set; }

        public ImportWizardWindowViewModel(IDialogCoordinator dialogCoordinator, ILogger<ImportWizardWindowViewModel> logger, ImportWizardImportTypeViewModel importTypeVm, ImportWizardCsvSettingsViewModel csvSettingsVm, ImportWizardImportContainerViewModel containerVm, ImportWizardRuleSettingsViewModel ruleVm, ImportWizardLapsWebSettingsViewModel lapsWebVm, ImportWizardImportReadyViewModel importReadyVm, IImportProviderFactory importProviderFactory, IImportResultsViewModelFactory resultsFactory, AuditOptions auditOptions, IEventAggregator eventAggregator)
        {
            this.logger = logger;
            this.dialogCoordinator = dialogCoordinator;
            this.DisplayName = Constants.AppName;
            this.dialogCoordinator = dialogCoordinator;
            this.importProviderFactory = importProviderFactory;
            this.resultsFactory = resultsFactory;
            this.auditOptions = auditOptions;
            this.eventAggregator = eventAggregator;

            // VM mappings
            this.importTypeVm = importTypeVm;
            this.csvSettingsVm = csvSettingsVm;
            this.containerVm = containerVm;
            this.ruleVm = ruleVm;
            this.lapsWebVm = lapsWebVm;
            this.importReadyVm = importReadyVm;

            // Bindings
            this.importTypeVm.Bind(t => t.ImportType, OnImportModeChanged);

            // Initial binds
            this.Items.Add(importTypeVm);
            this.BuildPages(ImportMode.Laps);
            this.ActiveItem = this.Items.First();
        }

        [SuppressPropertyChangedWarnings]
        private void OnImportModeChanged(object x, PropertyChangedExtendedEventArgs<ImportMode> y)
        {
            this.containerVm.SetImportMode(this.importTypeVm.ImportType);
            this.ruleVm.SetImportMode(this.importTypeVm.ImportType);
            this.BuildPages(this.importTypeVm.ImportType);
        }

        public override void ActivateItem(PropertyChangedBase item)
        {
            base.ActivateItem(item);

            if (item is INotifyDataErrorInfo e)
            {
                e.ErrorsChanged += E_ErrorsChanged;
            }

            this.UpdateNavigationCapabilities();
        }

        public override void DeactivateItem(PropertyChangedBase item)
        {
            base.DeactivateItem(item);

            if (item is INotifyDataErrorInfo e)
            {
                e.ErrorsChanged -= E_ErrorsChanged;
            }

            if (item is ImportResultsViewModel irvm)
            {
                irvm.Targets.ViewModels.CollectionChanged -= ViewModels_CollectionChanged;
                this.Items.Remove(item);
            }

            this.UpdateNavigationCapabilities();
        }

        private void E_ErrorsChanged(object sender, DataErrorsChangedEventArgs e)
        {
            this.UpdateNavigationCapabilities();
        }

        public bool CanNext
        {
            get
            {
                if (this.ActiveItem is INotifyDataErrorInfo vm && vm.HasErrors)
                {
                    return false;
                }

                return this.Items.IndexOf(this.ActiveItem) < this.Items.Count - 1;
            }
        }

        public void Next()
        {
            var oldItem = this.ActiveItem;

            int index = this.Items.IndexOf(this.ActiveItem);
            index++;

            if (index < this.Items.Count)
            {
                this.ActiveItem = this.Items[index];
            }

            if (oldItem != null && oldItem != this.ActiveItem)
            {
                this.DeactivateItem(oldItem);
            }

            this.UpdateNavigationCapabilities();
        }

        public bool CanBack => this.Items.IndexOf(this.ActiveItem) > 0;

        public void Back()
        {
            var oldItem = this.ActiveItem;

            int index = this.Items.IndexOf(this.ActiveItem);
            index--;

            if (index >= 0)
            {
                this.ActiveItem = this.Items[index];
            }

            if (oldItem != null && oldItem != this.ActiveItem)
            {
                this.DeactivateItem(oldItem);
            }

            this.UpdateNavigationCapabilities();
        }

        public void Cancel()
        {
            this.RequestClose();
        }

        public override async Task<bool> CanCloseAsync()
        {
            if (this.ActiveItem is ImportWizardImportTypeViewModel)
            {
                return true;
            }

            if (isSaved)
            {
                return true;
            }

            MetroDialogSettings settings = new MetroDialogSettings()
            {
                AffirmativeButtonText = "Yes",
                NegativeButtonText = "No"
            };

            return await this.dialogCoordinator.ShowMessageAsync(this, "Cancel", "Are you sure you want to cancel the import process?", MessageDialogStyle.AffirmativeAndNegative, settings) == MessageDialogResult.Affirmative;
        }

        public bool CanImport => this.ActiveItem is ImportResultsViewModel irvm && irvm.Targets.ViewModels.Count > 0;

        public async Task Import()
        {
            try
            {
                if (!(this.ActiveItem is ImportResultsViewModel irvm))
                {
                    return;
                }

                bool plural = irvm.Targets.ViewModels.Count != 1;
                
                MetroDialogSettings settings = new MetroDialogSettings()
                {
                    AffirmativeButtonText = "Yes",
                    NegativeButtonText = "No"
                };
                
                if (await this.dialogCoordinator.ShowMessageAsync(this, "Confirm import", $"Are you sure you want to import {(plural ? $"these {irvm.Targets.ViewModels.Count}" : "this")} authorization rule{(plural ? "s" : "")}?", MessageDialogStyle.AffirmativeAndNegative, settings) != MessageDialogResult.Affirmative)
                {
                    return;
                }

                this.auditOptions.NotificationChannels.Merge(irvm.NotificationChannels);
                this.AuthorizationViewModel.Merge(irvm.Targets, irvm.Merge, irvm.MergeOverwrite);

                this.eventAggregator.Publish(new NotificationSubscriptionReloadEvent());

                this.isSaved = true;

                this.RequestClose();
            }
            catch (Exception ex)
            {
                this.logger.LogError(EventIDs.UIGenericError, ex, "Merge error");
                await this.dialogCoordinator.ShowMessageAsync(this, "Import error", $"Could not merge the new rules into the authorization store. {ex.Message}");
            }
        }

        public async Task DoDiscovery()
        {
            try
            {
                this.progress = await this.dialogCoordinator.ShowProgressAsync(this, "Discovering authorization rules...", "Discovering objects", true);
                this.progress.Canceled += Progress_Canceled;
                this.progress.SetIndeterminate();
                this.progressCts = new CancellationTokenSource();
                this.progressCurrent = 0;

                ImportSettings settings = this.GetImportSettings();
                settings.CancellationToken = this.progressCts.Token;
                IImportProvider provider = this.importProviderFactory.CreateImportProvider(settings);

                await Task.Run(() =>
                {
                    this.progressMaximum = provider.GetEstimatedItemCount();
                    this.progress.Maximum = this.progressMaximum;

                    if (this.progressMaximum > 0)
                    {
                        provider.OnItemProcessStart += ImportProvider_OnStartProcessingComputer;
                    }
                });

                var results = await Task.Run(() => provider.Import());

                this.progress.SetIndeterminate();
                this.progress.SetMessage("Building authorization rules...");

                ImportResultsViewModel irvm = await resultsFactory.CreateViewModelAsync(results);

                irvm.Targets.ViewModels.CollectionChanged += ViewModels_CollectionChanged;

                await this.progress.CloseAsync();

                this.Items.Add(irvm);
                this.ActiveItem = irvm;
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                this.logger.LogError(EventIDs.UIGenericError, ex, "Discovery error");
                await this.dialogCoordinator.ShowMessageAsync(this, "Discovery error", $"Could not perform the import. {ex.Message}");
            }
            finally
            {
                if (this.progress.IsOpen)
                {
                    await this.progress.CloseAsync();
                }
            }
        }

        private void ViewModels_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            this.NotifyOfPropertyChange(nameof(this.CanImport));
        }

        private ImportSettings GetImportSettings()
        {
            ImportSettings importSettings;

            switch (this.importTypeVm.ImportType)
            {
                case ImportMode.CsvFile:
                    importSettings = new ImportSettingsCsv()
                    {
                        ImportFile = this.csvSettingsVm.ImportFile,
                        HasHeaderRow = this.csvSettingsVm.ImportFileHasHeaderRow
                    };
                    break;

                case ImportMode.Rpc:
                    importSettings = new ImportSettingsRpc();
                    break;

                case ImportMode.BitLocker:
                    importSettings = new ImportSettingsBitLocker();
                    break;

                case ImportMode.Laps:
                    importSettings = new ImportSettingsLaps();
                    break;

                case ImportMode.LapsWeb:
                    importSettings = new ImportSettingsLapsWeb
                    {
                        ImportFile = this.lapsWebVm.ImportFile,
                        ImportNotifications = this.lapsWebVm.ImportNotifications,
                        FailureTemplate = this.lapsWebVm.TemplateFailure,
                        SuccessTemplate = this.lapsWebVm.TemplateSuccess
                    };
                    break;

                default:
                    throw new InvalidOperationException("Unsupported import type");
            }

            if (importSettings is ImportSettingsComputerDiscovery cd)
            {
                cd.ImportOU = this.containerVm.Target;
                cd.DoNotConsolidate = this.containerVm.DoNotConsolidate;
                cd.DoNotConsolidateOnError = this.containerVm.DoNotConsolidateOnError;

                foreach (var sid in this.containerVm.FilteredComputers)
                {
                    cd.ComputerFilter.Add(sid.SecurityIdentifier);
                }
            }

            importSettings.ImportMode = this.importTypeVm.ImportType;
            importSettings.AllowBitLocker = this.ruleVm.AllowBitlocker;
            importSettings.AllowJit = this.ruleVm.AllowJit;
            importSettings.AllowLaps = this.ruleVm.AllowLaps;
            importSettings.AllowLapsHistory = this.ruleVm.AllowLapsHistory;
            importSettings.JitAuthorizingGroup = this.ruleVm.JitAuthorizingGroup;
            importSettings.JitExpireAfter = this.ruleVm.JitExpireAfter;
            importSettings.Notifications = this.ruleVm.Notifications?.Model;
            importSettings.RuleDescription = this.ruleVm.Description;

            foreach (var sid in this.containerVm.FilteredSids)
            {
                importSettings.PrincipalFilter.Add(sid.SecurityIdentifier);
            }

            return importSettings;
        }


        private void BuildPages(ImportMode mode)
        {
            this.RemoveWizardPages();

            switch (mode)
            {
                case ImportMode.CsvFile:
                    this.Items.Add(csvSettingsVm);
                    this.Items.Add(containerVm);
                    this.Items.Add(ruleVm);
                    this.Items.Add(importReadyVm);
                    break;

                case ImportMode.BitLocker:
                    this.Items.Add(containerVm);
                    this.Items.Add(ruleVm);
                    this.Items.Add(importReadyVm);
                    break;

                case ImportMode.Laps:
                    this.Items.Add(containerVm);
                    this.Items.Add(ruleVm);
                    this.Items.Add(importReadyVm);
                    break;

                case ImportMode.Rpc:
                    this.Items.Add(containerVm);
                    this.Items.Add(ruleVm);
                    this.Items.Add(importReadyVm);

                    break;

                case ImportMode.LapsWeb:
                    this.Items.Add(lapsWebVm);
                    this.Items.Add(ruleVm);
                    this.Items.Add(importReadyVm);
                    break;
            }

            this.UpdateNavigationCapabilities();
        }

        private void UpdateNavigationCapabilities()
        {
            if (this.ActiveItem is ImportWizardImportReadyViewModel)
            {
                this.NextButtonIsDefault = false;
                this.NextButtonVisible = false;
                this.ImportButtonIsDefault = false;
                this.ImportButtonVisible = false;
                this.DoDiscoveryButtonVisible = true;
                this.DoDiscoveryButtonIsDefault = true;
            }
            else if (this.ActiveItem is ImportResultsViewModel)
            {
                this.NextButtonIsDefault = false;
                this.NextButtonVisible = false;
                this.ImportButtonIsDefault = false;
                this.ImportButtonVisible = true;
                this.DoDiscoveryButtonVisible = false;
                this.DoDiscoveryButtonIsDefault = false;
            }
            else
            {
                this.NextButtonIsDefault = true;
                this.NextButtonVisible = true;
                this.ImportButtonIsDefault = false;
                this.ImportButtonVisible = false;
                this.DoDiscoveryButtonVisible = false;
                this.DoDiscoveryButtonIsDefault = false;
            }

            this.NotifyOfPropertyChange(nameof(this.CanNext));
            this.NotifyOfPropertyChange(nameof(this.CanBack));
            this.NotifyOfPropertyChange(nameof(this.CanImport));
        }

        private void RemoveWizardPages()
        {
            this.Items.RemoveRange(this.Items.Except(new[] { this.importTypeVm }).ToList());
        }

        private void Progress_Canceled(object sender, EventArgs e)
        {
            this.progressCts?.Cancel();
        }

        private void ImportProvider_OnStartProcessingComputer(object sender, ImportProcessingEventArgs e)
        {
            this.progress.SetMessage(e.Message);
            var val = Interlocked.Increment(ref this.progressCurrent);
            this.logger.LogTrace("Progress {count}/{max}", val, this.progressMaximum);
            this.progress.SetProgress(Math.Min(val, this.progressMaximum));
        }
    }
}
