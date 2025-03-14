﻿using System.ComponentModel.DataAnnotations;

namespace z76_backend.Models
{
    [TableName("user")]
    public class UserEntity
    {
        [Key]
        public Guid user_id { get; set; }
        public string username { get; set; }
        public string full_name { get; set; }

        public string password { get; set; }
        public string stock_manage { get; set; }
    }
}
