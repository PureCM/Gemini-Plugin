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
    /// A Gemini user.
    /// </summary>
    internal class GUser : ExUser
    {
        internal GUser(GFactory oGFactory, UserEN oGUser)
            : base(oGFactory, ExFactory.TPluginType.GeminiUser, System.Convert.ToUInt32(oGUser.UserID))
        {
            m_oGUser = oGUser;
        }

        private UserEN m_oGUser;

        /// <summary>
        /// The Gemini user object
        /// </summary>
        internal UserEN UserEN
        {
            get { return m_oGUser; }
        }

        /// <summary>
        /// Return the email address for this user
        /// </summary>
        internal override String GetEmailAddress()
        {
            return m_oGUser.EmailAddress;
        }

        /// <summary>
        /// Return the Gemini short name for the user
        /// </summary>
        internal override String GetName()
        {
            return m_oGUser.UserName;
        }

        /// <summary>
        /// Return the Gemini long description for the user
        /// </summary>
        internal override String GetDescription()
        {
            return m_oGUser.Comment;
        }
    }
}
