﻿using System;
using System.ComponentModel.DataAnnotations;

namespace E_Radar.Data.Models
{
    public class SentMessageModel
    {
        [Required]
        public long Id { get; set; }
        [Required]
        public DateTime CreatedTime { get; set; }
        [Required]
        public DateTime SentTime { get; set; }
        [Required]
        public string UniqueId { get; set; }
        [Required]
        public string From { get; set; }
        [Required]
        public string To { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
    }
}