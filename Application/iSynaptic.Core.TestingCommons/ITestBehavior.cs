﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace iSynaptic.Core.TestingCommons
{
    public interface ITestBehavior
    {
        void BeforeTest();
        void AfterTest();
    }
}
