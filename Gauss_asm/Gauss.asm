
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
    ; R8 = elim/pivot  (float)
    ; R9 = rowOffset/ elim_index/ sizeMatrix INT (DWORD) -> dla fukcji pivot    WYWALONE Z PG GLOWNEGO
    
   

    movss xmm2, dword ptr [r8]           ; piovt
   
   
    movss xmm3, dword ptr [r8+4]         ; elim (wiersz N+1, kolumna pivota))
    divss xmm3, xmm2                     ; elim/pivot -> co z zaokragleniem

   
   
    vmovups ymm0, [rcx]                  ; wiersz eliminujacy
    vmovups ymm1, [rdx]                  ; wiersz do eliminacji 
 
   
    vbroadcastss ymm3, xmm3              ; elim

   
    vmulps ymm0, ymm0, ymm3              ; rowN * (elim)
    vsubps ymm1, ymm1, ymm0              ; rowNext - rowNext*Wn[Y]/pivot

    vmovups [rdx], ymm1                  ; nadpisz ca³y wiersz Next
    vzeroupper                           ;
  
    ret  
    
gauss_elimination endp;

;===============================================

END                                      ;

