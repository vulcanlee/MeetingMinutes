using Microsoft.Extensions.Options;
using ProjectAssistant.DataModel.Models.Configurations;
using System;
using System.Collections.Generic;
using System.Text;

namespace ProjectAssistant.Business.Services.Options;

public class SystemSettingsConfigurationService
{
    private readonly SystemSettings Value;

    public SystemSettingsConfigurationService(IOptions<SystemSettings> options)
    {
        this.Value = options.Value;
    }
}
