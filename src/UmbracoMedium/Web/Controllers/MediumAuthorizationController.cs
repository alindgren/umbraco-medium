using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Umbraco.Web.Mvc;
using Umbraco.Core.Persistence;

/// <summary>
/// Summary description for MediumAuthorizationController
/// </summary>
public class MediumAuthorizationController : UmbracoAuthorizedController
{
    // need to define route - https://our.umbraco.org/documentation/Reference/Routing/Authorized/
    [HttpGet]
    public string Hello()
    {
        return "Hola";
    }

    public ActionResult AuthCallback()
    {
        //http://localhost:50437/umbraco/backoffice/PostToMedium/MediumAuthorization?state=secretstate&code=87ed2dcbf725

        // check error for access denied
        string error = Request.Params["error"];
        if (!string.IsNullOrEmpty(error))
        {
            return View("~/App_Plugins/PostToMedium/AuthCallback.cshtml");
        }

        // check secret state


        // get auth code
        string code = Request.Params["code"];
        if (!string.IsNullOrEmpty(code))
        {
            string clientId = ConfigurationManager.AppSettings["Medium_Client_ID"];
            string clientSecret = ConfigurationManager.AppSettings["Medium_Client_Secret"];
            var oAuthClient = new Medium.OAuthClient(clientId, clientSecret);
            var accessToken = oAuthClient.GetAccessToken(code, "http://umbraco-medium.com:50437/umbraco/backoffice/PostToMedium/MediumAuthorization");
            int userId = UmbracoContext.Security.CurrentUser.Id;

            var db = UmbracoContext.Application.DatabaseContext.Database;
            var userToken = db.FirstOrDefault<MediumUserToken>(new Sql().Select("*").From("MediumUserToken").Where("UserId = @0", userId));
            if (userToken == null)
            {
                userToken = new MediumUserToken()
                {
                    UserId = userId,
                    ExpiresAt = accessToken.ExpiresAt,
                    AccessToken = accessToken.AccessToken,
                    RefreshToken = accessToken.RefreshToken
                };
                db.Insert(userToken);
            } else
            {
                userToken.ExpiresAt = accessToken.ExpiresAt;
                userToken.AccessToken = accessToken.AccessToken;
                userToken.RefreshToken = accessToken.RefreshToken;
                db.Update(userToken);
            }

            return View("~/App_Plugins/PostToMedium/AuthCallback.cshtml");
        } else
        {
            // handle no code and no error?
        }

        return View("~/App_Plugins/PostToMedium/AuthCallback.cshtml");

    }
}