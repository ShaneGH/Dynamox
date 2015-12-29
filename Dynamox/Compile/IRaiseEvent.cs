﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Compile
{
    /// <summary>
    /// Signals that the implementing class has events to raise
    /// </summary>
    public interface IRaiseEvent
    {
        bool RaiseEvent(string eventName, object[] args);
    }
}
