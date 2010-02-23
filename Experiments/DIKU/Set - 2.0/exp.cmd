@echo.
@echo Running experiments in batch mode
@FOR /L %%b IN (30,10,110) DO Experiments-static.exe 68 4 m2000x2000-a.mat m2000x2000-b.mat %%b & move result-m2000x2000-a.mat-Parallel.Inverse.csv result-m2000x2000-a.mat-Parallel.Inverse-ts-%%b.csv
@FOR /L %%b IN (30,10,110) DO Experiments-static.exe 80 4 m2000x2000-a.mat m2000x2000-b.mat %%b & move result-m2000x2000-a.mat-SingleThreadedTiled.Inverse.csv result-m2000x2000-a.mat-SingleThreadedTiled.Inverse-ts-%%b.csv
@echo.
@FOR /L %%b IN (30,10,110) DO Experiments-static.exe 68 64 m2000x2000-a.mat m2000x2000-b.mat %%b & move result-m2000x2000-a.mat-MinusMatrixInverseMatrixMultiply.Inverse.csv result-m2000x2000-a.mat-Parallel.MinusMatrixInverseMatrixMultiply-ts-%%b.csv
@FOR /L %%b IN (30,10,110) DO Experiments-static.exe 80 64 m2000x2000-a.mat m2000x2000-b.mat %%b & move result-m2000x2000-a.mat-SingleThreadedTiled.MinusMatrixInverseMatrixMultiply.csv result-m2000x2000-a.mat-SingleThreadedTiled.MinusMatrixInverseMatrixMultiply-ts-%%b.csv