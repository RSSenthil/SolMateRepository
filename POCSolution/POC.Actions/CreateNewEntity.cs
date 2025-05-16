using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Workflow;
using System.Activities;

namespace POC.Actions
{
    public class CreateNewEntity : CodeActivity
    {
        [RequiredArgument]
        [Input("EntitySchemaName")]
        public InArgument<string> EntitySchemaName { get; set; }

        [RequiredArgument]
        [Input("EntityDisplayName")]
        public InArgument<string> EntityDisplayName { get; set; }

        [RequiredArgument]
        [Input("EntityPluralName")]
        public InArgument<string> EntityPluralName { get; set; }

        [RequiredArgument]
        [Input("SolutionUniqueName")]
        public InArgument<string> SolutionUniqueName { get; set; }
             
        [Output("Response")]
        public OutArgument<string> Response { get; set; }
        protected override void Execute(CodeActivityContext context)
        {
            string entitySchemaName = $"poc_{EntitySchemaName.Get(context)}";
            string entityDisplayName = EntityDisplayName.Get(context);
            string entityPluralName = EntityPluralName.Get(context);
            string solutionUniqueName = SolutionUniqueName.Get(context);

            ITracingService tracingService = context.GetExtension<ITracingService>();           
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(null);

            CreateEntityRequest request = new CreateEntityRequest {
                Entity = new EntityMetadata
                {
                    SchemaName = entitySchemaName,
                    DisplayName = new Label(entityDisplayName, 1033),
                    DisplayCollectionName = new Label(entityPluralName, 1033),
                    Description = new Label("This entity is created by SolMate", 1033),
                    OwnershipType = OwnershipTypes.UserOwned,
                    IsActivity = false,
                    IsMailMergeEnabled = new BooleanManagedProperty(true),
                    IsDuplicateDetectionEnabled = new BooleanManagedProperty(true)

                },
                PrimaryAttribute = new StringAttributeMetadata
                {
                    SchemaName = "poc_name",
                    RequiredLevel = new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.SystemRequired),
                    MaxLength = 100,
                    FormatName = StringFormatName.Text,
                    DisplayName = new Label("Name", 1033),
                    Description = new Label($"The primary column for the {entityDisplayName} table.", 1033)
                },
                SolutionUniqueName = solutionUniqueName
            };

            var response = (CreateEntityResponse)service.Execute(request);
            tracingService.Trace($"Response: {response.EntityId.ToString()}");            
            Response.Set(context, response.EntityId.ToString());
            // throw new NotImplementedException();
        }
    }
}
