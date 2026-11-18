using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MedLink.Domain.Enums
{
    public enum ReminderStatus
    {
        Scheduled = 1,
        Taken = 2,
        Snoozed = 3,
        NotTaken = 4
    }
}
