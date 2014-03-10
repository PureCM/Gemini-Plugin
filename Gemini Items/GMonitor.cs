using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using External_Items;
using Gemini_Items;

namespace Gemini_Items
{
    /// <summary>
    /// A class for monitoring the Gemini system to check for changes and synchronize with PureCM.
    /// </summary>
    internal class GMonitor : ExMonitor
    {
        internal GMonitor(GFactory oFactory, bool bForceSync)
            : base(oFactory, bForceSync)
        {
        }
    }
}
