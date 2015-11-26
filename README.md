# HypoLambda #

A small functional programming language for .NET.

Its syntax is based on Python but whitespaces are ignored. Everything is an expression.
The language runtime is embeddable, its AST is accesible and it compiles to portable pcode.
There's an REPL you can play with and the project includes lots of unit tests.


### Why? ###

Though I do not recommend using it to develop full applications it's very appropiate
as a template or customization language for a .NET application.

HL has a very small footprint which lets you embed it in any kind of .NET project
including mobile ones.


### Example ###

Calculate a factorial:

    f = lambda x:
        (x*f(x-1)) if x > 1 else 1;
    f(4)

returns 24.0

Closures are supported:

    add = (lambda: (
        counter = 2;
        lambda: (counter = counter + 1)
    ))();
    add();
    add()

returns 4.0. The `()` in the fourth line creates the closure.

Run HL within a .NET program and interact with it:

    var exp = new HypoLambda();
    exp.SymbolTable["A"] = "a";
    exp.Compile("A * 3");
    var result = exp.Run()

result contains "aaa".


### How do I get set up? ###

* You will need either Visual Studio o Xamarin Studio.
* Check out the repository.
* Open the solution with Visual Studio.
* The IDE should download all the necessary components.
* Compile and run the tests.
* Run the console application.

You can build and run the tests in OSX or Linux using the Makefile
in the command line shell.

Running the tests under Xamarin Studio is currently not supported
because the IDE doesn't integrate the NUnit console version 3.


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


### Future work ###

Asynchronous evaluations.

The `async` keyword transforms a lambda expression into a promise.
The fulfillment of the future value is implicit, blocking is
delayed until the actual is required.

    a_function = lambda x: x + 2;
    a_promise = async a_function;
    future_val = a_promise(3);
    ...
    result = future_val + 5

Namespaces or dictionaries.

Comments in code.


### Who do I talk to? ###

* For any doubt or inquiry post an issue or tweet me at http://twitter.com/herchu


