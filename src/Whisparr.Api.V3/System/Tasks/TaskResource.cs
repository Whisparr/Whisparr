﻿using System;
using Whisparr.Http.REST;

namespace Whisparr.Api.V3.System.Tasks
{
    public class TaskResource : RestResource
    {
        public string Name { get; set; }
        public string TaskName { get; set; }
        public int Interval { get; set; }
        public DateTime LastExecution { get; set; }
        public DateTime LastStartTime { get; set; }
        public DateTime NextExecution { get; set; }

        public TimeSpan LastDuration => LastExecution - LastStartTime;
    }
}
