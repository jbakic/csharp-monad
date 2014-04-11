﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Monad;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.IO;

namespace Monad.UnitTests
{
    [TestClass]
    public class IOTests
    {
        [TestMethod]
        public void TestIOMonadLazyLoading()
        {
            var m = I.O(() =>
                System.IO.File.ReadAllBytes(Assembly.GetCallingAssembly().Location)
                );

            var data = m();
        }

        [TestMethod]
        public void TestIOMonadBinding()
        {
            string data = "Testing 123";

            var result = from tmpFileName   in GetTempFileName()
                         from _             in WriteFile(tmpFileName, data)
                         from dataFromFile  in ReadFile(tmpFileName)
                         from __            in DeleteFile(tmpFileName)
                         select dataFromFile;

            Assert.IsTrue(result.Invoke() == "Testing 123");
        }

        [TestMethod]
        public void TestIOMonadBindingFluent()
        {
            string data = "Testing 123";

            var result = GetTempFileName()
                            .Then( tmpFileName  => { WriteFile(tmpFileName, data)(); return tmpFileName; } )
                            .Then( tmpFileName  => { return new { tmpFileName, data = ReadFile(tmpFileName)() }; })
                            .Then( context      => { DeleteFile(context.tmpFileName)(); return context.data; } );

            Assert.IsTrue(result.Invoke() == "Testing 123");
        }

        private static IO<Unit> DeleteFile(string tmpFileName)
        {
            return () =>
                Unit.Return(
                    () => File.Delete(tmpFileName)
                );
        }

        private static IO<string> ReadFile(string tmpFileName)
        {
            return () => File.ReadAllText(tmpFileName);
        }

        private static IO<Unit> WriteFile(string tmpFileName, string data)
        {
            return () =>
                Unit.Return(
                    () => File.WriteAllText(tmpFileName, data)
                );
        }

        private static IO<string> GetTempFileName()
        {
            return () => Path.GetTempFileName();
        }
    }
}