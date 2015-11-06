using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.CodeDom;

using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using LambdaLang;

namespace TestProject {
    /// <summary>
    /// Test de la clase <see cref="ECC.Lib.Expression"/>
    /// </summary>
    [TestClass]
    public class TS_Expression {

        #region Tests

        #region expresiones algebraicas

        [TestMethod]
        public void Prueba19_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(2+3\r\n)*\n5");
            Assert.AreEqual(25.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba20_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(2+3)*5");
            Assert.AreEqual(25.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba21_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(i+3)*5");
            Assert.AreEqual(1, exp.SymbolTable.Count);
            Assert.IsTrue(exp.SymbolTable.ContainsKey("i"));
        }

        [TestMethod]
        public void Prueba22_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(a.A+3)*5");
            Aux y = new Aux();
            y.A = 2;
            y.B = 1;
            exp.SymbolTable["a"] = y;
            Assert.AreEqual(25.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba23_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(2.0+3.0)*5.0");
            Assert.AreEqual(25.0, exp.Calculate());
        }

        [TestMethod, ExpectedException(typeof(System.ApplicationException))]
        public void Prueba24_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(2+*5.0");
        }

        // hacemos SetExpression con un solo parametro.
        [TestMethod, Ignore]
        public void Prueba25_TestRecursiveDescent() {
            Expression exp = new Expression();
            // exp.SetExpression("(A+B)*5", true);
            Aux y = new Aux();
            y.A = 2;
            y.B = 1;
            exp.SymbolTable["this"] = y;
            Assert.AreEqual(15.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba26_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(this.A+this.B)*5");
            Aux y = new Aux();
            y.A = 2;
            y.B = 1;
            exp.SymbolTable["this"] = y;
            Assert.AreEqual(15.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba27_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(A+B)*5");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(15.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba28_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("3/0");
            Assert.AreEqual(double.PositiveInfinity, exp.Calculate());
        }

        [TestMethod]
        public void Prueba29_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("({A.X}+{A.Y})*5");
            var A = new { X = 2.0, Y = 1.0 };
            exp.SymbolTable["A"] = A;
            exp.SymbolTable["this"] = A;
            Assert.AreEqual(15.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba30_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("({A.X}+{A.Y}) == (this.X + this.Y)");
            var A = new { X = 2.0, Y = 1.0 };
            exp.SymbolTable["A"] = A;
            exp.SymbolTable["this"] = A;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba31_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("t2.Ticks - t1.Ticks");
            exp.SymbolTable["t1"] = System.DateTime.Now;
            exp.SymbolTable["t2"] = System.DateTime.Now;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        #endregion

        #region expresiones logicas cadenas

        [TestMethod, Ignore]
        public void Prueba40_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A>B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba41_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A==B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod, Ignore]
        public void Prueba42_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A<B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod, Ignore]
        public void Prueba43_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A>=B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba45_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A!=B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod, Ignore]
        public void Prueba46_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(A<B)");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod, Ignore]
        public void Prueba47_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("not (A<B)");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba48_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("not (A!=B)");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba112_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A and not B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = null;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        #endregion

        #region expresiones logicas numeros

        [TestMethod]
        public void Prueba50_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A>B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba51_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A==B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba52_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A<B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba53_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A>=B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba55_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A!=B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba56_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(A<B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba57_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("not (A<B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba58_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("not A");
            exp.SymbolTable["A"] = 2.0;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba60_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(A > B) and (A > B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba61_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(A > B) and (A < B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba62_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(A < B) or (A < B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba63_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(A < B) or (A > B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba64_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("not ((A < B) or (A < B))");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba65_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("(not 0) * 2");
            Assert.AreEqual(2.0, exp.Calculate());
        }

        #endregion

        #region if expression

        [TestMethod]
        public void Prueba70_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("3 if 1 else 4");
            Assert.AreEqual(3.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba71_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("3 if 0 else 4");
            Assert.AreEqual(4.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba72_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("3 if not 0 else 4");
            Assert.AreEqual(3.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba73_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("3 if \"A\" == \"A\" else 4");
            Assert.AreEqual(3.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba74_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("3 if \"A\" != \"A\" else 4");
            Assert.AreEqual(4.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba75_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("\"fail\" if (A != \"A\") else \"ok\"");
            exp.SymbolTable["A"] = "A";
            Assert.AreEqual("ok", exp.Solve());
        }

        // tests para short-circuit.
        // el 1º es para asegurarnos que salta una excepcion.
        [TestMethod, ExpectedException(typeof(System.NullReferenceException))]
        public void Prueba130_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("x.X.A == 3");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            exp.Calculate();
        }

        // si hay short-circuit en AND no deberia haber excepcion.
        [TestMethod]
        public void Prueba131_TestRecursiveDescent() {
            var exp = new Expression("0 and x.X.A == 3");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        // si hay short-circuit en OR no deberia haber excepcion.
        [TestMethod]
        public void Prueba132_TestRecursiveDescent() {
            var exp = new Expression("1 or x.X.A == 3");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        // si hay short-circuit en IFF-TRUE no deberia haber excepcion.
        [TestMethod]
        public void Prueba133_TestRecursiveDescent() {
            var exp = new Expression("5 if 1 else x.X.A");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.AreEqual(5.0, exp.Calculate());
        }

        // si hay short-circuit en IFF-FALSE no deberia haber excepcion.
        [TestMethod]
        public void Prueba134_TestRecursiveDescent() {
            var exp = new Expression("x.X.A if 0 else 5");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.AreEqual(5.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba140_TestRecursiveDescent() {
            var exp = new Expression("1 and \"x\" and 3");
            Assert.AreEqual(3.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba141_TestRecursiveDescent() {
            var exp = new Expression("0 or \"\" or 3");
            Assert.AreEqual(3.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba142_TestRecursiveDescent() {
            var exp = new Expression("3 and 0");
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba143_TestRecursiveDescent() {
            var exp = new Expression("3 and not 0");
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba144_TestRecursiveDescent() {
            var exp = new Expression("0 or not 3");
            Assert.AreEqual(0.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba150_TestRecursiveDescent() {
            var exp = new Expression("\"y\" and 3 and \"x\"");
            Assert.AreEqual("x", exp.Solve());
        }

        [TestMethod]
        public void Prueba151_TestRecursiveDescent() {
            var exp = new Expression("0 or \"\" or \"x\"");
            Assert.AreEqual("x", exp.Solve());
        }

        [TestMethod]
        public void Prueba160_TestRecursiveDescent() {
            var exp = new Expression("not ( X and Y )");

            exp.SymbolTable["X"] = 2;
            exp.SymbolTable["Y"] = 3;
            Assert.AreEqual(0.0, exp.Calculate());

            exp.SymbolTable["X"] = 0;
            exp.SymbolTable["Y"] = 3;
            Assert.AreEqual(1.0, exp.Calculate());

            exp.SymbolTable["X"] = 2;
            exp.SymbolTable["Y"] = 0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        [TestMethod]
        public void Prueba161_TestRecursiveDescent() {
            var exp = new Expression("not X or not Y");

            exp.SymbolTable["X"] = 2;
            exp.SymbolTable["Y"] = 3;
            Assert.AreEqual(0.0, exp.Calculate());

            exp.SymbolTable["X"] = 0;
            exp.SymbolTable["Y"] = 3;
            Assert.AreEqual(1.0, exp.Calculate());

            exp.SymbolTable["X"] = 2;
            exp.SymbolTable["Y"] = 0;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        // de Expression.doc
        [TestMethod]
        public void Prueba170_TestRecursiveDescent() {
            var exp = new Expression("1 if this.S else 1/this.S");

            var aux = new Aux();
            aux.S = "AAA";
            exp.SymbolTable["this"] = aux;
            Assert.AreEqual(1.0, exp.Calculate());
        }

        // de Expression.doc
        [TestMethod]
        public void Prueba171_TestRecursiveDescent() {
            var exp = new Expression("this.A and (10 / this.A) > 0");

            var aux = new Aux();
            aux.A = 5.0;
            exp.SymbolTable["this"] = aux;
            Assert.AreEqual(1.0, exp.Calculate());

            aux.A = 0.0;
            Assert.AreEqual(0.0, exp.Calculate());
        }

        #endregion

        #region expresiones varias

        [TestMethod]
        public void Prueba80_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("1 + 2 if 3 > 4 else 5 - 6");
            Assert.AreEqual(-1, exp.Calculate());
        }

        [TestMethod]
        public void Prueba81_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("A + B if C > D else E - F");
            exp.SymbolTable["A"] = 1.0;
            exp.SymbolTable["B"] = 2.0;
            exp.SymbolTable["C"] = 3.0;
            exp.SymbolTable["D"] = 4.0;
            exp.SymbolTable["E"] = 5.0;
            exp.SymbolTable["F"] = 6.0;
            Assert.AreEqual(-1, exp.Calculate());
        }

        #endregion

        #region lambda expressions

        [TestMethod]
        public void Test_lambda_10() {
            Expression exp = new Expression();
            exp.SetExpression("3, 2");
            Assert.AreEqual(2.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_11() {
            Expression exp = new Expression();
            exp.SetExpression("3, 2, 5");
            Assert.AreEqual(5.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_12() {
            Expression exp = new Expression();
            exp.SetExpression("a = 3");
            Assert.AreEqual(3.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_13() {
            Expression exp = new Expression();
            exp.SetExpression("a = 3, 5, a");
            Assert.AreEqual(3.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_01() {
            Expression exp = new Expression();
            exp.SetExpression("(lambda 2+3)()");
            Assert.AreEqual(5.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_02() {
            Expression exp = new Expression();
            exp.SetExpression("v=3, (lambda 2+v)()");
            Assert.AreEqual(5.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_03() {
            Expression exp = new Expression();
            exp.SetExpression("(f = lambda 2+3), 9");
            Console.WriteLine();
            Console.WriteLine(exp.toString());
            Console.WriteLine();
            Assert.AreEqual(9.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_04() {
            Expression exp = new Expression();
            exp.SetExpression("(f = lambda 2+3), f()");
            Console.WriteLine();
            Console.WriteLine(exp.toString());
            Console.WriteLine();
            Assert.AreEqual(5.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_05() {
            Expression exp = new Expression();
            exp.SetExpression("(f = lambda 2+v), (v = 3), f()");
            Assert.AreEqual(5.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_06() {
            Expression exp = new Expression();
            exp.SetExpression("f = lambda 2+v, v = 3, f()");
            Assert.AreEqual(5.0, exp.Calculate());
        }

        [TestMethod]
        public void Test_lambda_07() {
            Expression exp = new Expression();
            exp.SetExpression("f = lambda (y=4, y+v), v = 3, f()");
            Assert.AreEqual(7.0, exp.Calculate());
        }


        #endregion

        #region cadenas

        [TestMethod]
        public void Prueba90_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("\"Hola\"");
            Assert.AreEqual("Hola", exp.Solve());
        }

        [TestMethod]
        public void Prueba91_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("\"A\" * 3");
            Assert.AreEqual("AAA", exp.Solve());
        }

        [TestMethod]
        public void Prueba92_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("\"A\" + \"B\"");
            Assert.AreEqual("AB", exp.Solve());
        }

        [TestMethod]
        public void Prueba93_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("  \"Hola \\\"Juan\\\"!\"  ");
            Assert.AreEqual("Hola \"Juan\"!", exp.Solve());
        }

        [TestMethod]
        public void Prueba94_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression(" \"A = {0:##.#}\" % a ");
            exp.SymbolTable["a"] = 100.0 / 3.0;
            Assert.AreEqual("A = 33,3", exp.Solve());
        }

        [TestMethod]
        public void Prueba95_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression(" \"Ahora: {0:dd/M/yyyy}\" % now ");
            exp.SymbolTable["now"] = new System.DateTime(2010, 3, 31, 14, 55, 58);
            Assert.AreEqual("Ahora: 31/3/2010", exp.Solve());
        }

        [TestMethod]
        public void Prueba96_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression(" ( \"Punto = ({0};{{0}})\" % x ) % y");
            exp.SymbolTable["x"] = 1.0;
            exp.SymbolTable["y"] = 2.0;
            Assert.AreEqual("Punto = (1;2)", exp.Solve());
        }

        [TestMethod]
        public void Prueba97_TestRecursiveDescent() {
            Expression exp = new Expression();
            exp.SetExpression("\"Hola \" + ( \"amigo\" if {x} == 2 else \"enemigo\")");
            exp.SymbolTable["x"] = 1.0;
            Assert.AreEqual("Hola enemigo", exp.Solve());
        }

        [TestMethod]
        public void Prueba100_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("5 if this.S == this.T else 4");
            Aux y1 = new Aux();
            y1.S = null;
            y1.T = "X";
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(4.0, e1.Calculate());
        }

        [TestMethod]
        public void Prueba101_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("5 if this.S else 4");
            Aux y1 = new Aux();
            y1.S = " ";
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(5.0, e1.Calculate());
        }

        [TestMethod]
        public void Prueba102_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("5 if this.S else 4");
            Aux y1 = new Aux();
            y1.S = "";
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(4.0, e1.Calculate());
        }

        [TestMethod]
        public void Prueba103_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("5 if not this.S else 4");
            Aux y1 = new Aux();
            y1.S = null;
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(5.0, e1.Calculate());
        }

        [TestMethod]
        public void Prueba104_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("5 if this.S and this.S == \"X\" else 4");
            Aux y1 = new Aux();
            y1.S = null;
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(4.0, e1.Calculate());
        }

        [TestMethod]
        public void Prueba105_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("5 if this.S and this.S != \"\" else 4");
            Aux y1 = new Aux();
            y1.S = "X";
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(5.0, e1.Calculate());
        }

        #endregion

        #region objetos

        [TestMethod]
        public void Prueba180_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("this.A1.A * this.A2.B");
            var a1 = new Aux();
            a1.A = 4;
            a1.B = 5;
            var a2 = new Aux();
            a2.A = 4;
            a2.B = 5;
            e1.SymbolTable["this"] = new { A1 = a1, A2 = a2 };
            Assert.AreEqual(20.0, e1.Calculate());
        }

        [TestMethod]
        public void Prueba181_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("X.X.S", true);

            var a1 = new Aux();
            a1.X = new Aux();
            a1.X.X = new Aux();
            a1.X.X.S = "Hola";
            e1.SymbolTable["this"] = a1;
            Assert.AreEqual("Hola", e1.Solve());
        }

        [TestMethod]
        public void Prueba182_TestRecursiveDescent() {
            Expression e1 = new Expression();
            e1.SetExpression("this.X.X.S");
            var a1 = new Aux();
            a1.X = new Aux();
            a1.X.X = new Aux();
            a1.X.X.S = "Hola";
            e1.SymbolTable["this"] = a1;
            Assert.AreEqual("Hola", e1.Solve());
        }

        [TestMethod]
        public void Prueba183_TestRecursiveDescent() {
            Expression e1 = new Expression("a1.X.X.S");
            var a1 = new Aux();
            a1.X = new Aux();
            a1.X.X = new Aux();
            a1.X.X.S = "Hola";
            e1.SymbolTable["a1"] = a1;
            Assert.AreEqual("Hola", e1.Solve());
        }

        #endregion

        #region serializacion

        /// <summary>
        /// Prueba Serializacion
        /// </summary>
        [TestMethod]
        public void Prueba98_Serializacion() {
            byte[] buffer;

            Expression e1 = new Expression();
            e1.SetExpression("(this.A + this.B) * 5");
            Aux y1 = new Aux();
            y1.A = 2;
            y1.B = 1;
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(15.0, e1.Calculate());

            // antes de serializar hay que desligar los objetos de la tabla
            // de simbolos para que no los serialice.
            List<string> keys = new List<string>(e1.SymbolTable.Keys);
            foreach (string k in keys)
                e1.SymbolTable[k] = null;

            // serializacion
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize(ms, e1);
                buffer = ms.ToArray();
            }
            Assert.IsTrue(0 < buffer.Length, "no hay nada en el buffer.");

            Expression e2;
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer)) {
                BinaryFormatter f = new BinaryFormatter();
                e2 = (Expression)f.Deserialize(ms);
            }

            //Compruebo la deserealizacion.
            Aux y2 = new Aux();
            y2.A = 2;
            y2.B = 1;
            e2.SymbolTable["this"] = y2;
            Assert.AreEqual(15.0, e2.Calculate());
        }

        /// <summary>
        /// Prueba Serializacion con error.
        /// </summary>
        [TestMethod,
        ExpectedException(typeof(SerializationException))]
        public void Prueba99_SerializacionError() {
            byte[] buffer;

            Expression e1 = new Expression();

            using (System.IO.MemoryStream ms = new System.IO.MemoryStream()) {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize(ms, e1);
                buffer = ms.ToArray();
            }
            Assert.IsTrue(0 < buffer.Length, "no hay nada en el buffer.");

            // meto un error en el objeto buffer.
            buffer[10] = (byte)'X';

            // leo.
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream(buffer)) {
                BinaryFormatter f = new BinaryFormatter();
                f.Deserialize(ms);
            }
            Assert.Fail("Debio saltar error de serialización");
        }

        #endregion

        #region clases auxiliares

        class Aux {
            public double A = 0.0;
            public double B = 0.0;
            public string S = "";
            public string T = "";
            public Aux X = null;
        }

        #endregion

        #endregion

        #region Test Setup & Teardown

        /// <summary>
        /// inicializo.
        /// </summary>
        [TestInitialize()]
        public void CreateObjects() {
        }

        /// <summary>
        /// cierro.
        /// </summary>
        [TestCleanup()]
        public void DeleteObjects() {
        }

        /// <summary>
        /// Inicializo lote de prueba.
        /// </summary>
        [ClassInitialize()]
        public static void FSU(TestContext testContext) {
        }

        /// <summary>
        /// Cierro lote de prueba.
        /// </summary>
        [ClassCleanup()]
        public static void FTU() {
        }

        #endregion

    }
}
