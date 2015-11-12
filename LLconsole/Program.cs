using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLconsole {
    class Program {
        static void run(string program) {
            try {
                var exp = new LambdaLang.Expression();
                exp.SetExpression(program);
                var res = exp.Solve();
                Console.WriteLine();
                Console.WriteLine(res.ToString());
                if (exp.LastError != "") {
                    Console.WriteLine(exp.LastError);
                }
            } catch (Exception e) {
                Console.WriteLine();
                Console.WriteLine(e.Message);
            }
        }

        static void loop() {
            string l;
            do {
                Console.WriteLine("... Enter a LambdaLang expression or program.");
                Console.WriteLine("... ENTER ENTER to run. ^Z ENTER to exit.");
                var pg = "";
                do {
                    l = Console.ReadLine();
                    if (l != null)
                        pg += l;
                } while (l != null && l != "");
                if (!string.IsNullOrEmpty(pg))
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
