#pragma once
#include <vector>
#include <string>
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
    std::vector<float> data;        // macierz w postaci 1D (row-major)


    // Konstruktor wczytuj¹cy macierz z pliku
    MatrixHandler(const std::string& path) {
        LoadMatrixFromFile(path);
    }

    void LoadMatrixFromFile(const std::string& path);

    // Dostêp do elementu (wiersz, kolumna)
    inline float& at(int r, int c) {
        return data[r * cols + c];
    }

    void SaveMatrixToFile(const std::string& path);

    // Przyk³adowa metoda: zerowanie ma³ych wartoœci
    void ZeroUntilEps(int startRow, int startCol) {
        for (int r = startRow; r < rows; ++r) {
            for (int c = startCol; c < cols; ++c) {
                if (std::abs(at(r, c)) < EPS)
                    at(r, c) = 0.0f;
            }
        }
    }




    //  Pivotowanie (czêœciowe)
    float ApplyPivot(int currentRow) {
        int pivotRow = currentRow;
        float maxAbs = std::fabs(data[currentRow * cols + currentRow]); //aktulany pivot

        // znajdŸ wiersz z najwiêkszym elementem w kolumnie
        for (int i = currentRow + 1; i < rows; ++i) {
            float val = std::fabs(data[i * cols + currentRow]);
            if (val > maxAbs) {
                maxAbs = val;
                pivotRow = i;
            }
        }

        // jeœli trzeba, zamieñ wiersze
        if (pivotRow != currentRow) {
            for (int j = 0; j < cols; ++j) {
                std::swap(data[currentRow * cols + j], data[pivotRow * cols + j]);
            }
        }
        return maxAbs;
    }

    void GaussElimination();
};
