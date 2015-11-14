using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLconsole {
    /// <summary>
    /// Evaluates LambdaLang program.
    /// </summary>
    class Program {
        static void run(string program) {
            try {
                var exp = new LambdaLang.Expression();
                exp.SetExpression(program);
                var res = exp.Solve();
                Console.WriteLine(">>> " + val2string(res));
                if (exp.LastError != "") {
                    Console.WriteLine(exp.LastError);
                }
            } catch (Exception e) {
                Console.WriteLine();
                Console.WriteLine(e.Message);
            }
        }

        static string val2string(object val) {
            if (val is double)
                return val.ToString();
            if (val is string)
                return "\"" + val + "\"";
            if (val is IList<object>) {
                var lval = val as IList<object>;
                var r = "["; var rest = false;
                foreach (var o in lval) {
                    if (rest) r = r + ","; rest = true;
                    r = r + " " + val2string(o);
                }
                r = r + " ]";
                return r;
            }
            return "none";
        }

        static void loop() {
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

        static void Main(string[] args) {
            // var p = "f = lambda (v): ( (v*f(v-1)) if v > 1 else 1 ); f(4)";
            // run(p);
            loop();
        }
    }
}
