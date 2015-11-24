using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace HL
{
    public partial class HypoLambda
    {
        private static string REGEX_VARS = @"[\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}_][\p{Ll}\p{Lu}\p{Lt}\p{Lo}\p{Lm}_\p{Nd}\.]*";
        private static string REGEX_NUMS = @"[\p{Nd}][\.\p{Nd}]*";

        private static Regex reVars = new Regex(REGEX_VARS);
        private static Regex reNums = new Regex(REGEX_NUMS);

        IEnumerable<Terminal> Lexer(string input) {

            int lnum = 1;
            int cpos = 1;

            // terminales = new List<Terminal>();

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
                    yield return new Terminal(TokenType.str, token, lnum, cpos);
                    input = input.Substring(p + 1); // el +1 saltea el ultimo "
                    continue;
                }
                m = reVars.Match(input);
                if (m.Success && m.Index == 0) {
                    token = m.Groups[0].Value;
                    if (reservedwords.ContainsKey(token)) {
                        // palabras reservadas
                        yield return reservedwords[token];
                    } else {
                        // variables.
                        string root = getRootSymbol(token);
                        if (!this.symbolTable.ContainsKey(root))
                            this.symbolTable.Add(root, null);
                        yield return new Terminal(TokenType.ident, token, lnum, cpos);
                    }
                    input = input.Substring(token.Length); cpos += token.Length;
                    continue;
                }
                m = reNums.Match(input);
                if (m.Success && m.Index == 0) {
                    token = m.Groups[0].Value;
                    double cte = double.Parse(token, System.Globalization.CultureInfo.InvariantCulture);
                    yield return new Terminal(TokenType.number, cte, lnum, cpos);
                    input = input.Substring(token.Length); cpos += token.Length;
                    continue;
                }
                switch (input[0]) {
                    case ':':
                        yield return new Terminal(TokenType.colon, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '+':
                        yield return new Terminal(TokenType.plus, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '-':
                        yield return new Terminal(TokenType.minus, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '*':
                        yield return new Terminal(TokenType.times, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '/':
                        yield return new Terminal(TokenType.slash, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '%':
                        yield return new Terminal(TokenType.perc, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '(':
                        yield return new Terminal(TokenType.lparen, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case ')':
                        yield return new Terminal(TokenType.rparen, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '{':
                        yield return new Terminal(TokenType.lcurly, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '}':
                        yield return new Terminal(TokenType.rcurly, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case ',':
                        yield return new Terminal(TokenType.comma, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case ';':
                        yield return new Terminal(TokenType.semicolon, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '[':
                        yield return new Terminal(TokenType.lbrac, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case ']':
                        yield return new Terminal(TokenType.rbrac, lnum, cpos);
                        input = input.Substring(1); cpos += 1;
                        continue;
                    case '!':
                        switch (input[1]) {
                            case '=':
                                yield return new Terminal(TokenType.neq, lnum, cpos);
                                input = input.Substring(2); cpos += 2;
                                continue;
                            default:
                                yield return new Terminal(TokenType.not, lnum, cpos);
                                input = input.Substring(1); cpos += 1;
                                continue;
                        }
                    case '>':
                        switch (input[1]) {
                            case '=':
                                yield return new Terminal(TokenType.gteq, lnum, cpos);
                                input = input.Substring(2); cpos += 2;
                                continue;
                            default:
                                yield return new Terminal(TokenType.gt, lnum, cpos);
                                input = input.Substring(1); cpos += 1;
                                continue;
                        }
                    case '<':
                        switch (input[1]) {
                            case '=':
                                yield return new Terminal(TokenType.lteq, lnum, cpos);
                                input = input.Substring(2); cpos += 2;
                                continue;
                            default:
                                yield return new Terminal(TokenType.lt, lnum, cpos);
                                input = input.Substring(1); cpos += 1;
                                continue;
                        }
                    case '=':
                        switch (input[1]) {
                            case '=':
                                yield return new Terminal(TokenType.eq, lnum, cpos);
                                input = input.Substring(2); cpos += 2;
                                continue;
                            default:
                                yield return new Terminal(TokenType.assig, lnum, cpos);
                                input = input.Substring(1); cpos += 1;
                                continue;
                        }
                }
                // si hay algo que no reconoce, sale.
                break;
            }
        }

        static Dictionary<string, TokenType> binary_operators = new Dictionary<string, TokenType>() { 
            { "+", TokenType.plus },
            { "-", TokenType.minus },
            { "*", TokenType.times },
            { "/", TokenType.slash },
            { "%", TokenType.perc },
            { "!=", TokenType.neq },
            { "!", TokenType.not },
            { ">=", TokenType.gteq },
            { ">", TokenType.gt },
            { "<=",TokenType.lteq },
            { "<", TokenType.lt },
            { "==",TokenType.eq },
            { "=",TokenType.assig },
        };
    }
}

