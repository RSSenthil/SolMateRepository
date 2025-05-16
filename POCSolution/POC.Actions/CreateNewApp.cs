using Microsoft.Crm.Sdk.Messages;
using Microsoft.SqlServer.Server;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;

namespace POC.Actions
{
    public class CreateNewApp : CodeActivity
    {
        [RequiredArgument]
        [Input("AppName")]
        public InArgument<string> AppName { get; set; }

        [RequiredArgument]
        [Input("SolutionName")]
        public InArgument<string> SolutionName { get; set; }

        [RequiredArgument]
        [Input("EntityUniqueName")]
        public InArgument<string> EntityUniqueName { get; set; }

        [RequiredArgument]
        [Input("EntityDisplayName")]
        public InArgument<string> EntityDisplayName { get; set; }

        [Output("Response")]
        public OutArgument<string> Response { get; set; }

        [Output("AppID")]
        public OutArgument<string> AppID { get; set; }

        [Output("SiteMapID")]
        public OutArgument<string> SiteMapID { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            string appName = AppName.Get(context);            
            string appNameUnique = appName.Replace(" ", "").ToLower();            
            string entityName = EntityDisplayName.Get(context);
            string entityNameUnique = EntityUniqueName.Get(context);
            string solutionNameUnique = SolutionName.Get(context).Replace(" ", "").ToLower();

            ITracingService tracingService = context.GetExtension<ITracingService>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(null);
            tracingService.Trace("start");

            try
            {
                Entity createApp = new Entity("appmodule");
                createApp["name"] = appName;
                createApp["uniquename"] = appNameUnique;
                createApp["webresourceid"] = new Guid("953b9fac-1e5e-e611-80d6-00155ded156f");
                createApp["clienttype"] = 4;
                var appid = service.Create(createApp);

                var addSolutionComponentRequest = new AddSolutionComponentRequest()
                {
                    ComponentType = 80,
                    ComponentId = (Guid)appid,
                    SolutionUniqueName = solutionNameUnique                    
                };
                service.Execute(addSolutionComponentRequest);
                tracingService.Trace("0");
                var siteMapXML = "<SiteMap IntroducedVersion='7.0.0.0'><Area Id='New_Area' ResourceId='SitemapDesigner.NewArea' DescriptionResourceId='SitemapDesigner.NewArea' ShowGroups='true' IntroducedVersion='7.0.0.0'><Group Id='New_Group' ResourceId='SitemapDesigner.NewGroup' DescriptionResourceId='SitemapDesigner.NewGroup' IntroducedVersion='7.0.0.0' IsProfile='false' ToolTipResourseId='SitemapDesigner.Unknown'><SubArea Id='New_SubArea' Icon='/_imgs/imagestrips/transparent_spacer.gif' IntroducedVersion='7.0.0.0' Entity='"+ entityNameUnique +"' Client='All,Outlook,OutlookLaptopClient,OutlookWorkstationClient,Web' AvailableOffline='true' PassParams='false' Sku='All,OnPremise,Live,SPLA'><Titles><Title LCID='1033' Title='"+ entityName +"' /></Titles></SubArea></Group></Area></SiteMap>";

                Entity createSiteMap = new Entity("sitemap");
                createSiteMap["sitemapname"] = appName;
                createSiteMap["sitemapnameunique"] = appNameUnique;
                createSiteMap["sitemapxml"] = siteMapXML;                
                var siteMapId = service.Create(createSiteMap);

                QueryExpression queryExp = new QueryExpression
                {
                    EntityName = "systemform",
                    ColumnSet = new ColumnSet("formid")
                };
                FilterExpression conditions = new FilterExpression(LogicalOperator.And);
                conditions.Conditions.Add(new ConditionExpression("objecttypecode", ConditionOperator.Equal, entityNameUnique));
                FilterExpression Orconditions = new FilterExpression(LogicalOperator.Or);
                Orconditions.Conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, 2));
                Orconditions.Conditions.Add(new ConditionExpression("type", ConditionOperator.Equal, 6));
                conditions.AddFilter(Orconditions);
                queryExp.Criteria.AddFilter(conditions);
                //queryExp.Criteria.AddFilter(Orconditions);
                EntityCollection forms = service.RetrieveMultiple(queryExp);
                var addAppComponent = new AddAppComponentsRequest();

                var qeToFetchXmlRequest = new QueryExpressionToFetchXmlRequest
                {
                    Query = queryExp
                };
                var qeToFetchXmlResponse = (QueryExpressionToFetchXmlResponse)service.Execute(qeToFetchXmlRequest);
                var fetchXml = qeToFetchXmlResponse.FetchXml;
                tracingService.Trace($"ObjectTypeCode: {entityNameUnique}");
                tracingService.Trace($"Fetch: {fetchXml}");
                //foreach (var form in forms.Entities)
                //{
                //    addAppComponent = new AddAppComponentsRequest()
                //    {
                //        AppId = appid,
                //        Components =
                //        {
                //            new EntityReference("systemform", new Guid(form["formid"].ToString()))
                //        }                        
                //    };
                //    service.Execute(addAppComponent);
                //    tracingService.Trace($"Form ID: {form["formid"].ToString()}");
                //}
                tracingService.Trace("2");
                tracingService.Trace($"Sitemap: {siteMapId}");
                //var addAppComponent2 = new AddAppComponentsRequest()
                //{
                //    AppId = appid,
                //    Components =
                //    {
                //        new EntityReference("sitemap", new Guid(siteMapId.ToString()))
                //    }
                //};
                //service.Execute(addAppComponent2);
                tracingService.Trace("3");
                addSolutionComponentRequest = new AddSolutionComponentRequest()
                {
                    ComponentType = 62,
                    ComponentId = (Guid)siteMapId,
                    SolutionUniqueName = solutionNameUnique
                };
                service.Execute(addSolutionComponentRequest);
                tracingService.Trace("4");

                Response.Set(context, "Created");
                AppID.Set(context, appid.ToString());
                SiteMapID.Set(context, siteMapId.ToString());
            }
            catch (Exception ex)
            {
                Response.Set(context, ex.Message);
                AppID.Set(context, "");
                SiteMapID.Set(context, "");
                tracingService.Trace(ex.Message);
            }
        }
    }
}
