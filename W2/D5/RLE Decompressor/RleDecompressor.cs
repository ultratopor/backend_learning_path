using System;
using System.Buffers;
using System.Collections.Generic;
using System.Text;

namespace RLE_Decompressor
{
    internal static class RleDecompressor
    {
        public static string DecomressToString(ReadOnlySpan<char> compressed)
        {
            var decompressedSize = CalculateDecompressedSize(compressed);
            char[] buffer = ArrayPool<char>.Shared.Rent(decompressedSize);

            try
            {
                Decompress(compressed, buffer.AsSpan().Slice(0, decompressedSize));
                return new string(buffer, 0, decompressedSize);
            }
            finally
            {
                ArrayPool<char>.Shared.Return(buffer);
            }
        }

        private static int CalculateDecompressedSize(ReadOnlySpan<char> input)
        {
            var totalSize = 0;

            for(int i = 1; i < input.Length; i++) // начинаем с одного, потому что первый символ всегда буква
            {
                var count = 0;
                while(i < input.Length && char.IsDigit(input[i]))
                {
                    count = count * 10 + (input[i] - '0');
                    i++;
                }
                
                totalSize += count;
            }

            return totalSize;
        }

        private static void Decompress(ReadOnlySpan<char> input, Span<char> output)
        {
            var inputIndex = 0;
            var outputIndex = 0;
            while(inputIndex < input.Length)
            {
                var currentChar = input[inputIndex++];
                var count = 0;
                
                while(inputIndex < input.Length && char.IsDigit(input[inputIndex]))
                {
                    count = count * 10 + (input[inputIndex] - '0');
                    inputIndex++;
                }

                output.Slice(outputIndex, count).Fill(currentChar);
                outputIndex += count;
            }
        }
    }
}
