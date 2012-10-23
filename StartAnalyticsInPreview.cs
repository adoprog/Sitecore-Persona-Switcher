using System;
using Sitecore;
using Sitecore.Analytics;
using Sitecore.Analytics.Configuration;
using Sitecore.Analytics.Extensions.AnalyticsPageExtensions;
using Sitecore.Analytics.Pipelines.HttpRequest;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines.RenderLayout;
using Sitecore.Sites;
using Sitecore.Web;

namespace ChangePersona
{
    /// <summary>
    /// Another version of StartAnalytics that is added by the ChangePersona.config include file.
    /// This version only runs in Preview mode and unlike the original StartAnalytics won't disable OMS in preview.
    /// </summary>
    public class StartAnalyticsInPreview : StartAnalytics
    {
        // Methods
        private static void Page_LoadComplete(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            if (AnalyticsTracker.IsActive)
            {
                AnalyticsTracker current = AnalyticsTracker.Current;
                if (current != null)
                {
                    AnalyticsSession currentSession = current.CurrentSession;
                    if (string.IsNullOrEmpty(currentSession.AspNetSessionId))
                    {
                        currentSession.AspNetSessionId = WebUtil.GetSessionID();
                    }
                }
            }
        }

        private static void Page_Unload(object sender, EventArgs e)
        {
            Assert.ArgumentNotNull(sender, "sender");
            Assert.ArgumentNotNull(e, "e");
            if (AnalyticsTracker.IsActive)
            {
                PageContext page = Context.Page;
                if ((page != null) && (page.Page != null))
                {
                    object obj2 = Context.Items["SC_ANALYTICS_IS_PAGE_VISITED"];
                    if ((obj2 == null) || ((bool) obj2))
                    {
                        AnalyticsTracker current = AnalyticsTracker.Current;
                        if (current != null)
                        {
                            current.CurrentPage.VisitPage();
                        }
                    }
                }
            }
        }

        public new void Process(RenderLayoutArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            if (AnalyticsSettings.Enabled)
            {
                SiteContext site = Context.Site;
                if (((site != null) && site.EnableAnalytics))
                {
                    PageContext context2 = Context.Page;
                    if (context2 != null)
                    {
                        System.Web.UI.Page page = context2.Page;
                        if (page != null)
                        {
                            AnalyticsTracker.StartTracking();
                            if (AnalyticsTracker.IsActive)
                            {
                                PersonaHelper.Initialize(AnalyticsTracker.Current);

                                page.LoadComplete += Page_LoadComplete;
                                page.Unload += Page_Unload;
                            }
                        }
                    }
                }
            }
        }
    }
}


