using System;
using System.ServiceModel;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;

namespace CloneCasePlugin
{
    public class CloneCasePlugin : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            ITracingService tracingService = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            IPluginExecutionContext context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            IOrganizationServiceFactory serviceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            IOrganizationService service = serviceFactory.CreateOrganizationService(context.UserId);

            try
            {
                if (context.InputParameters.Contains("Target") && context.InputParameters["Target"] is EntityReference targetRef)
                {
                    // Make sure it's the Case entity
                    if (targetRef.LogicalName != "incident")
                        return;

                    // Retrieve the full original Case using the EntityReference
                    Entity originalCase = service.Retrieve("incident", targetRef.Id, new ColumnSet(true));

                    // Now clone
                    Entity clonedCase = new Entity("incident");
                    foreach (var attribute in originalCase.Attributes)
                    {
                        if (!attribute.Key.Equals("incidentid") &&
                            !attribute.Key.Equals("createdon") &&
                            !attribute.Key.Equals("modifiedon") &&
                            !attribute.Key.Equals("createdby") &&
                            !attribute.Key.Equals("modifiedby") &&
                            !attribute.Key.Equals("ownerid") &&
                            !attribute.Key.Equals("statecode") &&
                            !attribute.Key.Equals("statuscode") &&
                            !attribute.Key.Equals("ticketnumber"))
                        {
                            clonedCase[attribute.Key] = attribute.Value;
                        }
                    }

                    clonedCase["statecode"] = new OptionSetValue(0);
                    clonedCase["statuscode"] = new OptionSetValue(1);

                    Guid clonedCaseId = service.Create(clonedCase);
                    context.OutputParameters["ClonedCaseId"] = clonedCaseId;
                }
            }
            catch (FaultException<OrganizationServiceFault> ex)
            {
                tracingService.Trace("CloneCasePlugin Error: {0}", ex);
                throw new InvalidPluginExecutionException("CloneCasePlugin encountered an error.", ex);
            }
            catch (Exception ex)
            {
                tracingService.Trace("CloneCasePlugin Unexpected Error: {0}", ex);
                throw;
            }
        }
    }
}
