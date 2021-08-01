﻿using Lithnet.AccessManager.Api.Providers;
using Lithnet.AccessManager.Api.Shared;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Security.Claims;
using System.Security.Principal;
using System.Threading.Tasks;
using Lithnet.AccessManager.Server;

namespace Lithnet.AccessManager.Api.Controllers
{
    [ApiController]
    [Route("agent/checkin")]
    [Produces("application/json")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    [Authorize(Constants.AuthZPolicyComputers)]
    [Authorize(Constants.AuthZPolicyApprovedClient)]
    public class AgentCheckInController : Controller
    {
        private readonly ILogger<AgentCheckInController> logger;
        private readonly IDeviceProvider deviceProvider;
        private readonly IApiErrorResponseProvider errorProvider;
        private readonly IActiveDirectory directory;
        private readonly IAadGraphApiProvider graph;
        private readonly ICheckInDataValidator checkinDataValidator;

        public AgentCheckInController(ILogger<AgentCheckInController> logger, IApiErrorResponseProvider errorProvider, IDeviceProvider deviceProvider, IActiveDirectory directory, IAadGraphApiProvider graph, ICheckInDataValidator checkinDataValidator)
        {
            this.logger = logger;
            this.errorProvider = errorProvider;
            this.deviceProvider = deviceProvider;
            this.directory = directory;
            this.graph = graph;
            this.checkinDataValidator = checkinDataValidator;
        }

        [HttpPost()]
        public async Task<IActionResult> UpdateAgentData([FromBody] AgentCheckIn data)
        {
            try
            {
                string deviceId = this.HttpContext.GetDeviceIdOrThrow();
                this.logger.LogTrace("Processing agent update request for {deviceId}", deviceId);

                var device = await this.deviceProvider.GetDeviceAsync(deviceId);
                device.ThrowOnDeviceDisabled();

                this.checkinDataValidator.ValidateCheckInData(data);
                device.AgentVersion = data.AgentVersion;
                device.ComputerName = data.Hostname;
                device.DnsName = data.DnsName;
                device.OperatingSystemFamily = data.OperatingSystem;
                device.OperatingSystemVersion = data.OperatingSystemVersion;
                device.OperatingSystemType = data.OperatingSystemType;

                await this.OverlayAuthorityData(device);
                await this.deviceProvider.UpdateDeviceAsync(device);

                this.logger.LogInformation("Agent check-in data updated for {deviceId}", deviceId);
                return this.NoContent();
            }
            catch (Exception ex)
            {
                return this.errorProvider.GetErrorResult(ex);
            }
        }

        private async Task OverlayAuthorityData(IDevice device)
        {
            if (device.AuthorityType == AuthorityType.ActiveDirectory)
            {
                IActiveDirectoryComputer computer = this.directory.GetComputer(new SecurityIdentifier(device.AuthorityDeviceId));
                device.ComputerName = computer.SamAccountName.TrimEnd('$');
            }
            else if (device.AuthorityType == AuthorityType.AzureActiveDirectory)
            {
                var computer = await graph.GetAadDeviceByIdAsync(device.AuthorityId, device.AuthorityDeviceId);
                device.ComputerName = computer.DisplayName;
            }
        }
    }
}
