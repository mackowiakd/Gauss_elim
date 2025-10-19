#pragma once
#include <vector>
#include <string>
#include <iostream>
#include <fstream>
#include <sstream>
#include <stdexcept>
#include <iomanip>

//napisac funkcje ktora wczyta macierz z pliku (utowrzy obiekt klasy MatrixHandler)

class MatrixHandler {
    static constexpr float EPS = 1.0e-5f;
    
public:
    int rows = 0;
    int cols = 0;
    float pivot = 10.0f;
    std::vector<float> data;        // macierz w postaci 1D (row-major)


    // Konstruktor wczytuj¹cy macierz z pliku
    MatrixHandler(const std::string& path) {
        LoadMatrixFromFile(path);
    }

    void LoadMatrixFromFile(const std::string& path);
    void print_matrix();

    // Dostêp do elementu (wiersz, kolumna)
    inline float& at(int r, int c) {
        return data[r * cols + c];
    }

    void SaveMatrixToFile(const std::string& path);

    // Przyk³adowa metoda: zerowanie ma³ych wartoœci
    void ZeroUntilEps(int startRow, int startCol);


	//  Pivotowanie (czêœciowe) -> nie musi nic zwracaæ tylko daje wiersz z najwiêkszym elementem w miejsce pivotu
    void ApplyPivot(int currentRow);

    void GaussElimination();
};
