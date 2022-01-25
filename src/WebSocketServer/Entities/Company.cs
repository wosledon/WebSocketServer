using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WebSocketServer.Entities
{
    public class Company
    {
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Introduction { get; set; }


        public ICollection<Employee> Employees { get; set; }
    }
}