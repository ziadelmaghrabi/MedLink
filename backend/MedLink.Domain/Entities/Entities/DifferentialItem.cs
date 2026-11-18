using MedLink.Domain.Common;
using MedLink.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Domain.Entities
{
    public class DifferentialItem : BaseEntity
    {
        public int AIDraftId { get; private set; }
        public AIDraft AIDraft { get; private set; } = null!;

        public int Rank { get; private set; }
        public string Diagnosis { get; private set; } = string.Empty;
        public string SupportingFindings { get; private set; } = string.Empty;
        public string? MissingInformation { get; private set; }

       
        public DifferentialDecision? Decision { get; private set; }

        private DifferentialItem() { }

        
        internal DifferentialItem(int rank, string diagnosis, string supportingFindings, string? missingInformation)
        {
            Rank = Guard.RequirePositive(rank, nameof(rank));
            Diagnosis = Guard.RequireText(diagnosis, nameof(diagnosis));
            SupportingFindings = Guard.RequireText(supportingFindings, nameof(supportingFindings));
            MissingInformation = Guard.OptionalText(missingInformation);
        }

        public void Decide(DifferentialDecision decision)
        {
            Decision = decision;
        }
    }
}


