using MedLink.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Domain.Entities
{
    public class ProviderInsurance
    {
        public int ProviderId { get; private set; }
        public HealthcareProvider Provider { get; private set; } = null!;

        public int InsuranceCompanyId { get; private set; }
        public InsuranceCompany InsuranceCompany { get; private set; } = null!;

        private ProviderInsurance() { }

         
        internal ProviderInsurance(int insuranceCompanyId)
        {
            InsuranceCompanyId = Guard.RequireId(insuranceCompanyId, nameof(insuranceCompanyId));
        }
    }
}

