#region Copyright
// 
// DotNetNukeŽ - http://www.dotnetnuke.com
// Copyright (c) 2002-2013
// by DotNetNuke Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion
#region Usings

using System;

using DotNetNuke.Common;
using DotNetNuke.Common.Utilities;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Users;
using DotNetNuke.Entities.Users.Membership;
using DotNetNuke.Instrumentation;
using DotNetNuke.Security;
using DotNetNuke.Security.Membership;
using DotNetNuke.Services.Localization;
using DotNetNuke.Services.Log.EventLog;


#endregion

namespace DotNetNuke.Modules.Admin.Security
{

   
    public partial class PasswordReset : UserModuleBase
    {
        private static readonly ILog Logger = LoggerSource.Instance.GetLogger(typeof(PasswordReset));

        #region Private Members

        private string _ipAddress;

        private string ResetToken
        {
            get
            {
                return ViewState["ResetToken"] != null ? Request.QueryString["resetToken"] : String.Empty;
            }
            set
            {
                ViewState.Add("ResetToken", value);
            }
        }

        #endregion

       
        #region Event Handlers

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);

            cmdChangePassword.Click +=cmdChangePassword_Click;
            
            hlCancel.NavigateUrl = Globals.NavigateURL();

            if (Request.QueryString["resetToken"] != null)
            {
                ResetToken = Request.QueryString["resetToken"];
                
            }

        }

        private void cmdChangePassword_Click(object sender, EventArgs e)
        {
            if (UserController.ChangePasswordByToken(PortalSettings.PortalId, txtUsername.Text, txtPassword.Text, ResetToken) == false)
            {
                resetMessages.Visible = true;
                var failed = Localization.GetString("FailedAttempt", LocalResourceFile);
                LogFailure(failed);
                lblHelp.Text = failed;
            }
            else
            {
                //Log user in to site
                LogSuccess();
                var loginStatus = UserLoginStatus.LOGIN_FAILURE;
                UserController.UserLogin(PortalSettings.PortalId, txtUsername.Text, txtPassword.Text, "", "", "", ref loginStatus, false);
                Response.Redirect("~/Default.aspx", true);
            }           
        }
       
        private void LogSuccess()
        {
            LogResult(string.Empty);
        }

        private void LogFailure(string reason)
        {
            LogResult(reason);
        }

        private void LogResult(string message)
        {
            var portalSecurity = new PortalSecurity();

            var objEventLog = new EventLogController();
            var objEventLogInfo = new LogInfo();
            
            objEventLogInfo.AddProperty("IP", _ipAddress);
            objEventLogInfo.LogPortalID = PortalSettings.PortalId;
            objEventLogInfo.LogPortalName = PortalSettings.PortalName;
            objEventLogInfo.LogUserID = UserId;
        //    objEventLogInfo.LogUserName = portalSecurity.InputFilter(txtUsername.Text,
          //                                                           PortalSecurity.FilterFlag.NoScripting | PortalSecurity.FilterFlag.NoAngleBrackets | PortalSecurity.FilterFlag.NoMarkup);
            if (string.IsNullOrEmpty(message))
            {
                objEventLogInfo.LogTypeKey = "PASSWORD_SENT_SUCCESS";
            }
            else
            {
                objEventLogInfo.LogTypeKey = "PASSWORD_SENT_FAILURE";
                objEventLogInfo.LogProperties.Add(new LogDetailInfo("Cause", message));
            }
            
            objEventLog.AddLog(objEventLogInfo);
        }

        #endregion

    }
}