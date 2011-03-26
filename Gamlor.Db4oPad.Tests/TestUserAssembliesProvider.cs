using System;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Gamlor.Db4oPad.Tests
{
    [TestFixture]
    public class TestUserAssembliesProvider
    {
        private readonly static string TwoAssemblies = @"..\..\Gamlor.Db4oPad.ExternalAssemblyForTests.dll"
                                              + Environment.NewLine +
                                              @"..\..\Other.Assembly.For.Loading.Tests.Db4objects.Db4o.dll";

        [Test]
        public void CreateEmptyOneWithNull()
        {
            SetupContext();
            var context = UserAssembliesProvider.CreateForCurrentAssemblyContext(null);
            Assert.IsFalse(context.GetAssemblies().Any());
        }
        [Test]
        public void CreateEmptyOneWitEmptyString()
        {
            SetupContext();
            var context = UserAssembliesProvider.CreateForCurrentAssemblyContext("");
            Assert.IsFalse(context.GetAssemblies().Any());
        }
        [Test]
        public void ReturnsASingleAssembly()
        {
            var ctxName = SetupContext();
            var context = UserAssembliesProvider.CreateForCurrentAssemblyContext(@"..\..\Gamlor.Db4oPad.ExternalAssemblyForTests.dll");
            string path = context.GetAssemblies().Single();
            Assert.AreEqual("Gamlor.Db4oPad.ExternalAssemblyForTests.dll",Path.GetFileName(path));
            Assert.IsTrue(path.Contains(Path.GetTempPath()));
            Assert.IsTrue(path.Contains(ctxName));
        }
        [Test]
        public void ReturnsMultiple()
        {
            var ctxName = SetupContext();
            var context = UserAssembliesProvider.CreateForCurrentAssemblyContext(TwoAssemblies);
            var assemblies = context.GetAssemblies();
            Assert.IsTrue(assemblies.All(p=>p.Contains(Path.GetTempPath())));
            Assert.IsTrue(assemblies.All(p => p.Contains(ctxName)));
            Assert.AreEqual(2, assemblies.Count());
        }
        [Test]
        public void CanRecreate()
        {
            var ctxName = SetupContext();
            var context1 = UserAssembliesProvider.CreateForCurrentAssemblyContext(TwoAssemblies);
            var context2 = UserAssembliesProvider.CreateForCurrentAssemblyContext(TwoAssemblies);
            var assemblies = context2.GetAssemblies();
            Assert.AreEqual(2, assemblies.Count());
        }
        [Test]
        public void FiltersDb4oAssemblies()
        {
            var originalAssemblies = TwoAssemblies
                +Environment.NewLine + @"Db4objects.Db4o.dll"
                +Environment.NewLine + @"Db4objects.Db4o.Linq.dll";
            var ctxName = SetupContext();
            var context = UserAssembliesProvider.CreateForCurrentAssemblyContext(originalAssemblies);
            var assemblies = context.GetAssemblies();
            Assert.AreEqual(2, assemblies.Count());
        }
        [Test]
        public void RestoreContext()
        {
            var ctxName = SetupContext();

            UserAssembliesProvider.CreateForCurrentAssemblyContext(TwoAssemblies);
            var context = UserAssembliesProvider.Restore();
            var assemblies = context.GetAssemblies();
            Assert.IsTrue(assemblies.All(p=>p.Contains(Path.GetTempPath())));
            Assert.IsTrue(assemblies.All(p => p.Contains(ctxName)));
            Assert.AreEqual(2, assemblies.Count());
        }
        [Test]
        public void DontFailIfNoContestAvailable()
        {
            SetupContext();
            var context = UserAssembliesProvider.Restore();
            var assemblies = context.GetAssemblies();
            Assert.AreEqual(0, assemblies.Count());
        }

        private string SetupContext()
        {
            var name = NewContextName();
            AppDomain.CurrentDomain.SetData(UserAssembliesProvider.IdKey,name);
            return name;
        }

        private string NewContextName()
        {
            return Guid.NewGuid().ToString();
        }
    }
}