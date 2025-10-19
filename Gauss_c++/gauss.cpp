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
			std::cout << val << " ";
        }

        if (rowVals.empty()) continue;

        if (rows == 0)
            cols = static_cast<int>(rowVals.size());
        else if (rowVals.size() != static_cast<size_t>(cols))
            throw std::runtime_error("Niezgodna liczba kolumn w wierszu " + std::to_string(rows));

        values.insert(values.end(), rowVals.begin(), rowVals.end());
        rows++;
        std:: cout<<"\n";
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

void MatrixHandler:: ZeroUntilEps(int startRow, int startCol) {
    for (int r = startRow+1; r < rows; r++) {
        for (int c = startCol; c < cols; c++) {
            if (std::fabs(at(r, c)) <= EPS)
                at(r, c) = 0.0f;
        }
    }
}
//  Pivotowanie (czêœciowe)
void MatrixHandler:: ApplyPivot(int currentRow) {
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


void MatrixHandler:: GaussElimination() {
   
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



void MatrixHandler::print_matrix(){
    
    for (int r = 0; r < rows; r++) {
        for (int y = 0; y < cols - 1; y++) {
            std::cout << at(r, y) << " ";
        }
        std::cout << "\n";
    }
    std::cout << "\n";

}

void MatrixHandler::GaussElimination_oneTask()  //=> ten kod muis byc w klasie ParallelExecution
{
    for (int y = 0; y < cols - 1; y++) {


        ApplyPivot(y);
        pivot = at(y, y);
        //threading start for n= y+1 to rows-1
        // gauss elimination(n)
        //threads.join_all();

        ZeroUntilEps(y, y);
        print_matrix();
    }
}

extern "C" __declspec(dllexport)
void start_gauss(const char* input_path, const char* output_path) {
    MatrixHandler matrix(input_path);
    matrix.GaussElimination();
    matrix.SaveMatrixToFile(output_path);
}