using SmartFood.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace SmartFood.Core.Entity
{
    public class CodeOTP : IEntity<int>
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Organization))]
        public int? IdOrganization { get; set; }
        public Organization Organization { get; set; }

        [ForeignKey(nameof(Restaurant))]
        public int? IdRestaurant { get; set; }
        public Restaurant Restaurant { get; set; }
        public string Code { get; set; }
        public DateTime? CodeExpirDate { get; set; }
        public CodeOTP(int? idOrganization, int? idRestaurant, string code, DateTime? codeExpirDate)
        {
            IdOrganization = idOrganization;
            IdRestaurant = idRestaurant;
            Code = code;
            CodeExpirDate = codeExpirDate;
        }
    }
}
