using SmartFood.Core.Enums;
using SmartFood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Drawing;
using System.Text;

namespace SmartFood.Core.Entity
{
    public class Admin : IEntity<int>
    {
        public Admin(string name, string email, string password)
        {
            RoleId = (int)UserType.Admin;
            Name = name;
            Email = email;
            Password = password;
        }

        [Key]
        public int Id { set; get; }
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }
        public Role Role { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public ICollection<Tokens> Tokens { get; set; }
    }
}
