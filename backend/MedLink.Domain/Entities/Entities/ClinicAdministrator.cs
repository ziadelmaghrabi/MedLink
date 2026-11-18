using MedLink.Domain.Common;
using MedLink.Domain.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Domain.Entities
{
    public class ClinicAdministrator : User
    {
         
        public int ProviderId { get; private set; }
        public HealthcareProvider Provider { get; private set; } = null!;

        public override string RoleName => RoleNames.ClinicAdministrator;

        private ClinicAdministrator() { }

        public ClinicAdministrator(string fullName, int providerId)
            : base(fullName)
        {
            ProviderId = Guard.RequireId(providerId, nameof(providerId));
        }
    }
}
