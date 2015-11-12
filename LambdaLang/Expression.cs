
using System;
using System.Collections.Generic;
using System.Text;
using System.Reflection; 
using System.Text.RegularExpressions;
using System.Linq;
// using ECC.Lib.Properties;

#if DEBUG
using System.Runtime.CompilerServices;
[assembly: InternalsVisibleTo("TestProject")]
#endif


namespace LambdaLang {

    /// <summary>
    /// Permite evaluar expresiones.
    /// </summary>
    /// <remarks>
    /// <include file='Expression.doc' path='ExpressionSintax' />
    /// </remarks>
    [Serializable]
    public class Expression {

        #region Fields

        private Dictionary<string, object> symbolTable;

        private static string REGEX_VARS = @"[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}_][\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}_\p{Nd}\.]*";
        private static string REGEX_NUMS = @"[\p{Nd}][\.\p{Nd}]*";
        private static string REGEX_OPER = @"[\*\+-/]";

        private static Regex reVars = new Regex(REGEX_VARS);
        private static Regex reNums = new Regex(REGEX_NUMS);
        private static Regex reOper = new Regex(REGEX_OPER);

        #endregion

        #region Constructors

        /// <summary>
        /// Constructor generico para usar el parser recursivo descendente.
        /// </summary>
        /// <param name="expStr">Expresion a compilar.</param>
        public Expression(string expStr)
            : this() {
            this.SetExpression(expStr, false);
        }

        /// <summary>
        /// Constructor generico para usar el parser recursivo descendente.
        /// </summary>
        public Expression() {
            this.symbolTable = new Dictionary<string, object>();

            this.reservedwords.Add("not",  new Terminal(TokenType.not, 0, 0));
            this.reservedwords.Add("and", new Terminal(TokenType.and, 0, 0));
            this.reservedwords.Add("or", new Terminal(TokenType.or, 0, 0));
            this.reservedwords.Add("if", new Terminal(TokenType.iff, 0, 0));
            this.reservedwords.Add("else", new Terminal(TokenType.els, 0, 0));
            this.reservedwords.Add("lambda", new Terminal(TokenType.lambda, 0, 0));
        }
        #endregion

        #region Properties, Accesors and Modifiers

        /// <summary>
        /// Tabla de símbolos.
        /// </summary>
        public Dictionary<string,object> SymbolTable {
            get { return this.symbolTable; }
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Obtains the last error message.
        /// </summary>
        public string LastError { get; private set; }

        /// <summary>
        /// Calcula el resultado de la expresión numérica.
        /// </summary>
        /// <returns>El resultado (<see cref="System.Double"/>).</returns>
        public double Calculate() {
            var res = this.Solve();
            // antes habia un cast pero en determinados casos
            // es necesario una conversion explicita.
            return Convert.ToDouble(res);
        }

        /// <summary>
        /// Resuelve la expresión.
        /// </summary>
        /// <returns>El resultado, que puede ser un <see cref="System.Double"/> o
        /// un <see cref="System.String"/></returns>
        public object Solve() {
            LastError = "";
            IList<Terminal> postorden = null;
            if (ast != null) {
                RecorreArbol r = new RecorreArbol();
                postorden = r.PostOrden(ast, null).ToList();
                return CalculateNPI2(postorden, null, 0);
            } else {
                throw new ApplicationException("Can't execute a non valid program.");
            }
        }

        /// <summary>
        /// Devuelve la lista de identificadores de la expresión.
        /// </summary>
        public IEnumerable<string> GetIdentifiers() {
            foreach (var t in terminales)
                if (t.TokenType == TokenType.ident)
                    yield return t.Value.ToString();
        }
        #endregion

        #region HMF recursivo descendente

        #region evaluacion.

        private object CalculateNPI2(IList<Terminal> proveedor, List<Dictionary<string, object>> locals, int stackFrames) {
            // los Convert.ToDouble son necesarios porque es posible que alguna
            // de las propiedades de objetos utilizados devuelvan int long o float.
            // asi nos aseguramos que los calculos se hagan siempre con double.

            var pila = new Stack<object>();
            if (locals == null) {
                locals = new List<Dictionary<string, object>>();
                locals.Add(new Dictionary<string, object>());
            }
            var currentscope = locals[locals.Count-1];

            double a, b, res;
            string sa, sb, str;
            string ignoreuptolabel = "";

            foreach (var t in proveedor) {

                if (stackFrames > 500) {
                    LastError = string.Format(Properties.Strings.Expression_MaxRecursionDepth, t.LN, t.CP);
                    throw new StackOverflowException(LastError);
                }

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
                        pila.Push(getValue((string)t.Value, locals));
                        break;
                    case TokenType.str:
                        str = (string)t.Value;
                        pila.Push(str);
                        break;

                    #endregion

                    #region operadores matematicos

                    case TokenType.plus:
                        if (pila.Peek() is string) {
                            sb = (string)pila.Pop();
                            sa = (string)pila.Pop();
                            str = sa + sb;
                            pila.Push(str);
                        } else {
                            b = Convert.ToDouble(pila.Pop());
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
                            LastError = string.Format(Properties.Strings.Expression_ZeroDivisionError, t.LN, t.CP);
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
                        if (pila.Peek() is string) {
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
                        if (pila.Peek() is string) {
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
                        object cond = pila.Pop();
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
                            var reslambda = lambdaeval(lambda, locals, binds, stackFrames);
                            pila.Push(reslambda);
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
                        pila.Push(t.Value);
                        break;
                    case TokenType.lambda:
                        var ls = new lambdatuple();
                        ls.Body = pila.Pop() as Nodo;
                        ls.Head = pila.Pop() as string[];
                        pila.Push(ls);
                        break;

                    case TokenType.list:
                        pila.Push(new List<object>());
                        break;

                    case TokenType.comma:
                        var tail = pila.Pop() as List<object>;
                        var head = pila.Pop();
                        var li = new List<object>();
                        li.Add(head); li.AddRange(tail);
                        pila.Push(li);
                        break;

                    case TokenType.semicolon:
                        var oc = pila.Pop(); pila.Pop();
                        pila.Push(oc);
                        break;

                    case TokenType.assig:
                        pila.Push(getValue(t.Value.ToString(), locals));
                        break;

                    #endregion
                }
            }
            if (pila.Count > 0)
                return pila.Pop();
            else
                return 0.0;
        }

        private object lambdaeval(lambdatuple lambda, List<Dictionary<string, object>> locals, IList<object> binds, int stackFrames) {
            if (lambda.Builtin != null) {
                return _bultins[lambda.Builtin](binds, this, locals, stackFrames);
            } else {
                var newscope = new Dictionary<string, object>();
                for (int j = 0; j < lambda.Head.Length; j++) {
                    if (j < binds.Count) {
                        newscope.Add(lambda.Head[j], binds[j]);
                    }
                }
                locals.Add(newscope);
                RecorreArbol r = new RecorreArbol();
                var postorden = r.PostOrden(lambda.Body, null).ToList();
                var reslambda = CalculateNPI2(postorden, locals, stackFrames + 1);
                locals.RemoveAt(locals.Count - 1);
                return reslambda;
            }
        }

        Dictionary<string, Func<IList<object>, Expression, List<Dictionary<string, object>>, int, object>>
            _bultins = new Dictionary<string, Func<IList<object>, Expression, List<Dictionary<string, object>>, int, object>>() {

            { "print", (binds,d,_,__) => {
                var s = String.Join(" ", binds.Select(x => x.ToString()));
                Console.WriteLine(s);
                return s; } },
            { "first", (binds,d,_,__) => {
                var l = binds[0] as IList<object>;
                return l[0]; } },
            { "rest", (binds,d,_,__) => {
                var l = binds[0] as IList<object>;
                return l.Skip(1).ToList(); } },
            { "__add1__", (binds,d,_,__) => {
                return ((double)binds[0]) + 1.0; } },
            { "map", (binds,exp,_,__) => {
                var l = binds[0] as lambdatuple;
                var mapped = binds
                    .Skip(1)
                    .Cast<List<object>>()
                    .Transpose()
                    .Select(p => exp.lambdaeval(l, _, p, __))
                    .ToList();
                return mapped; } },

        };

        class lambdatuple {
            internal string Builtin;
            internal string[] Head;
            internal Nodo Body;
        }

        private bool truthvalue(object value) {
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

        #region aux. p/ manejo de espacio de nombres.

        private object getValue(string name, List<Dictionary<string, object>> locals) {
            for (var i = locals.Count; i --> 0 ;) {
                if (locals[i].ContainsKey(name)) {
                    return locals[i][name];
                }
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
            return Expression.travel(o, trail.Split(new char[] { '.' }));
        }

        private static object travel(object obj, string[] namelist) {
            if (namelist.Length == 0)
                return obj;
            var name = namelist[0];
            var val = Expression.getValueFromField(obj, name);
            var namelistcdr = namelist.Skip(1).ToArray();
            return travel(val, namelistcdr);
        }

        private static object getValueFromField(object obj, string propName) {
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

        private string getRootSymbol(string name) {
            if (this._evalOnlyOneSymbol)
                return "this";
            string[] parts = name.Split(new char[] { '.' });
            return parts[0];
        }

        private string getTrailSymbols(string name) {
            string[] parts = name.Split(new char[] { '.' });
            if (parts.Length > 1)
                return string.Join(".", parts, 1, parts.Length - 1);
            else
                return string.Empty;
        }
        #endregion

        #region analisis sintactico.

        /// <summary>
        /// Inicializa la expresión.
        /// </summary>
        /// <remarks>
        /// <para>Hace un análsis sintáctico recursivo descendente
        /// construyendo un árbol sintáctico (AST) y la
        /// tabla de símbolos <see cref="Expression.SymbolTable"/></para>
        /// <include file='Expression.doc' path='ExpressionSintax' />
        /// </remarks>
        /// <param name="expStr">Expresion.</param>
        public void SetExpression(string expStr) {
            this.SetExpression(expStr, false);
        }

        /// <summary>
        /// Inicializa la expresión.
        /// </summary>
        /// <remarks>
        /// <para>Hace un análsis sintáctico recursivo descendente
        /// construyendo un árbol sintáctico (AST) y la
        /// tabla de símbolos <see cref="Expression.SymbolTable"/>.
        /// Si el parámetro <c>evalOnlyOneSymbol</c> va en <c>true</c> entonces la tabla de símbolos
        /// contendrá un solo objeto con clave "this".</para>
        /// <include file='Expression.doc' path='ExpressionSintax' />
        /// </remarks>
        /// <param name="expStr">Expresion.</param>
        /// <param name="evalOnlyOneSymbol">Si la expresión es de sólo un símbolo y cuyos nombres
        /// representan propiedades y no objetos, va en <c>true</c>. Caso contrario, <c>false</c>.
        /// </param>
        internal void SetExpression(string expStr, bool evalOnlyOneSymbol) {
            this._evalOnlyOneSymbol = evalOnlyOneSymbol;
            Lexer(expStr);
            reader = 0;
            nexttoken();
            ast = expresion();
        }

        private StringBuilder toString(Nodo n, StringBuilder prefix, bool isTail, StringBuilder sb) {
            if (n.Right != null) {
                toString(n.Right, new StringBuilder().Append(prefix).Append(isTail ? "│   " : "    "), false, sb);
            }
            sb.Append(prefix).Append(isTail ? "└── " : "┌── ").Append(n.Tag.ToString()).Append("\n");
            if (n.Left != null) {
                toString(n.Left, new StringBuilder().Append(prefix).Append(isTail ? "    " : "│   "), true, sb);
            }
            return sb;
        }

        private String toString(Nodo n) {
            return toString(ast, new StringBuilder(), true, new StringBuilder()).ToString();
        }

        internal String prettyPrintAST() {
            return toString(ast);
        }

        bool _evalOnlyOneSymbol = false;

        Nodo ast = null;

        Dictionary<string, Terminal> reservedwords = new Dictionary<string, Terminal>();

        int reader;
        List<Terminal> terminales;

        #region lexer

        void Lexer(string input) {

            int lnum = 1;
            int cpos = 1;

            terminales = new List<Terminal>();

            Match m;
            string token;
            while (true) {
                // input = input.TrimStart();
                {
                    int p = 0;
                    while (p < input.Length && char.IsWhiteSpace(input[p])) {
                        cpos += 1;
                        if (input[p] == '\n') {
                            lnum += 1;
                            cpos = 1;
                        }
                        p++;
                    };
                    input = input.Substring(p);
                }
                if (input.Length == 0)
                    break;
                if (input[0] == '\"') {
                    token = "";
                    int p = 1; // saltea el primer "
                    while (p < input.Length && input[p] != '\"') {
                        if (input[p] == '\\' && input[p + 1] == '\"')
                            p++;
                        if (input[p] == '\n') {
                            lnum += 1;
                            cpos = 1;
                        }
                        token += input[p];
                        p++;
                        cpos += 1;
                    }
                    terminales.Add(new Terminal(TokenType.str, token, lnum, cpos));
                    input = input.Substring(p + 1); // el +1 saltea el ultimo "
                    continue;
                }
                m = reVars.Match(input);
                if (m.Success && m.Index == 0) {
                    token = m.Groups[0].Value;
                    if (reservedwords.ContainsKey(token)) {
                        // palabras reservadas
                        terminales.Add(reservedwords[token]);
                    } else {
                        // variables.
                        string root = getRootSymbol(token);
                        if (!this.symbolTable.ContainsKey(root))
                            this.symbolTable.Add(root, null);
                        terminales.Add(new Terminal(TokenType.ident, token, lnum, cpos));
                    }
                    input = input.Substring(token.Length); cpos += token.Length;
                    continue;
                }
                m = reNums.Match(input);
                if (m.Success && m.Index == 0) {
                    token = m.Groups[0].Value;
                    double cte = double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                    terminales.Add(new Terminal(TokenType.number, cte, lnum, cpos));
                    input = input.Substring(token.Length); cpos += token.Length;
                    continue;
                }
                switch (input[0]) {
                    case ':':
                        terminales.Add(new Terminal(TokenType.colon, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '+':
                        terminales.Add(new Terminal(TokenType.plus, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '-':
                        terminales.Add(new Terminal(TokenType.minus, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '*':
                        terminales.Add(new Terminal(TokenType.times, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '/':
                        terminales.Add(new Terminal(TokenType.slash, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '%':
                        terminales.Add(new Terminal(TokenType.perc, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '(':
                        terminales.Add(new Terminal(TokenType.lparen, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case ')':
                        terminales.Add(new Terminal(TokenType.rparen, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '{':
                        terminales.Add(new Terminal(TokenType.lcurly, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '}':
                        terminales.Add(new Terminal(TokenType.rcurly, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case ',':
                        terminales.Add(new Terminal(TokenType.comma, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case ';':
                        terminales.Add(new Terminal(TokenType.semicolon, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '[':
                        terminales.Add(new Terminal(TokenType.lbrac, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case ']':
                        terminales.Add(new Terminal(TokenType.rbrac, lnum, cpos));
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '!':
                        switch (input[1]) {
                            case '=':
                                terminales.Add(new Terminal(TokenType.neq, lnum, cpos));
                                input = input.Substring(2); cpos += 2;
                                continue;
                            default:
                                terminales.Add(new Terminal(TokenType.not, lnum, cpos));
                                input = input.Substring(1); cpos += 1;
                                continue;
                        }
                    case '>':
                        switch (input[1]) {
                            case '=':
                                terminales.Add(new Terminal(TokenType.gteq, lnum, cpos));
                                input = input.Substring(2); cpos += 2;
                                continue;
                            default:
                                terminales.Add(new Terminal(TokenType.gt, lnum, cpos));
                                input = input.Substring(1); cpos += 1;
                                continue;
                        }
                    case '<':
                        switch (input[1]) {
                            case '=':
                                terminales.Add(new Terminal(TokenType.lteq, lnum, cpos));
                                input = input.Substring(2); cpos += 2;
                                continue;
                            default:
                                terminales.Add(new Terminal(TokenType.lt, lnum, cpos));
                                input = input.Substring(1); cpos += 1;
                                continue;
                        }
                    case '=':
                        switch (input[1]) {
                            case '=':
                                terminales.Add(new Terminal(TokenType.eq, lnum, cpos));
                                input = input.Substring(2); cpos += 2;
                                continue;
                            default:
                                terminales.Add(new Terminal(TokenType.assig, lnum, cpos));
                                input = input.Substring(1); cpos += 1;
                                continue;
                        }
                }
                // si hay algo que no reconoce, sale.
                break;
            }
        }

        #endregion

        #region parser pp/ dicho.

        void nexttoken() {
            if (reader >= this.terminales.Count)
                currenttoken = new Terminal(TokenType.NIL, currenttoken.LN, currenttoken.CP);
            else {
                currenttoken = terminales[reader++];
            }
            // para debug
            // Debug.WriteLine(currenttoken.TokenType.ToString());
        }

        Terminal lookaheadone() {
            if (reader >= this.terminales.Count)
                return new Terminal(TokenType.NIL, currenttoken.LN, currenttoken.CP);
            else {
                return terminales[reader];
            }
        }

        void error(string msg) {
            throw new ApplicationException(msg);
        }

        Terminal currenttoken;

        void expect(TokenType s) {
            if (currenttoken.TokenType != s) {
                string msg = string.Format(
                    Properties.Strings.Expression_Unexpected_symbol_Waits_for_comes,
                    s, currenttoken.TokenType, currenttoken.LN, currenttoken.CP);
                error(msg);
            }
        }

        Nodo factor() {
            Nodo f = null;
            switch (currenttoken.TokenType) {
                case TokenType.ident:
                    f = new Nodo(currenttoken);
                    break;
                case TokenType.number:
                    f = new Nodo(currenttoken);
                    break;
                case TokenType.str:
                    f = new Nodo(currenttoken);
                    break;
                case TokenType.lbrac:
                    nexttoken();
                    f = build_list();
                    expect(TokenType.rbrac);
                    break;
                case TokenType.lparen:
                    nexttoken();
                    f = expresion();
                    expect(TokenType.rparen);
                    break;
                case TokenType.lcurly:
                    nexttoken();
                    expect(TokenType.ident);
                    f = new Nodo(currenttoken);
                    nexttoken();
                    expect(TokenType.rcurly);
                    break;
                default:
                    var msg = string.Format(Properties.Strings.Expression_Syntax_error,
                        currenttoken.LN, currenttoken.CP);
                    error(msg);
                    break;
            }
            nexttoken();
            return f;
        }

        Nodo build_list() {
            var f = expresion_lambda();
            if (currenttoken.TokenType == TokenType.comma) {
                nexttoken();
                return new Nodo(new Terminal(TokenType.comma, currenttoken.LN, currenttoken.CP), f, build_list());
            } else {
                var empty = new Terminal(TokenType.list, currenttoken.LN, currenttoken.CP);
                var oplist = new Terminal(TokenType.comma, currenttoken.LN, currenttoken.CP);
                return new Nodo(oplist, f, new Nodo(empty));
            }
        }

        Nodo expresion_evaluada() {
            Nodo nizq = factor();
            if (currenttoken.TokenType == TokenType.lparen) {
                var tl = new Terminal(TokenType.list, currenttoken.LN, currenttoken.CP);
                var bindings = new Nodo(tl);
                nexttoken();
                if (currenttoken.TokenType != TokenType.rparen) {
                    bindings = build_list();
                }
                expect(TokenType.rparen);
                nexttoken();
                var op = new Terminal(TokenType.eval, currenttoken.LN, currenttoken.CP);
                Nodo n = new Nodo(op, bindings, nizq);
                return n;
            } else
                return nizq;
        }

        Nodo factor_negado() {
            Nodo nizq = null;
            if (currenttoken.TokenType == TokenType.not) {
                Terminal op = currenttoken;
                nexttoken();
                nizq = expresion_evaluada();
                Nodo n = new Nodo(op, nizq, null);
                return n;
            } else
                return expresion_evaluada();
        }

        Nodo termino() {
            Nodo nizq = factor_negado();
            if (currenttoken.TokenType == TokenType.times || currenttoken.TokenType == TokenType.slash ||
                currenttoken.TokenType == TokenType.perc) {
                Terminal op = currenttoken;
                nexttoken();
                Nodo nder = termino();
                Nodo n = new Nodo(op, nizq, nder);
                return n;
            } else
                return nizq;
        }

        Nodo expresion_suma() {
            Nodo nizq = termino();
            if (currenttoken.TokenType == TokenType.plus || currenttoken.TokenType == TokenType.minus) {
                Terminal op = currenttoken;
                nexttoken();
                Nodo nder = expresion_suma();
                Nodo n = new Nodo(op, nizq, nder);
                return n;
            } else
                return nizq;
        }

        Nodo expresion_logica() {
            Nodo nizq = expresion_suma();
            if (currenttoken.TokenType == TokenType.eq   || currenttoken.TokenType == TokenType.gt ||
                currenttoken.TokenType == TokenType.gteq || currenttoken.TokenType == TokenType.lt ||
                currenttoken.TokenType == TokenType.lteq || currenttoken.TokenType == TokenType.neq) {
                Terminal op = currenttoken;
                nexttoken();
                Nodo nder = expresion_logica();
                Nodo n = new Nodo(op, nizq, nder);
                return n;
            } else
                return nizq;
        }

        Nodo expresion_andor() {
            Nodo nizq = expresion_logica();
            if (currenttoken.TokenType == TokenType.and || currenttoken.TokenType == TokenType.or) {
                Terminal op = currenttoken;
                nexttoken();
                Nodo nder = expresion_andor();
                Nodo n = and_or_ast(op, nizq, nder);
                return n;
            } else
                return nizq;
        }

        Nodo expresion_cond() {
            Nodo whentrue = expresion_andor();
            if (currenttoken.TokenType == TokenType.iff) {
                Terminal op = currenttoken;
                nexttoken();
                Nodo condition = expresion_andor();
                expect(TokenType.els);
                nexttoken();
                Nodo whenfalse = expresion_cond();

                var n = new Nodo(new Terminal(TokenType.iff, currenttoken.LN, currenttoken.CP),
                    and_or_ast(new Terminal(TokenType.or, currenttoken.LN, currenttoken.CP),
                        and_or_ast(new Terminal(TokenType.and, currenttoken.LN, currenttoken.CP),
                            condition,
                            whentrue),
                        whenfalse),
                    null);
                return n;
            } else
                return whentrue;
        }

        List<string> name_list() {
            var names = new List<string>();
            if (currenttoken.TokenType == TokenType.ident) {
                names.Add(currenttoken.Value.ToString());
                nexttoken();
                if (currenttoken.TokenType == TokenType.comma) {
                    nexttoken();
                    names.AddRange(name_list());
                }
            }
            return names;
        }

        List<string> name_list_parens() {
            var rparen = false;
            if (currenttoken.TokenType == TokenType.lparen) {
                rparen = true;
                nexttoken();
            }
            var nl = name_list();
            if (rparen) {
                expect(TokenType.rparen);
                nexttoken();
            }
            return nl;
        }

        Nodo expresion_lambda() {
            if (currenttoken.TokenType == TokenType.lambda) {
                var op = currenttoken;
                nexttoken();
                var nl = name_list_parens();
                expect(TokenType.colon);
                var head = new Terminal(TokenType.lambdahead, nl.ToArray(), currenttoken.LN, currenttoken.CP);
                nexttoken();
                var body = new Terminal(TokenType.lambdabody, expresion_lambda(), currenttoken.LN, currenttoken.CP);
                return new Nodo(op, new Nodo(head), new Nodo(body));
            } else {
                return expresion_cond();
            }
        }

        Nodo expresion_list() {
            var f = expresion_single();
            if (currenttoken.TokenType == TokenType.semicolon) {
                nexttoken();
                return new Nodo(new Terminal(TokenType.semicolon, currenttoken.LN, currenttoken.CP), f, expresion_list());
            } else if (currenttoken.TokenType == TokenType.comma) {
                var msg = string.Format(Properties.Strings.Expression_Syntax_error,
                    currenttoken.LN, currenttoken.CP);
                error(msg);
                return null;
            } else
                return f;
        }

        Nodo asignacion() {
            var s = currenttoken.Value.ToString();
            nexttoken();
            expect(TokenType.assig);
            var op = new Terminal(TokenType.assig, s, currenttoken.LN, currenttoken.CP);
            nexttoken();
            var r = new Nodo(new Terminal(TokenType.identlocal, s, currenttoken.LN, currenttoken.CP));
            var l = expresion_single();
            return new Nodo(op, l, r);
        }

        Nodo expresion_single() {
            if (currenttoken.TokenType == TokenType.ident && lookaheadone().TokenType == TokenType.assig) {
                return asignacion();
            } else {
                return expresion_lambda();
            }
        }

        Nodo expresion() {
            return expresion_list();
        }

        private Terminal nilterminal() {
            return new Terminal(TokenType.NIL, null, currenttoken.LN, currenttoken.CP);
        }

        private Nodo and_or_ast(Terminal op, Nodo nizq, Nodo nder) {
            Nodo n = null;
            switch (op.TokenType) {
                case TokenType.and:
                    var lbljmpzero = Guid.NewGuid().ToString();
                    n = new Nodo(op,
                        nizq,
                        new Nodo(nilterminal(),
                            new Nodo(new Terminal(TokenType.jmpzero, lbljmpzero, currenttoken.LN, currenttoken.CP)),
                            new Nodo(nilterminal(),
                                nder,
                                new Nodo(new Terminal(TokenType.label, lbljmpzero, currenttoken.LN, currenttoken.CP)))));
                    break;
                case TokenType.or:
                    var lbljmpnotz = Guid.NewGuid().ToString();
                    n = new Nodo(op,
                        nizq,
                        new Nodo(nilterminal(),
                            new Nodo(new Terminal(TokenType.jmpnotz, lbljmpnotz, currenttoken.LN, currenttoken.CP)),
                            new Nodo(nilterminal(),
                                nder,
                                new Nodo(new Terminal(TokenType.label, lbljmpnotz, currenttoken.LN, currenttoken.CP)))));
                    break;
            }
            return n;
        }

        #endregion

        #endregion

        #endregion

        /// <summary> Function to set a table from a string </summary>
        /// <param name="tableStr"> input string </param>
        public static Dictionary<double, double> GetTableFromString(string tableStr) {
            Dictionary<double, double> tableDict = new Dictionary<double, double>();
            char[] lineSep = new char[1];
            lineSep[0] = ';';
            char[] fieldSep = new char[1];
            fieldSep[0] = ':';

            double key;
            double val;
            string[] lines = tableStr.Split(lineSep);
            try {
                foreach (string line in lines) {
                    string[] fields = line.Split(fieldSep);
                    var f0 = fields[0].Trim().ToLower();
                    if (f0 == "max") { key = double.MaxValue; } else { key = double.Parse(fields[0], System.Globalization.CultureInfo.InvariantCulture); }
                    val = double.Parse(fields[1], System.Globalization.CultureInfo.InvariantCulture);
                    tableDict.Add(key, val);
                }
                return tableDict;
            } catch (Exception e) {
                throw new ApplicationException(Properties.Strings.Cant_parse_table, e);
            }
        }
    }

    #region HMF classes

    enum TokenType {
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

    [Serializable]
    class Nodo {
        private Terminal o;
        private Nodo izq;
        private Nodo der;

        public Nodo(Terminal o) {
            this.o = o;
        }

        public Nodo(Terminal o, Nodo izq, Nodo der) {
            this.o = o;
            this.izq = izq;
            this.der = der;
        }

        public Terminal Tag {
            get { return this.o; }
        }

        public Nodo Left {
            get { return this.izq; }
        }

        public Nodo Right {
            get { return this.der; }
        }
    }

    class RecorreArbol {

        public IEnumerable<Terminal> PostOrden(Nodo root, Action<object> visiting) {
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

    [Serializable]
    class Terminal {

        public Terminal(TokenType tokenType, int linenumber, int position) {
            this.TokenType = tokenType;
            this.Value = null;
            this.LN = linenumber;
            this.CP = position;
        }

        public Terminal(TokenType tokenType, object value, int linenumber, int position)
            : this(tokenType, linenumber, position) {

            this.Value = value;
        }

        public TokenType TokenType {
            get;
            private set;
        }

        public object Value {
            get;
            private set;
        }

        internal int LN {
            get;
            private set;
        }

        internal int CP {
            get;
            private set;
        }

        public override string ToString() {
            return this.TokenType.ToString() + (this.Value != null ? " [" + this.Value.ToString() + "]" : "");
        }
    }

    static class myext {
        public static IEnumerable<IList<T>> Transpose<T>(this IEnumerable<IEnumerable<T>> source) {
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
