using System;
using System.ComponentModel.DataAnnotations;

namespace WebSocketServer.Entities
{
    public class Employee
    {
        public Guid CompanyId { get; set; }
        [Key]
        public Guid Id { get; set; }
        public string Name { get; set; }
        public int Age { get; set; }
    }
}