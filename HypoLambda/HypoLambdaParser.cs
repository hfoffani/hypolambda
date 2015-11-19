using System;
using System.Collections.Generic;

namespace HL
{
    public partial class HypoLambda
    {
        IEnumerator<Terminal> enu;

        void nexttoken(IEnumerator<Terminal> enumerator) {
            enu = enumerator;
            if (enu.MoveNext())
                lookaheadtoken = enu.Current;
            else
                lookaheadtoken = new Terminal(TokenType.NIL, 0, 0);
            nexttoken();
        }

        void nexttoken() {
            currenttoken = lookaheadtoken;
            if (enu.MoveNext())
                lookaheadtoken = enu.Current;
            else
                lookaheadtoken = new Terminal(TokenType.NIL, currenttoken.LN, currenttoken.CP);
        }

        Terminal lookaheadtoken = null;
        Terminal lookaheadone() {
            return lookaheadtoken;
        }

        void error(string msg) {
            throw new ApplicationException(msg);
        }

        Terminal currenttoken;

        void expect(TokenType s) {
            if (currenttoken.TokenType != s) {
                string msg = string.Format(
                    Strings.Expression_Unexpected_symbol_Waits_for_comes,
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
                    if (currenttoken.TokenType == TokenType.rbrac) {
                        var empty = new Terminal(TokenType.list, currenttoken.LN, currenttoken.CP);
                        f = new Nodo(empty);
                    } else
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
                    var msg = string.Format(Strings.Expression_Syntax_error,
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

        Nodo call(Nodo nizq) {
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
                var deep = new Nodo(op, bindings, nizq);
                return call(deep);
            } else
                return nizq;
        }

        Nodo expresion_evaluada() {
            Nodo nizq = factor();
            return call(nizq);
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
                var ln = currenttoken.LN; var cp = currenttoken.CP;
                var head = new Terminal(TokenType.lambdahead, nl.ToArray(), ln, cp);
                nexttoken();
                ln = currenttoken.LN; cp = currenttoken.CP;
                var lbljmp = newLabel();
                var jmplabel = new Terminal(TokenType.label, lbljmp, ln, cp);
                var jmp = new Terminal(TokenType.jmp, lbljmp, ln, cp);
                var body = expresion_lambda();
                ln = currenttoken.LN; cp = currenttoken.CP;
                var bodyop = new Terminal(TokenType.lambdabody, lbljmp, ln, cp);
                return new Nodo(op,
                    new Nodo(head),
                    new Nodo(bodyop,
                        new Nodo(jmp),
                        new Nodo(jmplabel,
                            body, null)));
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
                var msg = string.Format(Strings.Expression_Syntax_error,
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
                    var lbljmpzero = newLabel();
                    n = new Nodo(op,
                        nizq,
                        new Nodo(nilterminal(),
                            new Nodo(new Terminal(TokenType.jmpzero, lbljmpzero, currenttoken.LN, currenttoken.CP)),
                            new Nodo(nilterminal(),
                                nder,
                                new Nodo(new Terminal(TokenType.label, lbljmpzero, currenttoken.LN, currenttoken.CP)))));
                    break;
                case TokenType.or:
                    var lbljmpnotz = newLabel();
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
    }

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


}

