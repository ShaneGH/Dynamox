﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Mocks;

namespace Dynamox.Compile
{
    /// <summary>
    /// Build a method for a dynamic type based on a property getter or setter
    /// </summary>
    public abstract class PropertyBuilder : MethodBuilder
    {
        public PropertyBuilder(TypeBuilder toType, FieldInfo objBase, MethodInfo parentMethod)
            : base(toType, objBase, parentMethod)
        {
        }

        protected override MethodAttributes GetAttrs(MethodInfo forMethod)
        {
            return base.GetAttrs(forMethod) | MethodAttributes.SpecialName;
        }
    }
}