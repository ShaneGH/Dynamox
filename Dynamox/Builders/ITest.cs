﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Builders
{
    public interface ITest
    {
        DxSettings Settings { get; }

        TestBuilder Builder { get; }

        void Run();
    }
}
