using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OrderService.Domain.Enum
{
    public enum OrderStatus
    {
        Pending = 0,
        Completed = 10,
        Failed = 20,
        Cancelled = 30

    }
}
