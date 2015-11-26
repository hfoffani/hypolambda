/*
 * Copyright 2015 Hernán M. Foffani
 *
 * Licensed under the Apache License, Version 2.0 (the "License");
 * you may not use this file except in compliance with the License.
 * You may obtain a copy of the License at
 *
 *     http://www.apache.org/licenses/LICENSE-2.0
 *
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 *
 */

using NUnit.Framework;
using System;
using System.Globalization;

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using HL;

namespace Tests
{
    /// <summary>
    /// Tests of <see cref="HypoLambda"/>
    /// </summary>
    [TestFixture]
    public class TS_Expression
    {
        #region Tests

        #region algebraic expressions

        [Test]
        public void Test_19()
        {
            var exp = new HypoLambda();
            exp.Compile("(2+3\r\n)*\n5");
            Assert.AreEqual(25.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_20()
        {
            var exp = new HypoLambda();
            exp.Compile("(2+3)*5");
            Assert.AreEqual(25.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_21()
        {
            var exp = new HypoLambda();
            exp.Compile("(i+3)*5");
            Assert.AreEqual(1, exp.SymbolTable.Count);
            Assert.IsTrue(exp.SymbolTable.ContainsKey("i"));
        }

        [Test]
        public void Test_22()
        {
            var exp = new HypoLambda();
            exp.Compile("(a.A+3)*5");
            Aux y = new Aux();
            y.A = 2;
            y.B = 1;
            exp.SymbolTable["a"] = y;
            Assert.AreEqual(25.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_23()
        {
            var exp = new HypoLambda();
            exp.Compile("(2.0+3.0)*5.0");
            Assert.AreEqual(25.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_24()
        {
            var exp = new HypoLambda();
            Assert.Throws<ApplicationException>(()=> exp.Compile("(2+*5.0"));
        }

        [Test]
        public void Test_25()
        {
            var exp = new HypoLambda();
            exp.Compile("(A+B)*5", true);
            Aux y = new Aux();
            y.A = 2;
            y.B = 1;
            exp.SymbolTable["this"] = y;
            Assert.AreEqual(15.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_26()
        {
            var exp = new HypoLambda();
            exp.Compile("(this.A+this.B)*5");
            Aux y = new Aux();
            y.A = 2;
            y.B = 1;
            exp.SymbolTable["this"] = y;
            Assert.AreEqual(15.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_27()
        {
            var exp = new HypoLambda();
            exp.Compile("(A+B)*5");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(15.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_28()
        {
            var exp = new HypoLambda();
            exp.Compile("3/0");
            Assert.AreEqual(double.PositiveInfinity, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_29()
        {
            var exp = new HypoLambda();
            exp.Compile("({A.X}+{A.Y})*5");
            var A = new { X = 2.0, Y = 1.0 };
            exp.SymbolTable["A"] = A;
            exp.SymbolTable["this"] = A;
            Assert.AreEqual(15.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_30()
        {
            var exp = new HypoLambda();
            exp.Compile("({A.X}+{A.Y}) == (this.X + this.Y)");
            var A = new { X = 2.0, Y = 1.0 };
            exp.SymbolTable["A"] = A;
            exp.SymbolTable["this"] = A;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_31()
        {
            var exp = new HypoLambda();
            exp.Compile("t2.Ticks - t1.Ticks");
            var now = System.DateTime.Now;
            exp.SymbolTable["t1"] = now;
            exp.SymbolTable["t2"] = now;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        #endregion

        #region logic expressions strings

        [Test]
        public void Test_40()
        {
            var exp = new HypoLambda();
            exp.Compile("A>B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_41()
        {
            var exp = new HypoLambda();
            exp.Compile("A==B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_42()
        {
            var exp = new HypoLambda();
            exp.Compile("A<B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_43()
        {
            var exp = new HypoLambda();
            exp.Compile("A>=B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_45()
        {
            var exp = new HypoLambda();
            exp.Compile("A!=B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_46()
        {
            var exp = new HypoLambda();
            exp.Compile("(A<B)");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_47()
        {
            var exp = new HypoLambda();
            exp.Compile("not (A<B)");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_48()
        {
            var exp = new HypoLambda();
            exp.Compile("not (A!=B)");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = "b";
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_112()
        {
            var exp = new HypoLambda();
            exp.Compile("A and not B");
            exp.SymbolTable["A"] = "a";
            exp.SymbolTable["B"] = null;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        #endregion

        #region logic expressions numbers

        [Test]
        public void Test_50()
        {
            var exp = new HypoLambda();
            exp.Compile("A>B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_51()
        {
            var exp = new HypoLambda();
            exp.Compile("A==B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_52()
        {
            var exp = new HypoLambda();
            exp.Compile("A<B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_53()
        {
            var exp = new HypoLambda();
            exp.Compile("A>=B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_55()
        {
            var exp = new HypoLambda();
            exp.Compile("A!=B");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_56()
        {
            var exp = new HypoLambda();
            exp.Compile("(A<B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_57()
        {
            var exp = new HypoLambda();
            exp.Compile("not (A<B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_58()
        {
            var exp = new HypoLambda();
            exp.Compile("not A");
            exp.SymbolTable["A"] = 2.0;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_60()
        {
            var exp = new HypoLambda();
            exp.Compile("(A > B) and (A > B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_61()
        {
            var exp = new HypoLambda();
            exp.Compile("(A > B) and (A < B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_62()
        {
            var exp = new HypoLambda();
            exp.Compile("(A < B) or (A < B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_63()
        {
            var exp = new HypoLambda();
            exp.Compile("(A < B) or (A > B)");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_64()
        {
            var exp = new HypoLambda();
            exp.Compile("not ((A < B) or (A < B))");
            exp.SymbolTable["A"] = 2.0;
            exp.SymbolTable["B"] = 1.0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_65()
        {
            var exp = new HypoLambda();
            exp.Compile("(not 0) * 2");
            Assert.AreEqual(2.0, Convert.ToDouble(exp.Run()));
        }

        #endregion

        #region if expression

        [Test]
        public void Test_70()
        {
            var exp = new HypoLambda();
            exp.Compile("3 if 1 else 4");
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_71()
        {
            var exp = new HypoLambda();
            exp.Compile("3 if 0 else 4");
            Assert.AreEqual(4.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_72()
        {
            var exp = new HypoLambda();
            exp.Compile("3 if not 0 else 4");
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_73()
        {
            var exp = new HypoLambda();
            exp.Compile("3 if \"A\" == \"A\" else 4");
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_74()
        {
            var exp = new HypoLambda();
            exp.Compile("3 if \"A\" != \"A\" else 4");
            Assert.AreEqual(4.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_75()
        {
            var exp = new HypoLambda();
            exp.Compile("\"fail\" if (A != \"A\") else \"ok\"");
            exp.SymbolTable["A"] = "A";
            Assert.AreEqual("ok", exp.Run());
        }

        // tests para short-circuit.
        // el 1º es para asegurarnos que salta una excepcion.
        [Test]
        public void Test_130()
        {
            var exp = new HypoLambda();
            exp.Compile("x.X.A == 3");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.Throws<NullReferenceException>(() => Convert.ToDouble(exp.Run()));
        }

        // si hay short-circuit en AND no deberia haber excepcion.
        [Test]
        public void Test_131()
        {
            var exp = new HypoLambda("0 and x.X.A == 3");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        // si hay short-circuit en OR no deberia haber excepcion.
        [Test]
        public void Test_132()
        {
            var exp = new HypoLambda("1 or x.X.A == 3");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        // si hay short-circuit en IFF-TRUE no deberia haber excepcion.
        [Test]
        public void Test_133()
        {
            var exp = new HypoLambda("5 if 1 else x.X.A");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        // si hay short-circuit en IFF-FALSE no deberia haber excepcion.
        [Test]
        public void Test_134()
        {
            var exp = new HypoLambda("x.X.A if 0 else 5");
            var x = new Aux();
            exp.SymbolTable["x"] = x;
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_140()
        {
            var exp = new HypoLambda("1 and \"x\" and 3");
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_141()
        {
            var exp = new HypoLambda("0 or \"\" or 3");
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_142()
        {
            var exp = new HypoLambda("3 and 0");
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_143()
        {
            var exp = new HypoLambda("3 and not 0");
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_144()
        {
            var exp = new HypoLambda("0 or not 3");
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_150()
        {
            var exp = new HypoLambda("\"y\" and 3 and \"x\"");
            Assert.AreEqual("x", exp.Run());
        }

        [Test]
        public void Test_151()
        {
            var exp = new HypoLambda("0 or \"\" or \"x\"");
            Assert.AreEqual("x", exp.Run());
        }

        [Test]
        public void Test_160()
        {
            var exp = new HypoLambda("not ( X and Y )");

            exp.SymbolTable["X"] = 2;
            exp.SymbolTable["Y"] = 3;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));

            exp.SymbolTable["X"] = 0;
            exp.SymbolTable["Y"] = 3;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));

            exp.SymbolTable["X"] = 2;
            exp.SymbolTable["Y"] = 0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_161()
        {
            var exp = new HypoLambda("not X or not Y");

            exp.SymbolTable["X"] = 2;
            exp.SymbolTable["Y"] = 3;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));

            exp.SymbolTable["X"] = 0;
            exp.SymbolTable["Y"] = 3;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));

            exp.SymbolTable["X"] = 2;
            exp.SymbolTable["Y"] = 0;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        // de Expression.doc
        [Test]
        public void Test_170()
        {
            var exp = new HypoLambda("1 if this.S else 1/this.S");

            var aux = new Aux();
            aux.S = "AAA";
            exp.SymbolTable["this"] = aux;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));
        }

        // de Expression.doc
        [Test]
        public void Test_171()
        {
            var exp = new HypoLambda("this.A and (10 / this.A) > 0");

            var aux = new Aux();
            aux.A = 5.0;
            exp.SymbolTable["this"] = aux;
            Assert.AreEqual(1.0, Convert.ToDouble(exp.Run()));

            aux.A = 0.0;
            Assert.AreEqual(0.0, Convert.ToDouble(exp.Run()));
        }

        #endregion

        #region other expressions

        [Test]
        public void Test_80()
        {
            var exp = new HypoLambda();
            exp.Compile("1 + 2 if 3 > 4 else 5 - 6");
            Assert.AreEqual(-1, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_81()
        {
            var exp = new HypoLambda();
            exp.Compile("A + B if C > D else E - F");
            exp.SymbolTable["A"] = 1.0;
            exp.SymbolTable["B"] = 2.0;
            exp.SymbolTable["C"] = 3.0;
            exp.SymbolTable["D"] = 4.0;
            exp.SymbolTable["E"] = 5.0;
            exp.SymbolTable["F"] = 6.0;
            Assert.AreEqual(-1, Convert.ToDouble(exp.Run()));
        }

        #endregion

        #region lambda: expressions

        [Test]
        public void Test_lambda_10()
        {
            var exp = new HypoLambda();
            exp.Compile("3; 2");
            Assert.AreEqual(2.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_11()
        {
            var exp = new HypoLambda();
            exp.Compile("3; 2; 5");
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_12()
        {
            var exp = new HypoLambda();
            exp.Compile("a = 3");
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_13()
        {
            var exp = new HypoLambda();
            exp.Compile("a = 3; 5; a");
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_01()
        {
            var exp = new HypoLambda();
            exp.Compile("(lambda: 2+3)()");
            // Console.WriteLine();
            // Console.WriteLine(exp.prettyAST());
            // Console.WriteLine();
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_02()
        {
            var exp = new HypoLambda();
            exp.Compile("v=3; (lambda: 2+v)()");
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_03()
        {
            var exp = new HypoLambda();
            exp.Compile("(f = lambda: 2+3); 9");
            Assert.AreEqual(9.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_04()
        {
            var exp = new HypoLambda();
            exp.Compile("(f = lambda: 2+3); f()");
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_05()
        {
            var exp = new HypoLambda();
            exp.Compile("(f = lambda: 2+v); (v = 3); f()");
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_06()
        {
            var exp = new HypoLambda();
            exp.Compile("f = lambda: 2+v; v = 3; f()");
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_07()
        {
            var exp = new HypoLambda();
            exp.Compile("f = lambda: (y=4; y+v); v = 3; f()");
            Assert.AreEqual(7.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_08()
        {
            var exp = new HypoLambda();
            var prog = @"
f = lambda: (
    y = 4;
    y + v
);
v = 3;
f()
";
            exp.Compile(prog);
            Assert.AreEqual(7.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_21()
        {
            var exp = new HypoLambda();
            var prog = @"
u = 3;
v = (u+1; 5);
v
";
            exp.Compile(prog);
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_22()
        {
            var exp = new HypoLambda();
            var prog = @"
u = 3;
v = (4 if u < 2 else 5);
v
";
            exp.Compile(prog);
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_23()
        {
            var exp = new HypoLambda();
            var prog = @"
u = 3;
v = (u if u > 0 else 5);
v
";
            exp.Compile(prog);
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_24()
        {
            var exp = new HypoLambda();
            var prog = @"
f = lambda: (
    v = (u - 1 if u > 0 else u);
    v
);
u = 3;
f()
";
            exp.Compile(prog);
            Assert.AreEqual(2.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_30()
        {
            var exp = new HypoLambda();
            var prog = @"
f = lambda: (
    v = (v - 1 if v > 0 else v)
);
v = 3;
f()
";
            exp.Compile(prog);
            Assert.AreEqual(2.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_31()
        {
            var exp = new HypoLambda();
            var prog = @"
v = 3;
(v = v - 1) if v > 3 else 1;
v
";
            exp.Compile(prog);
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_32()
        {
            var exp = new HypoLambda();
            var prog = @"
f = lambda: (
    v - 1
);
v = 3;
(v = f()) if v > 3 else 1;
v
";
            exp.Compile(prog);
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_33()
        {
            var exp = new HypoLambda();
            var prog = @"
f = lambda: (
    v = v - 1
);
v = 3;
f() if v > 3 else 1;
v
";
            exp.Compile(prog);
            Assert.AreEqual(3.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_34()
        {
            var exp = new HypoLambda();
            var prog = @"
f = lambda: (
    (v = v - 1) if v > 0 else v);
    f() if v > 0 else 5
);
v = 3;
f()
";
            exp.Compile(prog);
            Assert.AreEqual(5.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_36()
        {
            var exp = new HypoLambda();
            var prog = "f = lambda a: (43+1); f()";
            exp.Compile(prog);
            // Console.WriteLine(exp.prettyAST());
            Assert.AreEqual(44.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_37()
        {
            var exp = new HypoLambda();
            var prog = "f = lambda a,b: (44); f()";
            exp.Compile(prog);
            Assert.AreEqual(44.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_38()
        {
            var exp = new HypoLambda();
            var prog = "f = lambda (a): (44); f()";
            exp.Compile(prog);
            Assert.AreEqual(44.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_39()
        {
            var exp = new HypoLambda();
            var prog = "f = lambda (a,b): (44); f()";
            exp.Compile(prog);
            Assert.AreEqual(44.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_40()
        {
            var exp = new HypoLambda();
            var prog = "f = lambda (a,b,c): (44); f(4,5,6)";
            exp.Compile(prog);
            Assert.AreEqual(44.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_41()
        {
            var exp = new HypoLambda();
            var prog = "f = lambda (a,b,c): (a+b+c); f(4,5,6)";
            exp.Compile(prog);
            Assert.AreEqual(15.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_factorial_of_v()
        {
            var exp = new HypoLambda();
            var prog = @"
factorial_of_v = lambda: ( (
    v = v - 1;
    (v+1) * factorial_of_v()
    ) if v > 1 else 1
);
v = 4;
factorial_of_v()
";
            exp.Compile(prog);
            Assert.AreEqual(24.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_factorial()
        {
            var exp = new HypoLambda();
            var prog = "f = lambda (v): ( (v*f(v-1)) if v > 1 else 1 ); f(4)";
            exp.Compile(prog);
            Assert.AreEqual(24.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_factorial_2()
        {
            var exp = new HypoLambda();
            var prog = "f = lambda v: (v*f(v-1)) if v > 1 else 1; f(4)";
            exp.Compile(prog);
            Assert.AreEqual(24.0, Convert.ToDouble(exp.Run()));
        }


        #endregion

        #region recursion

        [Test]
        public void Test_lambda_recursion()
        {
            var prog = @"
f = lambda x:
    (x*f(x-1)) if x > 1 else 1;
f(4)
";
            var exp = new HypoLambda();
            exp.Compile(prog);
            Assert.AreEqual(24.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_recursion_normal()
        {
            // sum of N first integers.
            var prog = @"
recsum = lambda x:
    x if x == 1 else x + recsum(x - 1);
recsum(5)
";
            var exp = new HypoLambda();
            exp.Compile(prog);
            // Console.WriteLine(exp.prettyAST());
            // Console.WriteLine(exp.prettyPCODE());
            Assert.AreEqual(15.0, Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_lambda_recursion_tail()
        {
            var prog = @"
recsum = lambda x, accum:
    accum if x == 0 else recsum(x - 1, accum + x);
recsum(5, 0)
";
            var exp = new HypoLambda();
            exp.Compile(prog);
            // Console.WriteLine(exp.prettyAST());
            // Console.WriteLine(exp.prettyPCODE());
            Assert.AreEqual(15.0, Convert.ToDouble(exp.Run()));
        }

        #endregion

        #region Errors

        [Test]
        public void Test_error_40()
        {
            var exp = new HypoLambda();
            var prog = @"
f = lambda: (
    f()
);
f()
";
            exp.Compile(prog);
            Assert.Throws<StackOverflowException>(() => Convert.ToDouble(exp.Run()));
        }

        [Test]
        public void Test_error_41()
        {
            var error = string.Format(
                            "Maximum recursion depth reached nearby line {0} position {1}.", 3, 6);

            var exp = new HypoLambda();
            var prog = @"
f = lambda: (
    f()
);
f()
";
            exp.Compile(prog);
            try {
                Convert.ToDouble(exp.Run());
            } catch (StackOverflowException ex) {
                Assert.AreEqual(error, ex.Message);
                return;
            }
            Assert.Fail();
        }

        [Test]
        public void Test_error_42()
        {
            var error = string.Format(
                            "Division by zero error nearby line {0} position {1}.", 4, 2);

            var exp = new HypoLambda();
            var prog = @"
v = 3;
u = 0;
v/u
";
            exp.Compile(prog);
            Convert.ToDouble(exp.Run());
            Assert.AreEqual(error, exp.LastError);
        }

        [Test]
        public void Test_error_43()
        {
            var error = string.Format(
                            "Unexpected symbol. Waits for {0} comes {1} nearby line {2} position {3}.", "els", "ident", 4, 8);

            var exp = new HypoLambda();
            var prog = @"
v = 3;
u = 0;
3 if v pepe 0
";
            try {
                exp.Compile(prog);
            } catch (ApplicationException ex) {
                Assert.AreEqual(error, ex.Message);
                return;
            }
            Assert.Fail();
        }

        [Test]
        public void Test_error_44()
        {
            var error = string.Format(
                            "Syntax error nearby line {0} position {1}.", 4, 6);

            var exp = new HypoLambda();
            var prog = @"
v = 3;
u = 0;
v + u;
";
            try {
                exp.Compile(prog);
            } catch (ApplicationException ex) {
                Assert.AreEqual(error, ex.Message);
                return;
            }
            Assert.Fail();
        }

        [Test]
        public void Test_error_45()
        {
            var exp = new HypoLambda();
            Assert.Throws<ApplicationException>(() => exp.Compile("3, 2"));
        }

        [Test]
        public void Test_error_46()
        {
            var exp = new HypoLambda();
            Assert.Throws<ApplicationException>(() => exp.Compile("f(3; 2)"));
        }

        [Test]
        public void Test_error_47()
        {
            var exp = new HypoLambda();
            Assert.Throws<ApplicationException>(() => exp.Compile("f(a = 2)"));
        }

        #endregion

        #region strings

        [Test]
        public void Test_90()
        {
            var exp = new HypoLambda();
            exp.Compile("\"Hola\"");
            Assert.AreEqual("Hola", exp.Run());
        }

        [Test]
        public void Test_91()
        {
            var exp = new HypoLambda();
            exp.Compile("\"A\" * 3");
            Assert.AreEqual("AAA", exp.Run());
        }

        [Test]
        public void Test_92()
        {
            var exp = new HypoLambda();
            exp.Compile("\"A\" + \"B\"");
            Assert.AreEqual("AB", exp.Run());
        }

        [Test]
        public void Test_93()
        {
            var exp = new HypoLambda();
            exp.Compile("  \"Hola \\\"Juan\\\"!\"  ");
            Assert.AreEqual("Hola \"Juan\"!", exp.Run());
        }

        [Test]
        public void Test_94()
        {
            var ocul = System.Threading.Thread.CurrentThread.CurrentCulture;
            System.Threading.Thread.CurrentThread.CurrentCulture = new CultureInfo("es-ES");

            var exp = new HypoLambda();
            exp.Compile(" \"A = {0:##.#}\" % a ");
            exp.SymbolTable["a"] = 100.0 / 3.0;
            var res = exp.Run();

            System.Threading.Thread.CurrentThread.CurrentCulture = ocul;

            Assert.AreEqual("A = 33,3", res);
        }

        [Test]
        public void Test_95()
        {
            var exp = new HypoLambda();
            exp.Compile(" \"Ahora: {0:dd/M/yyyy}\" % now ");
            exp.SymbolTable["now"] = new System.DateTime(2010, 3, 31, 14, 55, 58);
            Assert.AreEqual("Ahora: 31/3/2010", exp.Run());
        }

        [Test]
        public void Test_96()
        {
            var exp = new HypoLambda();
            exp.Compile(" ( \"Punto = ({0};{{0}})\" % x ) % y");
            exp.SymbolTable["x"] = 1.0;
            exp.SymbolTable["y"] = 2.0;
            Assert.AreEqual("Punto = (1;2)", exp.Run());
        }

        [Test]
        public void Test_97()
        {
            var exp = new HypoLambda();
            exp.Compile("\"Hola \" + ( \"amigo\" if {x} == 2 else \"enemigo\")");
            exp.SymbolTable["x"] = 1.0;
            Assert.AreEqual("Hola enemigo", exp.Run());
        }

        [Test]
        public void Test_100()
        {
            var e1 = new HypoLambda();
            e1.Compile("5 if this.S == this.T else 4");
            Aux y1 = new Aux();
            y1.S = null;
            y1.T = "X";
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(4.0, Convert.ToDouble(e1.Run()));
        }

        [Test]
        public void Test_101()
        {
            var e1 = new HypoLambda();
            e1.Compile("5 if this.S else 4");
            Aux y1 = new Aux();
            y1.S = " ";
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(5.0, Convert.ToDouble(e1.Run()));
        }

        [Test]
        public void Test_102()
        {
            var e1 = new HypoLambda();
            e1.Compile("5 if this.S else 4");
            Aux y1 = new Aux();
            y1.S = "";
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(4.0, Convert.ToDouble(e1.Run()));
        }

        [Test]
        public void Test_103()
        {
            var e1 = new HypoLambda();
            e1.Compile("5 if not this.S else 4");
            Aux y1 = new Aux();
            y1.S = null;
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(5.0, Convert.ToDouble(e1.Run()));
        }

        [Test]
        public void Test_104()
        {
            var e1 = new HypoLambda();
            e1.Compile("5 if this.S and this.S == \"X\" else 4");
            Aux y1 = new Aux();
            y1.S = null;
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(4.0, Convert.ToDouble(e1.Run()));
        }

        [Test]
        public void Test_105()
        {
            var e1 = new HypoLambda();
            e1.Compile("5 if this.S and this.S != \"\" else 4");
            Aux y1 = new Aux();
            y1.S = "X";
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(5.0, Convert.ToDouble(e1.Run()));
        }

        #endregion

        #region objects

        [Test]
        public void Test_180()
        {
            var e1 = new HypoLambda();
            e1.Compile("this.A1.A * this.A2.B");
            var a1 = new Aux();
            a1.A = 4;
            a1.B = 5;
            var a2 = new Aux();
            a2.A = 4;
            a2.B = 5;
            e1.SymbolTable["this"] = new { A1 = a1, A2 = a2 };
            Assert.AreEqual(20.0, Convert.ToDouble(e1.Run()));
        }

        [Test]
        public void Test_181()
        {
            var e1 = new HypoLambda();
            e1.Compile("X.X.S", true);

            var a1 = new Aux();
            a1.X = new Aux();
            a1.X.X = new Aux();
            a1.X.X.S = "Hola";
            e1.SymbolTable["this"] = a1;
            Assert.AreEqual("Hola", e1.Run());
        }

        [Test]
        public void Test_182()
        {
            var e1 = new HypoLambda();
            e1.Compile("this.X.X.S");
            var a1 = new Aux();
            a1.X = new Aux();
            a1.X.X = new Aux();
            a1.X.X.S = "Hola";
            e1.SymbolTable["this"] = a1;
            Assert.AreEqual("Hola", e1.Run());
        }

        [Test]
        public void Test_183()
        {
            var e1 = new HypoLambda("a1.X.X.S");
            var a1 = new Aux();
            a1.X = new Aux();
            a1.X.X = new Aux();
            a1.X.X.S = "Hola";
            e1.SymbolTable["a1"] = a1;
            Assert.AreEqual("Hola", e1.Run());
        }

        #endregion

        #region Lists & builtins

        [Test]
        public void Test_list_10()
        {
            var exp = new HypoLambda();
            exp.Compile("l = [1,2,3]");
            // Console.WriteLine(exp.prettyAST());
            var l = exp.Run() as IList<object>;

            Assert.IsNotNull(l);
            Assert.AreEqual(3, l.Count);
        }

        [Test]
        public void Test_list_11()
        {
            var exp = new HypoLambda();
            exp.Compile("([1,2,3] + 4 ) == [1,2,3,4]");
            var res = Convert.ToDouble(exp.Run());
            Assert.AreEqual(1.0, res);
        }


        [Test]
        public void Test_builtins_10()
        {
            var exp = new HypoLambda();
            exp.Compile("print(4,5,6)");
            var s = exp.Run() as string;
            Assert.AreEqual("4 5 6", s);
        }

        [Test]
        public void Test_builtins_11()
        {
            var exp = new HypoLambda();
            exp.Compile("first( [3,4,5] )");
            var s = Convert.ToDouble(exp.Run());
            Assert.AreEqual(3.0, s);
        }

        [Test]
        public void Test_builtins_12()
        {
            var exp = new HypoLambda();
            exp.Compile("first( rest( rest( [3,4,5] )))");
            var s = Convert.ToDouble(exp.Run());
            Assert.AreEqual(5.0, s);
        }

        [Test]
        public void Test_builtins_13()
        {
            var exp = new HypoLambda();
            exp.Compile("__add1__(4)");
            var s = Convert.ToDouble(exp.Run());
            Assert.AreEqual(5.0, s);
        }

        [Test]
        public void Test_builtins_14()
        {
            var exp = new HypoLambda();
            exp.Compile("l1 = map(__add1__, [3,4,5]); l2=[4,5,6]; l1==l2");
            var s = Convert.ToDouble(exp.Run());
            Assert.AreEqual(1.0, s);
        }

        [Test]
        public void Test_builtins_15()
        {
            var exp = new HypoLambda();
            exp.Compile("add=lambda (a,b): a+b; map(add,[3,4,5],[2,3,4]) == [5,7,9]");
            var s = Convert.ToDouble(exp.Run());
            Assert.AreEqual(1.0, s);
        }

        [Test]
        public void Test_builtins_16()
        {
            var exp = new HypoLambda();
            exp.Compile("map(\"-\",[3,4,5],[2,3,4]) == [1,1,1]");
            var s = Convert.ToDouble(exp.Run());
            Assert.AreEqual(1.0, s);
        }

        #endregion

        #region serialization

        [Test]
        public void Test_ToFrom_PCode()
        {
            var e1 = new HypoLambda();
            e1.Compile("(this.A + this.B) * 5");
            Aux y1 = new Aux();
            y1.A = 2;
            y1.B = 1;
            e1.SymbolTable["this"] = y1;
            Assert.AreEqual(15.0, Convert.ToDouble(e1.Run()));

            var pc1 = e1.ToPCODE();

            var e2 = new HypoLambda();
            e2.FromPCODE(pc1);
            //Compruebo la deserealizacion.
            Aux y2 = new Aux();
            y2.A = 2;
            y2.B = 1;
            e2.SymbolTable["this"] = y2;
            Assert.AreEqual(15.0, Convert.ToDouble(e2.Run()));
        }

        #endregion

        #region Examples

        [Test]
        public void Test_example_encapsulation()
        {
            var prog = @"
cls = lambda num: (
    n = num;
    lambda: (n = n + 1)
);
obj = cls(10);
obj();
obj()
";
            var ll = new HypoLambda();
            ll.Compile(prog);
            // Console.WriteLine(ll.prettyAST());
            var res = ll.Run();
            Assert.AreEqual(12.0, res);
        }

        [Test]
        public void Test_example_closure_1()
        {
            var prog = @"
                f = lambda x:
                        lambda y:
                            x + y
                ;
                f(2)(3)
            ";
            var ll = new HypoLambda();
            ll.Compile(prog);
            // Console.WriteLine(ll.prettyAST());
            var res = ll.Run();
            Assert.AreEqual(5.0, res);
        }

        [Test]
        public void Test_example_closure_2()
        {
            var prog = @"
add = (lambda: (
    counter = 2;
    lambda: (counter = counter + 1)
))();
add();
add()
            ";
            var ll = new HypoLambda();
            ll.Compile(prog);
            // Console.WriteLine(ll.prettyAST());
            var res = ll.Run();
            Assert.AreEqual(4.0, res);
        }

        [Test,
        Ignore("Nested names are not implemented")]
        public void Test_example_encapsulation_2()
        {
            var prog = @"
cls = lambda: (
    f = lambda: (3)
);
obj = cls();
obj.f()
            ";
            var ll = new HypoLambda();
            ll.Compile(prog);
            // Console.WriteLine(ll.prettyAST());
            var res = ll.Run();
            Assert.AreEqual(3.0, res);
        }

        [Test,
        Ignore("Nested names are not implemented")]
        public void Test_example_encapsulation_3()
        {
            var prog = @"
cls = lambda: (
    v = 4;
    w = 5
);
obj = cls();
obj.v
            ";
            var ll = new HypoLambda();
            ll.Compile(prog);
            // Console.WriteLine(ll.prettyAST());
            var res = ll.Run();
            Assert.AreEqual(4.0, res);
        }

        #endregion

        #region auxiliaries

        class Aux
        {
            public double A = 0.0;
            public double B = 0.0;
            public string S = "";
            public string T = "";
            public Aux X = null;
        }

        #endregion

        #endregion


    }
}
