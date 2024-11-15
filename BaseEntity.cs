using System;
using System.Collections.Generic;
using System.Text;

namespace GenericRepository
{
   public class BaseEntity
    {
        public int CreatedBy { get; set; }
        public DateTime CreatedOn { get; set; }=DateTime.Now;
        public DateTime UpdatedOn { get; set; } = DateTime.Now;
        public int UpdatedBy { get; set; }  
        public int CompanyId { get; set; }  // CompanyId field

    }
}

