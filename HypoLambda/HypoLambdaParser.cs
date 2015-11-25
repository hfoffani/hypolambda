using System;
using System.Collections.Generic;

namespace HL
{
    public partial class HypoLambda
    {
        IEnumerator<Terminal> enu;

        private string newLabel()
        {
            return string.Format(labelFmt, labelNumber++);
        }

        void nexttoken(IEnumerator<Terminal> enumerator)
        {
            enu = enumerator;
            if (enu.MoveNext())
                lookaheadtoken = enu.Current;
            else
                lookaheadtoken = new Terminal(TokenType.NIL, 0, 0);
            nexttoken();
        }

        void nexttoken()
        {
            currenttoken = lookaheadtoken;
            if (enu.MoveNext())
                lookaheadtoken = enu.Current;
            else
                lookaheadtoken = new Terminal(TokenType.NIL, currenttoken.LN, currenttoken.CP);
        }

        Terminal lookaheadtoken = null;
        Terminal lookaheadone()
        {
            return lookaheadtoken;
        }

        void error(string msg)
        {
            throw new ApplicationException(msg);
        }

        Terminal currenttoken;

        void expect(TokenType s)
        {
            if (currenttoken.TokenType != s) {
                string msg = string.Format(
                    Strings.Expression_Unexpected_symbol_Waits_for_comes,
                    s, currenttoken.TokenType, currenttoken.LN, currenttoken.CP);
                error(msg);
            }
        }

        Node factor()
        {
            Node f = null;
            switch (currenttoken.TokenType) {
                case TokenType.ident:
                    f = new Node(currenttoken);
                    break;
                case TokenType.number:
                    f = new Node(currenttoken);
                    break;
                case TokenType.str:
                    f = new Node(currenttoken);
                    break;
                case TokenType.lbrac:
                    nexttoken();
                    if (currenttoken.TokenType == TokenType.rbrac) {
                        var empty = new Terminal(TokenType.list, currenttoken.LN, currenttoken.CP);
                        f = new Node(empty);
                    } else
                        f = build_list();
                    expect(TokenType.rbrac);
                    break;
                case TokenType.lparen:
                    nexttoken();
                    f = expression();
                    expect(TokenType.rparen);
                    break;
                case TokenType.lcurly:
                    nexttoken();
                    expect(TokenType.ident);
                    f = new Node(currenttoken);
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

        Node build_list()
        {
            var f = expression_lambda();
            if (currenttoken.TokenType == TokenType.comma) {
                nexttoken();
                return new Node(new Terminal(TokenType.comma, currenttoken.LN, currenttoken.CP), f, build_list());
            } else {
                var empty = new Terminal(TokenType.list, currenttoken.LN, currenttoken.CP);
                var oplist = new Terminal(TokenType.comma, currenttoken.LN, currenttoken.CP);
                return new Node(oplist, f, new Node(empty));
            }
        }

        Node call(Node nleft)
        {
            if (currenttoken.TokenType == TokenType.lparen) {
                var tl = new Terminal(TokenType.list, currenttoken.LN, currenttoken.CP);
                var bindings = new Node(tl);
                nexttoken();
                if (currenttoken.TokenType != TokenType.rparen) {
                    bindings = build_list();
                }
                expect(TokenType.rparen);
                nexttoken();
                var op = new Terminal(TokenType.eval, currenttoken.LN, currenttoken.CP);
                var deep = new Node(op, bindings, nleft);
                // keep consuming evaluations.
                return call(deep);
            } else
                return nleft;
        }

        Node expression_evaluated()
        {
            Node nleft = factor();
            return call(nleft);
        }

        Node factor_negated()
        {
            Node nleft = null;
            if (currenttoken.TokenType == TokenType.not) {
                Terminal op = currenttoken;
                nexttoken();
                nleft = expression_evaluated();
                Node n = new Node(op, nleft, null);
                return n;
            } else
                return expression_evaluated();
        }

        Node term()
        {
            Node nleft = factor_negated();
            if (currenttoken.TokenType == TokenType.times || currenttoken.TokenType == TokenType.slash ||
                currenttoken.TokenType == TokenType.perc) {
                Terminal op = currenttoken;
                nexttoken();
                Node nright = term();
                Node n = new Node(op, nleft, nright);
                return n;
            } else
                return nleft;
        }

        Node expression_sum()
        {
            Node nleft = term();
            if (currenttoken.TokenType == TokenType.plus || currenttoken.TokenType == TokenType.minus) {
                Terminal op = currenttoken;
                nexttoken();
                Node nright = expression_sum();
                Node n = new Node(op, nleft, nright);
                return n;
            } else
                return nleft;
        }

        Node expression_logic()
        {
            Node nleft = expression_sum();
            if (currenttoken.TokenType == TokenType.eq || currenttoken.TokenType == TokenType.gt ||
                currenttoken.TokenType == TokenType.gteq || currenttoken.TokenType == TokenType.lt ||
                currenttoken.TokenType == TokenType.lteq || currenttoken.TokenType == TokenType.neq) {
                Terminal op = currenttoken;
                nexttoken();
                Node nright = expression_logic();
                Node n = new Node(op, nleft, nright);
                return n;
            } else
                return nleft;
        }

        Node expression_andor()
        {
            Node nleft = expression_logic();
            if (currenttoken.TokenType == TokenType.and || currenttoken.TokenType == TokenType.or) {
                Terminal op = currenttoken;
                nexttoken();
                Node nright = expression_andor();
                Node n = and_or_ast(op, nleft, nright);
                return n;
            } else
                return nleft;
        }

        Node expression_cond()
        {
            Node whentrue = expression_andor();
            if (currenttoken.TokenType == TokenType.iff) {
                nexttoken();
                Node condition = expression_andor();
                expect(TokenType.els);
                nexttoken();
                Node whenfalse = expression_cond();

                var n = new Node(new Terminal(TokenType.iff, currenttoken.LN, currenttoken.CP),
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

        List<string> name_list()
        {
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

        List<string> name_list_parens()
        {
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

        Node expression_lambda()
        {
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
                var body = expression_lambda();
                ln = currenttoken.LN; cp = currenttoken.CP;
                var bodyop = new Terminal(TokenType.lambdabody, lbljmp, ln, cp);
                return new Node(op,
                    new Node(head),
                    new Node(bodyop,
                        new Node(jmp),
                        new Node(jmplabel,
                            body, null)));
            } else {
                return expression_cond();
            }
        }

        Node expression_list()
        {
            var f = expression_single();
            if (currenttoken.TokenType == TokenType.semicolon) {
                nexttoken();
                return new Node(new Terminal(TokenType.semicolon, currenttoken.LN, currenttoken.CP), f, expression_list());
            } else if (currenttoken.TokenType == TokenType.comma) {
                var msg = string.Format(Strings.Expression_Syntax_error,
                    currenttoken.LN, currenttoken.CP);
                error(msg);
                return null;
            } else
                return f;
        }

        Node assignment()
        {
            var s = currenttoken.Value.ToString();
            nexttoken();
            expect(TokenType.assig);
            var op = new Terminal(TokenType.assig, s, currenttoken.LN, currenttoken.CP);
            nexttoken();
            var r = new Node(new Terminal(TokenType.identlocal, s, currenttoken.LN, currenttoken.CP));
            var l = expression_single();
            return new Node(op, l, r);
        }

        Node expression_single()
        {
            if (currenttoken.TokenType == TokenType.ident && lookaheadone().TokenType == TokenType.assig) {
                return assignment();
            } else {
                return expression_lambda();
            }
        }

        Node expression()
        {
            return expression_list();
        }

        private Terminal nilterminal()
        {
            return new Terminal(TokenType.NIL, null, currenttoken.LN, currenttoken.CP);
        }

        private Node and_or_ast(Terminal op, Node nleft, Node nright)
        {
            Node n = null;
            switch (op.TokenType) {
                case TokenType.and:
                    var lbljmpzero = newLabel();
                    n = new Node(op,
                        nleft,
                        new Node(nilterminal(),
                            new Node(new Terminal(TokenType.jmpzero, lbljmpzero, currenttoken.LN, currenttoken.CP)),
                            new Node(nilterminal(),
                                nright,
                                new Node(new Terminal(TokenType.label, lbljmpzero, currenttoken.LN, currenttoken.CP)))));
                    break;
                case TokenType.or:
                    var lbljmpnotz = newLabel();
                    n = new Node(op,
                        nleft,
                        new Node(nilterminal(),
                            new Node(new Terminal(TokenType.jmpnotz, lbljmpnotz, currenttoken.LN, currenttoken.CP)),
                            new Node(nilterminal(),
                                nright,
                                new Node(new Terminal(TokenType.label, lbljmpnotz, currenttoken.LN, currenttoken.CP)))));
                    break;
            }
            return n;
        }
    }

    class Node
    {
        private Terminal o;
        private Node left;
        private Node right;

        public Node(Terminal o)
        {
            this.o = o;
        }

        public Node(Terminal o, Node izq, Node der)
        {
            this.o = o;
            this.left = izq;
            this.right = der;
        }

        public Terminal Tag
        {
            get { return this.o; }
        }

        public Node Left
        {
            get { return this.left; }
        }

        public Node Right
        {
            get { return this.right; }
        }
    }

}

