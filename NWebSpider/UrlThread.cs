    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.IO;
    using System.Collections;
    using System.Drawing;
using System.Windows.Forms;

    namespace NWebSpider
    {
        class UrlThread
        {
            private int ThreadNum;              //线程数量
            private Thread[] ThreadList;        //线程列表
            private Thread ImageThread;
            private int List_Top,Image_Top;
            private List<string> UrlList=new List<string>();       // Url列表
            private List<string> ImageList=new List<string>();      // Image列表
            
            private bool UrlListLock = true;    //是否可读取

            private static object lockobj = new object();
            private static object lockimg = new object();

            int time = 0;
            public UrlThread(string url,int threadnum)
            {
                UrlList.Clear();                        // 创建
                UrlList.Add(url);
                UrlList.Add("http://edition.cnn.com");
                UrlList.Add("http://www.baidu.com");

                List_Top = 0;                     // 指向队列待操作的首地址
                Image_Top = 0;

                ThreadNum = threadnum;     // 创建多线程
                ThreadList = new Thread[ThreadNum];
                

            }

            // 开启
            public void Start()
            {
                for (int i = 0; i < ThreadNum; i++)
                {
                    ThreadList[i] = new Thread(new ThreadStart(Run));
                    ThreadList[i].Name = i.ToString();
                    ThreadList[i].Start();
                }
                ImageThread = new Thread(new ThreadStart(SearchImage));
                ImageThread.Start();
            }
            // 暂停
            public void Suspend()
            {
                for (int i = 0; i < ThreadList.Count(); i++)
                {
                    ThreadList[i].Suspend();
                }
            }
            // 继续
            public void Resume()
            {
                for (int i = 0; i < ThreadList.Count(); i++)
                {
                    ThreadList[i].Resume();
                }
            }
            //******抓取网页的方法
            private string GetWebContent(string Url)
            {

                string strResult = "";

                try
                {
                    //Lockit();
                    HttpWebRequest request =

                         (HttpWebRequest)WebRequest.Create(Url);

                    //声明一个HttpWebRequest请求

                    request.Timeout = 3000;

                    //设置连接超时时间

                    request.Headers.Set("Pragma", "no-cache");

                    HttpWebResponse response =

                         (HttpWebResponse)request.GetResponse();

                    Stream streamReceive = response.GetResponseStream();

                    Encoding encoding = Encoding.GetEncoding("UTF-8");

                    StreamReader streamReader =

                         new StreamReader(streamReceive, encoding);

                    strResult = streamReader.ReadToEnd();
                    //Unlockit();
                }

                catch
                {

                }

                return strResult;

            }
            private string getHtml(string url)
            {
                try
                {
                    WebClient myWebClient = new WebClient();
                    byte[] myDataBuffer = myWebClient.DownloadData(url);
                    return Encoding.Default.GetString(myDataBuffer);

                }
                catch (Exception)
                {
                    return "";
                }
            }
            //
            bool getUrlformList(out string url)
            {
                Lockit();
                time++;
                if (List_Top < UrlList.Count())
                {
                    url = UrlList[List_Top];
                    List_Top++;
                    if (!url.StartsWith("http://"))
                    {
                        url = "http://" + url;
                    }
                    Unlockit();
                    return true;
                }
                else
                {
                    url = "";
                    Unlockit();
                    return false;
                }

            }
            Hashtable urltable = new Hashtable();
            Hashtable imgtable = new Hashtable();
            void Run()
            {
                string Turl = "";
                bool doit = true;
                while (doit)
                {

                    if (getUrlformList(out Turl))
                    {
                        DateTime dt = DateTime.Now;
                        Lockit();
                        Form1.getInstance().richTextBox1.AppendText("This is " + Thread.CurrentThread.Name + " Thread No." + List_Top.ToString() + ":\r\nUrl:" + Turl + "\r\nNow Time is " + dt.ToString() + "\r\n");
                        Form1.getInstance().label3.Text = "执行数：" + List_Top.ToString();
                        Unlockit();
                        //string srcHtml = getHtml(Turl);
                        string srcHtml = GetWebContent(Turl);
                        string pattern = @"(?<=http://)[\w\.]+[^/]";　//C#正则表达式提取匹配URL的模式      
                        MatchCollection ms = new Regex(pattern).Matches(srcHtml);
                        foreach (Match mat in ms)
                        {
                            string str = mat.ToString();
                            if (str[str.Length - 1] == '\'' || str[str.Length - 1] == '\"')
                            {
                                str = str.Remove(str.Length - 1);
                            }
                            int key = str.GetHashCode();
                            if (!urltable.Contains(key))
                            {
                                if (AddUrl2List(str))
                                {
                                    urltable.Add(key, null);
                                }
                            }
                        }
                        pattern = @"<img\b[^<>]*?\bsrc[\s\t\r\n]*=[\s\t\r\n]*[""']?[\s\t\r\n]*(?<imgUrl>[^\s\t\r\n""'<>]*)[^<>]*?/?[\s\t\r\n]*>";
                        ms = new Regex(pattern).Matches(srcHtml);
                        foreach (Match mat in ms)
                        {
                            string str = mat.Groups["imgUrl"].Value;// ToString();
                            int i=1000;
                            int j=str.IndexOf(".img");
                            if (j > 0 && j < i) i = j;
                            j = str.IndexOf(".gif");
                            if (j > 0 && j < i) i = j;
                            j = str.IndexOf(".jpg");
                            if (j > 0 && j < i) i = j;
                            j = str.IndexOf(".jpeg");
                            if (j > 0 && j+1 < i) i = j+1;
                            if(i+4<str.Length) str = str.Remove(i+4);
                            lock (lockimg)
                            {
                                int key=str.GetHashCode();
                                if (!imgtable.Contains(key))
                                {
                                    imgtable.Add(key,null);
                                    ImageList.Add(str);
                                }
                            }
                        }
                        Thread.Sleep(1);
                    }

                }
            }
            void SearchImage()
            {
                bool doit=true;
                Bitmap bmp;
                byte[] hash=new byte[64];
                while (doit)
                {
                    lock (lockimg)
                    {
                        if (Image_Top < ImageList.Count())
                        {
                            download(ImageList[Image_Top]);
                            Image_Top++;
                            try
                            {
                                bmp = new Bitmap(fullname);
                                Form1.getInstance().pictureBox2.ImageLocation = fullname;
                                if (bmp.Height >= 8 && bmp.Width >= 8)
                                {
                                    bmp = new Bitmap(bmp, 8, 8);

                                    if (ImageClass.MatchHash(ImageClass.ImageHash(bmp), ImageClass.SrcHash) > ImageClass.MatchMax)
                                    {
                                        ImageClass.MatchMax++;
                                        Form1.getInstance().pictureBox3.ImageLocation = fullname;
                                        Form1.getInstance().textBox2.Text = ImageClass.MatchMax.ToString();
                                    }
                                }
                            }
                            catch (Exception)
                            {
                                
                            }
                           
                        }
                        else doit = false;
                    }
                    if (!doit)
                    {
                        Thread.Sleep(10);
                        doit = true;
                    }
                    Thread.Sleep(1);
                }
            }
            string fullname;
            private void download(string url)
            {
                try
                {
                    string extend = url.Substring(url.LastIndexOf("."));
                    string name = Image_Top.ToString();
                    string fileName = name + extend;
                    WebRequest myRequest = WebRequest.Create(url);
                    myRequest.Timeout = 3000;
                    Stream stream = myRequest.GetResponse().GetResponseStream();
                    Byte[] buffer = new byte[256];
                    string path = @"E:\\自动爬图\\";
                    if (Directory.Exists(path))
                    { }
                    else
                    {
                        Directory.CreateDirectory(path);
                    }
                    fullname = fileName = path + fileName;
                    FileStream filestream = new FileStream(fullname, FileMode.Create, FileAccess.Write);
                    int sizeCount = stream.Read(buffer, 0, 256);
                    try
                    {
                        int sum = 0;
                        while (sizeCount > 0)
                        {
                            Application.DoEvents();
                            filestream.Write(buffer, 0, sizeCount);
                            sizeCount = stream.Read(buffer, 0, 256);
                            sum += sizeCount;
                            //this.Text = "已传输" + sum + "字节";
                        }
                    }
                    catch (Exception ex)
                    {

                    }
                    filestream.Close();
                    stream.Close();
                }
                catch (Exception ex)
                {
                }
            }

            bool AddUrl2List(string url)
            {
                try
                {
                    Lockit();
                    UrlList.Add(url);
                    Unlockit();
                    return true;
                }
                catch (Exception)
                {
                    Unlockit();
                    return false;
                }
            }

            void Lockit()
            {
                while (!UrlListLock) ;
                UrlListLock = false;
            }
            void Unlockit()
            {
                UrlListLock = true;
            }
            
        }
    }
