﻿using System;
using System.Collections;
using System.Collections.Generic;

namespace House_management_Api.DTOs.Admin
{
    public class MemberViewDto
    {
        public string Id { get; set; }
        public string UserName { get; set; }
        public string LastName { get; set; }

        public string FirstName { get; set; }

        public bool IsLocked { get; set; }
        public DateTime DateCreated { get; set; }
        public IEnumerable<string> Roles { get; set; }
    }
}
