﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SQLiteKei.DataAccess.Models
{
    /// <summary>
    /// A model that represents a database's settings.
    /// </summary>
    public class DbSettings
    {
        public short? UserVersion { get; set; }

        public short? SchemaVersion { get; set; }

        public int? ApplicationId { get; set; }
    }
}
