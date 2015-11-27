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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using HL;

namespace HL
{
	/// <summary>
	/// Evaluates LambdaLang program.
	/// </summary>
	class Program
	{
		static void run(string program)
		{
			try {
				var exp = new HypoLambda();
				exp.Compile(program);
				var res = exp.Run();
				Console.WriteLine(">>> " + val2string(res));
				if (exp.ErrorMessage != "") {
					Console.WriteLine(exp.ErrorMessage);
				}
			} catch (Exception e) {
				Console.WriteLine();
				Console.WriteLine(e.Message);
			}
		}

		static string val2string(object val)
		{
			if (val is double)
				return val.ToString();
			if (val is string)
				return "\"" + val + "\"";
			if (val is IList<object>) {
				var lval = val as IList<object>;
				var r = "[";
				var rest = false;
				foreach (var o in lval) {
					if (rest)
						r = r + ",";
					rest = true;
					r = r + " " + val2string(o);
				}
				r = r + " ]";
				return r;
			}
			return "none";
		}

		static void loop()
		{
			string l;
			Console.WriteLine("... Enter a LambdaLang expression or program.");
			Console.WriteLine("... ENTER to execute. ^Z ENTER to exit.");
			do {
				Console.WriteLine("");
				var pg = "";
				do {
					l = Console.ReadLine();
					if (l != null)
						pg += l;
				} while (l != null && l != "");
				if (string.IsNullOrEmpty(pg))
					return;
				run(pg);
			} while (l != null);
		}

		static void Main(string[] args)
		{
			// var p = "f = lambda (v): ( (v*f(v-1)) if v > 1 else 1 ); f(4)";
			// run(p);
			loop();
		}
	}
}
