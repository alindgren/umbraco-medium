using System;
using System.Collections.Generic;
using System.Configuration;
using System.Web.Http;
using Umbraco.Web.Mvc;
using Umbraco.Core.Persistence;

/// <summary>
/// Summary description for PostToMediumApiController
/// </summary>
[PluginController("PostToMedium")]
public class PostToMediumApiController : Umbraco.Web.WebApi.UmbracoAuthorizedApiController
{
    // http://localhost:50437/umbraco/backoffice/PostToMedium/PostToMediumApi/Hello
    [HttpGet]
    public string Hello()
    {
        return "Hola";
    }

    [HttpGet]
    public DialogViewModel InitDialog()
    {
        string clientId = ConfigurationManager.AppSettings["Medium_Client_ID"];
        string clientSecret = ConfigurationManager.AppSettings["Medium_Client_Secret"];

        // if Medium Client ID and/or Secret are missing, return error message
        if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
            return new DialogViewModel() { Status = "error", ErrorMessage = "Medium Client ID and/or Secret not set." };

        // if record exists in MediumUserToken for user:
        int userId = UmbracoContext.Security.CurrentUser.Id;
        var db = UmbracoContext.Application.DatabaseContext.Database;
        var userToken = db.FirstOrDefault<MediumUserToken>(new Sql().Select("*").From("MediumUserToken").Where("UserId = @0", userId));

        if (userToken == null)
        {
            var oAuthClient = new Medium.OAuthClient(clientId, clientSecret);
            var url = oAuthClient.GetAuthorizeUrl(
            "secretstate",
            "http://umbraco-medium.com:50437/umbraco/backoffice/PostToMedium/MediumAuthorization",
            new[]
            {
                Medium.Authentication.Scope.BasicProfile,
                Medium.Authentication.Scope.ListPublications,
                Medium.Authentication.Scope.PublishPost
            });
            return new DialogViewModel() { AuthUrl = url, Status = "no_token" };
        }

        if (userToken != null)
        {
            var client = new Medium.Client();
            if (userToken.ExpiresAt.CompareTo(DateTime.Now.AddMinutes(-5)) < 0)
            {
                var oAuthClient = new Medium.OAuthClient(clientId, clientSecret);
                var newAccessToken = oAuthClient.GetAccessToken(userToken.RefreshToken);
                userToken.AccessToken = newAccessToken.AccessToken;
                db.Update(userToken);
            }
            var token = new Medium.Authentication.Token()
            {
                TokenType = "Bearer",
                AccessToken = userToken.AccessToken,
                RefreshToken = userToken.RefreshToken,
                ExpiresAt = userToken.ExpiresAt
            };
            var user = client.GetCurrentUser(token);
            //var publications = client.GetPublications(user.Id, token);
            //var _publications = new Dictionary<string, string>();
            //foreach (var publication in publications)
            //{
            //    _publications.Add(publication.Id, publication.Name);
            //}
            return new DialogViewModel()
            {
                Name = user.Name,
                Username = user.Username,
                UserImageUrl = user.ImageUrl,
                UserUrl = user.Url,
                //Publications = _publications,
                Status = "ok"
            };
        }

        return new DialogViewModel() { Status = "error", ErrorMessage = "Unknown error" };
    }

    [HttpGet]
    public DialogViewModel PostToMedium(int nodeId)
    {
        string clientId = ConfigurationManager.AppSettings["Medium_Client_ID"];
        string clientSecret = ConfigurationManager.AppSettings["Medium_Client_Secret"];

        // if record exists in MediumUserToken for user:
        int userId = UmbracoContext.Security.CurrentUser.Id;
        var db = UmbracoContext.Application.DatabaseContext.Database;
        var userToken = db.FirstOrDefault<MediumUserToken>(new Sql().Select("*").From("MediumUserToken").Where("UserId = @0", userId));
        if (userToken == null)
        {
            var oAuthClient = new Medium.OAuthClient(clientId, clientSecret);
            var url = oAuthClient.GetAuthorizeUrl(
            "secretstate",
            "http://umbraco-medium.com:50437/umbraco/backoffice/PostToMedium/MediumAuthorization",
            new[]
            {
                Medium.Authentication.Scope.BasicProfile,
                Medium.Authentication.Scope.ListPublications,
                Medium.Authentication.Scope.PublishPost
            });
            return new DialogViewModel() { AuthUrl = url, Status = "no_token" };
        }
        else
        {
            var client = new Medium.Client();
            if (userToken.ExpiresAt.CompareTo(DateTime.Now.AddMinutes(-5)) < 0)
            {
                var oAuthClient = new Medium.OAuthClient(clientId, clientSecret);
                var newAccessToken = oAuthClient.GetAccessToken(userToken.RefreshToken);
                userToken.AccessToken = newAccessToken.AccessToken;
                db.Update(userToken);
            }
            var token = new Medium.Authentication.Token()
            {
                TokenType = "Bearer",
                AccessToken = userToken.AccessToken,
                RefreshToken = userToken.RefreshToken,
                ExpiresAt = userToken.ExpiresAt
            };
            var user = client.GetCurrentUser(token);

            var node = UmbracoContext.ContentCache.GetById(nodeId);
            string title = node.Name;
            var format = node.ContentType.Alias == "ArticulateMarkdown" ? Medium.Models.ContentFormat.Markdown : Medium.Models.ContentFormat.Html;
            string[] tags = ((string)node.GetProperty("tags").Value).Split(',');
            string content = node.ContentType.Alias == "ArticulateMarkdown" ? (string)node.GetProperty("markdown").Value : (string)node.GetProperty("richText").DataValue;

            // TODO: probably need to make the content use full URLs for images (replace src="/media/ with src="http://mydomain.com/media/) for images to be 'side-loaded' via Medium.

            // Create a draft post. TODO: provide UI so user can choose publish status and license (with options for setting defaults in AppSettings
            // todo: set canonical URL and license
            var post = client.CreatePost(user.Id, new Medium.Models.CreatePostRequestBody
            {
                Title = title,
                ContentFormat = format,
                Tags = tags,
                Content = content,
                PublishStatus = Medium.Models.PublishStatus.Unlisted
            }, token);

            return new DialogViewModel() { Status = "posted", MediumPostUrl = post.Url };
        }
    }
}