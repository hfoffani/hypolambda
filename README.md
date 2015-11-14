# LambdaLang #

A small functional programming language for .NET.

### How do I get set up? ###

* Check out the repository.
* Open the solution with Xamarin Studio. The IDE should download all the necessary components.
* Compile and run the tests.
* Run the console application.


### Grammar ###

        expression :=   list_exprs

        list_exprs :=
                        single_exp
                    |   single_exp ";" list_exprs

        terminal :=
        				vname
        			|	number
        			|	string
        			|	list

        list := 		"[" ( terminal ("," terminal)* ) "]"

        single_exp :=
                        terminal
        			|	"(" expression ")"
					|   vname = single_exp
                    |   lambda_exp "(" single_exp ("," single_exp)* ")"
                    |   condition
                    |   single_exp "*" single_exp
                    |   single_exp "/" single_exp
                    |   single_exp "+" single_exp
                    |   single_exp "-" single_exp
                    |   single_exp "if" condition "else" single_exp

        condition :=    single_exp
                    |   condition "and" condition
                    |   condition "or" condition
                    |   "not" condition
                    |   single_exp "\>" single_exp
                    |   single_exp "\>=" single_exp
                    |   single_exp "<" single_exp
                    |   single_exp "<=" single_exp
                    |   single_exp "==" single_exp
                    |   single_exp "!=" single_exp

        lambda_exp :=	"lambda" ( "(" vname ("," vname)* ")" ) ":" single_exp



### Who do I talk to? ###

* For any doubt or inquiry post an issue or tweet me at http://twitter.com/herchu
