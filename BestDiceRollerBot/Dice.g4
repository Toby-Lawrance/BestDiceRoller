grammar Dice;

/*
 * Parser Rules
 */
 expression: multiplyTerm (TermOperator multiplyTerm)*  ;
 multiplyTerm: powerTerm (MultiplyOperator powerTerm)* ;
 powerTerm: coreTerm (PowerOperator coreTerm)? ;
 coreTerm: Literal | dieRoll | LeftBracket expression RightBracket ;
 dieRoll: (Natural)? DieD Natural (ExplodeMark)? ;

/*
 * Lexer Rules
 */
fragment DIGIT : [0-9] ;
fragment POSITIVEDIGIT : [1-9] ;

DieD : 'd' ;
ExplodeMark : '!' ;
Natural : POSITIVEDIGIT(DIGIT)* ;
TermOperator : ('+'|'-') ;
MultiplyOperator: ('*'|'/') ;
PowerOperator: ('^') ;
LeftBracket: ('(') ;
RightBracket: (')') ;
Literal : ('-')?DIGIT+(('.') DIGIT+)? ;
WhiteSpace : (' '|'\t'|'\r'|'\n')+ -> skip ;
