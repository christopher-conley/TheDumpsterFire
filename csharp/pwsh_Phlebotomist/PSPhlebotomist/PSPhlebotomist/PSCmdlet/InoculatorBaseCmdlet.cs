using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Management.Automation;
using System.Text;

namespace PSPhlebotomist.PSCmdlets
{
    internal class InoculatorBaseCmdlet : Cmdlet
    {

        private readonly ILogger<InoculatorBaseCmdlet> _logger;

        public InoculatorBaseCmdlet(ILogger<InoculatorBaseCmdlet> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

    }
}
