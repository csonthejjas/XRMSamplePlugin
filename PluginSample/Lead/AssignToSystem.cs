using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSample.Lead
{
    public class AssignToSystem : PluginBase
    {
        protected override void ExecuteContextLogic()
        {
            Entity lead = Service.Retrieve(PrimaryReference.LogicalName, PrimaryReference.Id, new ColumnSet("parentaccountid", "parentcontactid"));
            EntityReference accountRef = lead.GetAttributeValue<EntityReference>("parentaccountid");
            EntityReference contactRef = lead.GetAttributeValue<EntityReference>("parentcontactid");
            
            // default assignee is SYSTEM
            EntityReference assignee = SystemUser;

            // check if account exists on lead
            if(accountRef != null)
            {
                // assignee is account owner
                assignee = GetReferenceAttribute(accountRef, "ownerid");
            }

            // check if contact exists on lead
            else if(contactRef != null)
            {
                // check if account exists on contact
                EntityReference contactAccountRef = GetReferenceAttribute(contactRef, "parentcustomerid");
                if (contactAccountRef != null)
                {
                    // assignee is contacts parent account owner
                    assignee = GetReferenceAttribute(contactAccountRef, "ownerid");
                }
            }

            // assign lead to determined assignee
            AssignRequest request = new AssignRequest()
            {
                Assignee = assignee,
                Target = PrimaryReference
            };
            AssignResponse response = (AssignResponse)SystemService.Execute(request);
            Logger.Trace($"response {response}");
        }

        private EntityReference GetReferenceAttribute(EntityReference entityRef, string attributeName)
        {
            Entity entity = Service.Retrieve(entityRef.LogicalName, entityRef.Id, new ColumnSet(attributeName));
            return entity.GetAttributeValue<EntityReference>(attributeName);
        }
    }
}
