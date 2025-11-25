
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
    vcmpps ymm0, ymm3, ymm5, 17    ; YMM6 = Maska (111.. jeœli ma³e, 000.. jeœli du¿e)

    ; 3. Zerowanie (AND NOT)
    ; Zostawiamy tylko te wartoœci, gdzie maska to 0
    vandnps ymm1, ymm0, ymm1       ; Zeruje ma³e liczby, resztê zostawia

    ; ---------------------------------------------------------
    ; KROK E: Zapis
    ; ---------------------------------------------------------

    vmovups [rdx], ymm1                  ; nadpisz ca³y wiersz Next
    vzeroupper                           ;
  
    ret  
    
gauss_elimination endp;

;===============================================




; float calculate_dot_product(float* rowPtr, float* xPtr, int count)
; RCX = rowPtr
; RDX = xPtr
; R8  = count
; Zwraca wynik w XMM0

calculate_dot_product proc
    
    ; Zerujemy akumulator sumy (YMM0)
    vxorps ymm0, ymm0, ymm0 

    ; SprawdŸ, czy mamy chocia¿ 8 elementów do przetworzenia
    cmp r8, 8
    jl Scalar_Loop ; Jeœli mniej ni¿ 8, skocz do pêtli skalarnej

Vector_Loop:
    ; --- G£ÓWNA PÊTLA AVX (po 8 sztuk) ---
    
    vmovups ymm1, [rcx]      ; Za³aduj 8 liczb z macierzy
    vmovups ymm2, [rdx]      ; Za³aduj 8 liczb z wektora X
    
    vmulps ymm1, ymm1, ymm2  ; Wymnó¿: A * X
    vaddps ymm0, ymm0, ymm1  ; Dodaj do sumy czêœciowej w YMM0

    ; Przesuñ wskaŸniki o 8 floatów (32 bajty)
    add rcx, 32
    add rdx, 32
    sub r8, 8                ; Zmniejsz licznik

    cmp r8, 8
    jge Vector_Loop          ; Jeœli zosta³o >= 8, powtórz

    ; --- REDUKCJA POZIOMA (Horizontal Add) ---
    ; Teraz w YMM0 mamy [s7, s6, s5, s4, s3, s2, s1, s0]. Trzeba je dodaæ do siebie.
    
    ; 1. Dodaj górn¹ po³owê YMM do dolnej (128 bitów)
    vextractf128 xmm1, ymm0, 1
    vaddps xmm0, xmm0, xmm1
    
    ; 2. Dodaj poziomo (HADDPS) - redukcja do jednej liczby
    vhaddps xmm0, xmm0, xmm0 ; [s3+s2, s1+s0, ...]
    vhaddps xmm0, xmm0, xmm0 ; [suma_ca³kowita, ...] 
    
    ; Teraz w dolnym float XMM0 mamy sumê wektorow¹.

Scalar_Loop:
    ; --- PÊTLA SKALARNA (Dla resztek < 8) ---
    test r8, r8
    jz Done                  ; Jeœli count == 0, koniec

    vmovss xmm1, dword ptr [rcx] ; Za³aduj 1 float z macierzy
    vmovss xmm2, dword ptr [rdx] ; Za³aduj 1 float z wektora X
    
    vmulss xmm1, xmm1, xmm2      ; Pomnó¿
    vaddss xmm0, xmm0, xmm1      ; Dodaj do g³ównej sumy (XMM0)

    add rcx, 4
    add rdx, 4
    dec r8
    jmp Scalar_Loop

Done:
    ; Wynik jest ju¿ w XMM0, gotowy do zwrócenia
    vzeroupper
    ret

calculate_dot_product endp








END                                      ;

