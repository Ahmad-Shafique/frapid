﻿using Frapid.Configuration;
using Frapid.DataAccess;
using Frapid.DataAccess.Models;

namespace Frapid.DbPolicy
{
    public class PolicyValidator : IPolicy
    {
        public AccessTypeEnum AccessType { get; set; }
        public string ObjectNamespace { get; set; }
        public string ObjectName { get; set; }
        public bool HasAccess { get; private set; }
        public long LoginId { get; set; }
        public string Database { get; set; }

        public void Validate()
        {
            this.HasAccess = Validate(this);
        }

        private static bool Validate(IPolicy policy)
        {
            if (policy.LoginId == 0)
            {
                return false;
            }

            string sql = FrapidDbServer.GetProcedureCommand(policy.Database, "auth.has_access", new[] {"@0", "@1", "@2"});

            string entity = policy.ObjectNamespace + "." + policy.ObjectName;
            int type = (int) policy.AccessType;

            bool result = Factory.Scalar<bool>(policy.Database, sql, policy.LoginId, entity, type);
            return result;
        }
    }
}