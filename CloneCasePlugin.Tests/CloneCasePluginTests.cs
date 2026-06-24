using System;
using Microsoft.Xrm.Sdk;
using Moq;
using Xunit;

namespace CloneCasePlugin.Tests
{
    public class CloneCasePluginTests
    {
        [Fact]
        public void MissingTargetThrowsClearException()
        {
            PluginHarness harness = new PluginHarness();

            InvalidPluginExecutionException exception = Assert.Throws<InvalidPluginExecutionException>(
                () => harness.Plugin.Execute(harness.ServiceProvider.Object));

            Assert.Contains("Target input parameter is required", exception.Message);
            harness.OrganizationService.Verify(service => service.Create(It.IsAny<Entity>()), Times.Never);
        }

        [Fact]
        public void InvalidTargetTypeThrowsClearException()
        {
            PluginHarness harness = new PluginHarness();
            harness.InputParameters["Target"] = "not an entity reference";

            InvalidPluginExecutionException exception = Assert.Throws<InvalidPluginExecutionException>(
                () => harness.Plugin.Execute(harness.ServiceProvider.Object));

            Assert.Contains("must be an EntityReference", exception.Message);
        }

        [Fact]
        public void WrongLogicalNameThrowsClearException()
        {
            PluginHarness harness = new PluginHarness();
            harness.InputParameters["Target"] = new EntityReference("account", Guid.NewGuid());

            InvalidPluginExecutionException exception = Assert.Throws<InvalidPluginExecutionException>(
                () => harness.Plugin.Execute(harness.ServiceProvider.Object));

            Assert.Contains("incident entity", exception.Message);
        }

        [Fact]
        public void SuccessfulCloneCreatesIncident()
        {
            PluginHarness harness = PluginHarness.ForIncident(new Entity("incident") { ["title"] = "Original case" });

            harness.Plugin.Execute(harness.ServiceProvider.Object);

            harness.OrganizationService.Verify(service => service.Create(
                It.Is<Entity>(entity =>
                    entity.LogicalName == "incident" &&
                    (string)entity["title"] == "Original case" &&
                    ((OptionSetValue)entity["statecode"]).Value == 0 &&
                    ((OptionSetValue)entity["statuscode"]).Value == 1)), Times.Once);
        }

        [Fact]
        public void ExcludedFieldsAreNotCopied()
        {
            Entity source = new Entity("incident")
            {
                ["title"] = "Copy me",
                ["incidentid"] = Guid.NewGuid(),
                ["createdon"] = DateTime.UtcNow,
                ["modifiedon"] = DateTime.UtcNow,
                ["createdby"] = new EntityReference("systemuser", Guid.NewGuid()),
                ["modifiedby"] = new EntityReference("systemuser", Guid.NewGuid()),
                ["ownerid"] = new EntityReference("systemuser", Guid.NewGuid()),
                ["statecode"] = new OptionSetValue(1),
                ["statuscode"] = new OptionSetValue(2),
                ["ticketnumber"] = "CAS-123",
            };
            PluginHarness harness = PluginHarness.ForIncident(source);
            Entity created = null;
            harness.OrganizationService
                .Setup(service => service.Create(It.IsAny<Entity>()))
                .Callback<Entity>(entity => created = entity)
                .Returns(Guid.NewGuid());

            harness.Plugin.Execute(harness.ServiceProvider.Object);

            Assert.Equal("Copy me", created["title"]);
            Assert.False(created.Attributes.Contains("incidentid"));
            Assert.False(created.Attributes.Contains("createdon"));
            Assert.False(created.Attributes.Contains("modifiedon"));
            Assert.False(created.Attributes.Contains("createdby"));
            Assert.False(created.Attributes.Contains("modifiedby"));
            Assert.False(created.Attributes.Contains("ownerid"));
            Assert.False(created.Attributes.Contains("ticketnumber"));
            Assert.Equal(0, ((OptionSetValue)created["statecode"]).Value);
            Assert.Equal(1, ((OptionSetValue)created["statuscode"]).Value);
        }

        [Fact]
        public void CreatedGuidIsReturnedInClonedCaseId()
        {
            Guid clonedCaseId = Guid.NewGuid();
            PluginHarness harness = PluginHarness.ForIncident(new Entity("incident"));
            harness.OrganizationService.Setup(service => service.Create(It.IsAny<Entity>())).Returns(clonedCaseId);

            harness.Plugin.Execute(harness.ServiceProvider.Object);

            Assert.Equal(clonedCaseId, harness.OutputParameters["ClonedCaseId"]);
        }
    }

    internal sealed class PluginHarness
    {
        public readonly global::CloneCasePlugin.CloneCasePlugin Plugin = new global::CloneCasePlugin.CloneCasePlugin();
        public readonly Mock<IServiceProvider> ServiceProvider = new Mock<IServiceProvider>();
        public readonly Mock<IPluginExecutionContext> Context = new Mock<IPluginExecutionContext>();
        public readonly Mock<IOrganizationServiceFactory> ServiceFactory = new Mock<IOrganizationServiceFactory>();
        public readonly Mock<IOrganizationService> OrganizationService = new Mock<IOrganizationService>();
        public readonly Mock<ITracingService> TracingService = new Mock<ITracingService>();
        public readonly ParameterCollection InputParameters = new ParameterCollection();
        public readonly ParameterCollection OutputParameters = new ParameterCollection();

        public PluginHarness()
        {
            Context.SetupGet(context => context.InputParameters).Returns(InputParameters);
            Context.SetupGet(context => context.OutputParameters).Returns(OutputParameters);
            Context.SetupGet(context => context.UserId).Returns(Guid.NewGuid());
            ServiceFactory
                .Setup(factory => factory.CreateOrganizationService(It.IsAny<Guid?>()))
                .Returns(OrganizationService.Object);
            ServiceProvider.Setup(provider => provider.GetService(typeof(ITracingService))).Returns(TracingService.Object);
            ServiceProvider.Setup(provider => provider.GetService(typeof(IPluginExecutionContext))).Returns(Context.Object);
            ServiceProvider.Setup(provider => provider.GetService(typeof(IOrganizationServiceFactory))).Returns(ServiceFactory.Object);
        }

        public static PluginHarness ForIncident(Entity source)
        {
            PluginHarness harness = new PluginHarness();
            Guid sourceId = Guid.NewGuid();
            harness.InputParameters["Target"] = new EntityReference("incident", sourceId);
            harness.OrganizationService
                .Setup(service => service.Retrieve("incident", sourceId, It.IsAny<Microsoft.Xrm.Sdk.Query.ColumnSet>()))
                .Returns(source);
            harness.OrganizationService.Setup(service => service.Create(It.IsAny<Entity>())).Returns(Guid.NewGuid());
            return harness;
        }
    }
}
