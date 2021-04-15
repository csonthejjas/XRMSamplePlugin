using Microsoft.Crm.Sdk.Messages;
using Microsoft.Xrm.Sdk;
using Microsoft.Xrm.Sdk.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PluginSample.Activity
{
    public class AssignToRegardingContactPartnerOwner : PluginBase
    {
        protected override void ExecuteContextLogic()
        {
            Entity activity = Service.Retrieve(PrimaryReference.LogicalName, PrimaryReference.Id, new ColumnSet("regardingobjectid", "msdyncrm_associatedcustomerjourneyiteration"));

            EntityReference cjIteration = activity.GetAttributeValue<EntityReference>("msdyncrm_associatedcustomerjourneyiteration");
            // early return if not created from CJ 
            if (cjIteration == null)
            {
                Logger.Trace("cjIteration is null");
                return;
            }

            EntityReference contactRef = activity.GetAttributeValue<EntityReference>("regardingobjectid");
            //early return if regardingobject is not contact
            if (contactRef == null || contactRef.LogicalName != "contact")
            {
                Logger.Trace("regardingobjectid is null");
                return;
            }

            EntityReference parentPartnerRef = GetReferenceAttribute(contactRef, "parentcustomerid");
            // early return if contact has no parent account
            if (parentPartnerRef == null)
            {
                Logger.Trace("parentPartner is null");
                return;
            }

            EntityReference assignee = GetReferenceAttribute(parentPartnerRef, "ownerid");
            // create request to assign acting record to loaded account owner
            AssignRequest request = new AssignRequest()
            {
                Assignee = assignee,
                Target = PrimaryReference
            };
            AssignResponse response = (AssignResponse)SystemService.Execute(request);
            Logger.Trace($"response: {response}");
        }
        private EntityReference GetReferenceAttribute(EntityReference entityRef, string attributeName)
        {
            Entity entity = Service.Retrieve(entityRef.LogicalName, entityRef.Id, new ColumnSet(attributeName));
            return entity.GetAttributeValue<EntityReference>(attributeName);
        }

    }
}
