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
    /// Class for creating Gemini objects. Each Gemini project has a factory.
    /// </summary>
    class GFactory : ExFactory
    {
        internal GFactory(PureCM.Server.Plugin oPlugin, ServiceManager oServiceManager, Repository oPRepos, GOptions oOptions)
            : base( "Gemini", oPlugin )
        {
            m_oServiceManager = oServiceManager;
            m_oPRepos = oPRepos;
            m_oOptions = oOptions;

            if (m_oOptions.GProjectCreate)
            {
                ProjectEN[] atProjects = m_oServiceManager.ProjectsService.GetProjects();

                if (atProjects.Length > 0)
                {
                    if (m_oOptions.GProjectCreateTemplate.Length > 0)
                    {
                        m_tProjectCreationTemplate = null;

                        foreach (ProjectEN tProject in atProjects)
                        {
                            if (tProject.ProjectName == m_oOptions.GProjectCreateTemplate)
                            {
                                m_tProjectCreationTemplate = tProject;
                                break;
                            }
                        }

                        if (m_tProjectCreationTemplate == null)
                        {
                            m_oOptions.GProjectCreate = false;
                            Plugin.LogWarning("Disabling Gemini project creation. A Gemini projects with the name '" + m_oOptions.GProjectCreateTemplate + "' does not exist to use as a template.");
                        }
                    }
                    else
                    {
                        m_tProjectCreationTemplate = atProjects[0];
                    }
                }
                else
                {
                    m_oOptions.GProjectCreate = false;
                    Plugin.LogWarning("Disabling Gemini project creation. There are no existing Gemini projects to use as templates.");
                }
            }

            if (m_oOptions.GTaskCreate)
            {
                IssueTypeEN[] atTypes = m_oServiceManager.AdminService.GetIssueType();

                foreach(IssueTypeEN tType in atTypes)
                {
                    if (tType.Description == m_oOptions.GTaskCreateTypeDescription)
                    {
                        m_tTaskCreationType = tType;
                        break;
                    }
                }

                if (m_tTaskCreationType == null)
                {
                    Plugin.LogWarning("Disabling Gemini issue creation. The specified issue creation type '" + m_oOptions.GTaskCreateTypeDescription + "' is not a valid issue type description.");
                    m_oOptions.GTaskCreate = false;
                }
            }

            if (m_oOptions.GTaskCreate)
            {
                IssueStatusEN[] atStatus = m_oServiceManager.AdminService.GetIssueStatus();

                foreach (IssueStatusEN tStatus in atStatus)
                {
                    if (tStatus.Description == m_oOptions.GTaskCreateStatusDescription)
                    {
                        m_tTaskCreationStatus = tStatus;
                        break;
                    }
                }

                if (m_tTaskCreationStatus == null)
                {
                    Plugin.LogWarning("Disabling Gemini issue creation. The specified issue creation status '" + m_oOptions.GTaskCreateStatusDescription + "' is not a valid issue status description.");
                    m_oOptions.GTaskCreate = false;
                }
            }

            if (m_oOptions.GTaskCreate)
            {
                IssueSeverityEN[] atSeverities = m_oServiceManager.AdminService.GetIssueSeverity();

                foreach (IssueSeverityEN tSeverity in atSeverities)
                {
                    if (tSeverity.Description == m_oOptions.GTaskCreateSeverityDescription)
                    {
                        m_tTaskCreationSeverity = tSeverity;
                        break;
                    }
                }

                if (m_tTaskCreationSeverity == null)
                {
                    Plugin.LogWarning("Disabling Gemini issue creation. The specified issue creation severity '" + m_oOptions.GTaskCreateSeverityDescription + "' is not a valid issue severity description.");
                    m_oOptions.GTaskCreate = false;
                }
            }

            if (m_oOptions.GFeatureCreate)
            {
                IssueTypeEN[] atTypes = m_oServiceManager.AdminService.GetIssueType();

                foreach (IssueTypeEN tType in atTypes)
                {
                    if (tType.Description == m_oOptions.GFeatureCreateTypeDescription)
                    {
                        FeatureCreationType = tType;
                        break;
                    }
                }

                if (FeatureCreationType == null)
                {
                    Plugin.LogWarning("Disabling Gemini feature creation. The specified issue creation type '" + m_oOptions.GFeatureCreateTypeDescription + "' is not a valid issue type description.");
                    m_oOptions.GFeatureCreate = false;
                }
            }

            if (m_oOptions.GFeatureCreate)
            {
                IssueStatusEN[] atStatus = m_oServiceManager.AdminService.GetIssueStatus();

                foreach (IssueStatusEN tStatus in atStatus)
                {
                    if (tStatus.Description == m_oOptions.GFeatureCreateStatusDescription)
                    {
                        m_tFeatureCreationStatus = tStatus;
                        break;
                    }
                }

                if (m_tFeatureCreationStatus == null)
                {
                    Plugin.LogWarning("Disabling Gemini feature creation. The specified issue creation status '" + m_oOptions.GFeatureCreateStatusDescription + "' is not a valid issue status description.");
                    m_oOptions.GFeatureCreate = false;
                }
            }

            if (m_oOptions.GFeatureCreate)
            {
                IssueSeverityEN[] atSeverities = m_oServiceManager.AdminService.GetIssueSeverity();

                foreach (IssueSeverityEN tSeverity in atSeverities)
                {
                    if (tSeverity.Description == m_oOptions.GFeatureCreateSeverityDescription)
                    {
                        m_tFeatureCreationSeverity = tSeverity;
                        break;
                    }
                }

                if (m_tFeatureCreationSeverity == null)
                {
                    Plugin.LogWarning("Disabling Gemini feature creation. The specified issue creation severity '" + m_oOptions.GFeatureCreateSeverityDescription + "' is not a valid issue severity description.");
                    m_oOptions.GFeatureCreate = false;
                }
            }
        }

        internal IssuePriorityEN GetPriorityFromPCMPriority(int nProjectID, UInt16 nPCMPriority)
        {
            IssuePriorityEN[] atPriorities = m_oServiceManager.ProjectsService.GetPriorities(nProjectID);
            int nCount = atPriorities.Length;

            // Unfortunately the priorities in Gemini are reversed (5 is high, 1 is low), so switch it
            int nOrder = nCount - (int)nPCMPriority + 1;

            if (nOrder <= 0)
            {
                nOrder = 1;
            }

            foreach (IssuePriorityEN tPriority in atPriorities)
            {
                if (tPriority.Order == nOrder)
                {
                    return tPriority;
                }
            }

            Plugin.LogWarning("Failed to get Gemini priority with order '" + nOrder + "'.");

            if (atPriorities.Length > 0)
            {
                return atPriorities[0];
            }
            else
            {
                return null;
            }
        }

        /// <summary>
        /// Returns the Gemini Service Manager (this is used to view and update the Gemini database)
        /// </summary>
        internal ServiceManager ServiceManager
        {
            get { return m_oServiceManager; }
        }

        /// <summary>
        /// Get the corresponding PureCM repository.
        /// </summary>
        internal Repository PCMRepository
        {
            get { return m_oPRepos; }
        }

        /// <summary>
        /// Get the options.
        /// </summary>
        internal GOptions Options
        {
            get { return m_oOptions; }
        }

        private ServiceManager m_oServiceManager;
        private Repository m_oPRepos;
        private GOptions m_oOptions;
        private List<ExProject> m_aoProjects;

        private ProjectEN m_tProjectCreationTemplate;
        private IssueTypeEN m_tTaskCreationType;
        private IssueStatusEN m_tTaskCreationStatus;
        private IssueSeverityEN m_tTaskCreationSeverity;
        internal IssueTypeEN FeatureCreationType {get; set;}
        private IssueStatusEN m_tFeatureCreationStatus;
        private IssueSeverityEN m_tFeatureCreationSeverity;

        /// <summary>
        /// Get an array of all the Gemini projects
        /// </summary>
        internal override List<ExProject> GetProjects()
        {
            if ( m_aoProjects == null )
            {
                ProjectEN[] aoGProjects = ServiceManager.ProjectsService.GetProjects();
                m_aoProjects = new List<ExProject>();

                foreach (ProjectEN oGProject in aoGProjects)
                {
                    m_aoProjects.Add(new GProject(this, oGProject));
                }
            }

            return m_aoProjects;
        }

        /// <summary>
        /// Get the ID of the project we are synchronized with
        /// </summary>
        internal override UInt32 GetSyncID(TPluginType tType, UInt32 nID)
        {
            return m_oPRepos.GetPureCMIDFromPluginID(System.Convert.ToUInt32(tType), nID);
        }

        /// <summary>
        /// Set the ID of the project we are synchronized with
        /// </summary>
        internal override void SetSyncID(TPluginType tType, UInt32 nID, UInt32 nSyncID)
        {
            m_oPRepos.SetPureCMPluginID(System.Convert.ToUInt32(tType), nSyncID, nID);
        }

        /// <summary>
        /// Get the project from the Gemin project ID. Return null if the project is not synchronized.
        /// </summary>
        internal GProject GetProject(int nID)
        {
            if (nID > 0)
            {
                List<ExProject> aoProjects = GetProjects();

                foreach (ExProject oProject in aoProjects)
                {
                    if (oProject.ID == nID)
                    {
                        return oProject as GProject;
                    }
                }
            }

            return null;
        }

        /// <summary>
        /// Get the project from the synchronized project. Return null if the project is not synchronized.
        /// </summary>
        internal override ExProject GetProject(ExProject oSyncProject)
        {
            UInt32 nID = oSyncProject.SyncID;

            if ( nID > 0 )
            {
                return GetProject(Convert.ToInt32(nID));
            }

            return null;
        }

        /// <summary>
        /// Create a new project and synchronize it with this project
        /// </summary>
        internal override ExProject CreateProject(ExProject oSyncProject)
        {
            if (!oSyncProject.Include)
            {
                Plugin.LogWarning("Failed to create Gemini project '" + oSyncProject.Name + "'. This project is flagged as not to be included.");
                return null;
            }

            ProjectEN tProject = null;

            // If a Gemini project already exists with this name then use that
            {
                ProjectEN[] atProjects = ServiceManager.ProjectsService.GetProjects();

                foreach (ProjectEN tPossibleProject in atProjects)
                {
                    if (tPossibleProject.ProjectName == oSyncProject.Name)
                    {
                        tProject = tPossibleProject;
                        break;
                    }
                }
            }

            if (tProject == null)
            {
                if (!Options.GProjectCreate)
                {
                    Plugin.LogInfo("Failed to create Gemini project '" + oSyncProject.Name + "'. Gemini project creation is disabled.");
                    return null;
                }

                tProject = m_tProjectCreationTemplate;

                tProject.ProjectCode = oSyncProject.Name;
                tProject.ProjectName = oSyncProject.Name;
                tProject.ProjectDesc = oSyncProject.Description;

                // Gemini does not allow you to create projects without a description
                if (tProject.ProjectDesc.Length == 0)
                {
                    tProject.ProjectDesc = oSyncProject.Name;
                }

                try
                {
                    tProject = ServiceManager.ProjectsService.CreateProject(tProject);
                }
                catch (Exception e)
                {
                    Plugin.LogError("Failed to create Gemini project '" + oSyncProject.Name + "'. Exception (" + e.Message + "). Try creating a project with this name in Gemini.");
                    tProject = null;
                }
            }

            if (tProject != null)
            {
                GProject oGProject = new GProject(this, tProject);

                oGProject.SyncID = oSyncProject.ID;

                return oGProject;
            }

            return null;
        }

        /// <summary>
        /// Get all the Gemini versions for this Gemini project
        /// </summary>
        internal override List<ExVersion> GetVersions(ExProject oProject)
        {
            List<ExVersion> aoVersions = new List<ExVersion>();
            GProject oGProject = (GProject)oProject;
            VersionEN[] aoGVersions = ServiceManager.ProjectsService.GetVersions(oGProject.Project.ProjectID);

            foreach(VersionEN oGVersion in aoGVersions)
            {
                aoVersions.Add(GetVersion(oGProject,oGVersion));
            }

            return aoVersions;
        }

        /// <summary>
        /// Get the version from the Gemini version ID. Return null if the version is not synchronized.
        /// </summary>
        internal GVersion GetVersion(int nProjectID, int nID)
        {
            if ( (nProjectID > 0 ) && (nID > 0) )
            {
                GProject oProject = GetProject(nProjectID);

                if (oProject != null)
                {
                    VersionEN oGVersion = ServiceManager.ProjectsService.GetVersion(nProjectID, nID);

                    if (oGVersion != null)
                    {
                        return GetVersion(oProject, oGVersion);
                    }
                }
            }

            return null;
        }

        private static GVersion GetVersion(GProject oProject, VersionEN oGVersion)
        {
            return new GVersion(oProject, oGVersion);
        }

        /// <summary>
        /// Get the version from the synchronized version. Return null if the version is not synchronized.
        /// </summary>
        internal override ExVersion GetVersion(ExVersion oSyncVersion)
        {
            ExProject oProject = oSyncVersion.Project.GetSyncProject(false);

            if (oProject != null)
            {
                UInt32 nID = oSyncVersion.SyncID;

                if (nID > 0)
                {
                    return GetVersion(Convert.ToInt32(oProject.ID), Convert.ToInt32(nID));
                }
            }

            return null;
        }

        /// <summary>
        /// Create a new version and synchronize it with this version
        /// </summary>
        internal override ExVersion CreateVersion(ExVersion oSyncVersion)
        {
            if (!oSyncVersion.Include)
            {
                Plugin.LogWarning("Failed to create Gemini version '" + oSyncVersion.Name + "'. This version is flagged as not to be included.");
                return null;
            }

            ExProject tProject = oSyncVersion.Project.GetSyncProject(true);

            if (tProject != null)
            {
                VersionEN tVersion = null;

                // If a Gemini version already exists with this name then use that
                {
                    VersionEN[] atVersions = ServiceManager.ProjectsService.GetVersions(Convert.ToInt32(tProject.ID));

                    foreach (VersionEN tPossibleVersion in atVersions)
                    {
                        if (tPossibleVersion.VersionName == oSyncVersion.Name)
                        {
                            tVersion = tPossibleVersion;
                            break;
                        }
                    }
                }

                if (tVersion == null)
                {
                    tVersion = new VersionEN();

                    tVersion.VersionName = oSyncVersion.Name;
                    tVersion.VersionNumber = oSyncVersion.Name;
                    tVersion.VersionDesc = oSyncVersion.Description;
                    tVersion.ProjectID = Convert.ToInt32(tProject.ID);
                    tVersion.StartDate = DateTime.Today;
                    tVersion.ReleaseDate = tVersion.StartDate;

                    if (oSyncVersion.Version != null)
                    {
                        ExVersion tParentVersion = oSyncVersion.Version.GetSyncVersion(true);

                        if (tParentVersion != null)
                        {
                            tVersion.ParentVersionId = Convert.ToInt32(tParentVersion.ID);
                        }
                        else
                        {
                            Plugin.LogWarning("Failed to set parent version for Gemini version '" + oSyncVersion.Name + "'. Failed to get Gemini parent version.");
                        }
                    }

                    try
                    {
                        tVersion = ServiceManager.ProjectsService.CreateVersion(tVersion.ProjectID, tVersion);
                    }
                    catch (Exception e)
                    {
                        Plugin.LogError("Failed to create Gemini version '" + oSyncVersion.Name + "'. Exception (" + e.Message + "). Try creating a version with this name in Gemini.");
                        tProject = null;
                    }
                }

                if (tVersion != null)
                {
                    GVersion oGVersion = new GVersion(tProject as GProject, tVersion);

                    oGVersion.SyncID = oSyncVersion.ID;

                    return oGVersion;
                }

                return null;
            }
            else
            {
                Plugin.LogInfo("Failed to create Gemini version '" + oSyncVersion.Name + "'. Failed to get Gemini project.");
            }

            return null;
        }

        /// <summary>
        /// Get the task from the synchronized task. Return null if the task is not synchronized.
        /// </summary>
        internal override ExTask GetTask(ExTask oSyncTask)
        {
            GProject oProject = oSyncTask.Project.GetSyncProject(false) as GProject;

            if (oProject != null)
            {
                UInt32 nID = oSyncTask.SyncID;

                if (nID > 0)
                {
                    IssueEN oGIssue = ServiceManager.IssuesService.GetIssue(Convert.ToInt32(nID));

                    if (oGIssue != null)
                    {
                        return new GTask(oProject, oGIssue);
                    }
                }
            }

            return null;
        }

        internal ExTask CreateTask(ExProject tProject, IssueEN tIssue)
        {
            return new GTask(tProject as GProject, tIssue);
        }

        /// <summary>
        /// Create a new task and synchronize it with this task
        /// </summary>
        internal override ExTask CreateTask(ExTask oSyncTask)
        {
            Plugin.Trace("Creating Gemini issue for PureCM task '" + oSyncTask.Name + "'.");

            if ( !m_oOptions.GTaskCreate)
            {
                Plugin.LogWarning("Failed to create Gemini issue '" + oSyncTask.Name + "'. Task creation is disabled.");
                return null;
            }

            ExProject tProject = oSyncTask.Project.GetSyncProject(true);

            if (tProject != null)
            {
                IssueEN tIssue = new IssueEN();

                tIssue.ProjectID = Convert.ToInt32(tProject.ID);
                tIssue.IssueSummary = oSyncTask.Name;
                tIssue.IssueLongDesc = oSyncTask.Description;
                tIssue.IssuePriority = GetPriorityFromPCMPriority(tIssue.ProjectID, oSyncTask.Priority).ID;
                tIssue.IssueStatus = m_tTaskCreationStatus.StatusID;

                if (oSyncTask.IsFeature() && Options.GFeatureCreate)
                {
                    tIssue.IssueType = FeatureCreationType.ID;
                }
                else
                {
                    tIssue.IssueType = m_tTaskCreationType.ID;
                }

                tIssue.IssueSeverity = m_tTaskCreationSeverity.SeverityID;
                tIssue.RiskLevel = 1;
                tIssue.ReportedBy = m_oServiceManager.UsersService.WhoAmI().UserID;

                if (tIssue.IssueLongDesc.Length == 0)
                {
                    tIssue.IssueLongDesc = tIssue.IssueSummary;
                }

                if (oSyncTask.Version != null)
                {
                    ExVersion tParentVersion = oSyncTask.Version.GetSyncVersion(true);

                    if (tParentVersion != null)
                    {
                        tIssue.FixedInVersion = Convert.ToInt32(tParentVersion.ID);
                    }
                    else
                    {
                        Plugin.LogWarning("Failed to set fixed in version for Gemini issue '" + oSyncTask.Name + "'. Failed to get Gemini issue parent version.");
                    }
                }

                tIssue = ServiceManager.IssuesService.CreateIssue(tIssue);

                ExTask oTask = CreateTask(tProject, tIssue);

                oTask.SyncID = oSyncTask.ID;
                oSyncTask.Url = oTask.Url;

                return oTask;
            }
            else
            {
                Plugin.LogInfo("Failed to create Gemini issue '" + oSyncTask.Name + "'. Failed to get Gemini project.");
            }

            return null;
        }

        /// <summary>
        /// Get the user from the Gemini user id. Return null if the user is not synchronized.
        /// </summary>
        internal GUser GetUser(int nID)
        {
            if (nID != 0)
            {
                UserEN oGUser = ServiceManager.UsersService.GetUser(nID);

                if ((oGUser != null) && (oGUser.UserName != null) && (oGUser.UserName.Length > 0))
                {
                    return new GUser(this, oGUser);
                }
            }

            return null;
        }

        /// <summary>
        /// Get the user from the synchronized user. Return null if the user is not synchronized.
        /// </summary>
        internal override ExUser GetUser(ExUser oSyncUser)
        {
            UInt32 nID = oSyncUser.SyncID;

            if ( nID > 0 )
            {
                return GetUser(Convert.ToInt32(nID));
            }

            return null;
        }

        /// <summary>
        /// Create a new user and synchronize it with this user
        /// </summary>
        internal override ExUser CreateUser(ExUser oSyncUser)
        {
            UserEN tUser = ServiceManager.UsersService.GetUserByName(oSyncUser.Name);

            if (tUser == null)
            {
                tUser = new UserEN();

                tUser.UserName = oSyncUser.Name;
                tUser.Firstname = oSyncUser.Name;
                tUser.Surname = "PureCM";
                tUser.Comment = oSyncUser.Description;
                tUser.EmailAddress = oSyncUser.EmailAddress;
                tUser.Password = "secret";

                if (tUser.EmailAddress.Length == 0)
                {
                    tUser.EmailAddress = "Unspecified";
                }

                tUser = ServiceManager.UsersService.CreateUser(tUser);
            }

            if (tUser.UserID > 0)
            {
                GUser oGUser = new GUser(this, tUser);

                oGUser.SyncID = oSyncUser.ID;

                return oGUser;
            }
            else
            {
                Plugin.LogWarning("Failed to create Gemini user '" + oSyncUser.Name + "'. Do you have enough Gemini licenses?");
                return null;
            }
        }
    }
}
