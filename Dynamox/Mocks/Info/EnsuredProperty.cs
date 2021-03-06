﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dynamox.Mocks.Info
{
    /// <summary>
    /// Flag for MockBuilder class to indicate that the property set should be "Ensured"
    /// </summary>
    interface IEnsuredProperty
    {
        /// <summary>
        /// The actual property value
        /// </summary>
        object PropertyValue { get; }

        /// <summary>
        /// If true, the value of this object is ensured, otherwise it is not
        /// </summary>
        bool IsEnsured { get; }
    }

    /// <summary>
    /// Flag for MockBuilder class to indicate that the property set should be "Ensured"
    /// </summary>
    internal class EnsuredProperty : IEnsuredProperty
    {
        readonly object PropertyValue;

        /// <summary>
        /// If true, the value of this object is ensured, otherwise it is not
        /// </summary>
        public bool IsEnsured { get; set; }

        /// <summary>
        /// The actual property value
        /// </summary>
        object IEnsuredProperty.PropertyValue
        {
            get { return PropertyValue; }
        }

        public EnsuredProperty(object property)
        {
            PropertyValue = property;
        }
    }
}
