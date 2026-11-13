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
        public int? OrganizationId { get; set; }
        public Organization Organization { get; set; }
        public int? RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        public string Code { get; set; }
        public DateTime? CodeExpirDate { get; set; }
        public CodeOTP(int? OrganizationId, int? RestaurantId, string code, DateTime? codeExpirDate)
        {
            this.OrganizationId = OrganizationId;
            this.RestaurantId = RestaurantId;
            Code = code;
            CodeExpirDate = codeExpirDate;
        }
    }
}
