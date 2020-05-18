using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WordpressApi.DAL.Models
{
    public class AddUserToEvent
    {
        public int EventId { get; set; }
        public int UserId { get; set; }
    }
}
