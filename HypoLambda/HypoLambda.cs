
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection;

using System.Linq;

#if DEBUG
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Tests")]
#endif

namespace HL
{
    /// <summary>
    /// A Functional Language.
    /// </summary>
    public partial class HypoLambda
    {
        #region Fields

        private Dictionary<string, object> symbolTable;

        private int labelNumber = 1;
        private string labelFmt = "LBL_{0:D4}";

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor.
        /// </summary>
        /// <param name="expStr">Expression to compile.</param>
        public HypoLambda(string expStr)
            : this()
        {
            this.Compile(expStr, false);
        }

        /// <summary>
        /// Constructor.
        /// </summary>
        public HypoLambda()
        {
            this.symbolTable = new Dictionary<string, object>();

            this.reservedwords.Add("not", new Terminal(TokenType.not, 0, 0));
            this.reservedwords.Add("and", new Terminal(TokenType.and, 0, 0));
            this.reservedwords.Add("or", new Terminal(TokenType.or, 0, 0));
            this.reservedwords.Add("if", new Terminal(TokenType.iff, 0, 0));
            this.reservedwords.Add("else", new Terminal(TokenType.els, 0, 0));
            this.reservedwords.Add("lambda", new Terminal(TokenType.lambda, 0, 0));
        }

        #endregion

        #region Properties, Accesors and Modifiers

        /// <summary>
        /// Symbol table.
        /// </summary>
        public Dictionary<string, object> SymbolTable
        {
            get { return this.symbolTable; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Compiles an expression.
        /// </summary>
        /// <remarks>
        /// <para>Parse an expression using a recursive-descending
        /// parser, buidls an abstract syntactic tree and generates a PCODE.
        /// Completes the <see cref="Expression.SymbolTable"/></para>
        /// </remarks>
        /// <param name="expStr">Expresion.</param>
        public void Compile(string expStr)
        {
            this.Compile(expStr, false);
        }

        /// <summary>
        /// Run the expression.
        /// </summary>
        /// <returns>The result.</returns>
        public object Run()
        {
            LastError = "";
            if (pcode != null) {
                var lambda = new lambdatuple();
                lambda.Body = pcode;
                return CalculateNPI2(lambda, 0);
            } else {
                throw new ApplicationException("Can't execute a non valid program.");
            }
        }

        /// <summary>
        /// Obtains the list of identifiers of the expression.
        /// </summary>
        public IEnumerable<string> GetIdentifiers()
        {
            foreach (var t in pcode)
                if (t.TokenType == TokenType.ident)
                    yield return t.Value.ToString();
        }

        /// <summary>
        /// Obtains the last error message.
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Creates an expression from a given pcode.
        /// </summary>
        /// <param name="pcode">A pcode.</param>
        public void FromPCODE(string pcode)
        {
            this.pcode = new List<Terminal>();
            var ts = pcode.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var t in ts) {
                var parts = t.Split('|');
                var ttype = (TokenType)Enum.Parse(typeof(TokenType), parts[0]);
                var ln = int.Parse(parts[1]);
                var cp = int.Parse(parts[2]);
                var value = parts[3];
                var term = new Terminal(ttype, value, ln, cp);
                this.pcode.Add(term);
            }
        }

        /// <summary>
        /// Obtains the pcode corresponding to this instance.
        /// </summary>
        /// <returns>A pcode.</returns>
        public string ToPCODE()
        {
            var sb = new StringBuilder();
            foreach (var t in this.pcode) {
                sb.Append(t.TokenType.ToString()); sb.Append("|");
                sb.Append(t.LN.ToString()); sb.Append("|");
                sb.Append(t.CP.ToString()); sb.Append("|");
                sb.Append(t.Value == null ? "null" : t.Value.ToString());
                sb.AppendLine();
            }
            return sb.ToString();
        }

        #endregion

        #region evaluation.

        private object CalculateNPI2(lambdatuple closure, int stackFrames)
        {
            // los Convert.ToDouble son necesarios porque es posible que alguna
            // de las propiedades de objetos utilizados devuelvan int long o float.
            // asi nos aseguramos que los calculos se hagan siempre con double.

            var proveedor = closure.Body;
            var pila = new Stack<object>();
            var currentscope = closure.Locals;

            double a, b, res;
            string sa, sb, str;
            string ignoreuptolabel = "";

            for (int poster = 0; poster < proveedor.Count; poster++) {
                var t = proveedor[poster];

                #region check recursion depth.
                if (stackFrames > 500) {
                    LastError = string.Format(Strings.Expression_MaxRecursionDepth, t.LN, t.CP);
                    throw new StackOverflowException(LastError);
                }
                #endregion

                #region jump ppmente dicho
                if (ignoreuptolabel != "") {
                    if (t.TokenType == TokenType.label && ignoreuptolabel == (string)t.Value)
                        ignoreuptolabel = "";
                    continue;
                }
                #endregion

                switch (t.TokenType) {

                    #region tipos

                    case TokenType.number:
                        double cte = Convert.ToDouble(t.Value);
                        pila.Push(cte);
                        break;
                    case TokenType.ident:
                        pila.Push(getValue((string)t.Value, currentscope));
                        break;
                    case TokenType.str:
                        str = (string)t.Value;
                        pila.Push(str);
                        break;

                    #endregion

                    #region operadores matematicos

                    case TokenType.plus:
                        var x = pila.Pop();
                        if (pila.Peek() is List<object>) {
                            var lb = new List<object>((IList<object>)pila.Pop());
                            lb.Add(x);
                            pila.Push(lb);
                        } else if (pila.Peek() is string) {
                            sb = (string)x;
                            sa = (string)pila.Pop();
                            str = sa + sb;
                            pila.Push(str);
                        } else {
                            b = Convert.ToDouble(x);
                            a = Convert.ToDouble(pila.Pop());
                            res = a + b;
                            pila.Push(res);
                        }
                        break;
                    case TokenType.minus:
                        b = Convert.ToDouble(pila.Pop());
                        a = Convert.ToDouble(pila.Pop());
                        res = a - b;
                        pila.Push(res);
                        break;
                    case TokenType.times:
                        b = Convert.ToDouble(pila.Pop());
                        if (pila.Peek() is string) {
                            sa = (string)pila.Pop();
                            str = "";
                            for (; b > 0; b--) {
                                str += sa;
                            }
                            pila.Push(str);
                        } else {
                            a = Convert.ToDouble(pila.Pop());
                            res = a * b;
                            pila.Push(res);
                        }
                        break;
                    case TokenType.slash:
                        b = Convert.ToDouble(pila.Pop());
                        a = Convert.ToDouble(pila.Pop());
                        if (b == 0.0) {
                            LastError = string.Format(Strings.Expression_ZeroDivisionError, t.LN, t.CP);
                        }
                        res = a / b;
                        pila.Push(res);
                        break;
                    case TokenType.perc:
                        object o = pila.Pop();
                        sa = (string)pila.Pop();
                        pila.Push(string.Format(sa, o));
                        break;

                    #endregion

                    #region operadores de comparacion

                    case TokenType.eq:
                        if (pila.Peek() is IList<object>) {
                            var lb = (IList<object>)pila.Pop();
                            var la = (IList<object>)pila.Pop();
                            if (la.Count != lb.Count)
                                pila.Push(0.0);
                            else {
                                var areeq = 1.0;
                                for (int i = 0; i < la.Count; i++)
                                    if (!la[i].Equals(lb[i]))
                                        areeq = 0.0;
                                pila.Push(areeq);
                            }
                        } else if (pila.Peek() is string) {
                            sb = (string)pila.Pop();
                            sa = (string)pila.Pop();
                            pila.Push((sa == sb ? 1.0 : 0.0));
                        } else {
                            b = Convert.ToDouble(pila.Pop());
                            a = Convert.ToDouble(pila.Pop());
                            pila.Push((a == b ? 1.0 : 0.0));
                        }
                        break;
                    case TokenType.neq:
                        if (pila.Peek() is IList<object>) {
                            var lb = (IList<object>)pila.Pop();
                            var la = (IList<object>)pila.Pop();
                            if (la.Count != lb.Count)
                                pila.Push(1.0);
                            else {
                                var areneq = 0.0;
                                for (int i = 0; i < la.Count; i++)
                                    if (!la[i].Equals(lb[i]))
                                        areneq = 1.0;
                                pila.Push(areneq);
                            }
                        } else if (pila.Peek() is string) {
                            sb = (string)pila.Pop();
                            sa = (string)pila.Pop();
                            pila.Push((sa != sb ? 1.0 : 0.0));
                        } else {
                            b = Convert.ToDouble(pila.Pop());
                            a = Convert.ToDouble(pila.Pop());
                            pila.Push((a != b ? 1.0 : 0.0));
                        }
                        break;
                    case TokenType.gt:
                        if (pila.Peek() is string) {
                            sb = (string)pila.Pop();
                            sa = (string)pila.Pop();
                            pila.Push((sb.CompareTo(sa) > 0 ? 1.0 : 0.0));
                        } else {
                            b = Convert.ToDouble(pila.Pop());
                            a = Convert.ToDouble(pila.Pop());
                            pila.Push((a > b ? 1.0 : 0.0));
                        }
                        break;
                    case TokenType.gteq:
                        if (pila.Peek() is string) {
                            sb = (string)pila.Pop();
                            sa = (string)pila.Pop();
                            pila.Push((sb.CompareTo(sa) >= 0 ? 1.0 : 0.0));
                        } else {
                            b = Convert.ToDouble(pila.Pop());
                            a = Convert.ToDouble(pila.Pop());
                            pila.Push((a >= b ? 1.0 : 0.0));
                        }
                        break;
                    case TokenType.lt:
                        if (pila.Peek() is string) {
                            sb = (string)pila.Pop();
                            sa = (string)pila.Pop();
                            pila.Push((sb.CompareTo(sa) < 0 ? 1.0 : 0.0));
                        } else {
                            b = Convert.ToDouble(pila.Pop());
                            a = Convert.ToDouble(pila.Pop());
                            pila.Push((a < b ? 1.0 : 0.0));
                        }
                        break;
                    case TokenType.lteq:
                        if (pila.Peek() is string) {
                            sb = (string)pila.Pop();
                            sa = (string)pila.Pop();
                            pila.Push((sb.CompareTo(sa) <= 0 ? 1.0 : 0.0));
                        } else {
                            b = Convert.ToDouble(pila.Pop());
                            a = Convert.ToDouble(pila.Pop());
                            pila.Push((a <= b ? 1.0 : 0.0));
                        }
                        break;

                    #endregion

                    #region operadores booleanos

                    case TokenType.and:
                        // ya resuelto por los short-circuit.
                        break;
                    case TokenType.or:
                        // ya resuelto por los short-circuit.
                        break;
                    case TokenType.not:
                        res = (truthvalue(pila.Pop()) ? 0.0 : 1.0);
                        pila.Push(res);
                        break;

                    case TokenType.iff:
                        // codificado como and/or. arreglo stack.
                        object ores = pila.Pop();
                        pila.Pop();
                        pila.Push(ores);
                        break;

                    #endregion

                    #region jump tokens

                    case TokenType.jmpzero:
                        if (!truthvalue(pila.Peek())) {
                            ignoreuptolabel = (string)t.Value;
                        }
                        break;

                    case TokenType.jmpnotz:
                        if (truthvalue(pila.Peek())) {
                            ignoreuptolabel = (string)t.Value;
                        }
                        break;

                    case TokenType.jmp:
                        ignoreuptolabel = (string)t.Value;
                        break;

                    #endregion

                    #region Turing

                    case TokenType.eval:
                        var lambda = pila.Pop() as lambdatuple;
                        var binds = pila.Pop() as List<object>;
                        if (lambda != null) {
                            var reslambda = lambdaeval(lambda, currentscope, binds, stackFrames);
                            pila.Push(reslambda);
                            // debug_scope("after eval", currentscope);
                        }
                        break;

                    case TokenType.identlocal:
                        var vallocal = pila.Pop();
                        var valname = t.Value.ToString();
                        if (currentscope.ContainsKey(valname)) {
                            currentscope[valname] = vallocal;
                        } else {
                            currentscope.Add(valname, vallocal);
                        }
                        break;

                    case TokenType.lambdahead:
                        pila.Push(t.Value);
                        break;
                    case TokenType.lambdabody:
                        var pcbody = pcodebody(t.Value.ToString(), proveedor);
                        pila.Push(pcbody);
                        break;
                    case TokenType.lambda:
                        var ls = new lambdatuple();
                        ls.Body = pila.Pop() as IList<Terminal>;
                        ls.Head = pila.Pop() as string[];
                        ls.Locals = new Dictionary<string, object>(currentscope);
                        pila.Push(ls);
                        break;

                    case TokenType.list:
                        pila.Push(new List<object>());
                        break;

                    case TokenType.comma:
                        var tail = pila.Pop() as List<object>;
                        var head = pila.Pop();
                        var li = new List<object>();
                        li.Add(head);
                        li.AddRange(tail);
                        pila.Push(li);
                        break;

                    case TokenType.semicolon:
                        var oc = pila.Pop();
                        pila.Pop();
                        pila.Push(oc);
                        break;

                    case TokenType.assig:
                        pila.Push(getValue(t.Value.ToString(), currentscope));
                        break;

                    #endregion
                }
            }
            // debug_scope("before return", currentscope);
            if (pila.Count > 0)
                return pila.Pop();
            else
                return 0.0;
        }

        private void debug_scope(string label, Dictionary<string, object> scope)
        {
            Console.WriteLine(label);
            foreach (var valsss in scope) {
                Console.WriteLine(valsss.Key + " -> " + valsss.Value.ToString());
                var lll = valsss.Value as lambdatuple;
                if (lll != null) {
                    foreach (var lllvalsss in lll.Locals) {
                        Console.WriteLine("        " + lllvalsss.Key + " -> " + lllvalsss.Value.ToString());
                    }
                }
            }
            Console.WriteLine();
        }

        private List<Terminal> pcodebody(string label, IList<Terminal> pcode)
        {
            var body = new List<Terminal>();
            bool copy = false;
            for (int i = 0; i < pcode.Count; i++) {
                if (pcode[i].TokenType == TokenType.jmp && pcode[i].Value.ToString() == label) {
                    copy = true;
                    continue;
                }
                if (pcode[i].TokenType == TokenType.label && pcode[i].Value.ToString() == label)
                    break;
                if (copy)
                    body.Add(pcode[i]);
            }
            return body;
        }

        private object lambdaeval(lambdatuple lambda, IDictionary<string, object> enclosingScope, IList<object> binds, int stackFrames)
        {
            if (lambda.Builtin != null) {
                return _bultins[lambda.Builtin](binds, this, lambda.Locals, stackFrames);
            } else {
                var newscope = lambda.Locals;
                if (enclosingScope != newscope)
                    foreach (var enclosing in enclosingScope) {
                        newscope[enclosing.Key] = enclosing.Value;
                    }
                for (int j = 0; j < lambda.Head.Length; j++) {
                    if (j < binds.Count) {
                        var keyhead = lambda.Head[j];
                        if (newscope.ContainsKey(keyhead)) {
                            newscope[keyhead] = binds[j];
                        } else {
                            newscope.Add(keyhead, binds[j]);
                        }
                    }
                }
                var closure = new lambdatuple();
                closure.Head = lambda.Head;
                closure.Body = lambda.Body;
                closure.Locals = newscope;

                var reslambda = CalculateNPI2(closure, stackFrames + 1);
                return reslambda;
            }
        }

        Dictionary<string, Func<IList<object>, HypoLambda, Dictionary<string, object>, int, object>>
            _bultins = new Dictionary<string, Func<IList<object>, HypoLambda, Dictionary<string, object>, int, object>>() {
            { "print", (binds, exp, _, __) => {
                var s = String.Join(" ", binds.Select(x => x.ToString()));
                Console.WriteLine(s);
                return s; }},
            { "first", (binds, exp, _, __) => {
                var l = binds[0] as IList<object>;
                return l[0]; } },
            { "rest", (binds,exp,_,__) => {
                var l = binds[0] as IList<object>;
                return l.Skip(1).ToList(); } },
            { "__add1__", (binds,exp,_,__) => {
                return ((double)binds[0]) + 1.0; } },
            { "map", (binds,exp,_,__) => {
                var l = getfunc(binds[0]);
                var mapped = binds
                    .Skip(1)
                    .Cast<List<object>>()
                    .Transpose()
                    .Select(p => exp.lambdaeval(l, _, p, __))
                    .ToList();
                return mapped; } },
        };

        static lambdatuple getfunc(object lambda)
        {
            if (lambda is string) {
                var op = (string)lambda;
                if (!binary_operators.ContainsKey(op))
                    throw new ApplicationException("Unknown operator");
                var l = new lambdatuple();
                l.Body = new List<Terminal>();
                l.Body.Add(new Terminal(TokenType.ident, "__1__", 0, 0));
                l.Body.Add(new Terminal(TokenType.ident, "__2__", 0, 0));
                l.Body.Add(new Terminal(binary_operators[op], 0, 0));
                l.Head = new string[] { "__1__", "__2__" };
                return l;
            } else {
                return lambda as lambdatuple;
            }
        }

        class lambdatuple
        {
            internal string Builtin;
            internal string[] Head;
            internal IList<Terminal> Body;
            internal Dictionary<string, object> Locals = new Dictionary<string, object>();
        }

        private bool truthvalue(object value)
        {
            bool cond = false;
            if (value != null) {
                if (value.GetType().IsValueType)
                    cond = Convert.ToBoolean(value);
                else if (value is string && ((string)value).Length > 0)
                    cond = true;
            }
            return cond;
        }

        #endregion

        #region namespace managing.

        private object getValue(string name, Dictionary<string, object> locals)
        {
            if (locals.ContainsKey(name)) {
                return locals[name];
            }

            if (_bultins.ContainsKey(name)) {
                var bi = new lambdatuple();
                bi.Builtin = name;
                return bi;
            }

            if (this._evalOnlyOneSymbol && !name.StartsWith("this.")) {
                name = "this." + name;
            }
            string sym = getRootSymbol(name);
            object o = this.symbolTable[sym];

            string trail = getTrailSymbols(name);
            if (trail == string.Empty)
                return (o);
            // return Expression.getValueFromField(o, trail);
            return HypoLambda.travel(o, trail.Split(new char[] { '.' }));
        }

        private static object travel(object obj, string[] namelist)
        {
            if (namelist.Length == 0)
                return obj;
            var name = namelist[0];
            var val = HypoLambda.getValueFromField(obj, name);
            var namelistcdr = namelist.Skip(1).ToArray();
            return travel(val, namelistcdr);
        }

        private static object getValueFromField(object obj, string propName)
        {
            Type type = obj.GetType();
            // primero busco como propiedad.
            PropertyInfo pi = type.GetProperty(propName);
            if (pi == null) {
                // si no está, la busco como campo.
                FieldInfo fi = type.GetField(propName);
                return fi.GetValue(obj);
            }
            return pi.GetValue(obj, null);
        }

        private string getRootSymbol(string name)
        {
            if (this._evalOnlyOneSymbol)
                return "this";
            string[] parts = name.Split(new char[] { '.' });
            return parts[0];
        }

        private string getTrailSymbols(string name)
        {
            string[] parts = name.Split(new char[] { '.' });
            if (parts.Length > 1)
                return string.Join(".", parts, 1, parts.Length - 1);
            else
                return string.Empty;
        }
        #endregion

        #region analisis sintactico.

        internal void Compile(string expStr, bool evalOnlyOneSymbol)
        {
            this._evalOnlyOneSymbol = evalOnlyOneSymbol;
            var lexerStream = Lexer(expStr);
            nexttoken(lexerStream.GetEnumerator());
            ast = expression();
            var r = new RecorreArbol();
            pcode = r.PostOrden(ast, null).ToList();
        }

        private StringBuilder tree2string(Node n, StringBuilder prefix, bool isTail, StringBuilder sb)
        {
            if (n.Right != null) {
                tree2string(n.Right, new StringBuilder().Append(prefix).Append(isTail ? "│   " : "    "), false, sb);
            }
            sb.Append(prefix).Append(isTail ? "└── " : "┌── ").Append(n.Tag.ToString()).AppendLine("");
            if (n.Left != null) {
                tree2string(n.Left, new StringBuilder().Append(prefix).Append(isTail ? "    " : "│   "), true, sb);
            }
            return sb;
        }

        private String tree2string(Node n)
        {
            return tree2string(n, new StringBuilder(), true, new StringBuilder()).ToString();
        }

        internal String prettyAST()
        {
            if (ast == null)
                throw new ApplicationException("No current AST. Must compile before.");
            return tree2string(ast);
        }

        internal String prettyPCODE()
        {
            if (pcode == null)
                throw new ApplicationException("No current PCODE. Must compile before.");
            var sb = new StringBuilder();
            foreach (var t in pcode) {
                sb.AppendLine(t.ToString());
            }
            return sb.ToString();
        }

        bool _evalOnlyOneSymbol = false;

        Node ast = null;

        IList<Terminal> pcode = null;
        Dictionary<string, Terminal> reservedwords = new Dictionary<string, Terminal>();


        #endregion

    }

    #region auxiliary classes

    enum TokenType
    {
        ident,
        number,
        str,
        lparen,
        rparen,
        lcurly,
        rcurly,
        times,
        slash,
        perc,
        plus,
        minus,
        not,
        lt,
        gt,
        neq,
        eq,
        lteq,
        gteq,
        and,
        or,
        iff,
        els,
        jmpzero,
        jmpnotz,
        jmp,
        label,
        lambda,
        lambdahead,
        lambdabody,
        colon,
        list,
        eval,
        identlocal,
        comma,
        semicolon,
        lbrac,
        rbrac,
        assig,
        NIL
    }

    class RecorreArbol
    {

        public IEnumerable<Terminal> PostOrden(Node root, Action<object> visiting)
        {
            if (root != null) {
                foreach (var l in PostOrden(root.Left, visiting))
                    yield return l;
                foreach (var r in PostOrden(root.Right, visiting))
                    yield return r;

                if (visiting != null)
                    visiting(root);

                yield return root.Tag;
            }
        }
    }

    class Terminal
    {

        public Terminal(TokenType tokenType, int linenumber, int position)
        {
            this.TokenType = tokenType;
            this.Value = null;
            this.LN = linenumber;
            this.CP = position;
        }

        public Terminal(TokenType tokenType, object value, int linenumber, int position)
            : this(tokenType, linenumber, position)
        {

            this.Value = value;
        }

        public TokenType TokenType
        {
            get;
            private set;
        }

        public object Value
        {
            get;
            private set;
        }

        internal int LN
        {
            get;
            private set;
        }

        internal int CP
        {
            get;
            private set;
        }

        public override string ToString()
        {
            return this.TokenType.ToString() + (this.Value != null ? " [" + this.Value.ToString() + "]" : "");
        }
    }

    static class myext
    {
        public static IEnumerable<IList<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> source)
        {
            var enumerators = source.Select(e => e.GetEnumerator()).ToArray();
            try {
                while (enumerators.All(e => e.MoveNext())) {
                    yield return enumerators.Select(e => e.Current).ToArray();
                }
            } finally {
                foreach (var enumerator in enumerators)
                    enumerator.Dispose();
            }
        }
    }

    #endregion
}
