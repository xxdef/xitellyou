using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;

namespace xitellyou
{
    /// <summary>
    /// msdn.itellyou.cn 数据抓取
    /// author : liaowuping.
    /// date   : 2017.07.20.
    /// </summary>
    class itellyou
    {
        public void run()
        {
            ServicePointManager.ServerCertificateValidationCallback = new RemoteCertificateValidationCallback(CheckValidationResult);
            Dictionary<string, string> cateList = new Dictionary<string, string>();

            /*
            * 以下id都是从https://msdn.itellyou.cn/主页返回的数据中提取的
            * 因为这些分类应该不会再添加了
            * 所以这里就不解析主页数据
            * 而是直接写死在代码里
            **/
            cateList.Add("企业解决方案",    "aff8a80f-2dee-4bba-80ec-611ac56d3849");
            cateList.Add("MSDN 技术资源库", "23958de6-bedb-4998-825c-aa3d1e00d097");
            cateList.Add("工具和资源",      "95c4acfd-d1a6-41fe-b14d-a6816973d2aa");
            cateList.Add("应用程序",        "051d75ee-ff53-43fe-80e9-bac5c10fc0fb");
            cateList.Add("开发人员工具",    "fcf12b78-0662-4dd4-9a82-72040db91c9e");
            cateList.Add("操作系统",        "7ab5f0cb-7607-4bbe-9e88-50716dc43de6");
            cateList.Add("服务器",          "36d3766e-0efb-491e-961b-d1a419e06c68");
            cateList.Add("设计人员工具",    "5d6967f0-b58d-4385-8769-b886bfc2b78c");

            foreach (var cate in cateList)
            {
                string cateName = cate.Key;
                string cateId = cate.Value;

                ArrayList seriesList = getSeriesList(cateId);

                foreach (var seriesInfo in seriesList)
                {
                    Hashtable t = seriesInfo as Hashtable;
                    string seriesName = t["name"] as string;
                    string seriesId = t["id"] as string;

                    Hashtable langTable = getLangList(seriesId);
                    ArrayList langList = langTable["result"] as ArrayList;
                    foreach (var langInfo in langList)
                    {
                        Hashtable tt = langInfo as Hashtable;
                        string langId = tt["id"] as string;
                        string langName = tt["lang"] as string;

                        Hashtable result = getProductList(seriesId, langId);
                        ArrayList productList = result["result"] as ArrayList;
                        foreach (var productInfo in productList)
                        {
                            Hashtable ttt = productInfo as Hashtable;
                            string productId = ttt["id"] as string;
                            string productName = ttt["name"] as string;

                            Hashtable detailResult = getProductDetail(productId);
                            Hashtable detail = detailResult["result"] as Hashtable;
                            string url = detail["DownLoad"] as string;
                            string filename = detail["FileName"] as string;
                            string date = detail["PostDateString"] as string;
                            string sha1 = detail["SHA1"] as string;
                            string size = detail["size"] as string;

                            // 直接保存到当前运行的目录
                            string dir = cateName + "/" + seriesName + "/" + langName;
                            string path = dir + "/" + filename + ".txt";

                            if (Directory.Exists(dir) == false)
                                Directory.CreateDirectory(dir);

                            // 输出到文件
                            FileStream f = File.Open(path, FileMode.Create);
                            StreamWriter w = new StreamWriter(f);
                            w.WriteLine("文件名    = " + filename);
                            w.WriteLine("SHA1      = " + sha1);
                            w.WriteLine("文件大小  = " + size);
                            w.WriteLine("发布时间  = " + date);
                            w.WriteLine("下载链接  = " + url);
                            w.Close();
                            f.Close();

                            // 输出到控制台
                            Console.WriteLine("文件名    = " + filename);
                            Console.WriteLine("SHA1      = " + sha1);
                            Console.WriteLine("文件大小  = " + size);
                            Console.WriteLine("发布时间  = " + date);
                            Console.WriteLine("下载链接  = " + url);
                            Console.WriteLine();
                        }
                    }
                }
            }

        }
        /// <summary>
        /// 获取产品系列列表
        /// </summary>
        /// <param name="cateId">分类id</param>
        /// <returns>win7、win8、win10...</returns>
        private ArrayList getSeriesList(string cateId)
        {
            string text = httpPost("https://msdn.itellyou.cn/Category/Index", string.Format("id={0}", cateId));
            return MiniJsonExtensions.arrayListFromJson(text);
        }

        /// <summary>
        /// 获取产品系列的语言列表
        /// </summary>
        /// <param name="typeId">资源类型id</param>
        /// <returns>多国语言、英语、中文 - 简体、中文 - 繁体...</returns>
        private Hashtable getLangList(string typeId)
        {
            string text = httpPost("https://msdn.itellyou.cn/Category/GetLang", string.Format("id={0}", typeId));
            return MiniJsonExtensions.hashtableFromJson(text);
        }

        /// <summary>
        /// 获取产品列表
        /// </summary>
        /// <param name="typeId"></param>
        /// <param name="lang"></param>
        /// <returns></returns>
        private Hashtable getProductList(string typeId, string lang)
        {
            string text = httpPost("https://msdn.itellyou.cn/Category/GetList", string.Format("id={0}&lang={1}&filter=true", typeId, lang));
            return MiniJsonExtensions.hashtableFromJson(text);
        }

        /// <summary>
        /// 获取产品详细信息
        /// </summary>
        /// <param name="productId"></param>
        /// <returns></returns>
        private Hashtable getProductDetail(string productId)
        {
            string text = httpPost("https://msdn.itellyou.cn/Category/GetProduct", string.Format("id={0}", productId));
            return MiniJsonExtensions.hashtableFromJson(text);
        }

        /// <summary>
        /// http POST请求
        /// </summary>
        /// <param name="url">url</param>
        /// <param name="form">表单内容</param>
        /// <returns></returns>
        private string httpPost(string url, string form)
        {
            try
            {
                HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
                request.Method = "POST";
                request.CookieContainer = new CookieContainer();
                request.Referer = "https://msdn.itellyou.cn/";
                request.UserAgent = XConst.UserAgent;
                request.ContentType = "application/x-www-form-urlencoded; charset=UTF-8";
                Stream formStream = request.GetRequestStream();
                StreamWriter formStreamWriter = new StreamWriter(formStream);
                formStreamWriter.Write(form);
                formStreamWriter.Close();
                formStream.Close();

                HttpWebResponse response = request.GetResponse() as HttpWebResponse;
                var responseStream = response.GetResponseStream();
                StreamReader r = new StreamReader(responseStream);
                string text = r.ReadToEnd();
                r.Close();
                responseStream.Close();
                return text;
            }
            catch (Exception except)
            {
                return null;
            }
        }

        private static bool CheckValidationResult(object sender, X509Certificate certificate, X509Chain chain, SslPolicyErrors errors)
        {
            return true; //总是接受  
        }
    }
}
