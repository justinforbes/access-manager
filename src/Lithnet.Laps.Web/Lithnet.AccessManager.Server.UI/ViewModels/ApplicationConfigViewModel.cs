﻿using System;
using System.Linq;
using System.Runtime.CompilerServices;
using Lithnet.AccessManager.Server.Configuration;
using MahApps.Metro.Controls.Dialogs;
using Stylet;
using Vanara.Extensions;

namespace Lithnet.AccessManager.Server.UI
{
    public class ApplicationConfigViewModel : Conductor<PropertyChangedBase>.Collection.OneActive
    {
        private readonly IApplicationConfig model;

        private readonly IDialogCoordinator dialogCoordinator;

        public ApplicationConfigViewModel(
            IApplicationConfig model,
            IDialogCoordinator dialogCoordinator,
            AuthenticationViewModel authentication,
            AuthorizationViewModel authorization,
            UserInterfaceViewModel ui,
            RateLimitsViewModel rate,
            IpDetectionViewModel ip,
            AuditingViewModel audit,
            EmailViewModel mail,
            HostingViewModel hosting,
            ActiveDirectoryConfigurationViewModel ad,
            JitConfigurationViewModel jit,
            LapsConfigurationViewModel laps,
            HelpViewModel help)
        {
            this.model = model;
            this.dialogCoordinator = dialogCoordinator;

            this.Items.Add(hosting);
            this.Items.Add(authentication);
            this.Items.Add(ad);
            this.Items.Add(audit);
            this.Items.Add(authorization);
            this.Items.Add(jit);
            this.Items.Add(laps);
            this.Items.Add(ui);
            this.Items.Add(mail);
            this.Items.Add(rate);
            this.Items.Add(ip);

            this.OptionItems = new BindableCollection<PropertyChangedBase>();
            this.OptionItems.Add(help);

            this.ActiveItem = this.Items.First();
        }
        
        public BindableCollection<PropertyChangedBase> OptionItems { get; }

        public void Save()
        {
            try
            {
                this.model.Save(this.model.Path);
            }
            catch (Exception ex)
            {
                this.dialogCoordinator.ShowMessageAsync(this, "Error saving file", $"The configuration file could not be saved\r\n{ex.Message}");
            }
        }
    }
}
