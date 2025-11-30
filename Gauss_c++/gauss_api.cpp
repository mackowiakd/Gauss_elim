#include "gauss.h"



extern "C" {

    __declspec(dllexport)
    void start_gauss(const char* input_path, const char* output_path, const char* outp_slnVec) {
        MatrixHandler_cpp matrix(input_path);
        matrix.GaussElimination(); //to do threading
        matrix.BackSubstitution();
        matrix.SaveMatrixToFile(output_path);

		matrix.SaveSlnMtrx(outp_slnVec);
    } //do zmiany na pojedyncze funkcje

    __declspec(dllexport)
        MatrixHandler_cpp* create_matrix(const char* path) {
        return new MatrixHandler_cpp(path);
    }

    __declspec(dllexport)
        void destroy_matrix(MatrixHandler_cpp* ptr) {
        delete ptr;
    }
    __declspec(dllexport)
        void apply_pivot(MatrixHandler_cpp* ptr, int currentRow) {
        ptr->ApplyPivot(currentRow);
    }
    __declspec(dllexport)
        int get_rows(MatrixHandler_cpp* ptr) {
        return ptr->rows;
    };
    __declspec(dllexport)
        int get_cols(MatrixHandler_cpp* ptr) {
        return ptr->cols;
    };
    __declspec(dllexport)
        float get_data_at(MatrixHandler_cpp* ptr, int r, int c) {
        return ptr->at(r, c);
    };
    __declspec(dllexport)
		float get_eps_abs(MatrixHandler_cpp* ptr) {     
        return ptr->EPS_ABS;
	};
    __declspec(dllexport)
        float get_eps_rel(MatrixHandler_cpp* ptr) {
        return ptr->EPS_REL;
    };
    __declspec(dllexport)
        void zero_until_eps(MatrixHandler_cpp* ptr, int startRow, int startCol) {
        ptr->ZeroUntilEps(startRow, startCol);
    };

    __declspec(dllexport)
        void gauss_step(MatrixHandler_cpp* ptr, int pivotRow, int y) {
       
        ptr->GaussEliminationStep(pivotRow, y);
    
    }
    __declspec(dllexport)
        void back_substitution(MatrixHandler_cpp* ptr) {
        ptr->BackSubstitution();
	}
    __declspec(dllexport)
        void save_matrix(MatrixHandler_cpp* ptr, const char* path) {
        ptr->SaveMatrixToFile(path);
    }
    __declspec(dllexport)
        void save_result(MatrixHandler_cpp* ptr, const char* path) {
        ptr->SaveSlnMtrx(path);
    }

};