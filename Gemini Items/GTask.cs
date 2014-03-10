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
    /// A Gemini issue.
    /// </summary>
    internal class GTask : ExTask
    {
        internal GTask(GProject oProject, IssueEN oGIssue)
            : base(oProject, ExFactory.TPluginType.GeminiTask, Convert.ToUInt32(oGIssue.IssueID))
        {
            m_oGIssue = oGIssue;
        }

        private GFactory GFactory { get { return Factory as GFactory; } }

        private IssueEN m_oGIssue;

        /// <summary>
        /// The last time this task was synced with PureCM
        /// </summary>
        internal DateTime LastSyncTime { get { return GetLastSyncTime(); } set { SetLastSyncTime(value); } }

        /// <summary>
        /// Return the owner of the issue, or null if it has no owner. This is the equivalent of
        /// 'Assigned To' in Gemini.
        /// </summary>
        internal override ExUser GetOwner()
        {
            if (m_oGIssue.AssignedToUsername.Length > 0)
            {
                int nGUserID = 0;

                if (Int32.TryParse(m_oGIssue.AssignedTo, out nGUserID) && (nGUserID > 0))
                {
                    return GFactory.GetUser(nGUserID);
                }
            }

            return null;
        }

        /// <summary>
        /// Set the owner of the task, or null if it has no owner.
        /// </summary>
        internal override void SetOwner(ExUser oOwner)
        {
            GUser oGOwner = oOwner as GUser;

            if (oGOwner != null)
            {
                IssueResourceEN[] aoResources = new IssueResourceEN[1];

                aoResources[0] = new IssueResourceEN();

                aoResources[0].User = oGOwner.UserEN;
                aoResources[0].UserID = Convert.ToInt32(oGOwner.ID);

                m_oGIssue.IssueResources = aoResources;
            }
            else
            {
                if (oOwner != null)
                {
                    Factory.Plugin.LogWarning("Failed to update Gemini task '" + Name + "' owner to be '" + oOwner.Name + "'. The owner object is the wrong type.");
                }

                IssueResourceEN[] aoResources = new IssueResourceEN[0];
                m_oGIssue.IssueResources = aoResources;
            }

            m_oGIssue = GFactory.ServiceManager.IssuesService.UpdateIssue(m_oGIssue);
        }

        /// <summary>
        /// Set the project of the task (version will be null so it will go into the backlog)
        /// </summary>
        internal override void SetProject(ExProject oProject)
        {
            IssueComponentEN[] aoComponents = new IssueComponentEN[0];

            GFactory.ServiceManager.IssuesService.MoveIssue(m_oGIssue.IssueID, Convert.ToInt32(oProject.ID), aoComponents, false);

            m_oGIssue = GFactory.ServiceManager.IssuesService.GetIssue(m_oGIssue.IssueID);
        }

        /// <summary>
        /// Return the version in which the issue will be fixed, or null if it has no version. This is the equivalent of
        /// 'Fixed In' in Gemini.
        /// </summary>
        internal override ExVersion GetVersion()
        {
            if (m_oGIssue.FixedInVersion > 0)
            {
                return GFactory.GetVersion(m_oGIssue.ProjectID, m_oGIssue.FixedInVersion);
            }

            return null;
        }

        /// <summary>
        /// Set the version in which the task will be fixed, or null if it has no version.
        /// </summary>
        internal override void SetVersion(ExVersion oVersion)
        {
            if ( oVersion != null )
            {
                m_oGIssue.FixedInVersion = Convert.ToInt32(oVersion.ID);
            }
            else
            {
                m_oGIssue.FixedInVersion = 0;
            }

            m_oGIssue = GFactory.ServiceManager.IssuesService.UpdateIssue(m_oGIssue);
        }

        /// <summary>
        /// Get the parent issue for the issue
        /// </summary>
        internal override ExTask GetParentTask()
        {
            int nParentID = m_oGIssue.ParentIssueID.GetValueOrDefault(0);

            while (nParentID > 0)
            {
                IssueEN tParentIssue = GFactory.ServiceManager.IssuesService.GetIssue(nParentID);

                if (tParentIssue == null)
                    break;

                ExTask oParentTask = GFactory.CreateTask(Project, tParentIssue);

                return oParentTask;
            }
            return null;
        }

        /// <summary>
        /// Set the parent issue for the issue
        /// </summary>
        internal override void SetParentTask(ExTask oTaskParam)
        {
            int nOldParentID = m_oGIssue.ParentIssueID.GetValueOrDefault(0);
            int nNewParentID = 0;

            if (oTaskParam != null)
            {
                GTask oParentTask = (GTask)oTaskParam;

                nNewParentID = oParentTask.m_oGIssue.IssueID;
            }

            if (nOldParentID != nNewParentID)
            {
                //Update the issue parent id
                m_oGIssue.ParentIssueID = nNewParentID;
                m_oGIssue = GFactory.ServiceManager.IssuesService.UpdateIssue(m_oGIssue);
            }
        }

        /// <summary>
        /// Is this issue a feature?
        /// </summary>
        internal override bool IsFeature()
        {
            if (GFactory.FeatureCreationType != null)
            {
                return m_oGIssue.IssueType == GFactory.FeatureCreationType.ID;
            }

            return false;
        }

        /// <summary>
        /// Return the name of the issue. This is the equivalent of the Gemini issue summary.
        /// </summary>
        internal override String GetName()
        {
            return m_oGIssue.IssueSummary;
        }

        /// <summary>
        /// Set the name of the task.
        /// </summary>
        internal override void SetName(String strName)
        {
            m_oGIssue.IssueSummary = strName;

            m_oGIssue = GFactory.ServiceManager.IssuesService.UpdateIssue(m_oGIssue);
        }

        /// <summary>
        /// Return the Gemini long description for the issue
        /// </summary>
        internal override String GetDescription()
        {
            return m_oGIssue.IssueLongDesc;
        }

        /// <summary>
        /// Set the long description of the task.
        /// </summary>
        internal override void SetDescription(String strDescription)
        {
            m_oGIssue.IssueLongDesc = strDescription;

            m_oGIssue = GFactory.ServiceManager.IssuesService.UpdateIssue(m_oGIssue);
        }

        /// <summary>
        /// What state is this issue in?
        /// </summary>
        internal override SDK.TStreamDataState GetState()
        {
            IssueResolutionEN oIssueResolution = GFactory.ServiceManager.AdminService.GetIssueResolution(m_oGIssue.IssueResolution);

            if (oIssueResolution != null)
            {
                if (!oIssueResolution.IsFinal)
                {
                    return SDK.TStreamDataState.Open;
                }
            }

            return SDK.TStreamDataState.Closed;
        }

        /// <summary>
        /// Do we need to update the state for this task?
        /// </summary>
        internal override bool StateNeedsUpdating(SDK.TStreamDataState nSyncState)
        {
            SDK.TStreamDataState nState = State;

            if (nState == nSyncState)
            {
                return false;
            }
            else
            {
                return ((nState == SDK.TStreamDataState.Open) || (nSyncState == SDK.TStreamDataState.Open));
            }
        }

        /// <summary>
        /// Set the state for the task?
        /// </summary>
        internal override void SetState(SDK.TStreamDataState tState)
        {
            IssueResolutionEN[] aoIssueResolutions = GFactory.ServiceManager.AdminService.GetIssueResolution();

            switch( tState )
            {
                case SDK.TStreamDataState.Open:
                    {
                        // Use the first resolution which is not final
                        foreach(IssueResolutionEN oResolution in aoIssueResolutions)
                        {
                            if (!oResolution.IsFinal)
                            {
                                m_oGIssue.IssueResolution = oResolution.ResolutionID;
                                m_oGIssue = GFactory.ServiceManager.IssuesService.UpdateIssue(m_oGIssue);
                                return;
                            }
                        }
                        Factory.Plugin.LogWarning("Failed to update Gemini task '" + Name + "' state to be open. All the Gemini issue resolutions are final.");
                    }
                    break;
                case SDK.TStreamDataState.Completed:
                case SDK.TStreamDataState.Closed:
                case SDK.TStreamDataState.Rejected:
                    {
                        // Use the last resolution which is final
                        for( int nIdx = aoIssueResolutions.Length - 1; nIdx >= 0; nIdx-- )
                        {
                            if (aoIssueResolutions[nIdx].IsFinal)
                            {
                                m_oGIssue.IssueResolution = aoIssueResolutions[nIdx].ResolutionID;
                                m_oGIssue = GFactory.ServiceManager.IssuesService.UpdateIssue(m_oGIssue);
                                return;
                            }
                        }
                        Factory.Plugin.LogWarning("Failed to update Gemini task '" + Name + "' state to be completed, closed or rejected. None of the Gemini issue resolutions are final.");
                    }
                    break;
                case SDK.TStreamDataState.Unknown:
                    Factory.Plugin.LogWarning("Failed to update Gemini task '" + Name + "' state. Unknown state.");
                    break;
            }
        }

        /// <summary>
        /// Do we need to update the priority for this task?
        /// </summary>
        internal override bool PriorityNeedsUpdating(UInt16 nSyncPriority)
        {
            UInt16 nPriority = Priority;

            if (nPriority == nSyncPriority)
            {
                return false;
            }
            else
            {
                int nTotal = GFactory.ServiceManager.AdminService.GetIssuePriority().Length;

                if (nSyncPriority > nTotal)
                {
                    return (nPriority != nTotal);
                }
                else
                {
                    return true;
                }
            }
        }

        /// <summary>
        /// What is the priority of the issue? (1 is high, 5 is low)
        /// </summary>
        internal override UInt16 GetPriority()
        {
            IssuePriorityEN oIssuePriority = GFactory.ServiceManager.AdminService.GetIssuePriority(m_oGIssue.IssuePriority);

            if (oIssuePriority != null)
            {
                int nOrder = oIssuePriority.Order;
                int nCount = GFactory.ServiceManager.ProjectsService.GetPriorities(m_oGIssue.ProjectID).Length;

                // Unfortunately the priorities in Gemini are reversed (5 is high, 1 is low), so switch it
                if ((nCount - nOrder) >= 0)
                {
                    return System.Convert.ToUInt16(nCount - nOrder + 1);
                }
            }

            return 3; // 3 is average priority
        }

        /// <summary>
        /// Set the priority of the task? (1 is high, 5 is low)
        /// </summary>
        internal override void SetPriority(UInt16 nPriority)
        {
            IssuePriorityEN tPriority = GFactory.GetPriorityFromPCMPriority(m_oGIssue.ProjectID, nPriority);

            if (tPriority != null)
            {
                m_oGIssue.IssuePriority = tPriority.ID;
                m_oGIssue = GFactory.ServiceManager.IssuesService.UpdateIssue(m_oGIssue);
            }
            else
            {
                Factory.Plugin.LogWarning("Failed to update Gemini task '" + Name + "' priority. Failed to get a priority object.");
            }
        }

        /// <summary>
        /// What is the URL of the issue?
        /// </summary>
        internal override String GetUrl()
        {
            if (GFactory.Options.UpdateURL)
            {
                return GFactory.Options.GURL + "/Default.aspx?id=" + m_oGIssue.IssueID;
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Set the URL of the task?
        /// </summary>
        internal override void SetUrl(String strUrl)
        {
            // Doesn't make sense to do this in Gemini
        }

        /// <summary>
        /// When was this task last synced?
        /// </summary>
        private DateTime GetLastSyncTime()
        {
            GFactory oFactory = Factory as GFactory;
            UInt32 nYear = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskYear), ID);

            if (nYear > 0)
            {
                UInt32 nMonth = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskMonth), ID);
                UInt32 nDay = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskDay), ID);
                UInt32 nHour = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskHour), ID);
                UInt32 nMinute = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskMinute), ID);
                UInt32 nSecond = oFactory.PCMRepository.GetPluginIDFromPureCMID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskSecond), ID);

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
        /// Set the last time this task was synced
        /// </summary>
        private void SetLastSyncTime(DateTime oSyncTime)
        {
            GFactory oFactory = Factory as GFactory;

            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskYear), ID, Convert.ToUInt32(oSyncTime.Year));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskMonth), ID, Convert.ToUInt32(oSyncTime.Month));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskDay), ID, Convert.ToUInt32(oSyncTime.Day));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskHour), ID, Convert.ToUInt32(oSyncTime.Hour));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskMinute), ID, Convert.ToUInt32(oSyncTime.Minute));
            oFactory.PCMRepository.SetPureCMPluginID(Convert.ToUInt32(ExFactory.TPluginSyncType.GeminiSyncTimeTaskSecond), ID, Convert.ToUInt32(oSyncTime.Second));
        }

        /// <summary>
        /// This is called if a Gemini task has been updated as a result of PureCM changes. This is called after
        /// all updates have been applied
        /// </summary>
        internal override void OnSyncComplete()
        {
            // Store the date revised time so that we know to ignore this in ShouldInclude
            LastSyncTime = m_oGIssue.DateRevised;
        }

        /// <summary>
        /// Should the Gemini task be synchronized with PureCM
        /// </summary>
        internal override bool ShouldInclude()
        {
            if (base.ShouldInclude())
            {
                // We don't want to synchronize changes made by PureCM back into PureCM - so we need to check the LastSyncTime
                // is before the Gemini issue modification time
                DateTime oLastSyncTime = LastSyncTime;

                if (oLastSyncTime.Year > 0)
                {
                    if (oLastSyncTime.CompareTo(m_oGIssue.DateRevised) >= 0)
                    {
                        Factory.Plugin.Trace("Not syncing '" + Name + "' because the change was made in Gemini.");
                        return false;
                    }
                }

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
