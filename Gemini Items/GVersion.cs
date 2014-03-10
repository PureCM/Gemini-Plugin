using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CounterSoft.Gemini.Commons;
using CounterSoft.Gemini.Commons.Entity;
using CounterSoft.Gemini.WebServices;

using PureCM.Client;

using External_Items;

namespace Gemini_Items
{
    /// <summary>
    /// Wrapper class for a Gemini version.
    /// </summary>
    internal class GVersion : ExVersion
    {
        internal GVersion(GProject oProject, VersionEN oGVersion)
            : base(oProject, ExFactory.TPluginType.GeminiVersion, System.Convert.ToUInt32(oGVersion.VersionID))
        {
            m_oGVersion = oGVersion;
        }

        private GFactory GFactory { get { return Factory as GFactory; } }

        private VersionEN m_oGVersion;

        /// <summary>
        // Returns the Gemini name of the version.
        /// </summary>
        internal override String GetName()
        {
            return m_oGVersion.VersionName;
        }

        /// <summary>
        // Returns the Gemini description of the version.
        /// </summary>
        internal override String GetDescription()
        {
            return m_oGVersion.VersionDesc;
        }

        /// <summary>
        /// Who owns this Gemini version?
        /// </summary>
        internal override ExUser GetOwner()
        {
            return null;
        }

        /// <summary>
        /// What is the parent version for this version?
        /// </summary>
        internal override ExVersion GetVersion()
        {
            if (m_oGVersion.ParentVersionId.HasValue && m_oGVersion.ParentVersionId.Value != 0)
            {
                return GFactory.GetVersion(m_oGVersion.ProjectID, m_oGVersion.ParentVersionId.Value);
            }

            return null;
        }
    }
}
