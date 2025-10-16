#pragma once
#include <vector>
#include <string>
#include <fstream>
#include <sstream>
#include <stdexcept>
#include <iomanip>


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

    inline const float& at(int r, int c) const {
        return data[r * cols + c];
    }

    // Przyk³adowa metoda: zerowanie ma³ych wartoœci
    void ZeroUntilEps(int startRow, int startCol) {
        for (int r = startRow; r < rows; ++r) {
            for (int c = startCol; c < cols; ++c) {
                if (std::abs(at(r, c)) < EPS)
                    at(r, c) = 0.0f;
            }
        }
    }



    //  Zapis macierzy do pliku
    void SaveMatrixToFile(const std::string& path) const {
        std::ofstream file(path, std::ios::trunc);
        if (!file.is_open())
            throw std::runtime_error("Nie mozna zapisac pliku: " + path);

        file << std::fixed << std::setprecision(6);

        for (int r = 0; r < rows; ++r) {
            for (int c = 0; c < cols; ++c) {
                file << data[r * cols + c];
                if (c < cols - 1)
                    file << " ";
            }
            file << "\n";
        }

        file.close();
       // std::cout << "Plik zapisano w: " << path << std::endl;
    }

    //  Pivotowanie (czêœciowe)
    void ApplyPivot(int currentRow) {
        int pivotRow = currentRow;
        float maxAbs = std::fabs(data[currentRow * cols + currentRow]);

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
                std::swap(data[currentRow * cols + j],
                    data[pivotRow * cols + j]);
            }
        }
    }
};
