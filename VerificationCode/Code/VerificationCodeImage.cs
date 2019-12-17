using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace VerificationCode.Code
{
    public class VerificationCodeImage
    {

        /// <summary>  
        /// 随机汉字
        /// </summary>  
        /// <param name="number"></param>  
        /// <returns></returns>  
        private static string RandomHanZi(int number)
        {
            var str = "天地玄黄宇宙洪荒日月盈昃辰宿列张寒来暑往秋收冬藏闰余成岁律吕调阳云腾致雨露结为霜金生丽水玉出昆冈剑号巨阙珠称夜光果珍李柰菜重芥姜海咸河淡鳞潜羽翔龙师火帝鸟官人皇始制文字乃服衣裳推位让国有虞陶唐吊民伐罪周发殷汤坐朝问道垂拱平章爱育黎首臣伏戎羌遐迩体率宾归王";
            char[] str_char_arrary = str.ToArray();
            Random rand = new Random();
            HashSet<string> hs = new HashSet<string>();
            bool randomBool = true;
            while (randomBool)
            {
                if (hs.Count == number)
                    break;
                int rand_number = rand.Next(str_char_arrary.Length);
                hs.Add(str_char_arrary[rand_number].ToString());
            }
            string code = string.Join("", hs);
            return code;
        }

        /// <summary>  
        /// </summary>  
        /// <param name="numbers">生成位数（默认5位）</param>  
        /// <param name="_height">图片高度</param>  
        /// <param name="_width">图片宽度</param>  
        public static Task<VerificationCodeModel> CreateHanZi(int numbers = 5, int _height = 200, int _width = 200)
        {
            var imageModel = new VerificationCodeModel();
            string code = RandomHanZi(numbers);
            Bitmap Img = null;
            Graphics g = null;
            MemoryStream ms = null;
            Random random = new Random();

            Color[] color_Array = { Color.Black, Color.DarkBlue, Color.Green, Color.Orange, Color.Brown, Color.DarkCyan, Color.Purple };
            string[] fonts = { "lnk Free", "Segoe Print", "Comic Sans MS", "MV Boli", "华文行楷" };
            string _base = Environment.CurrentDirectory + "\\wwwroot\\verificationcodeImage\\";

            var _file_List = System.IO.Directory.GetFiles(_base);
            int imageCount = _file_List.Length;
            if (imageCount == 0)
                throw new Exception("image not Null");

            int imageRandom = random.Next(1, (imageCount + 1));
            string _random_file_image = _file_List[imageRandom - 1];
            var imageStream = Image.FromFile(_random_file_image);

            Img = new Bitmap(imageStream, _width, _height);
            imageStream.Dispose();
            g = Graphics.FromImage(Img);
            Color[] penColor = { Color.LightGray, Color.Green, Color.Blue };
            int code_length = code.Length;
            for (int i = 0; i < code_length; i++)
            {
                int cindex = random.Next(color_Array.Length);
                int findex = random.Next(fonts.Length);
                Font f = new Font(fonts[findex], 15, FontStyle.Bold);
                Brush b = new SolidBrush(color_Array[cindex]);
                int _y = random.Next(_height);
                if (_y > (_height - 30))
                    _y = _y - 60;

                int _x = _width / (i + 1);
                if ((_width - _x) < 50)
                {
                    _x = _width - 60;
                }
                string word = code.Substring(i, 1);
                if (imageModel.point_X_Y.Count < 2)
                {
                    imageModel.point_X_Y.Add(new Point_X_Y()
                    {
                        Word = word,
                        _X = _x,
                        _Y = _y,
                        Sort = i
                    });
                }
                g.DrawString(word, f, b, _x, _y);
            }
            ms = new MemoryStream();
            Img.Save(ms, ImageFormat.Jpeg);
            g.Dispose();
            Img.Dispose();
            ms.Dispose();
            imageModel.ImageBase64Str = "data:image/jpg;base64," + Convert.ToBase64String(ms.GetBuffer());
            return Task.FromResult(imageModel);
        }



    }


    public class Point_X_Y
    {
        public int _X { get; set; }

        public int _Y { get; set; }

        public int Sort { get; set; }

        public string Word { get; set; }

    }

    /// <summary>
    /// 滑动校验
    /// </summary>
    public class SlideVerifyCodeModel
    {
        public bool SlideCode { get; set; } = false;

        public DateTime timestamp { get; set; } = DateTime.Now;

    }


    public class VerificationCodeModel
    {
        public string ImageBase64Str { get; set; } = "";

        public List<Point_X_Y> point_X_Y { get; set; } = new List<Point_X_Y>();

    }

}
