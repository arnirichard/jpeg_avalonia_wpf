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

                float[][] doubles;
                string line;
                double v;

                //for (int i = 0; i < lines.Length; i++)
                //{
                //    line = lines[i];
                //    var splits = line.Trim().Split(" ");
                //    if (splits.Length == 0)
                //        continue;
                //    if (lines.Length - i < length)
                //        break;
                //    if (splits.Length == 1)
                //    {
                name = Path.GetFileNameWithoutExtension(fileName);
                doubles = ReadDoubles(lines);
                //List<PhonemeSample> phonemes = new List<PhonemeSample>();
                //foreach (var list in doubles)
                //   phonemes.Add(new PhonemeSample(list));
                //PhonemeModel model = new PhonemeModel(fileName, name, doubles.ToArray());
                PhonemeModel? model = PhonemeModel.FromData(name, doubles.ToArray());
                if(model != null)
                    PhonemeModel.Models.Add(model);
                result.Add(model);
                //    }
                //    i += length;
                //}
            }

            return result;
        }

        static float[][] ReadDoubles(string[] lines)
        {
            List<float[]> result = new List<float[]>();
             
            float value;
            List<float> list = new List<float>();

            for(int i = 0; i < lines.Length; i++)
            {

                list.Clear();
                string[] splits = lines[i].Split(',');
                for(int j = 0; j < splits.Length; j++)
                {
                    if (float.TryParse(splits[j], out value))
                        list.Add(value);
                }
                result.Add(list.ToArray());
            }

            return result.ToArray();
        }
    }
}
