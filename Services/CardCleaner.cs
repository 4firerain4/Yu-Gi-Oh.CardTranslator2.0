using System.Drawing;
using Emgu.CV;
using Emgu.CV.CvEnum;
using Emgu.CV.Structure;
using Yu_Gi_Oh.CardTranslator.Model;


namespace Yu_Gi_Oh.CardTranslator.Services
{
    public class CardCleaner
    {
        public static readonly string CacheDir = AppDomain.CurrentDomain.BaseDirectory + "Cache";
        public static void Clean(Card card)
        {
            var imgPath = CacheDir + "/SavedImages/" + card.Code + ".jpg";
            Mat image = CvInvoke.Imread(imgPath, ImreadModes.Color);
            List<(int, int, int, int)> cord = new List<(int, int, int, int)>
            {
                (58, 65, 612, 60), //Для удаления имени
                (32, 1129, 750, 22), //Кординаты нижней подписи
            };

            cord = AnalizeType(card, cord);

            Mat processedImage = image.Clone();


            foreach (var c in cord)
            {
                processedImage = ApplyBlur(RemoveText(processedImage, c), c);
            }

            processedImage.Save(CacheDir + "\\ProcessedImages\\" + card.Code + ".jpg");

        }

        static Mat RemoveText(Mat image, (int, int, int, int) tuple)
        {
            Mat mask = new Mat(image.Size, DepthType.Cv8U, 1);
            mask.SetTo(new MCvScalar(0));

            Rectangle textArea = new Rectangle(tuple.Item1, tuple.Item2, tuple.Item3, tuple.Item4);
            CvInvoke.Rectangle(mask, textArea, new MCvScalar(255), -1);

            Mat result = new Mat();
            CvInvoke.Inpaint(image, mask, result, 4, InpaintType.Telea);

            return result;
        }

        static Mat ApplyBlur(Mat image, (int, int, int, int) tuple)
        {
            Rectangle blurRect = new Rectangle(tuple.Item1, tuple.Item2, tuple.Item3 - 2, tuple.Item4 - 6);

            Mat roi = new Mat(image, blurRect);

            CvInvoke.GaussianBlur(roi, roi, new Size(21, 21), 0, 0, BorderType.Replicate);

            roi.CopyTo(new Mat(image, blurRect));

            return image;
        }

        private static List<(int, int, int, int)> AnalizeType(Card card, List<(int, int, int, int)> cord)
        {
            if (card.Types == "Continuous"
                || card.Types == "Equip"
                || card.Types == "Counter"
                || card.Types == "Field"
                || card.Types == "Quick-Play"
                || card.Types == "Ritual")
            {
                cord.Add((399, 143, 270, 53)); //Уладаление левой части типа ловушки/заклинания
                cord.Add((717, 143, 30, 53)); //Уладаление правой части типа ловушки/заклинания
                cord.Add((59, 896, 689, 185)); // Удаление описания заклинаний/ловушек
                return cord;
            }
            else if (card.Type.Contains("Card"))
            {
                cord.Add((399, 143, 340, 53)); // Удаление типа обычных ловушек/заклинаний
                cord.Add((59, 896, 689, 185)); // Удаление описания заклинаний/ловушек
                return cord;
            }
            cord.Add((59, 896, 689, 177)); //Для удаления описания)
            return cord;
        }
    }
}