using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using System.Text;

namespace Gemini_Items
{
    class GOptions
    {
        public GOptions(XElement oConfig)
        {
            GURL = oConfig.Element("GeminiURL").Value;
            GUser = oConfig.Element("GeminiUser").Value;
            GPassword = oConfig.Element("GeminiPassword").Value;
            GAPIKey = oConfig.Element("GeminiAPIKey").Value;
            PRepository = oConfig.Element("PureCMRepository").Value;

            if (oConfig.Element("Interval") != null)
            {
                Interval = UInt32.Parse(oConfig.Element("Interval").Value);
            }
            else
            {
                Interval = 60;
            }

            XElement tProjectCreationElement = oConfig.Element("ProjectCreation");

            if (tProjectCreationElement != null)
            {
                if (tProjectCreationElement.Element("Enabled") != null)
                {
                    GProjectCreate = tProjectCreationElement.Element("Enabled").Value.ToUpper() == "TRUE";
                }

                if (GProjectCreate)
                {
                    if (tProjectCreationElement.Element("TemplateProject") != null)
                    {
                        GProjectCreateTemplate = tProjectCreationElement.Element("TemplateProject").Value;
                    }
                    else
                    {
                        GProjectCreate = false;
                    }
                }
            }

            XElement tTaskCreationElement = oConfig.Element("TaskCreation");

            if ( tTaskCreationElement != null)
            {
                if (tTaskCreationElement.Element("Enabled") != null)
                {
                    GTaskCreate = tTaskCreationElement.Element("Enabled").Value.ToUpper() == "TRUE";
                }

                if (GTaskCreate)
                {
                    if (tTaskCreationElement.Element("CreationType") != null)
                    {
                        GTaskCreateTypeDescription = tTaskCreationElement.Element("CreationType").Value;
                    }
                    else
                    {
                        GTaskCreate = false;
                    }
                }

                if (GTaskCreate)
                {
                    if (tTaskCreationElement.Element("CreationStatus") != null)
                    {
                        GTaskCreateStatusDescription = tTaskCreationElement.Element("CreationStatus").Value;
                    }
                    else
                    {
                        GTaskCreate = false;
                    }
                }

                if (GTaskCreate)
                {
                    if (tTaskCreationElement.Element("CreationSeverity") != null)
                    {
                        GTaskCreateSeverityDescription = tTaskCreationElement.Element("CreationSeverity").Value;
                    }
                    else
                    {
                        GTaskCreate = false;
                    }
                }
            }

            XElement tFeatureCreationElement = oConfig.Element("FeatureCreation");

            if (tFeatureCreationElement != null)
            {
                if (tFeatureCreationElement.Element("Enabled") != null)
                {
                    GFeatureCreate = tFeatureCreationElement.Element("Enabled").Value.ToUpper() == "TRUE";
                }

                if (GFeatureCreate)
                {
                    if (tFeatureCreationElement.Element("CreationType") != null)
                    {
                        GFeatureCreateTypeDescription = tFeatureCreationElement.Element("CreationType").Value;
                    }
                    else
                    {
                        GFeatureCreate = false;
                    }
                }

                if (GFeatureCreate)
                {
                    if (tFeatureCreationElement.Element("CreationStatus") != null)
                    {
                        GFeatureCreateStatusDescription = tFeatureCreationElement.Element("CreationStatus").Value;
                    }
                    else
                    {
                        GFeatureCreate = false;
                    }
                }

                if (GFeatureCreate)
                {
                    if (tFeatureCreationElement.Element("CreationSeverity") != null)
                    {
                        GFeatureCreateSeverityDescription = tFeatureCreationElement.Element("CreationSeverity").Value;
                    }
                    else
                    {
                        GFeatureCreate = false;
                    }
                }
            }

            if (oConfig.Element("UpdateURL") != null)
            {
                UpdateURL = oConfig.Element("UpdateURL").Value.ToUpper() == "TRUE";
            }
            else
            {
                UpdateURL = true;
            }

            if (oConfig.Element("ForceGeminiSync") != null)
            {
                ForceGeminiSync = oConfig.Element("ForceGeminiSync").Value.ToUpper() == "TRUE";
            }

            if (oConfig.Element("ForcePureCMSync") != null)
            {
                ForcePureCMSync = oConfig.Element("ForcePureCMSync").Value.ToUpper() == "TRUE";
            }
        }

        /// <summary>
        /// The Gemini URL
        /// </summary>
        public String GURL { get; set; }

        /// <summary>
        /// The Gemini User
        /// </summary>
        public String GUser { get; set; }

        /// <summary>
        /// The Gemini User Password
        /// </summary>
        public String GPassword { get; set; }

        /// <summary>
        /// The Gemini API Key
        /// </summary>
        public String GAPIKey { get; set; }

        /// <summary>
        /// The PureCM repository name
        /// </summary>
        public String PRepository { get; set; }

        /// <summary>
        /// The interval in seconds between each Gemini-PureCM synchronization
        /// </summary>
        public uint Interval { get; set; }

        /// <summary>
        /// Whether to create Gemini projects from PureCM projects
        /// </summary>
        public bool GProjectCreate { get; set; }

        /// <summary>
        /// Whether to create Gemini projects from PureCM projects
        /// </summary>
        public string GProjectCreateTemplate { get; set; }

        /// <summary>
        /// Whether to create Gemini issues from PureCM tasks
        /// </summary>
        public bool GTaskCreate { get; set; }

        /// <summary>
        /// The creation type for Gemini issues
        /// </summary>
        public string GTaskCreateTypeDescription { get; set; }

        /// <summary>
        /// The creation status for Gemini issues
        /// </summary>
        public string GTaskCreateStatusDescription { get; set; }

        /// <summary>
        /// The creation severity for Gemini issues
        /// </summary>
        public string GTaskCreateSeverityDescription { get; set; }

        /// <summary>
        /// Whether to create Gemini issues from PureCM tasks
        /// </summary>
        public bool GFeatureCreate { get; set; }

        /// <summary>
        /// The creation type for Gemini issues
        /// </summary>
        public string GFeatureCreateTypeDescription { get; set; }

        /// <summary>
        /// The creation status for Gemini issues
        /// </summary>
        public string GFeatureCreateStatusDescription { get; set; }

        /// <summary>
        /// The creation severity for Gemini issues
        /// </summary>
        public string GFeatureCreateSeverityDescription { get; set; }

        /// <summary>
        /// Whether to update the PureCM task URLs to use the Gemini issue URLs
        /// </summary>
        public bool UpdateURL { get; set; }

        /// <summary>
        /// Whether we force all Gemini issues to be synchronized on startup
        /// </summary>
        public bool ForceGeminiSync { get; set; }

        /// <summary>
        /// Whether we force all PureCM tasks to be synchronized on startup
        /// </summary>
        public bool ForcePureCMSync { get; set; }
    }
}
