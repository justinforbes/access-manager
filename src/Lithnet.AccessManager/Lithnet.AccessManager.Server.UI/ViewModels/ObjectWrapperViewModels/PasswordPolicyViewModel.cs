﻿using Lithnet.AccessManager.Api;
using MahApps.Metro.Controls.Dialogs;
using MahApps.Metro.SimpleChildWindow;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Stylet;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Lithnet.AccessManager.Server.UI
{
    public sealed class PasswordPolicyViewModel : ValidatingModelBase, IViewAware
    {
        private readonly IViewModelFactory<AzureAdObjectSelectorViewModel> aadSelectorFactory;
        private readonly IViewModelFactory<AmsGroupSelectorViewModel> amsGroupSelectorFactory;
        private readonly IViewModelFactory<SelectTargetTypeViewModel> selectTargetTypeFactory;
        private readonly IDialogCoordinator dialogCoordinator;
        private readonly ILogger<PasswordPolicyViewModel> logger;
        private readonly IViewModelFactory<ExternalDialogWindowViewModel, Screen> externalDialogWindowFactory;
        private readonly IWindowManager windowManager;

        private bool isDefault;

        public PasswordPolicyEntry Model { get; }

        public PasswordPolicyViewModel(PasswordPolicyEntry model, IModelValidator<PasswordPolicyViewModel> validator, IViewModelFactory<AzureAdObjectSelectorViewModel> aadSelectorFactory, IViewModelFactory<SelectTargetTypeViewModel> selectTargetTypeFactory, IViewModelFactory<AmsGroupSelectorViewModel> amsGroupSelectorFactory, IDialogCoordinator dialogCoordinator, ILogger<PasswordPolicyViewModel> logger, IViewModelFactory<ExternalDialogWindowViewModel, Screen> externalDialogWindowFactory, IWindowManager windowManager)
        {
            this.Model = model;
            this.aadSelectorFactory = aadSelectorFactory;
            this.selectTargetTypeFactory = selectTargetTypeFactory;
            this.amsGroupSelectorFactory = amsGroupSelectorFactory;
            this.dialogCoordinator = dialogCoordinator;
            this.logger = logger;
            this.externalDialogWindowFactory = externalDialogWindowFactory;
            this.windowManager = windowManager;
            this.Validator = validator;
            this.Validate();
        }

        public bool IsDefault
        {
            get => this.isDefault;
            set
            {
                this.isDefault = value;
                this.Validate();
            }
        }

        public bool IsNotDefault => !this.IsDefault;

        public string Id
        {
            get => this.Model.Id;
            set => this.Model.Id = value;
        }

        public string Name
        {
            get => this.Model.Name;
            set => this.Model.Name = value;
        }

        public string TargetGroup
        {
            get => this.Model.TargetGroup;
            set => this.Model.TargetGroup = value;
        }

        public string TargetGroupCachedName
        {
            get => this.Model.TargetGroupCachedName;
            set => this.Model.TargetGroupCachedName = value;
        }

        public string DisplayName
        {
            get
            {
                if (string.IsNullOrWhiteSpace(this.TargetGroupCachedName))
                {
                    return this.TargetGroup;
                }
                else
                {
                    return this.TargetGroupCachedName;
                }
            }
        }

        public AuthorityType TargetType
        {
            get => this.Model.TargetType;
            set => this.Model.TargetType = value;
        }

        public int MaximumPasswordAgeDays
        {
            get => this.Model.MaximumPasswordAgeDays;
            set => this.Model.MaximumPasswordAgeDays = value;
        }

        public int MinimumNumberOfPasswords
        {
            get => this.Model.MinimumNumberOfPasswords;
            set => this.Model.MinimumNumberOfPasswords = value;
        }

        public int MinimumPasswordHistoryAgeDays
        {
            get => this.Model.MinimumPasswordHistoryAgeDays;
            set => this.Model.MinimumPasswordHistoryAgeDays = value;
        }

        public int PasswordLength
        {
            get => this.Model.PasswordLength;
            set => this.Model.PasswordLength = value;
        }

        public bool UseLower
        {
            get => this.Model.UseLower;
            set
            {
                this.Model.UseLower = value;
                this.Validate();
            }
        }


        public bool UseNumeric
        {
            get => this.Model.UseNumeric;
            set
            {
                this.Model.UseNumeric = value;
                this.Validate();
            }
        }


        public bool UseSymbol
        {
            get => this.Model.UseSymbol;
            set
            {
                this.Model.UseSymbol = value;
                this.Validate();
            }
        }


        public bool UseUpper
        {
            get => this.Model.UseUpper;
            set
            {
                this.Model.UseUpper = value;
                this.Validate();
            }
        }

        public async Task SelectTarget()
        {
            try
            {
                SelectTargetTypeViewModel vm = this.selectTargetTypeFactory.CreateViewModel();
                vm.AllowAdComputer = false;
                vm.AllowAdContainer = false;
                vm.AllowAdGroup = false;
                vm.AllowAmsComputer = false;
                vm.AllowAmsGroup = true;
                vm.AllowAzureAdComputer = false;
                vm.AllowAzureAdGroup = true;
                vm.AllowAzureAdTenant = false;

                if (!vm.TargetTypeValues.Any())
                {
                    return;
                }

                vm.SetDefaultTarget(Configuration.TargetType.AadGroup);

                DialogWindow w1 = new DialogWindow
                {
                    Title = "Select target type",
                    DataContext = vm,
                    SaveButtonName = "Next...",
                    SaveButtonIsDefault = true
                };

                await this.GetWindow().ShowChildWindowAsync(w1);

                if (w1.Result != MessageDialogResult.Affirmative)
                {
                    return;
                }

                if (vm.TargetType == Configuration.TargetType.AadGroup && vm.SelectedAad != null)
                {
                    this.SelectTargetAadGroup(vm.SelectedAad.TenantId);
                }
                if (vm.TargetType == Configuration.TargetType.AmsGroup)
                {
                    this.SelectTargetAmsGroup();
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(EventIDs.UIGenericError, ex, "Could not select a target");
                await this.dialogCoordinator.ShowMessageAsync(this, "Error", $"Could not select the target\r\n{ex.Message}");
            }
        }

        private void SelectTargetAmsGroup()
        {
            var selectorVm = this.amsGroupSelectorFactory.CreateViewModel();
            selectorVm.ShowBuiltInGroups = true;

            var evm = externalDialogWindowFactory.CreateViewModel(selectorVm);

            if (!windowManager.ShowDialog(evm) ?? false)
            {
                return;
            }

            if (selectorVm.SelectedItem != null)
            {
                this.TargetGroup = selectorVm.SelectedItem.Sid;
                this.TargetType = AuthorityType.Ams;
                this.TargetGroupCachedName = selectorVm.SelectedItem.Name;
            }
        }

        private void SelectTargetAadGroup(string tenant)
        {
            var selectorVm = this.aadSelectorFactory.CreateViewModel();
            selectorVm.Type = Configuration.TargetType.AadGroup;
            selectorVm.TenantId = tenant;
            var evm = externalDialogWindowFactory.CreateViewModel(selectorVm);

            if (!windowManager.ShowDialog(evm) ?? false)
            {
                return;
            }

            if (selectorVm.SelectedItem is Group g)
            {
                this.TargetGroup = g.GetSidString();
                this.TargetType = AuthorityType.AzureActiveDirectory;
                this.TargetGroupCachedName = g.DisplayName;
            }
        }

        public UIElement View { get; set; }

        public void AttachView(UIElement view)
        {
            this.View = view;
        }
    }
}