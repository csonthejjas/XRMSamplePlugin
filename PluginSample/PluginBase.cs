using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSample
{
    public abstract class PluginBase : IPlugin
    {
        public void Execute(IServiceProvider serviceProvider)
        {
            // Entry point
            InitService(serviceProvider);
            if (!TargetExists())
            {
                Logger.Trace("TARGET ENTITY NOT FOUND.");
                return;
            };
            InitPrimaryEntity();
            Logger.Trace("INIT DONE");
            // Execute actual plugin code
            ExecuteContextLogic();
            Logger.Trace("EXECUTE DONE");
        }

        protected abstract void ExecuteContextLogic();
        private void InitService(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider;
            Logger = (ITracingService)serviceProvider.GetService(typeof(ITracingService));
            Context = (IPluginExecutionContext)serviceProvider.GetService(typeof(IPluginExecutionContext));
            ServiceFactory = (IOrganizationServiceFactory)serviceProvider.GetService(typeof(IOrganizationServiceFactory));
            Service = ServiceAs(Context.UserId);
            SystemUser = new EntityReference("systemuser", GetSystemuserID("SYSTEM")); 
            SystemService = ServiceAs(SystemUser.Id);
        }
        private bool TargetExists()
        {
            return Context.InputParameters.Contains("Target");
        }
        private void InitPrimaryEntity()
        {
            switch (Context.MessageName)
            {
                case "Delete":
                    PrimaryReference = Context.InputParameters["Target"] as EntityReference;
                    PrimaryEntity = Service.Retrieve(PrimaryReference.LogicalName, PrimaryReference.Id, new ColumnSet(true));
                    break;
                default:
                    PrimaryEntity = Context.InputParameters["Target"] as Entity;
                    PrimaryReference = new EntityReference(Context.PrimaryEntityName, Context.PrimaryEntityId);
                    break;
            }
            Logger.Trace($"Primary reference: {PrimaryReference?.LogicalName} ({PrimaryReference?.Id.ToString()})");
        }
        protected IOrganizationService ServiceAs(Guid userId)
        {
            return ServiceFactory.CreateOrganizationService(userId);
        }
        protected IOrganizationService ServiceAs(string userName)
        {
            return ServiceAs(GetSystemuserID(userName));
        }
        private Guid GetSystemuserID(string userName)
        {
            QueryByAttribute queryUsers = new QueryByAttribute
            {
                EntityName = "systemuser",
                ColumnSet = new ColumnSet("systemuserid")
            };

            queryUsers.AddAttributeValue("fullname", userName);
            EntityCollection retrievedUsers = Service.RetrieveMultiple(queryUsers);
            Guid systemUserId = ((Entity)retrievedUsers.Entities[0]).Id;

            return systemUserId;
        }
        protected Entity PrimaryEntity { get; set; }
        protected EntityReference PrimaryReference { get; set; }
        protected EntityReference SystemUser { get; set; }

        protected ITracingService Logger { get; set; }

        protected IServiceProvider ServiceProvider { get; set; }
        protected IOrganizationService Service { get; set; }
        protected IOrganizationService SystemService { get; set; }
        protected IPluginExecutionContext Context { get; set; }
        protected IOrganizationServiceFactory ServiceFactory { get; set; }
    }
}
