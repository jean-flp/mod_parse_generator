grammar ggo;

// Exemplo de entrada esperada:
// B JOGA D4
// TABULEIRO: 9
// .........
// .........
// ....B....
// .........
// .........
// .........
// .........
// .........
// .........

input  : move_decl 'TABULEIRO:' board_size NEWLINE board EOF ;

move_decl : player 'JOGA' pos ;
player    : 'B' | 'P' ;
pos       : LETRA LINHA ;

board_size : BOARD_SIZE ;

board  : (row)+ ;
row    : cell+ NEWLINE? ;
cell   : 'B' | 'P' | '.' ;

LETRA      : [A-T] ;
LINHA      : ([1-9]|'1'[0-9]);
BOARD_SIZE : '9' | '13' | '19';

NEWLINE: [\r\n]+ ;
WS     : [ \t]+ -> skip ;

