#include "gauss.h"

extern "C" {

    void start_gauss(const char* input_path, const char* output_path) {
        MatrixHandler matrix(input_path);
        matrix.GaussElimination(); //to do threading
        matrix.SaveMatrixToFile(output_path);
    } //do zmiany na pojedyncze funkcje

    __declspec(dllexport)
        MatrixHandler* create_matrix(const char* path) {
        return new MatrixHandler(path);
    }

    __declspec(dllexport)
        void destroy_matrix(MatrixHandler* ptr) {
        delete ptr;
    }
    __declspec(dllexport)
        void apply_pivot(MatrixHandler* ptr, int currentRow) {
        ptr->ApplyPivot(currentRow);
    }
    __declspec(dllexport)
        int get_rows(MatrixHandler* ptr) {
        return ptr->rows;
    };
    __declspec(dllexport)
        int get_cols(MatrixHandler* ptr) {
        return ptr->cols;
    };
    __declspec(dllexport)
        void zero_until_eps(MatrixHandler* ptr, int startRow, int startCol) {
        ptr->ZeroUntilEps(startRow, startCol);
    };

    __declspec(dllexport)
        void gauss_step(MatrixHandler* ptr, int nRow, int nCol) {
        //for (int y = 0; y < ptr->cols - 1; y++) {

            //ptr->ApplyPivot(y);
            //ptr->set_pivot(ptr->at(y, y));
            //threading start for n= y+1 to rows- 
            //for (int n = y; n < ptr->rows - 1; n++) // each row -> different thread
        ptr->GaussEliminationStep(nRow, nCol);
        //threads.join_all();

       // ptr->ZeroUntilEps(y, y);


    }


    __declspec(dllexport)
        void save_matrix(MatrixHandler* ptr, const char* path) {
        ptr->SaveMatrixToFile(path);
    }

};