
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
        private List<Terminal> terminalList = new List<Terminal>();

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

            this.reservedwords.Add("not",  new Terminal(TokenType.not));
            this.reservedwords.Add("and",  new Terminal(TokenType.and));
            this.reservedwords.Add("or",   new Terminal(TokenType.or));
            this.reservedwords.Add("if",   new Terminal(TokenType.iff));
            this.reservedwords.Add("else", new Terminal(TokenType.els));
            this.reservedwords.Add("lambda", new Terminal(TokenType.lambda));
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
            IEnumerable<object> postorden = null;
            if (ast != null) {
                RecorreArbol r = new RecorreArbol();
                postorden = r.PostOrden(ast, null);
#if DEBUG
                postorden = postorden.ToList();
#endif
            } else {
                postorden = (IEnumerable<object>) this.terminalList;
            }
            return CalculateNPI2(postorden, null);
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

        private object CalculateNPI2(IEnumerable<object> proveedor, Dictionary<string, object> locals) {
            // los Convert.ToDouble son necesarios porque es posible que alguna
            // de las propiedades de objetos utilizados devuelvan int long o float.
            // asi nos aseguramos que los calculos se hagan siempre con double.

            var pila = new Stack<object>();
            if (locals == null)
                locals = new Dictionary<string, object>();
            else
                locals = new Dictionary<string, object>(locals);

            double a, b, res;
            string sa, sb, str;
            string ignoreuptolabel = "";

            foreach (Terminal t in proveedor) {

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
                        if (b == 0.0)
                            LastError = Properties.Strings.Expression_ZeroDivisionError;
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
                        b = Convert.ToDouble(pila.Pop());
                        a = Convert.ToDouble(pila.Pop());
                        pila.Push((a > b ? 1.0 : 0.0));
                        break;
                    case TokenType.gteq:
                        b = Convert.ToDouble(pila.Pop());
                        a = Convert.ToDouble(pila.Pop());
                        pila.Push((a >= b ? 1.0 : 0.0));
                        break;
                    case TokenType.lt:
                        b = Convert.ToDouble(pila.Pop());
                        a = Convert.ToDouble(pila.Pop());
                        pila.Push((a < b ? 1.0 : 0.0));
                        break;
                    case TokenType.lteq:
                        b = Convert.ToDouble(pila.Pop());
                        a = Convert.ToDouble(pila.Pop());
                        pila.Push((a <= b ? 1.0 : 0.0));
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

                    case TokenType.eval:
                        var lambda = getValue(t.Value.ToString(), locals) as Nodo;
                        if (lambda != null) {
                            var reslambda = CalculateNPI2((new RecorreArbol()).PostOrden(lambda, null), locals);
                            pila.Push(reslambda);
                        }
                        break;
                    case TokenType.identlocal:
                        var vallocal = pila.Pop();
                        locals.Add(t.Value.ToString(), vallocal);
                        break;
                    case TokenType.lambda:
                        pila.Push(t.Value);
                        break;
                    case TokenType.lambdahead:
                        pila.Push(t.Value);
                        break;
                    case TokenType.lambdabody:
                        pila.Push(t.Value);
                        break;

                    case TokenType.comma:
                        var oc = pila.Pop(); pila.Pop();
                        pila.Push(oc);
                        break;

                    case TokenType.assig:
                        pila.Push(getValue(t.Value.ToString(), locals));
                        break;

                }
            }
            if (pila.Count > 0)
                return pila.Pop();
            else
                return 0.0;
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

        private object getValue(string name, Dictionary<string, object> locals) {
            if (locals.Count > 0) {
                if (locals != null && locals.ContainsKey(name)) {
                    return locals[name];
                }
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

        internal String toString() {
            return toString(ast, new StringBuilder(), true, new StringBuilder()).ToString();
        }

        bool _evalOnlyOneSymbol = false;

        Nodo ast = null;

        Dictionary<string, Terminal> reservedwords = new Dictionary<string, Terminal>();

        int reader;
        List<Terminal> terminales;

        #region lexer

        void Lexer(string input) {

            terminales = new List<Terminal>();

            Match m;
            string token;
            while (true) {
                input = input.TrimStart();
                if (input.Length == 0)
                    break;
                if (input[0] == '\"') {
                    token = "";
                    int p = 1; // saltea el primer "
                    while (p < input.Length && input[p] != '\"') {
                        if (input[p] == '\\' && input[p + 1] == '\"')
                            p++;
                        token += input[p];
                        p++;
                    }
                    terminales.Add(new Terminal(TokenType.str, token));
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
                        terminales.Add(new Terminal(TokenType.ident, token));
                    }
                    input = input.Substring(token.Length);
                    continue;
                }
                m = reNums.Match(input);
                if (m.Success && m.Index == 0) {
                    token = m.Groups[0].Value;
                    double cte = double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                    terminales.Add(new Terminal(TokenType.number, cte));
                    input = input.Substring(token.Length);
                    continue;
                }
                switch (input[0]) {
                    case '+':
                        terminales.Add(new Terminal(TokenType.plus));
                        input = input.Substring(1);
                        continue;
                    case '-':
                        terminales.Add(new Terminal(TokenType.minus));
                        input = input.Substring(1);
                        continue;
                    case '*':
                        terminales.Add(new Terminal(TokenType.times));
                        input = input.Substring(1);
                        continue;
                    case '/':
                        terminales.Add(new Terminal(TokenType.slash));
                        input = input.Substring(1);
                        continue;
                    case '%':
                        terminales.Add(new Terminal(TokenType.perc));
                        input = input.Substring(1);
                        continue;
                    case '(':
                        terminales.Add(new Terminal(TokenType.lparen));
                        input = input.Substring(1);
                        continue;
                    case ')':
                        terminales.Add(new Terminal(TokenType.rparen));
                        input = input.Substring(1);
                        continue;
                    case '{':
                        terminales.Add(new Terminal(TokenType.lcurly));
                        input = input.Substring(1);
                        continue;
                    case '}':
                        terminales.Add(new Terminal(TokenType.rcurly));
                        input = input.Substring(1);
                        continue;
                    case ',':
                        terminales.Add(new Terminal(TokenType.comma));
                        input = input.Substring(1);
                        continue;
                    case '!':
                        switch (input[1]) {
                            case '=':
                                terminales.Add(new Terminal(TokenType.neq));
                                input = input.Substring(2);
                                continue;
                            default:
                                terminales.Add(new Terminal(TokenType.not));
                                input = input.Substring(1);
                                continue;
                        }
                    case '>':
                        switch (input[1]) {
                            case '=':
                                terminales.Add(new Terminal(TokenType.gteq));
                                input = input.Substring(2);
                                continue;
                            default:
                                terminales.Add(new Terminal(TokenType.gt));
                                input = input.Substring(1);
                                continue;
                        }
                    case '<':
                        switch (input[1]) {
                            case '=':
                                terminales.Add(new Terminal(TokenType.lteq));
                                input = input.Substring(2);
                                continue;
                            default:
                                terminales.Add(new Terminal(TokenType.lt));
                                input = input.Substring(1);
                                continue;
                        }
                    case '=':
                        switch (input[1]) {
                            case '=':
                                terminales.Add(new Terminal(TokenType.eq));
                                input = input.Substring(2);
                                continue;
                            default:
                                terminales.Add(new Terminal(TokenType.assig));
                                input = input.Substring(1);
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
                currenttoken = new Terminal(TokenType.NIL);
            else {
                currenttoken = terminales[reader++];
            }
            // para debug
            // Debug.WriteLine(currenttoken.TokenType.ToString());
        }

        Terminal lookaheadone() {
            if (reader >= this.terminales.Count)
                return new Terminal(TokenType.NIL);
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
                    Properties.Strings.Unexpected_symbol__Waits_for__0___comes__1_, s, currenttoken.TokenType);
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
                    error(Properties.Strings.factor__syntax_error);
                    break;
            }
            nexttoken();
            return f;
        }

        Nodo factor_negado() {
            Nodo nizq = null;
            if (currenttoken.TokenType == TokenType.not) {
                Terminal op = currenttoken;
                nexttoken();
                nizq = factor();
                Nodo n = new Nodo(op, nizq, null);
                return n;
            } else
                return factor();
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

                var n = new Nodo(new Terminal(TokenType.iff),
                    and_or_ast(new Terminal(TokenType.or),
                        and_or_ast(new Terminal(TokenType.and),
                            condition,
                            whentrue),
                        whenfalse),
                    null);
                return n;
            } else
                return whentrue;
        }

        Nodo expr_lambda_bind() {
            var l = expresion_lambda();
            nexttoken();
            return new Nodo(new Terminal(TokenType.eval), expresion(), l);
        }

        Nodo expresion_lambda() {
            var op = currenttoken;
            nexttoken();
            var body = new Terminal(TokenType.lambda, expresion());
            return new Nodo(body);
        }

        Nodo expresion_list() {
            var f = expresion_single();
            if (currenttoken.TokenType == TokenType.comma) {
                nexttoken();
                return new Nodo(new Terminal(TokenType.comma), f, expresion_list());
            } else
                return f;
        }

        Nodo asignacion(string s) {
            nexttoken();
            expect(TokenType.assig);
            var op = new Terminal(TokenType.assig, s);
            nexttoken();
            var r = new Nodo(new Terminal(TokenType.identlocal, s));
            var l = expresion_single();
            return new Nodo(op, l, r);
        }

        Nodo eval_named_lambda(string s) {
            nexttoken();
            expect(TokenType.lparen);
            var op = new Terminal(TokenType.eval, s);
            nexttoken();
            expect(TokenType.rparen);
            return new Nodo(op);
        }

        Nodo expresion_single() {
            if (currenttoken.TokenType == TokenType.ident) {
                var s = currenttoken.Value.ToString();
                if (lookaheadone().TokenType == TokenType.assig) {
                    return asignacion(s);
                } else if (lookaheadone().TokenType == TokenType.lparen) {
                    return eval_named_lambda(s);
                } else {
                    return expresion_cond();
                }
            } else if (currenttoken.TokenType == TokenType.lambda) {
                return expresion_lambda();
            } else {
                return expresion_cond();
            }
        }

        Nodo expresion() {
            return expresion_list();
        }

        private Terminal nilterminal() {
            return new Terminal(TokenType.NIL, null);
        }

        private Nodo and_or_ast(Terminal op, Nodo nizq, Nodo nder) {
            Nodo n = null;
            switch (op.TokenType) {
                case TokenType.and:
                    var lbljmpzero = Guid.NewGuid().ToString();
                    n = new Nodo(op,
                        nizq,
                        new Nodo(nilterminal(),
                            new Nodo(new Terminal(TokenType.jmpzero, lbljmpzero)),
                            new Nodo(nilterminal(),
                                nder,
                                new Nodo(new Terminal(TokenType.label, lbljmpzero)))));
                    break;
                case TokenType.or:
                    var lbljmpnotz = Guid.NewGuid().ToString();
                    n = new Nodo(op,
                        nizq,
                        new Nodo(nilterminal(),
                            new Nodo(new Terminal(TokenType.jmpnotz, lbljmpnotz)),
                            new Nodo(nilterminal(),
                                nder,
                                new Nodo(new Terminal(TokenType.label, lbljmpnotz)))));
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
        eval,
        identlocal,
        comma,
        assig,
        NIL
    }

    [Serializable]
    class Nodo {
        private object o;
        private Nodo izq;
        private Nodo der;

        public Nodo(object o) {
            this.o = o;
        }

        public Nodo(object o, Nodo izq, Nodo der) {
            this.o = o;
            this.izq = izq;
            this.der = der;
        }

        public object Tag {
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

        public List<object> GetListaPostOrden(Nodo root) {
            // l = new List<object>();
            return PostOrden(root, null).ToList();
            // return l;
        }

        public IEnumerable<object> PostOrden(Nodo root, Action<object> visiting) {
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
        object _value = null;
        TokenType _tokenType;

        public Terminal(TokenType tokenType) {
            this._tokenType = tokenType;
        }

        public Terminal(TokenType tokenType, object value) {
            this._tokenType = tokenType;
            this._value = value;
        }

        public TokenType TokenType {
            get { return this._tokenType; }
        }

        public object Value {
            get { return _value; }
        }

        public override string ToString() {
            return _tokenType.ToString() + (_value != null ? " [" + _value.ToString() + "]" : "");
        }
    }

    #endregion
}
