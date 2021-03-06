﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Dynamic;

namespace Dynamox.Builders
{
    public interface ITestData
    {
        dynamic TestBag { get; }
        dynamic Args { get; }
        dynamic CArgs { get; }
    }

    public class TestData : ITestData
    {
        public dynamic TestBag { get; private set; }
        public dynamic Args { get; private set; }
        public dynamic CArgs { get; private set; }

        public TestData(DynamicBag testBag, DxSettings settings)
        {
            TestBag = testBag;
            Args = new TestArranger(settings);
            CArgs = new TestArranger(settings);
        }
    }
}
