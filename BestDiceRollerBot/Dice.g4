grammar Dice;

/*
 * Parser Rules
 */
 expression: multiplyTerm (termOperation multiplyTerm)*  ;
 termOperation: (AddOperator | SubOperator) ;
 multiplyTerm: powerTerm (multOperation powerTerm)* ;
 multOperation: (MultOperator | DivOperator) ;
 powerTerm: coreTerm (PowerOperator coreTerm)? ;
 coreTerm: decimalNum  | dieRoll  | LeftBracket expression RightBracket ;
 dieRoll: (naturalNum)? DieD naturalNum (ExplodeMark)? ;
 naturalNum: Natural;
 decimalNum: (SubOperator)?Natural(Fractional)?;

/*
 * Lexer Rules
 */
fragment DIGIT : [0-9] ;
fragment POSITIVEDIGIT : [1-9] ;

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
