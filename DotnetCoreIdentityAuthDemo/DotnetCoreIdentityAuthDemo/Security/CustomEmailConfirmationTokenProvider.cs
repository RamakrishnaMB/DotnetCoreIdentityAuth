using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DotnetCoreIdentityAuthDemo.Security
{

    public class CustomEmailConfirmationTokenProvider<TUser> : DataProtectorTokenProvider<TUser> where TUser : class
    {
        public CustomEmailConfirmationTokenProvider(IDataProtectionProvider dataProtectionProvider, IOptions<CustomEmailConfirmationTokenProviderOptions> custmOptions, ILogger<DataProtectorTokenProvider<TUser>> logger) : base(dataProtectionProvider, custmOptions, logger)
        {

        }
    }
}
