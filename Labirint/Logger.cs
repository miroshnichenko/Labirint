using System;
using System.IO;

namespace Blasig.Labirint.GUI
{
    internal static class SimpleLogger
    {
        public static void Log(Exception ex)
        {
            try
            {
                string path = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
                if (!path.EndsWith(@"\"))
                    path += @"\";
                path += "Labirint.log";

                using (StreamWriter writer = File.AppendText(path))
                {
                    writer.WriteLine(DateTime.Now);
                    writer.WriteLine("===================");
                    writer.WriteLine(ex);
                    writer.Flush();
                }
            }
            catch(Exception)
            {}
        }
    }
}
