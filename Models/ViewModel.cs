using System;
using System.Collections.Generic;

namespace ExpenceTracker.Models
{
    public class ViewModel
    {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
    public class HeadModel
    {
        public int accid { get; set; }      
        public string headname { get; set; }
        public long code { get; set; }
      
        public bool ishead { get; set; }
    }
    public class Transections
    {
        public int accid { get; set; }
        public int cashid { get; set; }
        
        public decimal amount { get; set; }
       
    }
    public class Headbind
    {
        public List< Accountshead> cash { get; set; }
        public List<Accountshead> exp { get; set; }


    }
}
