using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using VerificationCode.Code;

namespace VerificationCode.Controllers
{
    public class HomeController : Controller
    {

        private VerificationCodeAESHelp _verificationCodeAESHelp;

        public HomeController(VerificationCodeAESHelp verificationCodeAESHelp)
        {
            this._verificationCodeAESHelp = verificationCodeAESHelp;
        }


        public IActionResult Index()
        {
            SlideVerifyCode();

            return View();
        }


        [HttpGet]
        [Route("/VerificationCodeImage")]
        public async Task<IActionResult> GetVerificationCodeImage()
        {
            var model = await VerificationCode.Code.VerificationCodeImage.CreateHanZi();
            var json_Model = Newtonsoft.Json.JsonConvert.SerializeObject(model.point_X_Y);
            string pointBase64str = this._verificationCodeAESHelp.AES_Encrypt_Return_Base64String(json_Model);
            this._verificationCodeAESHelp.SetCookie(VerificationCodeAESHelp._YZM, pointBase64str, 5);
            string msg = "请根据顺序点击【" + string.Join("", model.point_X_Y.Select(x => x.Word).ToList()) + "】";
            return Json(new { result = model.ImageBase64Str, msg = msg });
        }



        [Route("/verification/check")]
        [HttpPost]
        public IActionResult CheckCode(string code)
        {
            try
            {
                var pointList = new List<Point_X_Y>();
                try
                {
                    pointList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Point_X_Y>>(code);
                }
                catch (Exception)
                {
                    return Json(new { msg = "验证失败!", status = "error" });
                }

                if (pointList.Count != 2)
                    return Json(new { msg = "验证失败!", status = "error" });

                var _cookie = this._verificationCodeAESHelp.GetCookie(VerificationCodeAESHelp._YZM);

                if (string.IsNullOrEmpty(_cookie))
                    return Json(new { msg = "验证失败!", status = "error" });

                string _str = this._verificationCodeAESHelp.AES_Decrypt_Return_String(_cookie);

                var _cookiesPointList = Newtonsoft.Json.JsonConvert.DeserializeObject<List<Point_X_Y>>(_str);
                _cookiesPointList = _cookiesPointList.OrderBy(x => x.Sort).ToList();
                int i = 0;
                foreach (var item in pointList.AsParallel())
                {
                    int _x = _cookiesPointList[i]._X - item._X;
                    int _y = _cookiesPointList[i]._Y - item._Y;
                    _x = Math.Abs(_x);
                    _y = Math.Abs(_y);
                    if (_x > 25 || _y > 25)
                    {
                        return Json(new { msg = "验证失败!", status = "error" });
                    }
                    i++;
                }

                SlideVerifyCode(true);
            }
            catch (Exception)
            {
            }

            return Json(new { msg = "验证通过!", status = "ok" });
        }


        /// <summary>
        /// 登陆
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        [Route("/login")]
        [HttpPost]
        public IActionResult Login(string userName, string passWord)
        {
            var (_bool, msg) = VerifyValiate();
            if (!_bool)
            {
                return Json(new { msg = msg, status = "error" });
            }

            if (userName == "admin" && passWord == "admin")
            {
                return Json(new { msg ="登陆成功!", status = "ok" });
            }
            else
            {
                return Json(new { msg = "账号密码错误!", status = "error" });
            }
          
        }



        private (bool, string) VerifyValiate()
        {
            try
            {
                var _cookie = this._verificationCodeAESHelp.GetCookie(VerificationCodeAESHelp._SlideCode);
                if (string.IsNullOrEmpty(_cookie))
                {
                    SlideVerifyCode();
                    return (false, "请拖动滑块!");
                }

                string _str = this._verificationCodeAESHelp.AES_Decrypt_Return_String(_cookie);
                var sildeCodeModel = Newtonsoft.Json.JsonConvert.DeserializeObject<SlideVerifyCodeModel>(_str);
                if (!sildeCodeModel.SlideCode)
                    return (false, "请拖动滑块后点击汉字!");

                var _NowTime = DateTime.Now;
                var _time = sildeCodeModel.timestamp;
                var number = (_NowTime - _time).Minutes;
                if (number > 5)
                {
                    SlideVerifyCode();
                    return (false, "滑块验证码过期!");
                }
            }
            catch (Exception)
            {
                return (false, "滑动验证码失败!");
            }
            return (true, "");
        }


        private void SlideVerifyCode(bool _bool = false)
        {
            var json = Newtonsoft.Json.JsonConvert.SerializeObject(new SlideVerifyCodeModel() { SlideCode = _bool });
            string base64Str = this._verificationCodeAESHelp.AES_Encrypt_Return_Base64String(json);
            this._verificationCodeAESHelp.SetCookie(VerificationCodeAESHelp._SlideCode, base64Str, 10);

        }



    }
}
