using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdapterLogsSorter
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                new Sorter().SortAndThrowUp(args[0], args[1]);
            }
            catch (Exception)
            {

                throw;
            }
        }
    }

    public class Sorter
    {
        public void SortAndThrowUp(string folder, string fileRegex)
        {
            var contents = new SortedDictionary<DateTime, List<string>>();
            var currentLinesBuffer = new List<string>();
            DateTime currentDate = new DateTime();

            var directories = Directory.EnumerateDirectories(folder, "*", SearchOption.AllDirectories);

            foreach (var file in directories.SelectMany(d=> Directory.EnumerateFiles(d, fileRegex)))
            {
                //Console.WriteLine("Starting analysis for files: {0}", file);
                using (FileStream f = File.OpenRead(file))
                {
                    using (var reader = new StreamReader(f))
                    {
                        while (!reader.EndOfStream)
                        {
                            var line = reader.ReadLine();

                            //"2014-11-19 09:44:55,398"
                            DateTime time;
                            if ((line.Length > 23 && DateTime.TryParseExact(line.Substring(0, 23), "yyyy-MM-dd HH:mm:ss,fff",
                                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time))
                                ||
                                (line.Length > 30 && DateTime.TryParseExact(line.Substring(4, 26), "yyyy-MM-ddTHH:mm:ss.ffffff",
                                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out time)))
                            {
                                FlushToCache(currentLinesBuffer, contents, currentDate);
                                currentDate = time;
                            }

                            currentLinesBuffer.Add(line);
                        }
                    }
                }
            }
            FlushToCache(currentLinesBuffer, contents, currentDate);

            foreach (var entry in contents)
            {
                var firstLine = entry.Value.First();

                if (!firstLine.Contains("user_id:224112") &&
                    !firstLine.Contains("user_id:none"))
                    continue;

                //if (!entry.Value.Any(l => l.Contains("22 Sherbrook Rd")))
                //    continue;

                foreach (var line in entry.Value)
                {
                    Console.WriteLine(line);
                }
            }
        }

        private static void FlushToCache(List<string> currentLinesBuffer, SortedDictionary<DateTime, List<string>> contents, DateTime currentDate)
        {
            if (currentLinesBuffer.Count > 0)
            {
                if (contents.ContainsKey(currentDate))
                    contents[currentDate].AddRange(currentLinesBuffer);

                contents[currentDate] = currentLinesBuffer.ToList();

                currentLinesBuffer.Clear();
            }
        }
    }
}
