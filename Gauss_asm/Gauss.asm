
.data
   
  
   
    eps REAL4 1.0e-5                    ; tolerancja (dla float)

    mask_abs  DWORD 7FFFFFFFh           ; maska bitowa kasuj¹ca znak liczby zmiennoprzecinkowej | xmm4 = fabs(xmm3)
    float_inf DWORD 7F800000h           ; bitowy zapis +? w formacie IEEE-754 dla typu float.


PUBLIC gauss_elimination
;pivot PROTO                             ; arg -> wskaznik r9 

.code 

;===============================================;

gauss_elimination proc
   
    ; RCX = ptr rowN 256 bit
    ; RDX = ptr rowNext 256 bit
    ; XMM2 = gotowy wspó³czynnik 'factor' (przekazany z C#)
   

    ; Przenosimy factor z XMM2 do XMM3 (lub mo¿emy u¿yæ XMM2 bezpoœrednio)
    vbroadcastss ymm3, xmm2             ; Rozg³oœ gotowy 'factor'
   
   
    vmovups ymm0, [rcx]                  ; wiersz eliminujacy
    vmovups ymm1, [rdx]                  ; wiersz do eliminacji 
 
   
    vbroadcastss ymm2, xmm2              ; factor = elim/pivot

   
    vmulps ymm0, ymm0, ymm2              ; rowN * (elim)
    vsubps ymm1, ymm1, ymm0              ; rowNext - rowNext*Wn[Y]/pivot

    vmovups [rdx], ymm1                  ; nadpisz ca³y wiersz Next
    vzeroupper                           ;
  
    ret  
    
gauss_elimination endp;

;===============================================

END                                      ;

