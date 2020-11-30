using System;
using System.Collections.Generic;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ADO.Engine.BusinessEntities;

namespace ADO.Configuration.Tests
{
    [TestClass]
    public class TestCadences
    {
        private static byte fiscalMonthStart;
        private static int calendarYear;
        private static Cadence portfolioCadenceYear;
        private static Cadence portfolioCadenceSemester;
        private static Cadence portfolioCadenceQuarter;
        private static Cadence programCadence;
        private static Cadence sprintCadence;

        // Use ClassInitialize to run code before running the first test in the class
        [ClassInitialize()]
        public static void MyClassInitialize(TestContext testContext)
        {
            fiscalMonthStart = 8;
            calendarYear = 2019;
            portfolioCadenceYear = new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.FiscalYear, 0, 0, DayOfWeek.Sunday, 7);
            portfolioCadenceSemester = new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.FiscalSemester, 0, 0, DayOfWeek.Sunday, 7);
            portfolioCadenceQuarter = new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.FiscalQuarter, 0, 0, DayOfWeek.Sunday, 7);
            programCadence = new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.ProgramIncrement, 2, 5, DayOfWeek.Sunday, 7);
            sprintCadence = new Cadence(new DateTime(2019, 1, 1), new DateTime(2020, 12, 31), CadenceType.Sprint, 2, 0, DayOfWeek.Sunday, 7);
        }
        [TestMethod]
        public void TestQuarters()
        {
            for (var month = 1; month <= 12; month++)
            {
                var date = new DateTime(calendarYear, month, 01);
                Trace.WriteLine(string.Format("Date: {0}, Quarter: {1}, Financial Quarter: {2}", date, date.GetQuarter(), date.GetFinancialQuarter(fiscalMonthStart)));
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestQuarters_Equivalent()
        {
            for (var month = 1; month <= 12; month++)
            {
                var date = new DateTime(calendarYear, month, 01);
                Trace.WriteLine(string.Format("Date: {0}, Quarter: {1}, Financial Quarter: {2}", date, date.GetQuarter(), date.GetFinancialQuarter(1)));
                Assert.AreEqual(date.GetQuarter(), date.GetFinancialQuarter(1));
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestSemesters()
        {
            for (var month = 1; month <= 12; month++)
            {
                var date = new DateTime(calendarYear, month, 01);
                Trace.WriteLine(string.Format("Date: {0}, Semester: {1}, Financial Semester: {2}", date, date.GetSemester(), date.GetFinancialSemester(fiscalMonthStart)));
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestSemesters_Equivalent()
        {
            for (var month = 1; month <= 12; month++)
            {
                var date = new DateTime(calendarYear, month, 01);
                Trace.WriteLine(string.Format("Date: {0}, Semester: {1}, Financial Semester: {2}", date, date.GetSemester(), date.GetFinancialSemester(1)));
                Assert.AreEqual(date.GetSemester(), date.GetFinancialSemester(1));
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestYears()
        {
            for (var month = 1; month <= 12; month++)
            {
                var date = new DateTime(calendarYear, month, 01);
                Trace.WriteLine(string.Format("Date: {0}, Year: {1}, Financial Year: {2}", date, date.GetYear(), date.GetFinancialYear(fiscalMonthStart)));
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void TestYears_Equivalent()
        {
            for (var month = 1; month <= 12; month++)
            {
                var date = new DateTime(calendarYear, month, 01);
                Trace.WriteLine(string.Format("Date: {0}, Year: {1}, Financial Year: {2}", date, date.GetYear(), date.GetFinancialYear(1)));
                Assert.AreEqual(date.GetYear(), date.GetFinancialYear(1));
            }
            Assert.IsTrue(true);
        }
        [TestMethod]
        public void Test_Get_All_Iterations()
        {
            string pathPrefix = null;
            portfolioCadenceYear.GetIterations(pathPrefix, TeamLevel.Epic);
            portfolioCadenceSemester.GetIterations(pathPrefix, TeamLevel.Epic);
            portfolioCadenceQuarter.GetIterations(pathPrefix, TeamLevel.Epic);
            programCadence.GetIterations(pathPrefix, TeamLevel.Feature);
            sprintCadence.GetIterations(pathPrefix, TeamLevel.Requirement);
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void Test_CadenceOverlap()
        {
            List<Cadence> twoDistinctCadences
                = new List<Cadence>()
                {
                    new Cadence() {CadenceStart=new DateTime(2019,1,1), CadenceEnd=new DateTime(2019,1,31)},
                    new Cadence() {CadenceStart=new DateTime(2019,2,1), CadenceEnd=new DateTime(2019,2,28)},
                };
            Assert.IsTrue(twoDistinctCadences.CheckHasNoCadenceOverlap());
            List<Cadence> twoOverlappingByOneDay
                = new List<Cadence>()
                {
                    new Cadence() {CadenceStart=new DateTime(2019,1,1), CadenceEnd=new DateTime(2019,1,31)},
                    new Cadence() {CadenceStart=new DateTime(2019,1,31), CadenceEnd=new DateTime(2019,2,28)},
                };
            Assert.IsFalse(twoOverlappingByOneDay.CheckHasNoCadenceOverlap());
            List<Cadence> twoOverlappingByOneDaySwapOrder
                = new List<Cadence>()
                {
                    new Cadence() {CadenceStart=new DateTime(2019,1,31), CadenceEnd=new DateTime(2019,2,28)},
                    new Cadence() {CadenceStart=new DateTime(2019,1,1), CadenceEnd=new DateTime(2019,1,31)},
                };
            Assert.IsFalse(twoOverlappingByOneDaySwapOrder.CheckHasNoCadenceOverlap());
            List<Cadence> oneCadenceContainingTheOther
                = new List<Cadence>()
                {
                    new Cadence() {CadenceStart=new DateTime(2019,1,15), CadenceEnd=new DateTime(2019,1,19)},
                    new Cadence() {CadenceStart=new DateTime(2019,1,1), CadenceEnd=new DateTime(2019,1,31)},
                };
            Assert.IsFalse(oneCadenceContainingTheOther.CheckHasNoCadenceOverlap());
            List<Cadence> oneCadenceOverlappingTheOther
                = new List<Cadence>()
                {
                    new Cadence() {CadenceStart=new DateTime(2019,1,15), CadenceEnd=new DateTime(2019,2,15)},
                    new Cadence() {CadenceStart=new DateTime(2019,1,1), CadenceEnd=new DateTime(2019,1,31)},
                };
            Assert.IsFalse(oneCadenceOverlappingTheOther.CheckHasNoCadenceOverlap());
        }
    }
}
