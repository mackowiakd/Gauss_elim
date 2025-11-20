
.data
   
  
   
    mask_abs  DWORD 8 dup(7FFFFFFFh) ; Maska do zerowania bitu znaku (dla 8 floatów)
    eps_abs   REAL4 1.0e-6           ; Twoje sta³e
    eps_rel   REAL4 1.0e-4


PUBLIC gauss_elimination
;pivot PROTO                             ; arg -> wskaznik r9 

.code 

;===============================================;

gauss_elimination proc
   
    ; RCX = ptr rowN 256 bit
    ; RDX = ptr rowNext 256 bit
    ; XMM2 = gotowy wspó³czynnik 'factor' (przekazany z C#)
    ; XMM3 = |pivot| (przekazany z C#)

   ; ---------------------------------------------------------
    ; KROK A: Obliczenie Progu (Threshold)
    ; Wzór: Threshold = EPS_ABS + (EPS_REL * |pivot|)
    ; Robimy to na skalarach (XMM), bo wynik jest jedn¹ liczb¹.
    ; ---------------------------------------------------------
   ; 2. Oblicz EPS_REL * |pivot|
    movss  xmm4, dword ptr [eps_rel]
    mulss  xmm3, xmm4              ; XMM3 = |pivot| * eps_rel

    ; 3. Dodaj EPS_ABS
    movss  xmm4, dword ptr [eps_abs]
    addss  xmm3, xmm4              ; XMM3 = eps_abs + (|pivot| * eps_rel)
                                   ; Teraz XMM3 zawiera nasz finalny THRESHOLD

    ; ---------------------------------------------------------
    ; KROK B: Przygotowanie wektorów YMM
    ; ---------------------------------------------------------

    ; 1. Rozg³oœ obliczony Threshold na ca³y rejestr YMM5
    vbroadcastss ymm5, xmm3        ; YMM5 = [Threshold, Threshold, ...]

    ; 2. Rozg³oœ factor na ca³y rejestr YMM2
    vbroadcastss ymm2, xmm2        ; YMM2 = [factor, factor, ...]

    ; 3. Za³aduj maskê wartoœci bezwzglêdnej do YMM4
    vmovups ymm4, YMMWORD PTR [mask_abs]
  
    ; ---------------------------------------------------------
    ; KROK C: W³aœciwa Eliminacja Gaussa
    ; ---------------------------------------------------------
   
   
    vmovups ymm0, [rcx]                  ; wiersz eliminujacy
    vmovups ymm1, [rdx]                  ; wiersz do eliminacji 

   
    vmulps ymm0, ymm0, ymm2              ; rowN * (elim) //tutaj warto spr czy nie ma <eps
    vsubps ymm1, ymm1, ymm0              ; rowNext - rowNext*Wn[Y]/pivot
    ; ---------------------------------------------------------
    ; KROK D: ZeroUntilEps (Logika z Twojego C#)
    ; if (|wynik| < Threshold) wynik = 0
    ; ---------------------------------------------------------

    ; 1. Oblicz |wynik|
    vandps ymm3, ymm1, ymm4        ; YMM3 = |YMM1| (kasujemy bit znaku)

    ; 2. Porównaj: Czy |wynik| < Threshold (YMM5)?
    ; 17 = _CMP_LT_OQ (Less Than)
    vcmpps ymm6, ymm3, ymm5, 17    ; YMM6 = Maska (111.. jeœli ma³e, 000.. jeœli du¿e)

    ; 3. Zerowanie (AND NOT)
    ; Zostawiamy tylko te wartoœci, gdzie maska to 0
    vandnps ymm1, ymm6, ymm1       ; Zeruje ma³e liczby, resztê zostawia

    ; ---------------------------------------------------------
    ; KROK E: Zapis
    ; ---------------------------------------------------------

    vmovups [rdx], ymm1                  ; nadpisz ca³y wiersz Next
    vzeroupper                           ;
  
    ret  
    
gauss_elimination endp;

;===============================================

END                                      ;

