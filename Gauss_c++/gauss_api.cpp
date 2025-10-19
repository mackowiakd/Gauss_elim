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

   /* __declspec(dllexport)
        void gauss_step(MatrixHandler* ptr, int pivotRow) {
        ptr->GaussEliminationStep(pivotRow);
    }*/

    __declspec(dllexport)
        void save_matrix(MatrixHandler* ptr, const char* path) {
        ptr->SaveMatrixToFile(path);
    }

}