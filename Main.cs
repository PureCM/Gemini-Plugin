using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Mail;
using System.Xml.Linq;
using System.IO;
using System.Security;
using System.Text.RegularExpressions;
using System.Diagnostics;

using Gemini_Items;
using PureCM_Items;

using PureCM.Server;
using PureCM.Client;

using CounterSoft.Gemini.Commons;
using CounterSoft.Gemini.Commons.Entity;
using CounterSoft.Gemini.WebServices;

namespace Plugin_Gemini
{
    [EventHandlerDescription("Plugin that integrates with the Gemini Project Management System")]
    public class GeminiPlugin : PureCM.Server.Plugin
    {
        public override bool OnStart(XElement oXMLConfig, Connection oConnection)
        {
            // This is for debugging so we can attach to the process
            // System.Threading.Thread.Sleep(30000);

            m_oOptions = new GOptions(oXMLConfig);

            m_bForceGeminiSync = m_oOptions.ForceGeminiSync;
            m_bForcePureCMSync = m_oOptions.ForcePureCMSync;

            // Check that we can connect to Gemini and that it has at least one project
            ServiceManager oGServiceManager = new ServiceManager(m_oOptions.GURL, m_oOptions.GUser, m_oOptions.GPassword, m_oOptions.GAPIKey, false);
            ProjectEN[] aoGProjects = oGServiceManager.ProjectsService.GetProjects();

            if (aoGProjects.Length <= 0)
            {
                LogWarning("Unable to integrate with Gemini. Gemini does not contain any projects.");
                return false;
            }

            m_oRepos = oConnection.Repositories.ByName(m_oOptions.PRepository);

            if (m_oRepos == null)
            {
                LogWarning("Unable to integrate with Gemini. PureCM repository '" + m_oOptions.PRepository + "' does not exist.");
                return false;
            }

            oConnection.OnIdle = OnIdle;
            oConnection.OnChangeSubmitted = OnChangeSubmitted;
            oConnection.OnStreamCreated = OnStreamCreated;

            return true;
        }

        public override void OnStop()
        {
        }

        private void OnStreamCreated(StreamCreatedEvent evt)
        {
            if (evt.Repository != null)
            {
                evt.Repository.RefreshStreams();
            }
        }

        private void OnChangeSubmitted(ChangeSubmittedEvent oEvent)
        {
            Changeset oChangeset = oEvent.Changeset;

            if (oChangeset == null)
            {
                if (oEvent.Stream == null)
                {
                    LogError("Failed to process changeset '" + oEvent.ChangeID + "' in Gemini. The stream object is null.");
                }
                else
                {
                    LogError("Failed to process changeset '" + oEvent.ChangeID + "' in Gemini. The changeset object is null.");
                }
                return;
            }

            try
            {
                ServiceManager oGServiceManager = new ServiceManager(m_oOptions.GURL, m_oOptions.GUser, m_oOptions.GPassword, m_oOptions.GAPIKey, false);
                bool bSynchronized = false;

                foreach (ProjectItem oItem in oChangeset.LinkedProjectItems)
                {
                    UInt32 nGeminiIssueID = oChangeset.Stream.Repository.GetPluginIDFromPureCMID(System.Convert.ToUInt32(External_Items.ExFactory.TPluginType.GeminiTask), oItem.Id);

                    if (nGeminiIssueID > 0)
                    {
                        if (!bSynchronized)
                        {
                            Synchronize();
                        }

                        UpdateGeminiIssueOnSubmit(oGServiceManager, (int)nGeminiIssueID, oChangeset, oItem);
                    }
                }
            }
            catch (Exception oException)
            {
                LogError("Failed to process changeset in Gemini (" + oException + ").");
            }
        }

        private void UpdateGeminiIssueOnSubmit(ServiceManager oGServiceManager, int nGeminiIssueID, Changeset oChangeset, ProjectItem oProjectItem)
        {
            string strChangeItems = "";

            foreach (ChangeItem oItem in oChangeset.Items)
            {
                SourceControlEN oFileLink = new SourceControlEN();
                oFileLink.DateCreated = DateTime.Now;
                oFileLink.SourceControlRepository = oChangeset.Stream.StreamPath;
                oFileLink.IssueID = nGeminiIssueID;
                oFileLink.FileName = Path.GetFileName(oItem.Path);
                oFileLink.FilePath = oItem.Path;

                oGServiceManager.IssuesService.CreateSourceControlFile(nGeminiIssueID, oFileLink);

                switch (oItem.Type)
                {
                    case SDK.TPCMChangeItemType.pcmEdit:
                        if (oItem.RenamePath.Length > 0)
                        {
                            strChangeItems += "Renamed '" + oItem.Path + "' to '" + oItem.RenamePath + "'<br/>";
                        }
                        else
                        {
                            strChangeItems += "Edited '" + oItem.Path + "'<br/>";
                        }
                        break;
                    case SDK.TPCMChangeItemType.pcmAdd:
                        strChangeItems += "Added '" + oItem.Path + "'<br/>";
                        break;
                    case SDK.TPCMChangeItemType.pcmDelete:
                        strChangeItems += "Deleted '" + oItem.Path + "'<br/>";
                        break;
                }
            }

            IssueEN oGeminiIssue = oGServiceManager.IssuesService.GetIssue(nGeminiIssueID);
            IssueCommentEN oGeminiComment = new IssueCommentEN();

            oGeminiComment.ProjectID = oGeminiIssue.ProjectID;
            oGeminiComment.IssueID = oGeminiIssue.IssueID;
            oGeminiComment.UserID = oGServiceManager.UsersService.WhoAmI().UserID;
            oGeminiComment.Comment = GetComment(oChangeset, strChangeItems);
            oGeminiComment.DateCreated = DateTime.Now;

            oGServiceManager.IssuesService.CreateComment(oGeminiIssue.IssueID, oGeminiComment);

            // We don't want to synchronize this change back to PureCM the next time we sync so set
            // the lat sync time for the Gemini issue
            {
                oGeminiIssue = oGServiceManager.IssuesService.GetIssue(nGeminiIssueID);

                oChangeset.Stream.Repository.SetPureCMPluginID(Convert.ToUInt32(External_Items.ExFactory.TPluginSyncType.GeminiSyncTimeTaskYear), (UInt32)nGeminiIssueID, Convert.ToUInt32(oGeminiIssue.DateRevised.Year));
                oChangeset.Stream.Repository.SetPureCMPluginID(Convert.ToUInt32(External_Items.ExFactory.TPluginSyncType.GeminiSyncTimeTaskMonth), (UInt32)nGeminiIssueID, Convert.ToUInt32(oGeminiIssue.DateRevised.Month));
                oChangeset.Stream.Repository.SetPureCMPluginID(Convert.ToUInt32(External_Items.ExFactory.TPluginSyncType.GeminiSyncTimeTaskDay), (UInt32)nGeminiIssueID, Convert.ToUInt32(oGeminiIssue.DateRevised.Day));
                oChangeset.Stream.Repository.SetPureCMPluginID(Convert.ToUInt32(External_Items.ExFactory.TPluginSyncType.GeminiSyncTimeTaskHour), (UInt32)nGeminiIssueID, Convert.ToUInt32(oGeminiIssue.DateRevised.Hour));
                oChangeset.Stream.Repository.SetPureCMPluginID(Convert.ToUInt32(External_Items.ExFactory.TPluginSyncType.GeminiSyncTimeTaskMinute), (UInt32)nGeminiIssueID, Convert.ToUInt32(oGeminiIssue.DateRevised.Minute));
                oChangeset.Stream.Repository.SetPureCMPluginID(Convert.ToUInt32(External_Items.ExFactory.TPluginSyncType.GeminiSyncTimeTaskSecond), (UInt32)nGeminiIssueID, Convert.ToUInt32(oGeminiIssue.DateRevised.Second));
            }
        }

        private static string GetComment(Changeset oChangeset, string strChangeItems)
        {
            return string.Format("Change '{0}' submitted by '{1}'.{2}{2}Description:{2}{3}{2}{2}Change Items:{2}{4}",
                                 oChangeset.IdString,
                                 oChangeset.ClientName,
                                 "<br/>",
                                 oChangeset.Description,
                                 strChangeItems);
        }

        private void OnIdle()
        {
            if (!m_oStopwatch.IsRunning || (m_oStopwatch.ElapsedMilliseconds > m_oOptions.Interval * 1000))
            {
                Synchronize();
            }
        }

        private void Synchronize()
        {
            try
            {
                ServiceManager oGServiceManager = new ServiceManager(m_oOptions.GURL, m_oOptions.GUser, m_oOptions.GPassword, m_oOptions.GAPIKey, false);
                GFactory oGFactory = new GFactory(this, oGServiceManager, m_oRepos, m_oOptions);
                PCMFactory oPCMFactory = new PCMFactory(this, External_Items.ExFactory.TPluginType.GeminiProject, External_Items.ExFactory.TPluginSyncType.PureCMGeminiSyncTimeYear, m_oRepos, m_oOptions.GTaskCreate, true);

                oGFactory.SyncFactory = oPCMFactory;
                oPCMFactory.SyncFactory = oGFactory;

                GMonitor oGMonitor = new GMonitor(oGFactory, m_bForceGeminiSync);

                oGMonitor.CheckForUpdates();

                PCMMonitor oPMonitor = new PCMMonitor(oPCMFactory, m_bForcePureCMSync);

                oPMonitor.CheckForUpdates();

                m_bForceGeminiSync = false;
                m_bForcePureCMSync = false;

                m_oStopwatch.Reset();
                m_oStopwatch.Start();

                GC.Collect();
                GC.WaitForPendingFinalizers();
            }
            catch (Exception e)
            {
                LogError("Unhandled exception (" + e.Message + ").");
            }
        }

        private Repository m_oRepos;
        private GOptions m_oOptions;
        private bool m_bForceGeminiSync = false;
        private bool m_bForcePureCMSync = false;

        private Stopwatch m_oStopwatch = new Stopwatch();
    }
}
