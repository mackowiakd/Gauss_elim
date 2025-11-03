#include "gauss.h"

void MatrixHandler_cpp:: LoadMatrixFromFile(const std::string& path) {
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
			//std::cout << val << " ";
        }

        if (rowVals.empty()) continue;

        if (rows == 0)
            cols = static_cast<int>(rowVals.size());
        else if (rowVals.size() != static_cast<size_t>(cols))
            throw std::runtime_error("Niezgodna liczba kolumn w wierszu " + std::to_string(rows));

        values.insert(values.end(), rowVals.begin(), rowVals.end());
        rows++;
       // std:: cout<<"\n";
    }

    data = std::move(values);
    file.close();
}

//  Zapis macierzy do pliku

void MatrixHandler_cpp:: SaveMatrixToFile(const std::string& path)  {
    std::ofstream file(path, std::ios::trunc);
    if (!file.is_open())
        throw std::runtime_error("Nie mozna zapisac pliku: " + path);

    file << std::fixed << std::setprecision(4);

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

void MatrixHandler_cpp:: ZeroUntilEps(int startRow, int startCol) {
    float pivot = std::fabs(at(startRow, startCol));
    for (int r = startRow+1; r < rows; r++) {
        for (int c = startCol; c < cols; c++) {
            if (std::fabs(at(r, c)) < EPS_ABS + EPS_REL *pivot)
                at(r, c) = 0.0f;
        }
    }
}
//  Pivotowanie (czêœciowe)
void MatrixHandler_cpp:: ApplyPivot(int currentRow) {
    int pivotRow = currentRow;
    float maxAbs = std::fabs(at(currentRow, currentRow)); //aktulany pivot
    //currentRow==col
    // znajdŸ wiersz z najwiêkszym elementem w kolumnie
    for (int i = currentRow + 1; i < rows; i++) {
        float val = std::fabs(at(i,currentRow));
		
        if (val > maxAbs) {
            maxAbs = val;
            pivotRow = i;
        }
    }

    // jeœli trzeba, zamieñ wiersze
    if (pivotRow != currentRow) {
        for (int j = currentRow; j < cols; j++) {
            std::swap(data[currentRow * cols + j], data[pivotRow * cols + j]);
        }
    }
    return;
}

/* metoda dla jednowatkowego programu */
void MatrixHandler_cpp:: GaussElimination() {
   
    for (int y=0; y < cols - 1; y++) {
        
        
        ApplyPivot(y);
        pivot = at(y, y);

        if (std::fabs(pivot) > EPS) {

			
            for (int n = y; n < rows - 1; n++) // each row -> different thread
            {
                float factor = at(n + 1, y) / pivot;
                for (int j = y; j < cols; j++) {

                    at(n+1, j) -= factor * at(y, j); // factor* pivot[j]
                }
            }
        }
		//threads.join_all();
		 
         ZeroUntilEps(y, y);
         print_matrix();
        
    }

};



void MatrixHandler_cpp::print_matrix(){
    
    for (int r = 0; r < rows; r++) {
        for (int y = 0; y < cols - 1; y++) {
            std::cout << at(r, y) << " ";
        }
        std::cout << "\n";
    }
    std::cout << "\n";

}
/*dla wielowatkowej postaci */
void MatrixHandler_cpp::GaussEliminationStep(int pivotRow, int y) {
    if (std::fabs(pivot) > EPS) {

       
		float factor = at(pivotRow + 1, y) / at(y, y); //pivot = at(y,y);
        for (int j = y; j < cols; j++) {

            at(pivotRow + 1, j) -= factor * at(y, j); // factor* pivot[j]
        }
        
    }
}



