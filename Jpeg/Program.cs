// See https://aka.ms/new-console-template for more information

using System;
using JpegLib;
using SkiaSharp;

namespace Jpeg
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            if(args.Length != 3 ||
                (args[0] != "-d" && args[0] != "-e"))
            {
                Console.WriteLine("Usage: TestConsole -d/e fileNameIn fileNameOut (encode bmp only, decode jpg only)");
                return;
            }

            try
            {
                if (args[0] == "-d")
                {
                    await JpegDecoder.Decode(args[1], args[2]);
                }
                else
                {
                    JpegEncoder.Encode(args[1], args[2]);
                }
            }
            catch(Exception ex) 
            {
                Console.WriteLine("Error: "+ex.Message);
            }
        }
    }
} 