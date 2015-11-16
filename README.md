# HypoLambda #

A small functional programming language for .NET.

The language runtime is embeddable, its AST is accesible and it compiles to portable pcode.


### Example ###

Calculate a factorial

    f = lambda x:
        (x*f(x-1)) if x > 1 else 1;
    f(4)

returns 24.0


### How do I get set up? ###

* Check out the repository.
* Open the solution with Xamarin Studio. The IDE should download all the necessary components.
* Compile and run the tests.
* Run the console application.


### Grammar ###

    expression :=   single_exp ( ";" single_exp )*

    terminal   :=   var_name | number | string | list

    list       :=   "[" ( terminal ("," terminal)* ) "]"

    single_exp :=   terminal
                |   "(" expression ")"
                |   var_name = single_exp
                |   lambda_exp "(" single_exp ("," single_exp)* ")"
                |   condition
                |   single_exp "*" single_exp
                |   single_exp "/" single_exp
                |   single_exp "+" single_exp
                |   single_exp "-" single_exp
                |   single_exp "if" condition "else" single_exp

    condition  :=   single_exp
                |   condition "and" condition
                |   condition "or" condition
                |   "not" condition
                |   single_exp ">" single_exp
                |   single_exp ">=" single_exp
                |   single_exp "<" single_exp
                |   single_exp "<=" single_exp
                |   single_exp "==" single_exp
                |   single_exp "!=" single_exp

    lambda_exp :=   "lambda" ( "(" var_name ("," var_name)* ")" ) ":" single_exp



### Who do I talk to? ###

* For any doubt or inquiry post an issue or tweet me at http://twitter.com/herchu

