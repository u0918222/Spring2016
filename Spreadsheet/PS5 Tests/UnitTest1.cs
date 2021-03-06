﻿using SS;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using Formulas;
using System.IO;
using System.Text.RegularExpressions;
namespace GradingTests
{
    /// <summary>
    /// These are grading tests for PS5
    ///</summary>
    [TestClass()]
    public class SpreadsheetTest
    {
        /// <summary>
        /// Used to make assertions about set equality.  Everything is converted first to
        /// upper case.
        /// </summary>
        public static void AssertSetEqualsIgnoreCase(IEnumerable<string> s1, IEnumerable<string> s2)
        {
            var set1 = new HashSet<String>();
            foreach (string s in s1)
            {
                set1.Add(s.ToUpper());
            }

            var set2 = new HashSet<String>();
            foreach (string s in s2)
            {
                set2.Add(s.ToUpper());
            }

            Assert.IsTrue(new HashSet<string>(set1).SetEquals(set2));
        }
        // EMPTY SPREADSHEETS
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test1()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.GetCellContents(null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test2()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.GetCellContents("AA");
        }

        [TestMethod()]
        public void Test3()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Assert.AreEqual("", s.GetCellContents("A2"));
        }

        // SETTING CELL TO A DOUBLE
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test4()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "1.5");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test5()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1A", "1.5");
        }

        [TestMethod()]
        public void Test6()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "1.5");
            Assert.AreEqual(1.5, (double)s.GetCellContents("Z7"), 1e-9);
        }

        // SETTING CELL TO A STRING
        [TestMethod()]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Test7()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A8", (string)null);
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test8()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, "hello");
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test9()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("AZ", "hello");
        }

        [TestMethod()]
        public void Test10()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7", "hello");
            Assert.AreEqual("hello", s.GetCellContents("Z7"));
        }

        // SETTING CELL TO A FORMULA
        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test11()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell(null, new Formula("2").ToString());
        }

        [TestMethod()]
        [ExpectedException(typeof(InvalidNameException))]
        public void Test12()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("AZ", new Formula("2").ToString());
        }

        [TestMethod()]
        public void Test13()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("Z7","=3");
            Formula f = (Formula)s.GetCellContents("Z7");
            Assert.AreEqual(3, f.Evaluate(x => 0), 1e-6);
        }

        // CIRCULAR FORMULA DETECTION
        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test14()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1","=A2");
            s.SetContentsOfCell("A2","=A1");
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test15()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1","=A2+A3");
            s.SetContentsOfCell("A3","=A4+A5");
            s.SetContentsOfCell("A5","=A6+A7");
            s.SetContentsOfCell("A7","=A1+A1");
        }

        [TestMethod()]
        [ExpectedException(typeof(CircularException))]
        public void Test16()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            try
            {
                s.SetContentsOfCell("A1","=A2+A3");
                s.SetContentsOfCell("A2", "15");
                s.SetContentsOfCell("A3", "30");
                s.SetContentsOfCell("A2","=A3*A1");
            }
            catch (CircularException e)
            {
                Assert.AreEqual(15, (double)s.GetCellContents("A2"), 1e-9);
                throw e;
            }
        }

        // NONEMPTY CELLS
        [TestMethod()]
        public void Test17()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test18()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test19()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("C2", "hello");
            s.SetContentsOfCell("C2", "");
            Assert.IsFalse(s.GetNamesOfAllNonemptyCells().GetEnumerator().MoveNext());
        }

        [TestMethod()]
        public void Test20()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] { "B1" });
        }

        [TestMethod()]
        public void Test21()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "52.25");
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] { "B1" });
        }

        [TestMethod()]
        public void Test22()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", new Formula("3.5").ToString());
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] { "B1" });
        }

        [TestMethod()]
        public void Test23()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", "hello");
            s.SetContentsOfCell("B1", new Formula("3.5").ToString());
            AssertSetEqualsIgnoreCase(s.GetNamesOfAllNonemptyCells(), new string[] { "A1", "B1", "C1" });
        }

        // RETURN VALUE OF SET CELL CONTENTS
        [TestMethod()]
        public void Test24()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("B1", "hello");
            s.SetContentsOfCell("C1", new Formula("5").ToString());
            AssertSetEqualsIgnoreCase(s.SetContentsOfCell("A1", "17.2"), new string[] { "A1" });
        }

        [TestMethod()]
        public void Test25()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("C1", new Formula("5").ToString());
            AssertSetEqualsIgnoreCase(s.SetContentsOfCell("B1", "hello"), new string[] { "B1" });
        }

        [TestMethod()]
        public void Test26()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "17.2");
            s.SetContentsOfCell("B1", "hello");
            AssertSetEqualsIgnoreCase(s.SetContentsOfCell("C1", new Formula("5").ToString()), new string[] { "C1" });
        }

        [TestMethod()]
        public void Test27()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=A2+A3");
            s.SetContentsOfCell("A2", "6");
            s.SetContentsOfCell("A3", ("=A2+A4"));
            s.SetContentsOfCell("A4", ("=A2+A5"));
            HashSet<string> result = new HashSet<string>(s.SetContentsOfCell("A5", "82.5"));
            AssertSetEqualsIgnoreCase(result, new string[] { "A5", "A4", "A3", "A1" });
        }

        // CHANGING CELLS
        [TestMethod()]
        public void Test28()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", ("A2+A3"));
            s.SetContentsOfCell("A1", "2.5");
            Assert.AreEqual(2.5, (double)s.GetCellContents("A1"), 1e-9);
        }

        [TestMethod()]
        public void Test29()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", new Formula("A2+A3").ToString());
            s.SetContentsOfCell("A1", "Hello");
            Assert.AreEqual("Hello", (string)s.GetCellContents("A1"));
        }

        [TestMethod()]
        public void Test30()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "Hello");
            s.SetContentsOfCell("A1", ("=23"));
            Assert.AreEqual(23, ((Formula)s.GetCellContents("A1")).Evaluate(x => 0));
        }
        // STRESS TESTS
        /// <summary>
        /// Assigns extra cells to the professor's original Test31.  With the values assigned,
        /// the value of A1 should be 265. 
        /// </summary>
        [TestMethod()]
        public void Test31()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1+B2");
            s.SetContentsOfCell("B1", "=C1-C2");
            s.SetContentsOfCell("B2", "=C3*C4");
            s.SetContentsOfCell("C1", "=D1*D2");
            s.SetContentsOfCell("C2", "=D3*D4");
            s.SetContentsOfCell("C3", "=D5*D6");
            s.SetContentsOfCell("C4", "=D7*D8");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("D2", "=E1");
            s.SetContentsOfCell("D3", "=E1");
            s.SetContentsOfCell("D4", "=E1");
            s.SetContentsOfCell("D5", "=E1");
            s.SetContentsOfCell("D6", "=E1");
            s.SetContentsOfCell("D7", "=E1");
            s.SetContentsOfCell("D8", "=E1");

            s.SetContentsOfCell("E1", "5.00");
            s.SetContentsOfCell("E2", "2");
            s.SetContentsOfCell("D4", "=E2");
            s.SetContentsOfCell("D8", "=E2");

            double answer = (double)s.GetCellValue("A1");

            Assert.AreEqual(answer, 265.00);

            //AssertSetEqualsIgnoreCase(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }, cells);
        }
        /// <summary>
        /// Exactly the same as Test31 but this time, it saves an XML file
        /// to the project folder. That file will eventually be opened in Test31c
        /// </summary>
        [TestMethod()]
        public void Test31b()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            s.SetContentsOfCell("A1", "=B1+B2");
            s.SetContentsOfCell("B1", "=C1-C2");
            s.SetContentsOfCell("B2", "=C3*C4");
            s.SetContentsOfCell("C1", "=D1*D2");
            s.SetContentsOfCell("C2", "=D3*D4");
            s.SetContentsOfCell("C3", "=D5*D6");
            s.SetContentsOfCell("C4", "=D7*D8");
            s.SetContentsOfCell("D1", "=E1");
            s.SetContentsOfCell("D2", "=E1");
            s.SetContentsOfCell("D3", "=E1");
            s.SetContentsOfCell("D4", "=E1");
            s.SetContentsOfCell("D5", "=E1");
            s.SetContentsOfCell("D6", "=E1");
            s.SetContentsOfCell("D7", "=E1");
            s.SetContentsOfCell("D8", "=E1");

            s.SetContentsOfCell("E1", "5.00");
            s.SetContentsOfCell("E2", "2");
            s.SetContentsOfCell("D4", "=E2");
            s.SetContentsOfCell("D8", "=E2");

            //using (TextWriter test = File.CreateText("C:\\Users\\kucab2345\\Desktop\\text.xml"))
            using (TextWriter test = File.CreateText("../../text.xml"))
            {
                s.Save(test);
            }
            
            //AssertSetEqualsIgnoreCase(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }, cells);
        }
        /// <summary>
        /// Saves a file of the previous test and opens it back up.
        /// Asserts that the evaluation of the cell value is still correct
        /// after the entire save and load process.
        /// </summary>
        [TestMethod()]
        public void Test31c()
        {
            AbstractSpreadsheet s;
            using (TextReader test = File.OpenText("../../text.xml"))
            {
                s = new Spreadsheet(test);
            }
            List<string> names = new List<string>();

            foreach(string i in s.GetNamesOfAllNonemptyCells())
            {
                names.Add(i);
            }

            Assert.AreEqual(17, names.Count);
            Assert.AreEqual(265.00, (double)s.GetCellValue("A1"));
            //AssertSetEqualsIgnoreCase(new HashSet<string>() { "A1", "B1", "B2", "C1", "C2", "C3", "C4", "D1", "D2", "D3", "D4", "D5", "D6", "D7", "D8", "E1" }, cells);
        }
        [TestMethod()]
        public void Test32()
        {
            Test31();
        }
        [TestMethod()]
        public void Test33()
        {
            Test31();
        }
        [TestMethod()]
        public void Test34()
        {
            Test31();
        }

        [TestMethod()]
        public void Test35()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {
                cells.Add("A" + i.ToString());
                AssertSetEqualsIgnoreCase(cells, s.SetContentsOfCell("A" + i, ("=A" + (i + 1))));
            }
        }
        /// <summary>
        /// Stress test that creates 200 cells, saves them, reads them back in, and
        /// checks that the value was preserved across the save and load of so many cells
        /// </summary>
        [TestMethod()]
        public void Test35a()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            ISet<String> cells = new HashSet<string>();
            for (int i = 1; i < 200; i++)
            {
                cells.Add("A" + i.ToString());
                AssertSetEqualsIgnoreCase(cells, s.SetContentsOfCell("A" + i, ("=A" + (i + 1))));
            }
            s.SetContentsOfCell("A199", "999");
            using (TextWriter outFile = File.CreateText("../../StressTest.xml"))
            {
                s.Save(outFile);
            }
            AbstractSpreadsheet s1;
            using (TextReader inFile = File.OpenText("../../StressTest.xml"))
            {
                s1 = new Spreadsheet(inFile);
            }
            Assert.AreEqual(999, (double)s1.GetCellValue("A199"));

        }
        [TestMethod()]
        public void Test36()
        {
            Test35();
        }
        [TestMethod()]
        public void Test37()
        {
            Test35();
        }
        [TestMethod()]
        public void Test38()
        {
            Test35();
        }
        [TestMethod()]
        public void Test39()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            for (int i = 1; i < 200; i++)
            {
                s.SetContentsOfCell("A" + i, ("=A" + (i + 1)));
            }
            try
            {
                s.SetContentsOfCell("A150","=A50");
                Assert.Fail();
            }
            catch (CircularException)
            {
            }
        }
        [TestMethod()]
        public void Test40()
        {
            Test39();
        }
        [TestMethod()]
        public void Test41()
        {
            Test39();
        }
        [TestMethod()]
        public void Test42()
        {
            Test39();
        }

        [TestMethod()]
        public void Test43()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            for (int i = 0; i < 500; i++)
            {
                s.SetContentsOfCell("A1" + i, ("=A1" + (i + 1).ToString()));
            }

            ISet<string> sss = s.SetContentsOfCell("A1499", "25.0");
            Assert.AreEqual(500, sss.Count);
            for (int i = 0; i < 500; i++)
            {
                Assert.IsTrue(sss.Contains("A1" + i));
            }

            sss = s.SetContentsOfCell("A1249", "25.0");
            Assert.AreEqual(250, sss.Count);
            for (int i = 0; i < 250; i++)
            {
                Assert.IsTrue(sss.Contains("A1" + i));
            }


        }

        [TestMethod()]
        public void Test44()
        {
            Test43();
        }
        [TestMethod()]
        public void Test45()
        {
            Test43();
        }
        [TestMethod()]
        public void Test46()
        {
            Test43();
        }

        [TestMethod()]
        public void Test47()
        {
            RunRandomizedTest(47, 2519);
        }
        [TestMethod()]
        public void Test48()
        {
            RunRandomizedTest(48, 2521);
        }
        [TestMethod()]
        public void Test49()
        {
            RunRandomizedTest(49, 2526);
        }
        [TestMethod()]
        public void Test50()
        {
            RunRandomizedTest(50, 2521);
        }
        /// <summary>
        /// Invalid source name for XML file. Should throw IO error
        /// </summary>
        [ExpectedException(typeof(FileNotFoundException))]
        [TestMethod]
        public void Test51()
        {
            AbstractSpreadsheet s;
            using (TextReader test = File.OpenText("../../bob.xml"))
            {
                s = new Spreadsheet(test);
            }
        }
        /// <summary>
        /// Tests the second spreadsheet constructor that takes in a regex argument for IsValid
        /// </summary>
        [TestMethod]
        public void Test52()
        {
            Regex re = new Regex(@"^[A-Z]+[1-9][0-9]*$");
            AbstractSpreadsheet s = new Spreadsheet(re);

            s.SetContentsOfCell("a1", "100");
        }
        /// <summary>
        /// Tried to open a spreadsheet source file WHILE saving it, forces an IO error
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(IOException))]
        public void Test53()
        {
            AbstractSpreadsheet s = new Spreadsheet();
            AbstractSpreadsheet s1;
            s.SetContentsOfCell("A1", "100");
            TextReader source = File.OpenText("../../Test53.xml");
            using (TextWriter test = File.CreateText("../../Test53.xml"))
            {
                s.Save(test);
                s = new Spreadsheet(source);
            }
        }
        /// <summary>
        /// Custom XML w. force call name duplicates. Should throw an IOexception
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadException))]
        public void Test54()
        {
            AbstractSpreadsheet s;

            using (TextReader source = File.OpenText("../../DuplicateTest.xml"))
            {
                s = new Spreadsheet(source);
            }
        }
        /// <summary>
        /// Custom XML w. force cell circular dependency. Should throw an SpreadSheetReadException
        /// </summary>
        [TestMethod]
        [ExpectedException(typeof(SpreadsheetReadException))]
        public void Test55()
        {
            AbstractSpreadsheet s;

            using (TextReader source = File.OpenText("../../CircularDepCheck.xml"))
            {
                s = new Spreadsheet(source);
            }
        }
        /// <summary>
        /// Creating a cell with a string as the contents, saves, it, then reopens
        /// </summary>
        [TestMethod]
        public void Test56()
        {
            AbstractSpreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "This is a test string.");

            using (TextWriter outFile = File.CreateText("../../StringSaveandWriteTest.xml"))
            {
                s.Save(outFile);
            }
            AbstractSpreadsheet s1;
            using(TextReader source = File.OpenText("../../StringSaveandWriteTest.xml"))
            {
                s1 = new Spreadsheet(source);
            }
            Assert.AreEqual("This is a test string.", s1.GetCellValue("A1"));
            Assert.AreEqual("This is a test string.", s1.GetCellValue("a1"));
        }
        
        /// <summary>
        /// Forcing a FormulaError as a cell value, and trying to return it.
        /// </summary>
        [TestMethod]
        public void Test57()
        {
            AbstractSpreadsheet s = new Spreadsheet();

            s.SetContentsOfCell("A1", "=A2 + 3");
            s.SetContentsOfCell("A2", "=A3");
            
            Assert.IsTrue(s.GetCellValue("A1") is FormulaError);
        }
        /// <summary>
        /// Testing returning an empty Cell. Should get an empty string as contents.
        /// </summary>
        [TestMethod]
        public void Test58()
        {
            AbstractSpreadsheet s = new Spreadsheet();


            Assert.AreEqual("",s.GetCellValue("A1"));
        }
        /// <summary>
        /// opening a friend's xml file and checking that all values have come across into my spreadsheet program.
        /// </summary>
        [TestMethod]
        public void OtherCompiledXMLTest()
        {
            AbstractSpreadsheet s;
            using (TextReader inFile = File.OpenText("../../FriendXML.xml"))
            {
                s = new Spreadsheet(inFile);
            }

            Assert.AreEqual(0.15, s.GetCellValue("A6"));
        }
        /// <param name="seed"></param>
        /// <param name="size"></param>
        public void RunRandomizedTest(int seed, int size)
        {
            AbstractSpreadsheet s = new Spreadsheet();
            Random rand = new Random(seed);
            for (int i = 0; i < 10000; i++)
            {
                try
                {
                    switch (rand.Next(3))
                    {
                        case 0:
                            s.SetContentsOfCell(randomName(rand), "3.14");
                            break;
                        case 1:
                            s.SetContentsOfCell(randomName(rand), "hello");
                            break;
                        case 2:
                            s.SetContentsOfCell(randomName(rand), randomFormula(rand));
                            break;
                    }
                }
                catch (CircularException)
                {
                }
            }
            ISet<string> set = new HashSet<string>(s.GetNamesOfAllNonemptyCells());
            Assert.AreEqual(size, set.Count);
        }

        private String randomName(Random rand)
        {
            return "ABCDEFGHIJKLMNOPQRSTUVWXYZ".Substring(rand.Next(26), 1) + (rand.Next(99) + 1);
        }

        private String randomFormula(Random rand)
        {
            String f = randomName(rand);
            for (int i = 0; i < 10; i++)
            {
                switch (rand.Next(4))
                {
                    case 0:
                        f += "+";
                        break;
                    case 1:
                        f += "-";
                        break;
                    case 2:
                        f += "*";
                        break;
                    case 3:
                        f += "/";
                        break;
                }
                switch (rand.Next(2))
                {
                    case 0:
                        f += 7.2;
                        break;
                    case 1:
                        f += randomName(rand);
                        break;
                }
            }
            return f;
        }
    }
}