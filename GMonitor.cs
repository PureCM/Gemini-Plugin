using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Plugin_Gemini.Gemini_Items;
using Plugin_Gemini.PureCM_Items;

using PureCM.Client;
using PureCM.Server;

using CounterSoft.Gemini.Commons;
using CounterSoft.Gemini.Commons.Entity;
using CounterSoft.Gemini.WebServices;

namespace Plugin_Gemini
{
    /// <summary>
    /// A class for monitoring the Gemini system to check for changes and synchronize with PureCM.
    /// </summary>
    class GMonitor
    {
        public GMonitor(ServiceManager oServiceManager, Repository oPRepos, Options oOptions)
        {
            m_aoGFactories = GFactory.GetFactories(oServiceManager, oPRepos, oOptions);
        }

        /// <summary>
        /// Check for any updates to Gemini issues since the last check and synchronize the changes
        /// with PureCM if necessary.
        /// </summary>
        public void CheckForUpdates()
        {
            PluginManager.Trace("Checking for Gemini updates");

            foreach (GFactory oFactory in m_aoGFactories)
            {
                if (oFactory.Project.Include)
                {
                    System.DateTime oNow = System.DateTime.Now;

                    GIssue[] aoIssues = oFactory.Project.GetRecentIssues( );

                    foreach (GIssue oIssue in aoIssues)
                    {
                        UInt32 nPTaskID = oIssue.PureCMID;

                        if (nPTaskID == 0)
                        {
                            ProcessNewIssue(oIssue);
                        }
                        else
                        {
                            ProcessExistingIssue(oIssue);
                        }
                    }

                    oFactory.Project.SetLastSyncTime(oNow);
                }
                else
                {
                    PluginManager.Trace("Gemini project '" + oFactory.Project.Name + "' will not be syncrhonized with PureCM. It is marked as exlcuded.");
                }
            }
            PluginManager.Trace("Finished checking for Gemini updates");
        }

        private void ProcessNewIssue( GIssue oIssue )
        {
            // Check if we need to create a PureCM task for this issue
            if (oIssue.Include)
            {
                PProject oPProject = oIssue.Factory.Project.GetPProject(true);

                if (oPProject != null)
                {
                    PFactory oPFactory = oPProject.PFactory;
                    GUser oUser = oIssue.Owner;

                    if (oUser != null)
                    {
                        if (oUser.Include)
                        {
                            PUser oPUser = oUser.GetPUser(oPFactory, true);

                            if (oPUser != null)
                            {
                                GVersion oVersion = oIssue.Version;

                                if (oVersion != null)
                                {
                                    PVersion oPVersion = oVersion.GetPVersion(oPFactory, true);

                                    if (oPVersion != null)
                                    {

                                        PTask oPTask = null;
                                        PFeature oPFeature = null;
                                        
                                        if (oPFactory.CreateTask(oIssue, oPUser, oPVersion, ref oPFeature, ref oPTask))
                                        {
                                            PluginManager.Trace("Created PureCM tasks for Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + "). The PureCM task has id '" + oPTask.ID + "', was created in version '" + oPVersion.Item.Name + "' and is owned by '" + oPUser.Name + "'.");
                                        }
                                        else
                                        {
                                            PluginManager.Trace("Failed to create PureCM tasks for Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + "). Using version '" + oPVersion.Item.Name + "' and is owned by '" + oPUser.Name + "'.");
                                        }
                                    }
                                    else
                                    {
                                        PluginManager.Trace("Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + ") will not be synchronized with PureCM. Could not find a PureCM version for Gemini version '" + oVersion.Name + "'.");
                                    }
                                }
                                else
                                {
                                    PluginManager.Trace("Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + ") will not be synchronized with PureCM. It does not have a 'Fixed In' version.");
                                }
                            }
                            else
                            {
                                PluginManager.Trace("Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + ") will not be synchronized with PureCM. Failed to get PureCM user for Gemini user '" + oUser.Name + "'.");
                            }
                        }
                        else
                        {
                            PluginManager.Trace("Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + ") will not be synchronized with PureCM. The user '" + oUser.Name + "' is marked as excluded.");
                        }
                    }
                    else
                    {
                        PluginManager.Trace("Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + ") will not be synchronized with PureCM. It is not assigned to anyone.");
                    }
                }
                else
                {
                    PluginManager.Trace("Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + ") could not be synchronized with PureCM. Unable to get a PureCM project.");
                }
            }
            else
            {
                PluginManager.Trace("Gemini issue '" + oIssue.Summary + "' (" + oIssue.ID + ") will not be syncrhonized with PureCM. It is marked as excluded.");
            }
        }

        private void ProcessExistingIssue(GIssue oIssue)
        {
            if (oIssue.Include)
            {
                oIssue.SyncWithPureCM();
            }
            else
            {
                // TODO : Need to deactive the PureCM Issue
            }
        }

        private GFactory[] m_aoGFactories;
    }
}
