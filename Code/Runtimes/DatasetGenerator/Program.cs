using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TiledMatrixInversion.Math;

namespace TiledMatrixInversion.Runtimes.DatasetGenerator
{
    class Program
    {
        static void Main(string[] args)
        {
            int type;
            if (!(args.Length > 0 && int.TryParse(args[0], out type)))
            {
                Console.Write("1) BTM or 2) Matrix?");
                while (!(int.TryParse(Console.ReadLine(), out type) && (type == 1 || type == 2)))
                {
                    Console.WriteLine("Invalid input, please try again... ");
                }

                Console.WriteLine();
            }

            if (type == 1)
            {
                int matrixSize;
                int minBlockSize;
                int maxBlockSize;

                if (!(args.Length > 1 && int.TryParse(args[1], out matrixSize)))
                {
                    Console.WriteLine("Enter BTM Size: ");
                    while (!(int.TryParse(Console.ReadLine(), out matrixSize) && matrixSize > 0))
                    {
                        Console.WriteLine("Invalid input, please try again... ");
                    }
                }

                if (!(args.Length > 2 && int.TryParse(args[2], out minBlockSize)))
                {
                    Console.WriteLine("Enter Min Block Size: ");
                    while (!(int.TryParse(Console.ReadLine(), out minBlockSize) && minBlockSize > 0))
                    {
                        Console.WriteLine("Invalid input, please try again... ");
                    }
                }

                if (!(args.Length > 3 && int.TryParse(args[3], out maxBlockSize)))
                {
                    Console.WriteLine("Enter Max Block Size: ");
                    while (!(int.TryParse(Console.ReadLine(), out maxBlockSize) && maxBlockSize > 0))
                    {
                        Console.WriteLine("Invalid input, please try again... ");
                    }
                }

                string filename = string.Format("ds{0}x{1}x{2}.btm", matrixSize, minBlockSize, maxBlockSize);
                var data = BlockTridiagonalMatrix<double>.CreateBlockTridiagonalMatrix<double>(matrixSize, minBlockSize, maxBlockSize, Matrix<double>.CreateNewRandomDoubleMatrix);
                BlockTridiagonalMatrix<double>.SerializeToFile(data, filename);
            }
            else
            {
                int rows;
                int cols;

                if (!(args.Length > 1 && int.TryParse(args[1], out rows)))
                {
                    Console.WriteLine("Enter Rows: ");
                    while (!(int.TryParse(Console.ReadLine(), out rows) && rows > 0))
                    {
                        Console.WriteLine("Invalid input, please try again... ");
                    }
                }

                if (!(args.Length > 2 && int.TryParse(args[2], out cols)))
                {
                    Console.WriteLine("Enter Columns: ");
                    while (!(int.TryParse(Console.ReadLine(), out cols) && cols > 0))
                    {
                        Console.WriteLine("Invalid input, please try again... ");
                    }
                }
                string x = args.Length > 3 ? "-" + args[3] : "";
                string filename = string.Format("m{0}x{1}{2}.mat", rows, cols, x);
                var data = Matrix<double>.CreateNewRandomDoubleMatrix(rows, cols);
                Matrix<double>.SerializeToFile(data, filename);
            }

            Console.WriteLine("Done");
        }
    }
}