﻿// Skeleton written by Joe Zachary for CS 3500, January 2015
// Revised by Joe Zachary, January 2016
// JLZ Repaired pair of mistakes, January 23, 2016
// Additional code written by Henry Kucab 1/28/16 PS2 Commit
// Testing master commit
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Formulas
{
    /// <summary>
    /// Represents formulas written in standard infix notation using standard precedence
    /// rules.  Provides a means to evaluate Formulas.  Formulas can be composed of
    /// non-negative floating-point numbers, variables, left and right parentheses, and
    /// the four binary operator symbols +, -, *, and /.  (The unary operators + and -
    /// are not allowed.)
    /// </summary>
    public struct Formula
    {
        /// <summary>
        /// rawFormula : A list that contains the raw tokens from the formula as strings. Is an instance variable in the
        /// Formula class, accessible for all class methods.
        /// </summary>
        private List<string> rawFormula;
        /// Creates a Formula from a string that consists of a standard infix expression composed
        /// from non-negative floating-point numbers (using C#-like syntax for double/int literals), 
        /// variable symbols (a letter followed by zero or more letters and/or digits), left and right
        /// parentheses, and the four binary operator symbols +, -, *, and /.  White space is
        /// permitted between tokens, but is not required.
        /// 
        /// Examples of a valid parameter to this constructor are:
        ///     "2.5e9 + x5 / 17"
        ///     "(5 * 2) + 8"
        ///     "x*y-2+35/9"
        ///     
        /// Examples of invalid parameters are:
        ///     "_"
        ///     "-5.3"
        ///     "2 5 + 3"
        /// 
        /// If the formula is syntacticaly invalid, throws a FormulaFormatException with an 
        /// explanatory Message.
        /// </summary>
        public Formula(String formula) : this(formula, a => a, validator => true){
        }
        /// <summary>
        /// Formula constructor that takes in a string formula, passes it to a normalizer to get it in canoical
        /// form, and then runs that through a validator to check it's formatting validity. 
        /// </summary>
        /// <param name="formula"></param>
        /// <param name="normalizer"></param>
        /// <param name="validator"></param>
        public Formula(String formula, Normalizer normalizer, Validator validator)
        {
            int count = 0;
            rawFormula = new List<string>();
            int openParenthesis = 0, closeParenthesis = 0;
            char[] charformula = formula.ToCharArray();

            foreach (string b in GetTokens(formula))//Adds tokens into raw formula after the prior checks
            {
                rawFormula.Add(b);
                if(char.IsLetter(rawFormula[count][0]) == true)
                {
                    rawFormula[count] = normalizer(rawFormula[count]);
                    if (validator(rawFormula[count]) == false)
                    {
                        throw new FormulaFormatException("Validation Failed");
                    }
                }
                count++;
            }
            if (rawFormula.Count < 1)//ensures formula not empty
            {
                throw new FormulaFormatException("Formula length too short!");
            }
            if (char.IsLetterOrDigit(rawFormula[0][0]) == false && rawFormula[0][0] != '(')//checks first character validity
            {
                throw new FormulaFormatException("Starting character in formula invalid");
            }
            if (char.IsLetterOrDigit(rawFormula[rawFormula.Count - 1][0]) == false && rawFormula[rawFormula.Count - 1][0] != ')')//checks last character validity
            {
                throw new FormulaFormatException("Ending character in formula invalid");
            }
            foreach (char i in formula)//Counts number of parenthesis and checks for negative numbers
            {
                double test;
                if (i == '(')
                {
                    openParenthesis++;
                }
                if (i == ')')
                {
                    closeParenthesis++;
                }
                if (closeParenthesis > openParenthesis)
                {
                    throw new FormulaFormatException("Number of closing parenthesis exceeds number of opening parenthesis thus far (Perhaps you have )...( ? )");
                }
                double.TryParse(i.ToString(), out test);
                if (test < 0)
                {
                    throw new FormulaFormatException("Cannot have negative numbers");
                }
            }
            if (openParenthesis != closeParenthesis)//Validates number of parenthesis tokens
            {
                throw new FormulaFormatException("Number of '(' and ')' not equal");
            }
            for (int i = 0; i < rawFormula.Count() - 1; i++)//Checks for back-to-back operators or numbers.
            {
                double test;
                bool isoperand = false;
                if (rawFormula[i] == "+" || rawFormula[i] == "-" || rawFormula[i] == "*" || rawFormula[i] == "/")
                {
                    isoperand = true;
                    if ((rawFormula[i + 1] == "+" || rawFormula[i + 1] == "-" || rawFormula[i + 1] == "*" || rawFormula[i + 1] == "/") && isoperand == true)
                    {
                        throw new FormulaFormatException("Consecutive operands illegal");
                    }
                }
                if (double.TryParse(rawFormula[i], out test) == true)
                {
                    if (double.TryParse(rawFormula[i + 1], out test) == true || char.IsLetter(rawFormula[i + 1][0]) == true)
                    {
                        throw new FormulaFormatException("Missing operands");
                    }
                }
                if (rawFormula[i] == ")" && double.TryParse(rawFormula[i + 1], out test) == true)
                {
                    throw new FormulaFormatException("Cannot have number immediately after ) character.");
                }
                if (double.TryParse(rawFormula[i], out test) == true && rawFormula[i + 1] == "(")
                {
                    throw new FormulaFormatException("Cannot have number directly infront of ( character.");
                }
                if (charformula[i] == '(' && (charformula[i + 1] == '+' || charformula[i + 1] == '-' || charformula[i + 1] == '*' || charformula[i + 1] == '/'))
                {
                    throw new FormulaFormatException("Cannot have operator immediately after ( character");
                }
                if (charformula[i] == ')' && (charformula[i - 1] == '+' || charformula[i - 1] == '-' || charformula[i - 1] == '*' || charformula[i - 1] == '/'))
                {
                    throw new FormulaFormatException("Cannot have operator immediately before ) character");
                }
                if ((char.IsLetter(rawFormula[i][0]) && char.IsLetterOrDigit(rawFormula[i + 1][0])) || (char.IsLetter(rawFormula[i][0]) && double.TryParse(rawFormula[i + 1], out test)))
                {
                    throw new FormulaFormatException("Missing operator");
                }
            }
            for (int i = 0; i < rawFormula.Count; i++)//Addendum. Checks every token to ensure every character is valid.
            {
                for(int j = 0; j < rawFormula[i].Length; j++)
                {
                    string temp = rawFormula[i];
                    double test = 0;
                    if(temp != ")" && temp != "(" && temp != "+" && temp != "-" && temp != "*" && temp != "/" && (!double.TryParse(temp, out test)) && !char.IsLetterOrDigit(temp[0]))
                    {
                        throw new FormulaFormatException("Invalid character");
                    }
                }
            }
        }
        /// <summary>
        /// Runs through RawFormula after it has been normalized, adds any character that is a letter
        /// to the variableSet and returns it as an ISet HashSet
        /// </summary>
        /// <returns></returns>
        public ISet<string> GetVariables()
        {
            if(rawFormula == null)//Add zero into rawFormula if no parameter was provided
            {
                rawFormula = new List<string>();
                rawFormula.Add("0");
            }
            ISet<string> variableSet = new HashSet<string>();//Create new HashSet as the ISet
            foreach(string i in rawFormula)
            { 
                if(char.IsLetter(i[0]) == true)//If the string is a letter, consider it a variable
                {
                    variableSet.Add(i);
                }
            }
            return variableSet;
        }
        /// <summary>
        /// Evaluates this Formula, using the Lookup delegate to determine the values of variables.  (The
        /// delegate takes a variable name as a parameter and returns its value (if it has one) or throws
        /// an UndefinedVariableException (otherwise).  Uses the standard precedence rules when doing the evaluation.
        /// 
        /// If no undefined variables or divisions by zero are encountered when evaluating 
        /// this Formula, its value is returned.  Otherwise, throws a FormulaEvaluationException  
        /// with an explanatory Message.
        /// </summary>
        public double Evaluate(Lookup lookup)
        {
            Stack<double> valueStack = new Stack<double>();
            Stack<string> operatorStack = new Stack<string>();
            double test;
            if (rawFormula == null)//Add zero into rawFormula if no parameter was provided
            {
                rawFormula = new List<string>();
                rawFormula.Add("0");
            }
            foreach (string i in rawFormula)
            {
                if (double.TryParse(i, out test) == true)//If i is a DOUBLE, pop top operator and pop a values, apply operand to value and i, and push result to valuestack
                {
                    if (operatorStack.Count() != 0 && (operatorStack.Peek() == "*" || operatorStack.Peek() == "/"))
                    {
                        if (operatorStack.Peek() == "*")
                        {
                            double pop = valueStack.Pop();
                            operatorStack.Pop();
                            valueStack.Push(test * pop);
                        }
                        else if (operatorStack.Peek() == "/")
                        {
                            double pop = valueStack.Pop();
                            operatorStack.Pop();
                            if(test != 0)
                            {
                                valueStack.Push(pop / test);
                            }
                            else
                            {
                                throw new FormulaEvaluationException("Cannot divide by zero");
                            }
                        }
                    }
                    else//Otherwise, push the value
                    {
                        valueStack.Push(test);
                    }//end * and /
                }//end i as double
                else if(i == "+" || i == "-")//if i is a addition or subtraction operator, pop operator and pop top 2 values, apply operator to values, and push value to valuestack
                {
                    if (operatorStack.Count() != 0 && (operatorStack.Peek() == "+" || operatorStack.Peek() == "-"))
                    {
                        double var1, var2, resultant;
                        var1 = valueStack.Pop();
                        var2 = valueStack.Pop();
                        if(operatorStack.Peek() == "+")
                        {
                            resultant = var1 + var2;
                        }
                        else
                        {
                            resultant = var2 - var1;
                        }
                        operatorStack.Pop();
                        valueStack.Push(resultant);
                    }
                    operatorStack.Push(i);//Regardless, push i on the operator stack
                }//end i as + or -
                else if(i == "*" || i == "/" || i == "(")//if i is a multiply, divide, or ( symbol, simply push t on operator stack
                {
                    operatorStack.Push(i);
                }
                else if(i == ")")//if i is a ) symbol
                {
                    if(operatorStack.Count != 0 && (operatorStack.Peek() == "+" || operatorStack.Peek() == "-"))//if it is +-, proceed as in the +- case above
                    {
                        double var1, var2, resultant;
                        var1 = valueStack.Pop();
                        var2 = valueStack.Pop();
                        if (operatorStack.Peek() == "+")
                        {
                            resultant = var1 + var2;
                        }
                        else
                        {
                            resultant = var2 - var1;
                        }
                        operatorStack.Pop();
                        valueStack.Push(resultant);
                    }
                    operatorStack.Pop();//Regardless, push i on the operator stack
                    if(operatorStack.Count != 0 && (operatorStack.Peek() == "*" || operatorStack.Peek() == "/"))//If i is * or /, proceeed as described in prior * / case
                    {
                        double var1, var2, resultant;
                        var1 = valueStack.Pop();
                        var2 = valueStack.Pop();
                        if (operatorStack.Peek() == "*")
                        {
                            resultant = var1 * var2;
                        }
                        else
                        {
                            if(var1 != 0)
                            {
                                resultant = var2 / var1;
                            }
                            else
                            {
                                throw new FormulaEvaluationException("Cannot divide by zero");
                            }
                        }
                        operatorStack.Pop();
                        valueStack.Push(resultant);//push result onto value stack
                    }
                }//end t as )
                else//if t is a variable, proceed as in +- case with looked up value
                {
                    if (operatorStack.Count != 0 && (operatorStack.Peek() == "*" || operatorStack.Peek() == "/"))
                    {
                        if (operatorStack.Peek() == "*")
                        {
                            double pop = valueStack.Pop();
                            operatorStack.Pop();
                            try
                            {
                                valueStack.Push(lookup(i) * pop);
                            }
                            catch(UndefinedVariableException)
                            {
                                throw new FormulaEvaluationException(i + " : Missing Definition");
                            }
                        }
                        else if (operatorStack.Peek() == "/")
                        {
                            double pop = valueStack.Pop();
                            operatorStack.Pop();
                            if(lookup(i) != 0)
                            {
                                try
                                {
                                    valueStack.Push(pop / lookup(i));
                                }
                                catch(UndefinedVariableException)
                                {
                                    throw new FormulaEvaluationException(i + " : Missing Definition");
                                }
                            }
                            else
                            {
                                throw new FormulaEvaluationException("Cannot divide by zero");
                            }
                        }
                    }
                    else
                    {
                        try
                        {
                            valueStack.Push(lookup(i));
                        }
                        catch(UndefinedVariableException)
                        {
                            throw new FormulaEvaluationException(i + " : Missing Definition");
                        }
                    }//end * and /
                }
            }
            if(operatorStack.Count == 0)//after the last token is processed, final valuestack value is result
            {
                return valueStack.Pop();
            }
            else//otherwise, apply final operand to remaining two values
            {
                double var1, var2;
                var1 = valueStack.Pop();
                var2 = valueStack.Pop();
                if (operatorStack.Peek() == "+")
                {
                    return var1 + var2;
                }
                else
                {
                    return var2 - var1;
                }
            }
        }

        /// <summary>
        /// Given a formula, enumerates the tokens that compose it.  Tokens are left paren,
        /// right paren, one of the four operator symbols, a string consisting of a letter followed by
        /// zero or more digits and/or letters, a double literal, and anything that doesn't
        /// match one of those patterns.  There are no empty tokens, and no token contains white space.
        /// </summary>
        private static IEnumerable<string> GetTokens(String formula)
        {
            // Patterns for individual tokens
            String lpPattern = @"\(";
            String rpPattern = @"\)";
            String opPattern = @"[\+\-*/]";
            String varPattern = @"[a-zA-Z][0-9a-zA-Z]*";
            String doublePattern = @"(?: \d+\.\d* | \d*\.\d+ | \d+ ) (?: e[\+-]?\d+)?";
            String spacePattern = @"\s+";

            // Overall pattern
            String pattern = String.Format("({0}) | ({1}) | ({2}) | ({3}) | ({4}) | ({5})",
                                            lpPattern, rpPattern, opPattern, varPattern, doublePattern, spacePattern);

            // Enumerate matching tokens that don't consist solely of white space.
            foreach (String s in Regex.Split(formula, pattern, RegexOptions.IgnorePatternWhitespace))
            {
                if (!Regex.IsMatch(s, @"^\s*$", RegexOptions.Singleline))
                {
                    yield return s;
                }
            }
        }
        /// <summary>
        /// Returns a concatenated string of all the individual tokens from the rawFormula.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            string daString = "";
            if (rawFormula == null)//Add zero into rawFormula if no parameter was provided
            {
                rawFormula = new List<string>();
                rawFormula.Add("0");
            }
            for (int i = 0; i < rawFormula.Count(); i++)
            {
                if(daString.Length == 0)//append token w/o space on first
                {
                    daString = rawFormula[i];
                }
                else//Append a space and then next token
                {
                    daString = daString + rawFormula[i];
                }
            }
            return daString;
        }
    }

    /// <summary>
    /// A Lookup method is one that maps some strings to double values.  Given a string,
    /// such a function can either return a double (meaning that the string maps to the
    /// double) or throw an UndefinedVariableException (meaning that the string is unmapped 
    /// to a value. Exactly how a Lookup method decides which strings map to doubles and which
    /// don't is up to the implementation of the method.
    /// </summary>
    public delegate double Lookup(string s);
    /// <summary>
    /// The Normalizer method is one that can take tokens and change them based on their passed arguments.
    /// For example, in the test cases, Normalizer4 can take a in a formula and covert all lower-case variables into
    /// their upper case equivalents. It modifies the formula tokens into the defined form. 1 argument constructor 
    /// simply applies the identity formula (aka, does nothing to the token)
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public delegate string Normalizer(string s);
    /// <summary>
    /// The Validator method allows the user to impose more restrictions on the formula format. It is applied before
    /// the default restrictions are executed and AFTER the normalizer applies it's changes. Returns true or false per each
    /// token's case. Default 1 argument constructor forces it to TRUE
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public delegate bool Validator(string s);
    /// <summary>
    /// Used to report that a Lookup delegate is unable to determine the value
    /// of a variable.
    /// </summary>
    public class UndefinedVariableException : Exception
    {
        /// <summary>
        /// Constructs an UndefinedVariableException containing whose message is the
        /// undefined variable.
        /// </summary>
        /// <param name="variable"></param>
        public UndefinedVariableException(String variable)
            : base(variable)
        {
        }
    }

    /// <summary>
    /// Used to report syntactic errors in the parameter to the Formula constructor.
    /// </summary>
    public class FormulaFormatException : Exception
    {
        /// <summary>
        /// Constructs a FormulaFormatException containing the explanatory message.
        /// </summary>
        public FormulaFormatException(String message) : base(message)
        {
        }
    }

    /// <summary>
    /// Used to report errors that occur when evaluating a Formula.
    /// </summary>
    public class FormulaEvaluationException : Exception
    {
        /// <summary>
        /// Constructs a FormulaEvaluationException containing the explanatory message.
        /// </summary>
        public FormulaEvaluationException(String message) : base(message)
        {
        }
    }

}
