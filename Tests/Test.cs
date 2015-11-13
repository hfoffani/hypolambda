using NUnit.Framework;
using System;

using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

using LambdaLang;

namespace Tests {
	/// <summary>
	/// Test de la clase <see cref="ECC.Lib.Expression"/>
	/// </summary>
	[TestFixture]
	public class TS_Expression {

		#region Tests

		#region expresiones algebraicas

		[Test]
		public void Prueba19_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(2+3\r\n)*\n5");
			Assert.AreEqual(25.0, exp.Calculate());
		}

		[Test]
		public void Prueba20_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(2+3)*5");
			Assert.AreEqual(25.0, exp.Calculate());
		}

		[Test]
		public void Prueba21_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(i+3)*5");
			Assert.AreEqual(1, exp.SymbolTable.Count);
			Assert.IsTrue(exp.SymbolTable.ContainsKey("i"));
		}

		[Test]
		public void Prueba22_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(a.A+3)*5");
			Aux y = new Aux();
			y.A = 2;
			y.B = 1;
			exp.SymbolTable["a"] = y;
			Assert.AreEqual(25.0, exp.Calculate());
		}

		[Test]
		public void Prueba23_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(2.0+3.0)*5.0");
			Assert.AreEqual(25.0, exp.Calculate());
		}

		[Test, ExpectedException(typeof(System.ApplicationException))]
		public void Prueba24_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(2+*5.0");
		}

		[Test]
		public void Prueba25_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(A+B)*5", true);
			Aux y = new Aux();
			y.A = 2;
			y.B = 1;
			exp.SymbolTable["this"] = y;
			Assert.AreEqual(15.0, exp.Calculate());
		}

		[Test]
		public void Prueba26_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(this.A+this.B)*5");
			Aux y = new Aux();
			y.A = 2;
			y.B = 1;
			exp.SymbolTable["this"] = y;
			Assert.AreEqual(15.0, exp.Calculate());
		}

		[Test]
		public void Prueba27_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(A+B)*5");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(15.0, exp.Calculate());
		}

		[Test]
		public void Prueba28_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("3/0");
			Assert.AreEqual(double.PositiveInfinity, exp.Calculate());
		}

		[Test]
		public void Prueba29_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("({A.X}+{A.Y})*5");
			var A = new { X = 2.0, Y = 1.0 };
			exp.SymbolTable["A"] = A;
			exp.SymbolTable["this"] = A;
			Assert.AreEqual(15.0, exp.Calculate());
		}

		[Test]
		public void Prueba30_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("({A.X}+{A.Y}) == (this.X + this.Y)");
			var A = new { X = 2.0, Y = 1.0 };
			exp.SymbolTable["A"] = A;
			exp.SymbolTable["this"] = A;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba31_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("t2.Ticks - t1.Ticks");
			var now = System.DateTime.Now;
			exp.SymbolTable["t1"] = now;
			exp.SymbolTable["t2"] = now;
			Assert.AreEqual(0.0, exp.Calculate());
		}

		#endregion

		#region expresiones logicas cadenas

		[Test]
		public void Prueba40_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A>B");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = "b";
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba41_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A==B");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = "b";
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba42_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A<B");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = "b";
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba43_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A>=B");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = "b";
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba45_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A!=B");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = "b";
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba46_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(A<B)");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = "b";
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba47_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("not (A<B)");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = "b";
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba48_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("not (A!=B)");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = "b";
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba112_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A and not B");
			exp.SymbolTable["A"] = "a";
			exp.SymbolTable["B"] = null;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		#endregion

		#region expresiones logicas numeros

		[Test]
		public void Prueba50_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A>B");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba51_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A==B");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba52_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A<B");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba53_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A>=B");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba55_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("A!=B");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba56_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(A<B)");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba57_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("not (A<B)");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba58_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("not A");
			exp.SymbolTable["A"] = 2.0;
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba60_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(A > B) and (A > B)");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba61_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(A > B) and (A < B)");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba62_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(A < B) or (A < B)");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba63_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(A < B) or (A > B)");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba64_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("not ((A < B) or (A < B))");
			exp.SymbolTable["A"] = 2.0;
			exp.SymbolTable["B"] = 1.0;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba65_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("(not 0) * 2");
			Assert.AreEqual(2.0, exp.Calculate());
		}

		#endregion

		#region if expression

		[Test]
		public void Prueba70_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("3 if 1 else 4");
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Prueba71_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("3 if 0 else 4");
			Assert.AreEqual(4.0, exp.Calculate());
		}

		[Test]
		public void Prueba72_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("3 if not 0 else 4");
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Prueba73_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("3 if \"A\" == \"A\" else 4");
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Prueba74_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("3 if \"A\" != \"A\" else 4");
			Assert.AreEqual(4.0, exp.Calculate());
		}

		[Test]
		public void Prueba75_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("\"fail\" if (A != \"A\") else \"ok\"");
			exp.SymbolTable["A"] = "A";
			Assert.AreEqual("ok", exp.Solve());
		}

		// tests para short-circuit.
		// el 1º es para asegurarnos que salta una excepcion.
		[Test, ExpectedException(typeof(System.NullReferenceException))]
		public void Prueba130_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("x.X.A == 3");
			var x = new Aux();
			exp.SymbolTable["x"] = x;
			exp.Calculate();
		}

		// si hay short-circuit en AND no deberia haber excepcion.
		[Test]
		public void Prueba131_TestRecursiveDescent() {
			var exp = new Expression("0 and x.X.A == 3");
			var x = new Aux();
			exp.SymbolTable["x"] = x;
			Assert.AreEqual(0.0, exp.Calculate());
		}

		// si hay short-circuit en OR no deberia haber excepcion.
		[Test]
		public void Prueba132_TestRecursiveDescent() {
			var exp = new Expression("1 or x.X.A == 3");
			var x = new Aux();
			exp.SymbolTable["x"] = x;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		// si hay short-circuit en IFF-TRUE no deberia haber excepcion.
		[Test]
		public void Prueba133_TestRecursiveDescent() {
			var exp = new Expression("5 if 1 else x.X.A");
			var x = new Aux();
			exp.SymbolTable["x"] = x;
			Assert.AreEqual(5.0, exp.Calculate());
		}

		// si hay short-circuit en IFF-FALSE no deberia haber excepcion.
		[Test]
		public void Prueba134_TestRecursiveDescent() {
			var exp = new Expression("x.X.A if 0 else 5");
			var x = new Aux();
			exp.SymbolTable["x"] = x;
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Prueba140_TestRecursiveDescent() {
			var exp = new Expression("1 and \"x\" and 3");
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Prueba141_TestRecursiveDescent() {
			var exp = new Expression("0 or \"\" or 3");
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Prueba142_TestRecursiveDescent() {
			var exp = new Expression("3 and 0");
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba143_TestRecursiveDescent() {
			var exp = new Expression("3 and not 0");
			Assert.AreEqual(1.0, exp.Calculate());
		}

		[Test]
		public void Prueba144_TestRecursiveDescent() {
			var exp = new Expression("0 or not 3");
			Assert.AreEqual(0.0, exp.Calculate());
		}

		[Test]
		public void Prueba150_TestRecursiveDescent() {
			var exp = new Expression("\"y\" and 3 and \"x\"");
			Assert.AreEqual("x", exp.Solve());
		}

		[Test]
		public void Prueba151_TestRecursiveDescent() {
			var exp = new Expression("0 or \"\" or \"x\"");
			Assert.AreEqual("x", exp.Solve());
		}

		[Test]
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

		[Test]
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
		[Test]
		public void Prueba170_TestRecursiveDescent() {
			var exp = new Expression("1 if this.S else 1/this.S");

			var aux = new Aux();
			aux.S = "AAA";
			exp.SymbolTable["this"] = aux;
			Assert.AreEqual(1.0, exp.Calculate());
		}

		// de Expression.doc
		[Test]
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

		[Test]
		public void Prueba80_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("1 + 2 if 3 > 4 else 5 - 6");
			Assert.AreEqual(-1, exp.Calculate());
		}

		[Test]
		public void Prueba81_TestRecursiveDescent() {
			var exp = new Expression();
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

		#region lambda: expressions

		[Test]
		public void Test_lambda_10() {
			var exp = new Expression();
			exp.SetExpression("3; 2");
			Assert.AreEqual(2.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_11() {
			var exp = new Expression();
			exp.SetExpression("3; 2; 5");
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_12() {
			var exp = new Expression();
			exp.SetExpression("a = 3");
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_13() {
			var exp = new Expression();
			exp.SetExpression("a = 3; 5; a");
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_01() {
			var exp = new Expression();
			exp.SetExpression("(lambda: 2+3)()");
			Console.WriteLine();
			Console.WriteLine(exp.prettyAST());
			Console.WriteLine();
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_02() {
			var exp = new Expression();
			exp.SetExpression("v=3; (lambda: 2+v)()");
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_03() {
			var exp = new Expression();
			exp.SetExpression("(f = lambda: 2+3); 9");
			Assert.AreEqual(9.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_04() {
			var exp = new Expression();
			exp.SetExpression("(f = lambda: 2+3); f()");
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_05() {
			var exp = new Expression();
			exp.SetExpression("(f = lambda: 2+v); (v = 3); f()");
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_06() {
			var exp = new Expression();
			exp.SetExpression("f = lambda: 2+v; v = 3; f()");
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_07() {
			var exp = new Expression();
			exp.SetExpression("f = lambda: (y=4; y+v); v = 3; f()");
			Assert.AreEqual(7.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_08() {
			var exp = new Expression();
			var prog = @"
f = lambda: (
    y = 4;
    y + v
);
v = 3;
f()
";
			exp.SetExpression(prog);
			Assert.AreEqual(7.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_21() {
			var exp = new Expression();
			var prog = @"
u = 3;
v = (u+1; 5);
v
";
			exp.SetExpression(prog);
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_22() {
			var exp = new Expression();
			var prog = @"
u = 3;
v = (4 if u < 2 else 5);
v
";
			exp.SetExpression(prog);
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_23() {
			var exp = new Expression();
			var prog = @"
u = 3;
v = (u if u > 0 else 5);
v
";
			exp.SetExpression(prog);
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_24() {
			var exp = new Expression();
			var prog = @"
f = lambda: (
    v = (u - 1 if u > 0 else u);
    v
);
u = 3;
f()
";
			exp.SetExpression(prog);
			Assert.AreEqual(2.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_30() {
			var exp = new Expression();
			var prog = @"
f = lambda: (
    v = (v - 1 if v > 0 else v)
);
v = 3;
f()
";
			exp.SetExpression(prog);
			Assert.AreEqual(2.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_31() {
			var exp = new Expression();
			var prog = @"
v = 3;
(v = v - 1) if v > 3 else 1;
v
";
			exp.SetExpression(prog);
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_32() {
			var exp = new Expression();
			var prog = @"
f = lambda: (
    v - 1
);
v = 3;
(v = f()) if v > 3 else 1;
v
";
			exp.SetExpression(prog);
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_33() {
			var exp = new Expression();
			var prog = @"
f = lambda: (
    v = v - 1
);
v = 3;
f() if v > 3 else 1;
v
";
			exp.SetExpression(prog);
			Assert.AreEqual(3.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_34() {
			var exp = new Expression();
			var prog = @"
f = lambda: (
    (v = v - 1) if v > 0 else v);
    f() if v > 0 else 5
);
v = 3;
f()
";
			exp.SetExpression(prog);
			Assert.AreEqual(5.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_36() {
			var exp = new Expression();
			var prog = "f = lambda a: (43+1); f()";
			exp.SetExpression(prog);
			Console.WriteLine(exp.prettyAST());
			Assert.AreEqual(44.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_37() {
			var exp = new Expression();
			var prog = "f = lambda a,b: (44); f()";
			exp.SetExpression(prog);
			Assert.AreEqual(44.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_38() {
			var exp = new Expression();
			var prog = "f = lambda (a): (44); f()";
			exp.SetExpression(prog);
			Assert.AreEqual(44.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_39() {
			var exp = new Expression();
			var prog = "f = lambda (a,b): (44); f()";
			exp.SetExpression(prog);
			Assert.AreEqual(44.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_40() {
			var exp = new Expression();
			var prog = "f = lambda (a,b,c): (44); f(4,5,6)";
			exp.SetExpression(prog);
			Assert.AreEqual(44.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_41() {
			var exp = new Expression();
			var prog = "f = lambda (a,b,c): (a+b+c); f(4,5,6)";
			exp.SetExpression(prog);
			Assert.AreEqual(15.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_factorial_of_v() {
			var exp = new Expression();
			var prog = @"
factorial_of_v = lambda: ( (
    v = v - 1;
    (v+1) * factorial_of_v()
    ) if v > 1 else 1
);
v = 4;
factorial_of_v()
";
			exp.SetExpression(prog);
			Assert.AreEqual(24.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_factorial() {
			var exp = new Expression();
			var prog = "f = lambda (v): ( (v*f(v-1)) if v > 1 else 1 ); f(4)";
			exp.SetExpression(prog);
			Assert.AreEqual(24.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_factorial_2() {
			var exp = new Expression();
			var prog = "f = lambda v: (v*f(v-1)) if v > 1 else 1; f(4)";
			exp.SetExpression(prog);
			Assert.AreEqual(24.0, exp.Calculate());
		}


		#endregion

		#region recursion

		[Test]
		public void Test_lambda_recursion() {
			var prog = @"
f = lambda x:
    (x*f(x-1)) if x > 1 else 1;
f(4)
";
			var exp = new Expression();
			exp.SetExpression(prog);
			Assert.AreEqual(24.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_recursion_normal() {
			// sum of N first integers.
			var prog = @"
recsum = lambda x:
    x if x == 1 else x + recsum(x - 1);
recsum(5)
";
			var exp = new Expression();
			exp.SetExpression(prog);
			Console.WriteLine(exp.prettyAST());
			Console.WriteLine(exp.prettyPCODE());
			Assert.AreEqual(15.0, exp.Calculate());
		}

		[Test]
		public void Test_lambda_recursion_tail() {
			var prog = @"
recsum = lambda x, accum:
    accum if x == 0 else recsum(x - 1, accum + x);
recsum(5, 0)
";
			var exp = new Expression();
			exp.SetExpression(prog);
			Console.WriteLine(exp.prettyAST());
			Console.WriteLine(exp.prettyPCODE());
			Assert.AreEqual(15.0, exp.Calculate());
		}

		#endregion

		#region Errors

		[Test, ExpectedException(typeof(StackOverflowException))]
		public void Test_error_40() {
			var exp = new Expression();
			var prog = @"
f = lambda: (
    f()
);
f()
";
			exp.SetExpression(prog);
			exp.Calculate();
		}

		[Test]
		public void Test_error_41() {
			var error = string.Format(
				"Maximum recursion depth reached nearby line {0} position {1}.", 3, 6);

			var exp = new Expression();
			var prog = @"
f = lambda: (
    f()
);
f()
";
			exp.SetExpression(prog);
			try {
				exp.Calculate();
			} catch (StackOverflowException ex) {
				Assert.AreEqual(error, ex.Message);
				return;
			}
			Assert.Fail();
		}

		[Test]
		public void Test_error_42() {
			var error = string.Format(
				"Division by zero error nearby line {0} position {1}.", 4, 2);

			var exp = new Expression();
			var prog = @"
v = 3;
u = 0;
v/u
";
			exp.SetExpression(prog);
			exp.Calculate();
			Assert.AreEqual(error, exp.LastError);
		}

		[Test]
		public void Test_error_43() {
			var error = string.Format(
				"Unexpected symbol. Waits for {0} comes {1} nearby line {2} position {3}.", "els","ident", 4, 8);

			var exp = new Expression();
			var prog = @"
v = 3;
u = 0;
3 if v pepe 0
";
			try {
				exp.SetExpression(prog);
			} catch (ApplicationException ex) {
				Assert.AreEqual(error, ex.Message);
				return;
			}
			Assert.Fail();
		}

		[Test]
		public void Test_error_44() {
			var error = string.Format(
				"Syntax error nearby line {0} position {1}.", 4, 6);

			var exp = new Expression();
			var prog = @"
v = 3;
u = 0;
v + u;
";
			try {
				exp.SetExpression(prog);
			} catch (ApplicationException ex) {
				Assert.AreEqual(error, ex.Message);
				return;
			}
			Assert.Fail();
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void Test_error_45() {
			var exp = new Expression();
			exp.SetExpression("3, 2");
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void Test_error_46() {
			var exp = new Expression();
			exp.SetExpression("f(3; 2)");
		}

		[Test, ExpectedException(typeof(ApplicationException))]
		public void Test_error_47() {
			var exp = new Expression();
			exp.SetExpression("f(a = 2)");
		}

		#endregion

		#region cadenas

		[Test]
		public void Prueba90_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("\"Hola\"");
			Assert.AreEqual("Hola", exp.Solve());
		}

		[Test]
		public void Prueba91_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("\"A\" * 3");
			Assert.AreEqual("AAA", exp.Solve());
		}

		[Test]
		public void Prueba92_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("\"A\" + \"B\"");
			Assert.AreEqual("AB", exp.Solve());
		}

		[Test]
		public void Prueba93_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("  \"Hola \\\"Juan\\\"!\"  ");
			Assert.AreEqual("Hola \"Juan\"!", exp.Solve());
		}

		[Test]
		public void Prueba94_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression(" \"A = {0:##.#}\" % a ");
			exp.SymbolTable["a"] = 100.0 / 3.0;
			Assert.AreEqual("A = 33,3", exp.Solve());
		}

		[Test]
		public void Prueba95_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression(" \"Ahora: {0:dd/M/yyyy}\" % now ");
			exp.SymbolTable["now"] = new System.DateTime(2010, 3, 31, 14, 55, 58);
			Assert.AreEqual("Ahora: 31/3/2010", exp.Solve());
		}

		[Test]
		public void Prueba96_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression(" ( \"Punto = ({0};{{0}})\" % x ) % y");
			exp.SymbolTable["x"] = 1.0;
			exp.SymbolTable["y"] = 2.0;
			Assert.AreEqual("Punto = (1;2)", exp.Solve());
		}

		[Test]
		public void Prueba97_TestRecursiveDescent() {
			var exp = new Expression();
			exp.SetExpression("\"Hola \" + ( \"amigo\" if {x} == 2 else \"enemigo\")");
			exp.SymbolTable["x"] = 1.0;
			Assert.AreEqual("Hola enemigo", exp.Solve());
		}

		[Test]
		public void Prueba100_TestRecursiveDescent() {
			Expression e1 = new Expression();
			e1.SetExpression("5 if this.S == this.T else 4");
			Aux y1 = new Aux();
			y1.S = null;
			y1.T = "X";
			e1.SymbolTable["this"] = y1;
			Assert.AreEqual(4.0, e1.Calculate());
		}

		[Test]
		public void Prueba101_TestRecursiveDescent() {
			Expression e1 = new Expression();
			e1.SetExpression("5 if this.S else 4");
			Aux y1 = new Aux();
			y1.S = " ";
			e1.SymbolTable["this"] = y1;
			Assert.AreEqual(5.0, e1.Calculate());
		}

		[Test]
		public void Prueba102_TestRecursiveDescent() {
			Expression e1 = new Expression();
			e1.SetExpression("5 if this.S else 4");
			Aux y1 = new Aux();
			y1.S = "";
			e1.SymbolTable["this"] = y1;
			Assert.AreEqual(4.0, e1.Calculate());
		}

		[Test]
		public void Prueba103_TestRecursiveDescent() {
			Expression e1 = new Expression();
			e1.SetExpression("5 if not this.S else 4");
			Aux y1 = new Aux();
			y1.S = null;
			e1.SymbolTable["this"] = y1;
			Assert.AreEqual(5.0, e1.Calculate());
		}

		[Test]
		public void Prueba104_TestRecursiveDescent() {
			Expression e1 = new Expression();
			e1.SetExpression("5 if this.S and this.S == \"X\" else 4");
			Aux y1 = new Aux();
			y1.S = null;
			e1.SymbolTable["this"] = y1;
			Assert.AreEqual(4.0, e1.Calculate());
		}

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		[Test]
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

		#region Lists & builtins

		[Test]
		public void Test_list_10() {
			var exp = new Expression();
			exp.SetExpression("l = [1,2,3]");
			Console.WriteLine(exp.prettyAST());
			var l = exp.Solve() as IList<object>;

			Assert.IsNotNull(l);
			Assert.AreEqual(3, l.Count);
		}

		[Test]
		public void Test_list_11() {
			var exp = new Expression();
			exp.SetExpression("([1,2,3] + 4 ) == [1,2,3,4]");
			var res = exp.Calculate();
			Assert.AreEqual(1.0, res);
		}


		[Test]
		public void Test_builtins_10() {
			var exp = new Expression();
			exp.SetExpression("print(4,5,6)");
			var s = exp.Solve() as string;
			Assert.AreEqual("4 5 6", s);
		}

		[Test]
		public void Test_builtins_11() {
			var exp = new Expression();
			exp.SetExpression("first( [3,4,5] )");
			var s = exp.Calculate();
			Assert.AreEqual(3.0, s);
		}

		[Test]
		public void Test_builtins_12() {
			var exp = new Expression();
			exp.SetExpression("first( rest( rest( [3,4,5] )))");
			var s = exp.Calculate();
			Assert.AreEqual(5.0, s);
		}

		[Test]
		public void Test_builtins_13() {
			var exp = new Expression();
			exp.SetExpression("__add1__(4)");
			var s = exp.Calculate();
			Assert.AreEqual(5.0, s);
		}

		[Test]
		public void Test_builtins_14() {
			var exp = new Expression();
			exp.SetExpression("l1 = map(__add1__, [3,4,5]); l2=[4,5,6]; l1==l2");
			var s = exp.Calculate();
			Assert.AreEqual(1.0, s);
		}

		[Test]
		public void Test_builtins_15() {
			var exp = new Expression();
			exp.SetExpression("add=lambda (a,b): a+b; map(add,[3,4,5],[2,3,4]) == [5,7,9]");
			var s = exp.Calculate();
			Assert.AreEqual(1.0, s);
		}

		[Test]
		public void Test_builtins_16() {
			var exp = new Expression();
			exp.SetExpression("map(\"-\",[3,4,5],[2,3,4]) == [1,1,1]");
			var s = exp.Calculate();
			Assert.AreEqual(1.0, s);
		}

		#endregion

		#region serializacion

		/// <summary>
		/// Prueba Serializacion
		/// </summary>
		[Test]
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
		[Test,
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


	}
}
