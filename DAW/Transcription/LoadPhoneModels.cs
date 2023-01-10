using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;

namespace DAW.Transcription
{
    internal static class LoadPhoneModels
    {
        public static List<PhonemeModel> Load()
        {
            string path = @"Transcription\Data\";
            var files = Directory.EnumerateFiles(path);
            List<PhonemeModel> result = new List<PhonemeModel>();

            foreach (var file in files)
            {
                string fileName = Path.GetFileNameWithoutExtension(file);
                var lines = File.ReadAllLines(file);
                string name;
                int length = 30;

                List<double> doubles;
                string line;
                double v;

                for (int i = 0; i < lines.Length; i++)
                {
                    line = lines[i];
                    var splits = line.Trim().Split(" ");
                    if (splits.Length == 0)
                        continue;
                    if (lines.Length - i < length)
                        break;
                    if (splits.Length == 1)
                    {
                        name = splits[0].Trim();
                        doubles = ReadDoubles(lines, i + 1, length);
                        result.Add(new PhonemeModel(fileName, name, doubles.ToArray()));
                    }
                    i += length;
                }
            }

            return result;
        }

        static List<double> ReadDoubles(string[] lines, int index, int length)
        {
            List<double> result = new List<double>();
            double value;
            for(int i = index; i < index+length; i++)
            {
                if (double.TryParse(lines[i], out value))
                    result.Add(value);
            }

            return result;
        }
    }
}
