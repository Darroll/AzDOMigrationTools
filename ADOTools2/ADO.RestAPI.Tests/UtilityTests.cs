using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ADO.RestAPI.Tests
{
    [TestClass]
    public class UtilityTests
    {
        [TestMethod]
        public void Test_base64_decode()
        {
            string toDecode1 = "Uy0xLTktMTU1MTM3NDI0NS0xMDA2Mjg3MjIzLTQxMTQ0NDYxNTAtMjE1NjYwNDAyNC00MDA2Njg1ODg4LTEtMTA2NTk1ODM1Ny01NzQ0MDYyMTgtMjE2MTQwNzM2Ni0yNDA3MDMyMzc";
            string toDecode2 = "Uy0xLTktMTU1MTM3NDI0NS00MTcwNzg4NDcyLTM0MzkzNDQ3MTQtMjg5MzE0ODY0NC0zMDYwNDE5MzI4LTEtMTI2OTI0NzIzNi0xNzc5NjYzNjgwLTI2MjAxMjk0NjItODQ2MDU1Mzc4";
            int len = toDecode1.Length;
            var actual1 = ADO.Tools.Utility.Base64Decode(toDecode1);
            var actual2 = ADO.Tools.Utility.Base64Decode(toDecode2);
            var expected1 = "S-1-9-1551374245-1006287223-4114446150-2156604024-4006685888-1-1065958357-574406218-2161407366-240703237";
            var expected2 = "S-1-9-1551374245-4170788472-3439344714-2893148644-3060419328-1-1269247236-1779663680-2620129462-846055378";
            Assert.AreEqual(expected1, actual1);
            Assert.AreEqual(expected2, actual2);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public void Test_version()
        {
            string version = "";
            var cv = new Version(version);
        }
    }
}