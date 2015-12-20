﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Dynamox.Compile;
using NUnit.Framework;

namespace Dynamox.Tests.Compile
{
    [TestFixture]
    public class TypeOverrideDescriptorTests
    {
        public abstract class MethodTestType1
        {
            public void NonOverridable1() { }
            public virtual void NonOverridable4() { }

            // non overridable
            public void Overridable1(int val) { }
            public virtual void Overridable1() { }
            public abstract void Overridable2();
            public virtual void Overridable3() { }
        }

        public abstract class MethodTestType2 : MethodTestType1
        {
            public void NonOverridable2() { }
            public override void Overridable1() { }
            public sealed override void NonOverridable4() { }

            protected virtual void Overridable4() { }
            internal virtual void Overridable5() { }
            protected internal virtual void Overridable6() { }
            private void NonOverridable5() { }

            internal abstract void NonOverridable7();

        }

        public class MethodTestType3 : MethodTestType2
        {
            public void NonOverridable3() { }
            public override void Overridable1() { }
            public override void Overridable2() { }

            internal sealed override void NonOverridable7() { }

            public virtual int Property { get; set; }
        }

        [Test]
        public void MethodsTest()
        {
            // arrange
            // act
            var allMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var subject = new TypeOverrideDescriptor(typeof(MethodTestType3));

            // assert
            var methodAsserts = new Action<MethodInfo>[] 
            {
                m => Assert.AreEqual(m, typeof(MethodTestType3).GetMethod("Overridable1", allMembers, null, Type.EmptyTypes, null)),
                m => Assert.AreEqual(m, typeof(MethodTestType3).GetMethod("Overridable2", allMembers)),
                m => Assert.AreEqual(m, typeof(MethodTestType3).GetMethod("Overridable4", allMembers), "XX"),
                m => Assert.AreEqual(m, typeof(MethodTestType3).GetMethod("Overridable6", allMembers), "ZZ"),
                m => Assert.AreEqual(m, typeof(MethodTestType3).GetMethod("Overridable3", allMembers)),
                m => Assert.AreEqual(m, typeof(MethodTestType3).GetMethod("ToString", allMembers)),
                m => Assert.AreEqual(m, typeof(MethodTestType3).GetMethod("Equals", allMembers)),
                m => Assert.AreEqual(m, typeof(MethodTestType3).GetMethod("GetHashCode", allMembers)),
            };

            Assert.AreEqual(methodAsserts.Count(), subject.OverridableMethods.Count());
            for (var i = 0; i < subject.OverridableMethods.Count(); i++)
            {
                methodAsserts[i](subject.OverridableMethods.ElementAt(i));
            }
        }

        public abstract class PropertyTestType1
        {
            public int NonOverridable1 { get;set; }
            public virtual int NonOverridable4 { get; set; }

            public virtual int Overridable1 { get; set; }
            public abstract int Overridable2 { get; set; }
            public virtual int Overridable3 { get; set; }
        }

        public abstract class PropertyTestType2 : PropertyTestType1
        {
            public int NonOverridable2 { get; set; }
            public override int Overridable1 { get; set; }
            public sealed override int NonOverridable4 { get; set; }

            protected virtual int Overridable4 { get; set; }
            internal virtual int Overridable5 { get; set; }
            protected internal virtual int Overridable6 { get; set; }
            private int NonOverridable5 { get; set; }

            internal abstract int NonOverridable7 { get; set; }

        }

        public class PropertyTestType3 : PropertyTestType2
        {
            public int NonOverridable3 { get; set; }
            public override int Overridable1 { get; set; }
            public override int Overridable2 { get; set; }

            internal sealed override int NonOverridable7 { get; set; }
        }

        [Test]
        public void PropertiesTest()
        {
            // arrange
            // act
            var allMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var subject = new TypeOverrideDescriptor(typeof(PropertyTestType3));

            // assert
            var propertyAsserts = new Action<PropertyInfo>[] 
            {
                m => Assert.AreEqual(m, typeof(PropertyTestType3).GetProperty("Overridable1", allMembers)),
                m => Assert.AreEqual(m, typeof(PropertyTestType3).GetProperty("Overridable2", allMembers)),
                m => Assert.AreEqual(m, typeof(PropertyTestType3).GetProperty("Overridable3", allMembers)),
                m => Assert.AreEqual(m, typeof(PropertyTestType3).GetProperty("Overridable4", allMembers)),
                m => Assert.AreEqual(m, typeof(PropertyTestType3).GetProperty("Overridable5", allMembers)),
                m => Assert.AreEqual(m, typeof(PropertyTestType3).GetProperty("Overridable6", allMembers))
            };

            Assert.AreEqual(propertyAsserts.Count(), subject.OverridableProperties.Count());
            for (var i = 0; i < subject.OverridableProperties.Count(); i++)
            {
                propertyAsserts[i](subject.OverridableProperties.ElementAt(i));
            }
        }

        public interface I1
        {
            int Overridable1 { get; set; }
            void Overridable2();

            object this[int index] { get; set; }
        }

        public interface I2 : I1
        {
            int Overridable3 { get; set; }
            void Overridable4();

            object this[int index] { get; set; }

        }

        public class InterfaceTests1 : I2
        {
            int I2.Overridable3 { get; set; }
            void I2.Overridable4() { }
            int I1.Overridable1 { get; set; }
            void I1.Overridable2() { }

            object I1.this[int index]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            object I2.this[int index]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        public class InterfaceTests2 : I2
        {
            public int Overridable1 { get; set; }
            public void Overridable2() { }
            public int Overridable3 { get; set; }
            public void Overridable4() { }

            public object this[int index]
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }
        }

        [Test]
        public void InterfaceTest1()
        {
            InterfaceTest(typeof(InterfaceTests1));
        }

        [Test]
        public void InterfaceTest2()
        {
            InterfaceTest(typeof(InterfaceTests2));
        }

        [Test]
        public void InterfaceTest3()
        {
            InterfaceTest(typeof(I1));
        }

        [Test]
        public void InterfaceTest4()
        {
            InterfaceTest(typeof(I2));
        }

        void InterfaceTest(Type toTest)
        {
            // arrange
            // act
            var allMembers = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;
            var subject = new TypeOverrideDescriptor(toTest);

            // assert
            var interfaceAsserts = new List<Action<InterfaceDescriptor>>();
            if (typeof(I2).IsAssignableFrom(toTest))
                interfaceAsserts.Add(i =>
                {
                    Assert.AreEqual(i.OverridableIndexes.Count(), 1);
                    Assert.AreEqual(i.OverridableIndexes.ElementAt(0).ElementAt(0).ParameterType, typeof(int));
                    Assert.AreEqual(i.OverridableProperties.Count(), 1);
                    Assert.AreEqual(i.OverridableProperties.ElementAt(0), typeof(I2).GetProperty("Overridable3", allMembers));
                    Assert.AreEqual(i.OverridableMethods.Count(), 1);
                    Assert.AreEqual(i.OverridableMethods.ElementAt(0), typeof(I2).GetMethod("Overridable4", allMembers));
                });
            if (typeof(I1).IsAssignableFrom(toTest))
                interfaceAsserts.Add(i =>
                {
                    Assert.AreEqual(i.OverridableIndexes.Count(), 1);
                    Assert.AreEqual(i.OverridableIndexes.ElementAt(0).ElementAt(0).ParameterType, typeof(int));
                    Assert.AreEqual(i.OverridableProperties.Count(), 1);
                    Assert.AreEqual(i.OverridableProperties.ElementAt(0), typeof(I1).GetProperty("Overridable1", allMembers));
                    Assert.AreEqual(i.OverridableMethods.Count(), 1);
                    Assert.AreEqual(i.OverridableMethods.ElementAt(0), typeof(I1).GetMethod("Overridable2", allMembers));
                });

            Assert.AreEqual(interfaceAsserts.Count(), subject.OverridableInterfaces.Count());
            for (var i = 0; i < subject.OverridableInterfaces.Count(); i++)
            {
                interfaceAsserts[i](subject.OverridableInterfaces.ElementAt(i));
            }
        }

        [Test]
        public void AbstractInternal()
        {
            // arrange
            // act
            // assert
            Assert.IsFalse(new TypeOverrideDescriptor(typeof(MethodTestType1)).HasAbstractInternal);
            Assert.IsTrue(new TypeOverrideDescriptor(typeof(MethodTestType2)).HasAbstractInternal);
            Assert.IsFalse(new TypeOverrideDescriptor(typeof(MethodTestType3)).HasAbstractInternal);
        }

        public class TheIndexes
        {
            public object this[int index]
            {
                get { return null; }
                set { }
            }

            public virtual object this[int index1, string index2]
            {
                get { return null; }
                set { }
            }

            object this[bool index1, string index2]
            {
                get { return null; }
                set { }
            }
        }

        [Test]
        public void Indexes()
        {
            // arrange
            // act
            var desc = new TypeOverrideDescriptor(typeof(TheIndexes));

            // assert
            Assert.AreEqual(desc.OverridableIndexes.Count(), 1);
            Assert.AreEqual(desc.OverridableIndexes.ElementAt(0).Count(), 2);
            Assert.AreEqual(desc.OverridableIndexes.ElementAt(0).ElementAt(0).ParameterType, typeof(int));
            Assert.AreEqual(desc.OverridableIndexes.ElementAt(0).ElementAt(1).ParameterType, typeof(string));
        }
    }
}