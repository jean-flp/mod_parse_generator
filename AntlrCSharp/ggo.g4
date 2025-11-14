grammar ggo;
import CommonLexerRules;

// Sequencia de movimentos
seq_move  : (move_decl'//')*(move_decl);

//jogada
move_decl : JOGADOR '|' pos ;

pos       : LETRA LINHA
          | 'PASSA';

//tokens
JOGADOR   : 'B' | 'P' ;
LETRA     : [A-T] ;
LINHA     : ([1-9]|'1'[0-9]);

//BOARD_SIZE : '9' | '13' | '19';