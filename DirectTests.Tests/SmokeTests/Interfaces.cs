﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace DirectTests.Tests.SmokeTests
{
    [TestFixture]
    public class Interfaces
    {
        public interface IRepo1
        {
            Add.Entity GetEntity(int id);
        }

        public interface IRepoFactory
        {
            IRepo1 GetRepo(bool whatever);
        }

        public class Add
        {
            readonly IRepoFactory RepositoryFactory;
            readonly IRepo1 Repo;

            public Add(IRepoFactory repo1)
            {
                RepositoryFactory = repo1;
            }

            public Add(IRepo1 repo1)
            {
                Repo = repo1;
            }

            public int Execute1(int id, int add)
            {
                var entity = Repo.GetEntity(id);
                return entity.Number * add;
            }

            public int Execute2(int id, int add)
            {
                var repo = RepositoryFactory.GetRepo(true);
                var entity = repo.GetEntity(id);
                return entity.Number * add;
            }

            public class Entity
            {
                public int Id { get; set; }
                public int Number { get; set; }
            }
        }

        [Test]
        public void SimpleMethodMock()
        {
            Framework.Test("My test")
                .Arrange(bag =>
                {
                    bag.entityNumber = 2;
                    bag.id = 55;
                    bag.repo
                        .GetEntity(Framework.Method<int>(a => a == bag.id))
                        .Return(new Add.Entity { Id = bag.id, Number = bag.entityNumber });
                })

                .UseParentAct()
                .Act(bag =>
                {
                    bag.constructorNumber = 55;
                    var subject = new Add(bag.repo.As<IRepo1>());
                    return subject.Execute1((int)bag.id, (int)bag.constructorNumber);
                })

                .SkipParentAssert()
                .Assert((bag, result) =>
                {
                    if (result != 110)
                        throw new InvalidOperationException();
                })
                
                .Run();
        }

        [Test]
        public void FactoryTypeMock()
        {
            Framework.Test("My test")
                .Arrange(bag =>
                {
                    bag.entityNumber = 2;
                    bag.commandId = 55;
                    bag.factory
                        .GetRepo(true)
                        .GetEntity(Framework.Method<int>(a => a == bag.commandId))
                        .Return(new Add.Entity { Number = bag.entityNumber });
                })

                .UseParentAct()
                .Act(bag =>
                {
                    bag.constructorNumber = 55;
                    var subject = new Add(bag.factory.As<IRepoFactory>());
                    return subject.Execute2(bag.commandId, bag.constructorNumber);
                })

                .SkipParentAssert()
                .Assert((bag, result) =>
                {
                    if (result != 110)
                        throw new InvalidOperationException();
                })
                
                .Run();
        }

        [Test]
        public void SimpleForSubjectAct1()
        {
            Framework.Test("My test")
                .Subject(typeof(Add).GetConstructor(new Type[] { typeof(IRepo1) }))
                .For(typeof(Add).GetMethod("Execute1"))
                .Arrange(bag =>
                {
                    bag.Args.add = 2;
                    bag.Args.id = 55;
                    bag.CArgs.repo1
                        .GetEntity(Framework.Method<int>(a => a == bag.Args.id))
                        .Return(new Add.Entity { Id = bag.Args.id, Number = bag.Args.id });
                })

                .SkipParentAssert()
                .Assert((bag, result) =>
                {
                    if ((int)result != 110)
                        throw new InvalidOperationException();
                })

                .Run();
        }

        //[Test]
        //public void SimpleForSubjectAct2()
        //{
        //    Framework.Test("My test")
        //        .Subject(() => new Add((IRepo1)null))
        //        .For(a => a.Execute1(0, 0))
        //        .Arrange(bag =>
        //        {
        //            bag.Args.add = 2;
        //            bag.Args.id = 55;
        //            bag.CArgs.repo1
        //                .GetEntity(Framework.Method<int>(a => a == bag.Args.id))
        //                .Return(new Add.Entity { Id = bag.Args.id, Number = bag.Args.id });
        //        })

        //        .SkipParentAssert()
        //        .Assert((bag, result) =>
        //        {
        //            if (result != 110)
        //                throw new InvalidOperationException();
        //        })

        //        .Run();
        //}
    }
}
