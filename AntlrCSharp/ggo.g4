grammar ggo;

// Sequencia de movimentos
seq_move  : (move_decl SEP)*(move_decl);

//jogada
move_decl : JOGADOR '|' pos ;

pos       : LETRA LINHA
          | 'PASSA';

//tokens
JOGADOR   : 'Branco' | 'Preto' ;
LETRA     : [A-HJ-T];
LINHA     : ([1-9]|'1'[0-9]);

SEP : '/' ;
NEWLINE: [\r\n]+ -> skip;
WS     : [ \t]+ -> skip ;