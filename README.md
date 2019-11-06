# HypoLambda

A very compact functional programming language for .NET.

Its syntax is based on Python, but whitespaces are ignored. Everything is an expression.
The language runtime is embeddable, its AST is accessible, and it compiles to portable pcode.
There's a REPL you can play with, and the project includes lots of unit tests.


### Why?

Though I do not recommend using it to develop full applications, it's very appropriate
as a template or customization language for a .NET application.

HL has a tiny footprint which lets you embed it in any kind of .NET project
including mobile ones.


### Example

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
    exp.Externals["A"] = "a";
    exp.Compile("A * 3");
    var result = exp.Run()

result contains "aaa".


### How do I get set up?

The easiest way is to add HypoLambda to your .NET project
using Nuget.

If you want to try a console build HypoLambda:

* You will need Visual Studio. You can use the Community
  Editions (either from Microsoft or Xamarin).
* Check out the repository.
* Open the solution with Visual Studio.
* The IDE should download all the necessary components.
* Compile and run the tests.
* Run the console application.

You can build and run the tests in OSX or Linux using the Makefile
in the command line shell although the tests fails to run from
the Makefile.


### Grammar

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


### Future work

For HL to take on serious tasks it will need to support:

* Add objects or dictionaries or maybe namespaces?
* Include comments in the code.
* Implement tail recursion.
* Apply tail recursion to loops.

#### Asynchronous evaluations

Provide an `async` keyword that transforms a lambda expression
into a promise. The fulfillment of the future value would be
implicit and blocking delayed until the actual value is required.

    a_function = lambda x: x + 2;
    a_promise = async a_function;
    future_val = a_promise(3);
    ...
    result = future_val + 5

#### Other planned features

* Compressed (or binary) pcode.
* Restricted execution environment. (CPU, Memory and I/O)
* Rewrite the parser as shift-reduce.

Of course, I'm open to suggestions...


### License

This project is published under the
[Apache License](http://www.apache.org/licenses/LICENSE-2.0).

### Contributions

I gratefully honor Pull Requests.
Please, consider formatting the code with K&R style and four spaces tabs.

### Who do I talk to?

For questions or requests post an issue here or tweet me at
[@herchu](http://twitter.com/herchu)


