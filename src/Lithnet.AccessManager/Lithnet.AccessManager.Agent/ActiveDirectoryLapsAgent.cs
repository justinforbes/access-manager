﻿using System;
using System.Security.Principal;
using Lithnet.AccessManager.Agent.Providers;
using Microsoft.Extensions.Logging;

namespace Lithnet.AccessManager.Agent
{
    public class ActiveDirectoryLapsAgent
    {
        private readonly ILogger<ActiveDirectoryLapsAgent> logger;
        private readonly IDirectory directory;
        private readonly IActiveDirectoryLapsSettingsProvider settings;
        private readonly IPasswordGenerator passwordGenerator;
        private readonly ILocalSam sam;
        private readonly ILithnetAdminPasswordProvider lithnetAdminPasswordProvider;

        public ActiveDirectoryLapsAgent(ILogger<ActiveDirectoryLapsAgent> logger, IDirectory directory, IActiveDirectoryLapsSettingsProvider settings, IPasswordGenerator passwordGenerator, ILocalSam sam, ILithnetAdminPasswordProvider lithnetAdminPasswordProvider)
        {
            this.logger = logger;
            this.directory = directory;
            this.settings = settings;
            this.passwordGenerator = passwordGenerator;
            this.sam = sam;
            this.lithnetAdminPasswordProvider = lithnetAdminPasswordProvider;
        }

        public void DoCheck()
        {
            try
            {
                IActiveDirectoryComputer computer = this.directory.GetComputer(this.sam.GetMachineNTAccountName());

                if (this.HasPasswordExpired(computer))
                {
                    this.logger.LogTrace(EventIDs.PasswordExpired, "Password has expired and needs to be changed");
                    this.ChangePassword(computer);
                }
            }
            catch (Exception ex)
            {
                this.logger.LogError(EventIDs.LapsUnexpectedException, ex, "The LAPS agent process encountered an error");
            }
        }

        public bool HasPasswordExpired(IActiveDirectoryComputer computer)
        {
            try
            {
                return this.lithnetAdminPasswordProvider.HasPasswordExpired(computer, this.settings.MsMcsAdmPwdBehaviour == PasswordAttributeBehaviour.Populate);
            }
            catch (Exception ex)
            {
                this.logger.LogWarning(EventIDs.PasswordExpiryCheckFailure, ex, "Could not check the password expiry date");
                return false;
            }
        }

        public void ChangePassword(IActiveDirectoryComputer computer, SecurityIdentifier sid = null)
        {
            try
            {
                if (sid == null)
                {
                    sid = this.sam.GetWellKnownSid(WellKnownSidType.AccountAdministratorSid);
                }

                string newPassword = this.passwordGenerator.Generate(this.settings);
                string accountName = this.sam.GetBuiltInAdministratorAccountName();
                DateTime rotationInstant = DateTime.UtcNow;
                DateTime expiryDate = DateTime.UtcNow.AddDays(Math.Max(this.settings.MaximumPasswordAgeDays, 1));

                this.lithnetAdminPasswordProvider.UpdateCurrentPassword(computer, accountName, newPassword, rotationInstant, expiryDate, this.settings.PasswordHistoryDaysToKeep, this.settings.MsMcsAdmPwdBehaviour);

                this.logger.LogTrace(EventIDs.SetPasswordOnAmAttribute, "Set password on Lithnet Access Manager attribute");

                if (this.settings.MsMcsAdmPwdBehaviour == PasswordAttributeBehaviour.Populate)
                {
                    this.logger.LogTrace(EventIDs.SetPasswordOnLapsAttribute, "Set password on Microsoft LAPS attribute");
                }

                this.sam.SetLocalAccountPassword(sid, newPassword);
                this.logger.LogInformation(EventIDs.SetPassword, "The local administrator password has been changed and will expire on {expiryDate}", expiryDate.ToLocalTime());
            }
            catch (Exception ex)
            {
                this.logger.LogError(EventIDs.PasswordChangeFailure, ex, "The password change operation failed");
            }
        }
    }
}
