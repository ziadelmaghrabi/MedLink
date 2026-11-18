using MedLink.Domain.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Domain.Entities
{
    public class Medication : BaseEntity
    {
        public string DrugName { get; private set; } = string.Empty;

        private Medication() { }

        public Medication(string drugName)
        {
            DrugName = Guard.RequireText(drugName, nameof(drugName));
        }

        public void Rename(string drugName)
        {
            DrugName = Guard.RequireText(drugName, nameof(drugName));
        }
    }
}
