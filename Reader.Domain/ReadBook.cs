using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Reader.Domain
{
    public class ReadBook : BaseEntity
    {
        public long Id { get; set; }    
        public string Name { get; set; }
        public string GoogleId { get; set; }
        public DateTime? StartDate { get; set; }
        public DateTime? EndDate { get; set;}
    }
}
