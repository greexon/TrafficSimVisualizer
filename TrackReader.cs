using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.IO;
using System.Globalization;

namespace Visualizer
{
    class TrackReader
    {

        public List<KeyValuePair<double, double>> ReadData()
        {
            List<KeyValuePair<double, double>> coord = new List<KeyValuePair<double,double>>();
            CultureInfo cultureInfo = new CultureInfo("en-US");
            using (StreamReader streamReader = new StreamReader(Common.FIN))
            {
                string line = streamReader.ReadLine();
                int.TryParse(line, out Common.CARS_COUNT);
                while ((line = streamReader.ReadLine()) != null)
                {
                    string[] words = line.Split(' ');
                    double xCoord;
                    double yCoord;
                    double.TryParse(words[0], NumberStyles.AllowDecimalPoint, cultureInfo, out xCoord);
                    double.TryParse(words[1], NumberStyles.AllowDecimalPoint, cultureInfo, out yCoord);
                    coord.Add(new KeyValuePair<double, double>(xCoord, yCoord));
                }
            }
            return coord;
        }
    }
}
