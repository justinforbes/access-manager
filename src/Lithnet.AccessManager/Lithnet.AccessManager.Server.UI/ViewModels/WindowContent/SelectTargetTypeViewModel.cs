﻿using System;
using System.Collections.Generic;
using System.Linq;
using Lithnet.AccessManager.Api;
using Lithnet.AccessManager.Enterprise;
using Lithnet.AccessManager.Server.Configuration;
using Lithnet.AccessManager.Server.UI.Providers;
using Stylet;

namespace Lithnet.AccessManager.Server.UI
{
    public class SelectTargetTypeViewModel : Screen
    {
        private readonly AzureAdOptions azureAdOptions;
        private readonly IRegistryProvider registryProvider;
        private readonly IAmsLicenseManager licenseManager;

        public SelectTargetTypeViewModel(AzureAdOptions azureAdOptions, IDomainTrustProvider domainTrustProvider, IViewModelFactory<AzureAdTenantDetailsViewModel, AzureAdTenantDetails> tenantDetailsFactory, IRegistryProvider registryProvider, IAmsLicenseManager licenseManager)
        {
            this.azureAdOptions = azureAdOptions;
            this.registryProvider = registryProvider;
            this.licenseManager = licenseManager;

            this.AvailableForests = domainTrustProvider.GetForests().Select(t => t.Name).ToList();
            this.AvailableAads = new List<AzureAdTenantDetailsViewModel>();

            foreach (var tenant in azureAdOptions.Tenants)
            {
                this.AvailableAads.Add(tenantDetailsFactory.CreateViewModel(tenant));
            }

            this.SelectedForest = this.AvailableForests.FirstOrDefault();
            this.SelectedAad = this.AvailableAads.FirstOrDefault();
        }

        public bool AllowAzureAdGroup { get; set; } = true;

        public bool AllowAzureAdTenant { get; set; } = true;

        public bool AllowAzureAdComputer { get; set; } = true;

        public bool AllowAdComputer { get; set; } = true;

        public bool AllowAdGroup { get; set; } = true;

        public bool AllowAdContainer { get; set; } = true;

        public bool AllowAmsGroup { get; set; } = true;

        public bool AllowAmsComputer { get; set; } = true;

        public string SelectedForest { get; set; }

        public List<string> AvailableForests { get; set; }

        public TargetType TargetType { get; set; }

        public bool ShowForest => this.TargetType == TargetType.AdComputer || this.TargetType == TargetType.AdGroup;

        public IEnumerable<TargetType> TargetTypeValues
        {
            get
            {
                if (this.AllowAdComputer)
                {
                    yield return TargetType.AdComputer;
                }

                if (this.AllowAdGroup)
                {
                    yield return TargetType.AdGroup;
                }

                if (this.AllowAdContainer)
                {
                    yield return TargetType.AdContainer;
                }

                if (this.licenseManager.IsFeatureEnabled(LicensedFeatures.AmsApi) && this.registryProvider.ApiEnabled)
                {
                    if (this.licenseManager.IsFeatureEnabled(LicensedFeatures.AzureAdDeviceSupport) && this.azureAdOptions.Tenants.Count > 0)
                    {
                        if (this.AllowAzureAdComputer)
                        {
                            yield return TargetType.AadComputer;
                        }

                        if (this.AllowAzureAdGroup)
                        {
                            yield return TargetType.AadGroup;
                        }

                        if (this.AllowAzureAdTenant)
                        {
                            yield return TargetType.AadTenant;
                        }
                    }

                    if (this.licenseManager.IsFeatureEnabled(LicensedFeatures.AmsRegisteredDeviceSupport))
                    {
                        if (this.AllowAmsComputer)
                        {
                            yield return TargetType.AmsComputer;
                        }

                        if (this.AllowAmsGroup)
                        {
                            yield return TargetType.AmsGroup;
                        }
                    }
                }
            }
        }

        public void SetDefaultTarget(TargetType type)
        {
            if (this.TargetTypeValues.Contains(type))
            {
                this.TargetType = type;
            }
            else
            {
                this.TargetType = this.TargetTypeValues.FirstOrDefault();
            }
        }

        public AzureAdTenantDetailsViewModel SelectedAad { get; set; }

        public bool ShowAad => this.TargetType.IsAadTarget();

        public List<AzureAdTenantDetailsViewModel> AvailableAads { get; set; }
    }
}