grammar Dice;

/*
 * Parser Rules
 */
 expression: multiplyTerm (TermOperator multiplyTerm)*  ;
 multiplyTerm: powerTerm (MultiplyOperator powerTerm)* ;
 powerTerm: coreTerm (PowerOperator coreTerm)? ;
 coreTerm: Literal | LeftBracket expression RightBracket ;


/*
 * Lexer Rules
 */
fragment DIGIT : [0-9] ;

TermOperator : ('+'|'-') ;
MultiplyOperator: ('*'|'/') ;
PowerOperator: ('^') ;
LeftBracket: ('(') ;
RightBracket: (')') ;
Literal : ('-')?DIGIT+(('.') DIGIT+)? ;
WhiteSpace : (' '|'\t'|'\r'|'\n')+ -> skip ;

