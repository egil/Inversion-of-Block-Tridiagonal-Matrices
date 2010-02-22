namespace TiledMatrixInversion.Resources
{
    public static class Helpers
    {
        public static T[][] Init<T>(int rows, int columns)
        {
            var jaggedArray = new T[rows][];
            for (int i = 0; i < rows; i++)
            {
                jaggedArray[i] = new T[columns];
            }
            return jaggedArray;
        }
    }
}