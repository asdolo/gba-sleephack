namespace GBA.Sleephack;

public class EmptyDataAddressFinder
{
    private readonly byte[] _data;

    public static EmptyDataAddressFinder For(byte[] data)
    {
        return new EmptyDataAddressFinder(data);
    }
    
    private EmptyDataAddressFinder(byte[] data)
    {
        _data = data;
    }
    
    public int FindWithSize(int freeSpaceSize)
    {
        var initialIndex = _data.Length - freeSpaceSize;

        // Align to 32 bits
        initialIndex = (initialIndex >> 4) << 4;
        
        for (var i = initialIndex; i >= 0; i -= 0x10)
        {
            if (!IsCleanData(GetSubArray(_data, i, freeSpaceSize), i, freeSpaceSize)) continue;
            
            return i;
        }

        // Can't find anything
        throw new Exception($"Could not find an address to fit {freeSpaceSize} bytes");
    }

    private static bool IsCleanData(byte[] data, int i, int size)
    {
        var entropy = CalculateEntropy(data);
        return entropy == 0;
    }

    private static double CalculateEntropy(byte[] data)
    {
        double entropy = 0;
        var byteCount = new long[byte.MaxValue + 1];

        foreach (var b in data)
            byteCount[b]++;
        
        foreach (var count in byteCount)
        {
            var probability =  (double)count / data.Length;

            if (probability > 0)
                entropy -= probability * Math.Log2(probability);
        }
        return entropy / 8;
    }

    private static byte[] GetSubArray(byte[] data, long index, long length)
    {
        byte[] result = new byte[length];
        Array.Copy(data, index, result, 0, length);
        return result;
    }

}