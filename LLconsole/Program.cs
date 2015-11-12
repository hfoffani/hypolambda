using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LLconsole {
    class Program {
        static void run(string program) {
            var exp = new LambdaLang.Expression();
            exp.SetExpression(program);
            var res = exp.Solve();
            Console.WriteLine();
            Console.WriteLine(res.ToString());
        }

        static void loop() {
            string l;
            do {
                Console.WriteLine("...");
                var pg = "";
                do {
                    l = Console.ReadLine();
                    if (l != null)
                        pg += l;
                } while (l != null && l != "");
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
