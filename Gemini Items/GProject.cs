using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CounterSoft.Gemini.Commons;
using CounterSoft.Gemini.Commons.Entity;
using CounterSoft.Gemini.WebServices;

using PureCM.Client;
using PureCM.Server;

using External_Items;

namespace Gemini_Items
{
    /// <summary>
    /// A wrapper class for a Gemini project.
    /// </summary>
    internal class GProject : ExProject
    {
        internal GProject(GFactory oFactory, ProjectEN oGProject)
            : base(oFactory, ExFactory.TPluginType.GeminiProject, Convert.ToUInt32(oGProject.ProjectID))
        {
            m_oGProject = oGProject;
        }

        private GFactory GFactory { get { return Factory as GFactory; } }

        private ProjectEN m_oGProject;

        /// <summary>
        /// The Gemini project item.
        /// </summary>
        public ProjectEN Project
        {
            get { return m_oGProject; }
        }

        /// <summary>
        /// Returns an array of the Gemini issues which have been updated since the last synchronise.
        /// This will include issues which have been added and deleted.
        /// </summary>
        internal override List<ExTask> GetRecentTasks()
        {
            DateTime oLastSyncTime = LastSyncTime;
            IssuesFilterEN oIssueFilter = IssuesFilterEN.CreateProjectFilter(0, m_oGProject.ProjectID);

            oIssueFilter.ExcludeClosed = false;

            if (oLastSyncTime.Ticks > 0)
            {
                oIssueFilter.RevisedAfter = oLastSyncTime.ToString();
            }

            IssueEN[] aoGIssues = GFactory.ServiceManager.IssuesService.GetFilteredIssues(oIssueFilter);

            List<ExTask> aoIssues = new List<ExTask>();

            foreach (IssueEN oGIssue in aoGIssues)
            {
                // Gemini only checks the revised after day, so do the compare to check for hours & minutes
                if (oGIssue.DateRevised.CompareTo(oLastSyncTime) > 0)
                {
                    aoIssues.Add(new GTask(this, oGIssue));
                }
            }
            return aoIssues;
        }

        /// <summary>
        /// When were the tasks last synchronized?
        /// </summary>
        internal override DateTime GetLastSyncTime()
        {
            GFactory oFactory = Factory as GFactory;
            UInt32 nYear = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeYear), ID);

            if (nYear > 0)
            {
                UInt32 nMonth = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeMonth), ID);
                UInt32 nDay = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeDay), ID);
                UInt32 nHour = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeHour), ID);
                UInt32 nMinute = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeMinute), ID);
                UInt32 nSecond = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeSecond), ID);

                if ((nMonth > 0) && (nDay > 0))
                {
                    return new DateTime(Convert.ToInt32(nYear), Convert.ToInt32(nMonth), Convert.ToInt32(nDay), Convert.ToInt32(nHour), Convert.ToInt32(nMinute), Convert.ToInt32(nSecond));
                }
                else
                {
                    return new DateTime();
                }
            }

            return new DateTime();
        }

        /// <summary>
        /// Set when the tasks last synchronized
        /// </summary>
        internal override void SetLastSyncTime(DateTime oSyncTime)
        {
            GFactory oFactory = Factory as GFactory;

            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeYear), ID, Convert.ToUInt32(oSyncTime.Year));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeMonth), ID, Convert.ToUInt32(oSyncTime.Month));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeDay), ID, Convert.ToUInt32(oSyncTime.Day));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeHour), ID, Convert.ToUInt32(oSyncTime.Hour));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeMinute), ID, Convert.ToUInt32(oSyncTime.Minute));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeSecond), ID, Convert.ToUInt32(oSyncTime.Second));
        }

        /// <summary>
        /// Should this project be synchronized with the other system?
        /// </summary>
        internal override bool ShouldInclude()
        {
            return !m_oGProject.ProjectArchived;
        }

        /// <summary>
        /// Return the Gemini short name for the issue
        /// </summary>
        internal override String GetName()
        {
            return m_oGProject.ProjectName;
        }

        /// <summary>
        /// Return the Gemini long description for the issue
        /// </summary>
        internal override String GetDescription()
        {
            return m_oGProject.ProjectDesc;
        }
    }
}
