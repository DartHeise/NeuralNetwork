using System.Drawing;

class Program
{
    static void SaveResults(string path, List<Bitmap> bitmaps)
    {
        int index = 0;
        foreach (Bitmap bitmap in bitmaps)
        {
            bitmap.Save(path + $"image{index++}.png");
        }
    }

    static float FindBrightnessValues(Bitmap bitmap, float[] brightnessValues)
    {
        float finalBrightnessValue = 0;
        for (int i = 0; i < bitmap.Height; i++)
        {
            for (int j = 0; j < bitmap.Width; j++)
            {
                var pixel = bitmap.GetPixel(j, i);
                brightnessValues[i] += (float)(0.212 * pixel.R + 0.715 * pixel.G + 0.072 * pixel.B);
            }
            brightnessValues[i] /= bitmap.Width;
            finalBrightnessValue += brightnessValues[i];
        }
        return (finalBrightnessValue / bitmap.Height);
    }

    static Dictionary<int, int> FillDictionary(Bitmap bitmap, float[] brightnessValues, float upperBound, float bottomBound)
    {
        Dictionary<int, int> Bounds = new Dictionary<int, int>();
        bool isUpperBoundStarted = false;
        int key = 0;
        for (int i = 2; i < bitmap.Height - 3; i++)
        {

            if (!isUpperBoundStarted
             && (brightnessValues[i - 2] > upperBound)
             && (brightnessValues[i - 1] > upperBound)
             && (brightnessValues[i] < bottomBound)
             && (brightnessValues[i + 1] < bottomBound)
             && (brightnessValues[i + 2] < bottomBound)
             && (brightnessValues[i + 3] < bottomBound))
            {
                key = i;
                isUpperBoundStarted = true;
            }

            if ((isUpperBoundStarted
              && brightnessValues[i] < upperBound
              && brightnessValues[i + 1] > bottomBound)
              || (isUpperBoundStarted
              && brightnessValues[i + 1] > bottomBound
              && brightnessValues[i + 2] > bottomBound
              && brightnessValues[i + 3] > bottomBound))
            {
                Bounds.Add(key, i);
                isUpperBoundStarted = false;
            }
        }
        return Bounds;
    }

    static int FindMinHeight(Dictionary<int, int> Bounds)
    {
        int minHeight = 0;
        bool firstElement = true;
        foreach (var bound in Bounds)
        {
            if (firstElement)
            {
                minHeight = bound.Value - bound.Key;
                firstElement = false;
            }
            else
            {
                if (minHeight > (bound.Value - bound.Key))
                    minHeight = bound.Value - bound.Key;
            }
        }
        return minHeight;
    }
    static void Main(string[] args)
    {
        Bitmap bitmap = new Bitmap(@"C:\Users\Иван\Desktop\input.png");

        float[] brightnessValues = new float[bitmap.Height];
        float finalBrightnessValue = FindBrightnessValues(bitmap, brightnessValues);

        float upperBound = finalBrightnessValue;
        float bottomBound = finalBrightnessValue;
        Dictionary<int, int> Bounds = FillDictionary(bitmap, brightnessValues, upperBound, bottomBound);


        int minHeight = FindMinHeight(Bounds);

        var lines = new List<Bitmap>();
        int deltaHeight = (int)(minHeight * 0.7);
        foreach (var bound in Bounds)
        {
            int lineUpperBound = bound.Key - deltaHeight;
            if (lineUpperBound < 0)
                lineUpperBound = 0;
            int lineBottomBound = bound.Value + deltaHeight;
            if (lineBottomBound > bitmap.Height)
                lineBottomBound = bitmap.Height;
            Bitmap line = new Bitmap(bitmap.Width, lineBottomBound - lineUpperBound);

            for (int i = 0; i < line.Height; i++)
            {
                for (int j = 0; j < line.Width; j++)
                {
                    line.SetPixel(j, i, bitmap.GetPixel(j, lineUpperBound + i));
                }
            }
            lines.Add(line);
        }

        SaveResults(@"C:\Users\Иван\Desktop\lines\", lines);

        var linesWithIncreasedConstrast = new List<Bitmap>(lines.Count);
        var blackPointsDictionaries = new Dictionary<Bitmap, List<(int, int)>>();
        foreach (Bitmap line in lines)
        {
            List<(int, int)> blackPixelsList = new List<(int, int)>();
            linesWithIncreasedConstrast.Add(new Bitmap(line.Width, line.Height));
            for (int i = 0; i < line.Height; i++)
            {
                for (int j = 0; j < line.Width; j++)
                {
                    Color color = line.GetPixel(j, i);
                    if (0.212 * color.R + 0.715 * color.G + 0.072 * color.B > 128)
                        linesWithIncreasedConstrast[linesWithIncreasedConstrast.Count - 1].SetPixel(j, i, Color.White);
                    else
                    {
                        linesWithIncreasedConstrast[linesWithIncreasedConstrast.Count - 1].SetPixel(j, i, Color.Black);
                        blackPixelsList.Add((j, i));
                    }
                }
            }
            blackPointsDictionaries.Add(linesWithIncreasedConstrast.Last(), blackPixelsList);
        }

        SaveResults(@"C:\Users\Иван\Desktop\modifiedLines\", linesWithIncreasedConstrast);

        foreach (var dic in blackPointsDictionaries)
        {
            for (int i = 0; i < dic.Value.Count; i++)
            {
                int weight = dic.Value[i].Item1;
                int height = dic.Value[i].Item2;

                for (int j = 0; j < dic.Key.Height; j++)
                {
                    if (dic.Key.Height > height + j)
                    {
                        dic.Key.SetPixel(weight, height + j, Color.Black);
                        dic.Key.SetPixel(weight - 1, height + j, Color.Black);
                        dic.Key.SetPixel(weight + 1, height + j, Color.Black);
                        dic.Key.SetPixel(weight - 2, height + j, Color.Black);
                        dic.Key.SetPixel(weight + 2, height + j, Color.Black);
                    }
                    if (height - j >= 0)
                    {
                        dic.Key.SetPixel(weight, height - j, Color.Black);
                        dic.Key.SetPixel(weight - 1, height - j, Color.Black);
                        dic.Key.SetPixel(weight + 1, height - j, Color.Black);
                        dic.Key.SetPixel(weight - 2, height - j, Color.Black);
                        dic.Key.SetPixel(weight + 2, height - j, Color.Black);
                    }
                }
            }

        }

        SaveResults(@"C:\Users\Иван\Desktop\modifiedV2\", linesWithIncreasedConstrast);

        int index = 1;
        int j2 = 0;
        var BrightnessMassive = new List<float>();
        foreach (Bitmap line in linesWithIncreasedConstrast)
        {
            var wordsList = new List<Bitmap>();
            float[] brightnessValuesForWords = new float[line.Width];
            float finalBrightnessValueForWords = 0;
            for (int i = 0; i < line.Width; i++)
            {
                for (int j = 0; j < line.Height; j++)
                {
                    var pixel = line.GetPixel(i, j);
                    brightnessValuesForWords[i] += (float)(0.212 * pixel.R + 0.715 * pixel.G + 0.072 * pixel.B);
                }
                brightnessValuesForWords[i] /= line.Height;
                finalBrightnessValueForWords += brightnessValuesForWords[i];
            }
            finalBrightnessValueForWords /= line.Width;
            BrightnessMassive.Add(finalBrightnessValueForWords);

            float leftBound = finalBrightnessValueForWords;
            float rightBound = finalBrightnessValueForWords;

            bool isLeftBoundStarted = false;
            var WordBounds = new Dictionary<int, int>();
            int Wordkey = 0;

            for (int i = 2; i < line.Width - 4; i++)
            {

                if (!isLeftBoundStarted
                 && (brightnessValuesForWords[i - 1] > leftBound)
                 && (brightnessValuesForWords[i] < leftBound)
                 && (brightnessValuesForWords[i + 1] < leftBound))
                {
                    Wordkey = i;
                    isLeftBoundStarted = true;
                }

                if (isLeftBoundStarted
                  && brightnessValuesForWords[i - 2] < rightBound
                  && brightnessValuesForWords[i - 1] < rightBound
                  && brightnessValuesForWords[i] > rightBound
                  && brightnessValuesForWords[i + 1] > rightBound
                  && brightnessValuesForWords[i + 2] > rightBound
                  && brightnessValuesForWords[i + 3] > rightBound
                  && brightnessValuesForWords[i + 4] > rightBound)
                {
                    WordBounds.Add(Wordkey, i);
                    isLeftBoundStarted = false;
                }
            }

            var words = new List<Bitmap>();
            foreach (var bound in WordBounds)
            {
                int lineLeftBound = bound.Key;
                int lineRightBound = bound.Value;
                Bitmap word = new Bitmap(lineRightBound - lineLeftBound, line.Height);

                for (int i = 0; i < word.Height; i++)
                {
                    for (int j = 0; j < word.Width; j++)
                    {
                        word.SetPixel(j, i, lines[j2].GetPixel(lineLeftBound + j, i));
                    }
                }
                wordsList.Add(word);
            }
            Directory.CreateDirectory(@$"C:\Users\Иван\Desktop\words\line{index}");
            SaveResults(@$"C:\Users\Иван\Desktop\words\line{index++}\", wordsList);
            j2++;
        }

        //string[] files = Directory.GetFiles(@"C:\Users\Иван\Desktop\test");
        //List<int[,]> pixelMassivesList = new List<int[,]>();
        //for (int i = 0; i < files.Length; i++)
        //{
        //    pixelMassivesList.Add(getPixelMassives(files[i]));
        //}
    }

    //static int[,] getPixelMassives(string path)
    //{
    //    Bitmap bitmap = new Bitmap(path);
    //    int[,] pixelMassive = new int[bitmap.Height, bitmap.Width];
    //    for (int i = 0; i < bitmap.Height; i++)
    //    {
    //        for (int j = 0; j < bitmap.Width; j++)
    //        {
    //            Color color = bitmap.GetPixel(j, i);
    //            if (0.212 * color.R + 0.715 * color.G + 0.072 * color.B > 128)
    //                pixelMassive[j, i] = 1;
    //            else
    //                pixelMassive[j, i] = 0;
    //        }
    //    }
    //    return pixelMassive;
    //}
}
