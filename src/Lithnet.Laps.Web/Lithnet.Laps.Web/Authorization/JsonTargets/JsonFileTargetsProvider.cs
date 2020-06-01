﻿using System.Collections.Generic;
using System.IO;
using Lithnet.Laps.Web.AppSettings;
using Microsoft.AspNetCore.Hosting;
using Newtonsoft.Json;

namespace Lithnet.Laps.Web.Authorization
{
    public class JsonFileTargetsProvider : IJsonTargetsProvider
    {
        private readonly IAuthorizationSettings config;

        private readonly IWebHostEnvironment env;

        public JsonFileTargetsProvider(IAuthorizationSettings config, IWebHostEnvironment env)
        {
            this.config = config;
            this.env = env;
        }

        private IList<JsonTarget> targets;

        public IList<JsonTarget> Targets
        {
            get
            {
                if (targets == null)
                {
                    if (this.config.JsonProviderEnabled)
                    {
                        string path = Path.Combine(this.env.ContentRootPath, this.config.JsonAuthorizationFile); 
                        if (!File.Exists(path))
                        {
                            throw new FileNotFoundException($"The JSON authorization file was not found: {path}");
                        }

                        var targetFile = JsonConvert.DeserializeObject<JsonTargets>(File.ReadAllText(path));

                        this.targets = targetFile?.Targets ?? new List<JsonTarget>();
                    }
                    else
                    {
                        this.targets = new List<JsonTarget>();
                    }
                }

                return targets;
            }
        }
    }
}