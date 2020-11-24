grammar Dice;

/*
 * Parser Rules
 */
 expression: multiplyTerm (termOperation multiplyTerm)*  ;
 termOperation: (AddOperator | SubOperator) ;
 multiplyTerm: powerTerm (multOperation powerTerm)* ;
 multOperation: (MultOperator | DivOperator) ;
 powerTerm: coreTerm (PowerOperator coreTerm)? ;
 coreTerm: decimalNum  | diceRoll | keepExpression | LeftBracket expression RightBracket ;
 diceRoll: (naturalNum)? dieRoll ;
 dieRoll: DieD naturalNum (ExplodeMark)? ;
 keepExpression: keepOptions (KeepHighest | KeepLowest) naturalNum ;
 keepOptions: diceRoll | LeftBracket expression RightBracket (LeftBracket expression RightBracket)+ ; //Two or more expressions
 naturalNum: Natural;
 decimalNum: (SubOperator)?Natural(Fractional)?;

/*
 * Lexer Rules
 */
fragment DIGIT : [0-9] ;
fragment POSITIVEDIGIT : [1-9] ;

KeepHighest : ('k')?'h' ;
KeepLowest : ('k')?'l' ;
DieD : ('d' | 'D') ;
ExplodeMark : '!' ;
Natural : (POSITIVEDIGIT)DIGIT* ;
Fractional : ('.')DIGIT+ ;
AddOperator : '+' ;
SubOperator : '-' ;
MultOperator: '*' ;
DivOperator: '/' ;
PowerOperator: '^' ;
LeftBracket: '(' ;
RightBracket: ')' ;
WhiteSpace : [ \t\n\r]+ -> skip ;
