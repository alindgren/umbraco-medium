using System;
using Umbraco.Core;
using Umbraco.Web.Trees;
using Umbraco.Web.Models.Trees;
using System.Web.Routing;
using System.Web.Mvc;
using Umbraco.Core.Persistence;

/// <summary>
/// Summary description for RegisterEvents
/// </summary>
public class RegisterEvents : ApplicationEventHandler
{
    protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication, ApplicationContext applicationContext)
    {
        TreeControllerBase.MenuRendering += TreeControllerBase_MenuRendering;
        base.ApplicationStarted(umbracoApplication, applicationContext);

        // note: should use Umbraco.Core.Configuration.GlobalSettings.UmbracoMvcArea but it was inaccesible. See https://github.com/umbraco/UmbracoDocs/pull/367#issuecomment-240121475
        RouteTable.Routes.MapRoute(
          name: "PostToMedium",
          url: "umbraco/backoffice/UmbracoMedium/{MediumAuthorization}/{action}",
          defaults: new
          {
              controller = "MediumAuthorization",
              action = "AuthCallback"
          });

        //Get the Umbraco Database context
        var ctx = applicationContext.DatabaseContext;
        var db = new DatabaseSchemaHelper(ctx.Database, applicationContext.ProfilingLogger.Logger, ctx.SqlSyntax);

        //Check if the DB table does NOT exist
        if (!db.TableExist("MediumUserToken"))
        {
            //Create DB table - and set overwrite to false
            db.CreateTable<MediumUserToken>(false);
        }
    }

    void TreeControllerBase_MenuRendering(TreeControllerBase sender, MenuRenderingEventArgs e)
    {
        if (sender.TreeAlias == "content")
        {
            var nodeId = Int32.Parse(e.NodeId);
            var contentService = sender.UmbracoContext.Application.Services.ContentService;
            var node = contentService.GetById(nodeId);
            if (node.ContentType.Alias == "ArticulateMarkdown" || node.ContentType.Alias == "ArticulateRichText")
            {
                var m = new MenuItem("postToMedium", "Post to Medium");
                m.AdditionalData.Add("actionView", "/App_Plugins/UmbracoMedium/views/postToMedium.html");
                //m.AdditionalData.Add("test", "additionalData");
                m.Icon = "umb-content";
                e.Menu.Items.Add(m);
            }
        }
    }
}