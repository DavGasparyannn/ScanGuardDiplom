﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ScanGuard.Domain.Enums;

namespace ScanGuard.Domain.Entity
{
    public class VulnerabilityEntity
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public WebsiteScanEntity ScanEntity { get; set; }
        public VulnerabilityType VulnerabilityType { get; set; }
    }
}
