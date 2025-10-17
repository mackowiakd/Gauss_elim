#include "gauss.h"

void MatrixHandler:: LoadMatrixFromFile(const std::string& path) {
    std::ifstream file(path);
    if (!file.is_open())
        throw std::runtime_error("Nie mozna otworzyc pliku: " + path);

    std::string line;
    std::vector<float> values;
    rows = 0;
    cols = 0;

    while (std::getline(file, line)) {
        std::istringstream iss(line);
        std::vector<float> rowVals;
        float val;
        while (iss >> val) {
            rowVals.push_back(val);
        }

        if (rowVals.empty()) continue;

        if (rows == 0)
            cols = static_cast<int>(rowVals.size());
        else if (rowVals.size() != static_cast<size_t>(cols))
            throw std::runtime_error("Niezgodna liczba kolumn w wierszu " + std::to_string(rows));

        values.insert(values.end(), rowVals.begin(), rowVals.end());
        rows++;
    }

    data = std::move(values);
    file.close();
}

//  Zapis macierzy do pliku
void MatrixHandler:: SaveMatrixToFile(const std::string& path)  {
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
void MatrixHandler:: GaussElimination() {
    int y = 0;
    int n = 0;

    for (y; y < rows - 1; y++) {

        if (ApplyPivot(y) > EPS) {

            float factor = at(n + 1, y) / at(n, y);

            for (int n = y; n < rows - 1; n++) {
                for (int j = 0; j < cols; j++) {

                    at(n+1, j) -= factor * at(n, j);
                }
            }
        }

        else {
            ZeroUntilEps(y, y);
        }
    }

};

extern "C" __declspec(dllexport)
void start_gauss(const char* input_path, const char* output_path) {
    MatrixHandler matrix(input_path);
    matrix.GaussElimination();
    matrix.SaveMatrixToFile(output_path);
}