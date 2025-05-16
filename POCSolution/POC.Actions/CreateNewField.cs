using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Messages;
using Microsoft.Xrm.Sdk.Metadata;
using Microsoft.Xrm.Sdk.Workflow;
using System;
using System.Activities;
using System.Collections.Generic;

namespace POC.Actions
{
    public class CreateNewField : CodeActivity
    {
        [RequiredArgument]
        [Input("FieldDisplayName")]
        public InArgument<string> FieldDisplayName { get; set; }

        [RequiredArgument]
        [Input("FieldUniqueName")]
        public InArgument<string> FieldUniqueName { get; set; }

        [RequiredArgument]
        [Input("FieldType")]
        public InArgument<string> FieldType { get; set; }

        [RequiredArgument]
        [Input("IsRequired")]
        public InArgument<string> IsRequired { get; set; }

        [RequiredArgument]
        [Input("SolutionUniqueName")]
        public InArgument<string> SolutionUniqueName { get; set; }

        [RequiredArgument]
        [Input("EntityLogicalName")]
        public InArgument<string> EntityLogicalName { get; set; }

        [Output("Response")]
        public OutArgument<string> Response { get; set; }
        protected override void Execute(CodeActivityContext context)
        {            
            string fieldDisplayName = FieldDisplayName.Get(context);
            string fieldUniqueName = FieldUniqueName.Get(context);
            string fieldType = FieldType.Get(context);
            string entityLogicalName = EntityLogicalName.Get(context);
            string solutionUniqueName = SolutionUniqueName.Get(context);
            string isRequired = IsRequired.Get(context);
            int languageCode = 1033;
            string prefix = "poc"; 

            ITracingService tracingService = context.GetExtension<ITracingService>();
            IOrganizationServiceFactory serviceFactory = context.GetExtension<IOrganizationServiceFactory>();
            IOrganizationService service = serviceFactory.CreateOrganizationService(null);

            var addedAttributes = new List<AttributeMetadata>();
            var fieldRequiredLevel = (isRequired == "Yes") ? new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.SystemRequired) : new AttributeRequiredLevelManagedProperty(AttributeRequiredLevel.None);
            
            switch (fieldType)
                {                
                    case "boolean":
                        BooleanAttributeMetadata boolAttribute = new BooleanAttributeMetadata()
                        {                            
                            SchemaName = $"{prefix}_{fieldUniqueName}",
                            LogicalName = $"{prefix}_{fieldUniqueName}",
                            DisplayName = new Label(fieldDisplayName, languageCode),
                            RequiredLevel = fieldRequiredLevel,
                            Description = new Label("This field is created by SolMate", languageCode),
                            OptionSet = new BooleanOptionSetMetadata(
                            new OptionMetadata(new Label("True", languageCode), 1),
                            new OptionMetadata(new Label("False", languageCode), 0)
                            )
                        };
                    addedAttributes.Add(boolAttribute);
                    break;
                case "datetime":
                    DateTimeAttributeMetadata dtAttribute = new DateTimeAttributeMetadata()
                    {                        
                        SchemaName = $"{prefix}_{fieldUniqueName}",
                        LogicalName = $"{prefix}_{fieldUniqueName}",
                        DisplayName = new Label(fieldDisplayName, languageCode),
                        RequiredLevel = fieldRequiredLevel,
                        Description = new Label("This field is created by SolMate", languageCode),
                        Format = Microsoft.Xrm.Sdk.Metadata.DateTimeFormat.DateOnly,
                        ImeMode = ImeMode.Disabled
                    };
                    addedAttributes.Add(dtAttribute);
                    break;
                case "decimal":
                    DecimalAttributeMetadata decimalAttribute = new DecimalAttributeMetadata()
                    {                       
                        SchemaName = $"{prefix}_{fieldUniqueName}",
                        LogicalName = $"{prefix}_{fieldUniqueName}",
                        DisplayName = new Label(fieldDisplayName, languageCode),
                        RequiredLevel = fieldRequiredLevel,
                        Description = new Label("This field is created by SolMate", languageCode),
                        MaxValue = 100,
                        MinValue = 0,
                        Precision = 1
                    };
                    addedAttributes.Add(decimalAttribute);
                    break;
                case "int":
                    IntegerAttributeMetadata integerAttribute = new IntegerAttributeMetadata()
                    {
                        SchemaName = $"{prefix}_{fieldUniqueName}",
                        LogicalName = $"{prefix}_{fieldUniqueName}",
                        DisplayName = new Label(fieldDisplayName, languageCode),
                        RequiredLevel = fieldRequiredLevel,
                        Description = new Label("This field is created by SolMate", languageCode),
                        Format = IntegerFormat.None,
                        MaxValue = 100,
                        MinValue = 0
                    };
                    addedAttributes.Add(integerAttribute);
                    break;
                case "memo":
                    MemoAttributeMetadata memoAttribute = new MemoAttributeMetadata()
                    {
                        SchemaName = $"{prefix}_{fieldUniqueName}",
                        LogicalName = $"{prefix}_{fieldUniqueName}",
                        DisplayName = new Label(fieldDisplayName, languageCode),
                        RequiredLevel = fieldRequiredLevel,
                        Description = new Label("This field is created by SolMate", languageCode),
                        Format = StringFormat.TextArea,
                        ImeMode = ImeMode.Disabled,
                        MaxLength = 500
                    };
                    addedAttributes.Add(memoAttribute);
                    break;
                case "money":
                    MoneyAttributeMetadata moneyAttribute = new MoneyAttributeMetadata()
                    {
                        SchemaName = $"{prefix}_{fieldUniqueName}",
                        LogicalName = $"{prefix}_{fieldUniqueName}",
                        DisplayName = new Label(fieldDisplayName, languageCode),
                        RequiredLevel = fieldRequiredLevel,
                        Description = new Label("This field is created by SolMate", languageCode),
                        MaxValue = 1000.00,
                        MinValue = 0.00,
                        Precision = 1,
                        PrecisionSource = 1,
                        ImeMode = ImeMode.Disabled
                    };
                    addedAttributes.Add(moneyAttribute);
                    break;
                case "optionset":
                    PicklistAttributeMetadata pickListAttribute =
                     new PicklistAttributeMetadata()
                     {
                         SchemaName = $"{prefix}_{fieldUniqueName}",
                         LogicalName = $"{prefix}_{fieldUniqueName}",
                         DisplayName = new Label(fieldDisplayName, languageCode),
                         RequiredLevel = fieldRequiredLevel,
                         Description = new Label("This field is created by SolMate", languageCode),
                         OptionSet = new OptionSetMetadata
                         {
                             IsGlobal = false,
                             OptionSetType = OptionSetType.Picklist,
                             Options =
                              {
                                 new OptionMetadata(
                                    new Label("Option1", languageCode), null),
                                 new OptionMetadata(
                                    new Label("Option2", languageCode), null),
                                 new OptionMetadata(
                                    new Label("Option3", languageCode), null)
                              }
                         }
                     };
                    addedAttributes.Add(pickListAttribute);
                    break;
                case "string":                    
                    StringAttributeMetadata stringAttribute = new StringAttributeMetadata
                    {
                        SchemaName = $"{prefix}_{fieldUniqueName}",
                        LogicalName = $"{prefix}_{fieldUniqueName}",
                        DisplayName = new Label(fieldDisplayName, languageCode),
                        RequiredLevel = fieldRequiredLevel,
                        Description = new Label("This field is created by SolMate", languageCode),
                        MaxLength = 100
                    };
                    addedAttributes.Add(stringAttribute);
                    break;
                case "multiselect":
                    MultiSelectPicklistAttributeMetadata multiSelectOptionSetAttribute = new MultiSelectPicklistAttributeMetadata()
                    {
                        SchemaName = $"{prefix}_{fieldUniqueName}",
                        LogicalName = $"{prefix}_{fieldUniqueName}",
                        DisplayName = new Label(fieldDisplayName, languageCode),
                        RequiredLevel = fieldRequiredLevel,
                        Description = new Label("This field is created by SolMate", languageCode),
                        OptionSet = new OptionSetMetadata()
                        {
                            IsGlobal = false,
                            OptionSetType = OptionSetType.Picklist,
                            Options = {
                            new OptionMetadata(new Label("First Option",languageCode),null),
                            new OptionMetadata(new Label("Second Option",languageCode),null),
                            new OptionMetadata(new Label("Third Option",languageCode),null)
                            }
                        }
                    };
                    addedAttributes.Add(multiSelectOptionSetAttribute);
                    break;
                default:
                    break;
                }
          
            foreach (AttributeMetadata columnDefinition in addedAttributes)
            {                
                CreateAttributeRequest request = new CreateAttributeRequest()
                {
                    EntityName = entityLogicalName,
                    Attribute = columnDefinition,
                    SolutionUniqueName = solutionUniqueName                    
                };
               
                var response = (CreateAttributeResponse)service.Execute(request);                
                tracingService.Trace($"Response: {response.AttributeId.ToString()}");
                Response.Set(context, response.AttributeId.ToString());
            }
        }
    }
}
