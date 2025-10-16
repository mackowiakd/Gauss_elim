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