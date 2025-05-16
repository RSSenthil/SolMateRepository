using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Organization;
using Microsoft.Xrm.Sdk.Query;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;
using System.Security.Principal;

namespace POC.Actions
{
    public class CreateNewForm : CodeActivity
    {
        [RequiredArgument]
        [Input("EntName")]
        public InArgument<string> EntName { get; set; }

        [RequiredArgument]
        [Input("FormName")]
        public InArgument<string> FormName { get; set; }

        [Output("Response")]
        public OutArgument<string> Response { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            string formName = FormName.Get(context);
            string entityName = EntName.Get(context);
            string formNameUnique = formName.Replace(" ","").ToLower();

            ITracingService tracingService = context.GetExtension<ITracingService>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(null);
            string allfields = string.Empty;            

            RetrieveEntityRequest retrieveEntityRequest = new RetrieveEntityRequest()
            {
                EntityFilters = EntityFilters.All,
                LogicalName = entityName,
                RetrieveAsIfPublished = true
            };

            RetrieveEntityResponse retrieveEntityResponse = (RetrieveEntityResponse)service.Execute(retrieveEntityRequest);

            EntityMetadata currentEntity = retrieveEntityResponse.EntityMetadata;
            foreach (AttributeMetadata attribute in currentEntity.Attributes)
            {

                if (attribute.DisplayName.LocalizedLabels.Count > 0 && attribute.LogicalName.StartsWith("poc_") && !attribute.LogicalName.Equals(entityName + "id"))
                {
                    tracingService.Trace($"Field Logical: {attribute.LogicalName}");
                    tracingService.Trace($"Field Display: {attribute.DisplayName.LocalizedLabels[0].Label}");
                }

                if (attribute.DisplayName.LocalizedLabels.Count > 0 && attribute.LogicalName.StartsWith("poc_") && !attribute.LogicalName.Equals(entityName+"id"))
                {
                    //tracingService.Trace($"Field Display: {attribute.DisplayName.LocalizedLabels[0].Label}");
                    allfields += "<row><cell><labels><label description='"+ attribute.DisplayName.LocalizedLabels[0].Label +"' languagecode='1033' /></labels><control id='"+ attribute.LogicalName +"' classid='{4273EDBD-AC1D-40d3-9FB2-095C621B552D}' datafieldname='{attribute.LogicalName}' /></cell></row>";
                }
                //else
                //{
                //    tracingService.Trace($"Field Display: {attribute.DisplayName.LocalizedLabels[0].Label}");
                //    allfields += $"<row><cell><labels><label description='{attribute.DisplayName.LocalizedLabels.Label}' languagecode='1033' /></labels><control id='{attribute.LogicalName}' datafieldname='{attribute.LogicalName}' /></cell></row>";
                //}
            }

            string xmlString = "<form shownavigationbar='true' showImage='false' maxWidth='1920'><tabs><tab verticallayout='true' id='{8bc1d925-f3b7-4f62-84ba-d9602fff07c8}' IsUserDefined='1' labelid='{852c1928-9437-4c8e-8ff3-2afc4bcb02e2}'><labels><label description='General' languagecode='1033' /></labels><columns><column width='100%'><sections><section showlabel='false' showbar='false' IsUserDefined='0' id='{aaa78c6b-4661-467a-9926-43f0045fb9fb}' labelid='{98d42a99-443c-4900-9d6a-120372191543}'><labels><label description='General' languagecode='1033' /></labels><rows>" + allfields + "<row><cell id='{d35169f1-b3e6-410f-99a3-356eeef5bdcf}' labelid='{7fa5444c-b870-4e8e-b669-eee8022dff19}'><labels><label description='Owner' languagecode='1033' /></labels><control id='ownerid' classid='{270BD3DB-D9AF-4782-9025-509E298DEC0A}' datafieldname='ownerid' /></cell></row></rows></section></sections></column></columns></tab></tabs><Navigation><NavBar></NavBar><NavBarAreas><NavBarArea Id='Info'><Titles><Title LCID='1033' Text='Common' /></Titles></NavBarArea><NavBarArea Id='Sales'><Titles><Title LCID='1033' Text='Sales' /></Titles></NavBarArea><NavBarArea Id='Service'><Titles><Title LCID='1033' Text='Service' /></Titles></NavBarArea><NavBarArea Id='Marketing'><Titles><Title LCID='1033' Text='Marketing' /></Titles></NavBarArea><NavBarArea Id='ProcessCenter'><Titles><Title LCID='1033' Text='Process Sessions' /></Titles></NavBarArea></NavBarAreas></Navigation><DisplayConditions Order='1'><Role Id='{627090ff-40a3-4053-8790-584edc5be201}' /><Role Id='{119f245c-3cc8-4b62-b31c-d1a046ced15d}' /></DisplayConditions></form>";

            QueryExpression queryExp = new QueryExpression
            {
                EntityName = "systemform",
                ColumnSet = new ColumnSet("name")
            };
           
            FilterExpression conditions = new FilterExpression(LogicalOperator.And);
            conditions.Conditions.Add(new ConditionExpression("name", ConditionOperator.Equal, formName));
            conditions.Conditions.Add(new ConditionExpression("objecttypecode", ConditionOperator.Equal, "poc_window"));
            queryExp.Criteria.AddFilter(conditions);            
            EntityCollection forms = service.RetrieveMultiple(queryExp);

            if (forms.Entities.Count == 0)
            {
                tracingService.Trace("Form doesn't exist."); 

                Entity createForm = new Entity("systemform");
                createForm["name"] = formName;
                createForm["objecttypecode"] = entityName;
                createForm["type"] = new OptionSetValue(2);
                createForm["formxml"] = xmlString;

                try
                {
                    var formId = service.Create(createForm);
                    Console.WriteLine("Form Created for Entity " + entityName);

                    var addSolutionComponentRequest = new AddSolutionComponentRequest()
                    {
                        ComponentType = 60,
                        ComponentId = (Guid)formId,
                        SolutionUniqueName = "aientities",
                        DoNotIncludeSubcomponents = true
                    };
                    service.Execute(addSolutionComponentRequest);

                    Response.Set(context, "created");                    
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Failed for Entity : " + entityName);
                    Response.Set(context, ex.Message);
                    tracingService.Trace(ex.Message);
                }
            }
            else
            {
                tracingService.Trace("Form already exist.");
                Response.Set(context, "Already exist");
            }
        }
    }
}
