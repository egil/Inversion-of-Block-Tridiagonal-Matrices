using System;
using System.Collections;
using System.Diagnostics;
using TiledMatrixInversion.Math;
using System.Threading;

namespace TiledMatrixInversion.ParallelBlockMatrixInverter.OperationResults
{
    //public class OperationResult<T>
    //{
    //    private readonly bool[,] _statusTable;
    //    private long _operationsLeft;
        
    //    public OperationResult(int rows, int columns)
    //    {
    //        Init(rows, columns);
    //        _statusTable = new bool[rows, columns];
    //        Data = new Matrix<Matrix<T>>(rows, columns);            
    //    }

    //    public OperationResult(Matrix<Matrix<T>> data) : this(data, true) { }
    //    public OperationResult(Matrix<Matrix<T>> data, bool completed)
    //    {
    //        Init(data.Rows, data.Columns);
    //        Interlocked.Exchange(ref _operationsLeft, 0);
    //        _statusTable = !completed ? new bool[data.Rows,data.Columns] : null;
    //        Data = data;
    //    }

    //    void Init(int rows, int columns)
    //    {
    //        Rows = rows;
    //        Columns = columns;
    //        Interlocked.Exchange(ref _operationsLeft, rows*columns);
    //    }

    //    public bool Completed 
    //    { 
    //        get { return Interlocked.Read(ref _operationsLeft) <= 0; }            
    //    }
    //    public Matrix<Matrix<T>> Data { get; set; }

    //    public int Rows { get; private set; }
    //    public int Columns { get; private set; }

    //    /// <summary>
    //    /// Returns true if Data[i, j] is ready, otherwise false.
    //    /// </summary>
    //    /// <param name="i">One-indexed reference to the row.</param>
    //    /// <param name="j">One-indexed reference to the column.</param>
    //    /// <returns></returns>
    //    public bool this[int i, int j]
    //    {
    //        get
    //        {
    //            return Completed || _statusTable[i - 1, j - 1];
    //        }
    //        set
    //        {
    //            Debug.Assert(!_statusTable[i - 1, j - 1], "Status table already set.");
    //            Debug.Assert(value, "Trying to update status table with value false is not allowed.");

    //            // it is assumed that the status table will only be set to true once.                
    //            _statusTable[i - 1, j - 1] = value;

    //            Interlocked.Decrement(ref _operationsLeft);
    //        }
    //    }
    //}
}