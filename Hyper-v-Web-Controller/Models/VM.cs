﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Hyper_v_Web_Controller.Models
{
    public enum VMState
    {
        Starting =-1,
        Creating = 0,
        Enabled = 2,
        Disabled = 3,
        Disabling = 4
    }
    public class VM
    {
        [Required]
        public int Id { get; set; }
        [Required]
        public string VmName { get; set; }
        public DateTime CreationTime { get; set; }
        public int UsageTime { get; set; }

        [Required]
        public int CreatorId { get; set; }
        public User Creator { get; set; }

        [Required]
        public int RealizedVMImageId { get; set; }
        public VMImage RealizedVMImage { get; set; }
        [NotMapped]
        public VMState machineState { get; set; } = VMState.Disabled;
        [NotMapped]
        public string? ip { get; set; } = null;

    }
}
