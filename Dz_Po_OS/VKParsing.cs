using System.Collections.Generic;
using System.Linq;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Windows;
using System.IO;
using System.Threading;
//using Newtonsoft.Json;

namespace Dz_Po_OS
{   
    class VKParsing
    {
        public enum SerType {Text,Image,Refer }
        private ChromeDriver chrome;
        public string login, password;
        //public string textpost,ReferOnPost;
        private List<IWebElement> Feedrow;
        //public List<IWebElement> wallposts;


        public VKParsing()
        {
            this.Feedrow = new List<IWebElement>();                                                                                                                                                                                                                
        }
        private void TryToChoose(ref List<IWebElement> fd)
        {
            try
            {
                fd = (from item in chrome.FindElementsByClassName("post") where item.Displayed select item).ToList();
            }
            catch(StaleElementReferenceException)
            {

            }
            
        }
        private void FillTheForm(string FormName, string FormText)
        {
            List<IWebElement> webs = (from item in chrome.FindElementsByName(FormName) where item.Displayed select item).ToList();
            if (!webs.Any())
            {
                return;
            }
            webs[0].Clear();
            webs[0].SendKeys(FormText);
        }
        private void ClickTheButton(string ButtonId)
        {
            List<IWebElement> webs = (from item in chrome.FindElementsById(ButtonId) where item.Displayed select item).ToList();
            if (!webs.Any())
            {
                return;
            }
            webs[0].Click();
        }
        public void OpenWeb()
        {
            try
            {
                this.chrome = new ChromeDriver();
                chrome.Navigate().GoToUrl("https://vk.com/feed");
            }
            catch (DriverServiceNotFoundException)
            {
                MessageBox.Show("В папке отсутствует chromedriver.exe!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public void Authorize()
        {
            try
            {
                FillTheForm("email",login);

                FillTheForm("pass",password);

                ClickTheButton("login_button");
            }
            catch
            {
                MessageBox.Show("Произошла ошибка авторизации!", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Error);                
            }
            

        }//на каждый пост один id
        public  void Serialization( string path,SerType st)
        {
            try
            {
                string id;
                void Serialize(string media)
                {

                    if (media != "")
                    {   //string[] written
                        string[] filejson = new string[2];
                        filejson[0] = id; filejson[1] = media;
                        var options = new JsonSerializerOptions
                        {
                            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                            //WriteIndented = true
                        };
                        using (StreamWriter fs = new StreamWriter(new FileStream(path, FileMode.Append)))
                        {
                            fs.WriteLine(JsonSerializer.Serialize(filejson, options));
                            fs.Close();
                        }
                    }
                }
                //Выбираем,что сериализуем (Текст, ссылка, изображение)
                switch (st)
                {
                    case SerType.Text:
                        for (int i = 0; i < Feedrow.Count(); i++)
                        {
                            id = (Feedrow[i].GetAttribute("id") != null) ? Feedrow[i].GetAttribute("id") : "";
                            string textpost = (Feedrow[i].FindElements(By.ClassName("wall_post_text")).Any()) ? Feedrow[i].FindElements(By.ClassName("wall_post_text"))[0].Text : "";

                            Serialize(textpost);

                        }
                        break;

                    case SerType.Refer:
                        for (int i = 0; i < Feedrow.Count(); i++)
                        {
                            id = (Feedrow[i].GetAttribute("id") != null) ? Feedrow[i].GetAttribute("id") : "";
                            //string id = (Feedrow[i].GetAttribute("id") != null) ? Feedrow[i].GetAttribute("id") : "";
                            string ReferOnPost = (Feedrow[i].FindElements(By.ClassName("post_link")).Any()) ? Feedrow[i].FindElements(By.ClassName("post_link"))[0].GetAttribute("href") : "";

                            Serialize(ReferOnPost);
                            //MessageBox.Show("Поток проверил и мб записал ссылку на пост № " + Convert.ToString(i + 1));

                        }
                        break;

                    case SerType.Image:
                        for (int i = 0; i < Feedrow.Count(); i++)
                        {
                            id = (Feedrow[i].GetAttribute("id") != null) ? Feedrow[i].GetAttribute("id") : "";
                            List<IWebElement> media = (Feedrow[i].FindElements(By.ClassName("image_cover"))).ToList();
                            if (media.Any())
                            {
                                Regex Input = new Regex(@"background-image: url(\S*)", RegexOptions.Compiled);
                                Match match;

                                List<string> filejson = new List<string>();//Сложные построения с регулярными выражениями и выделением ссылок на изображения
                                filejson.Add(id);
                                foreach (IWebElement item in media)
                                {
                                    match = Input.Match(item.GetAttribute("style"));
                                    string[] f = match.Value.Split(')', '(');
                                    if (f.Count() > 1)
                                    {
                                        filejson.Add(f[1].Trim('"'));
                                    }
                                }



                                var options = new JsonSerializerOptions
                                {
                                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                                    //WriteIndented = true
                                };

                                using (StreamWriter fs = new StreamWriter(new FileStream(path, FileMode.Append)))
                                {
                                    if (filejson.Count() > 1)
                                    {
                                        filejson[1] = filejson[1].TrimEnd(',');
                                    }
                                    fs.WriteLine(JsonSerializer.Serialize(filejson, options));
                                    fs.Close();
                                }
                            }
                        }
                        break;
                    default:
                        break;
                }
                MessageBox.Show("запись в файлы закончена!");
            }
            catch(StaleElementReferenceException)
            {
                MessageBox.Show("Ошмбка обработки поста, пожалуйста, повторите позже", "Ошибка!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
                
            
        }
        
        public void ExtraThread()
        {
            for (int i = 0; i < Feedrow.Count(); i++)
            {
                switch (i % 4)
                {   
                    case 0:
                        break;
                    case 1:
                        ReadFile("TextMediaVK.json");
                        
                        break;
                    case 2:
                        ReadFile("ImageMediaVK.json");
                        break;

                    case 3:
                        ReadFile("ReferOnPostVk.json");
                        break;
                }
            }
            MessageBox.Show("Чтение всех файлов завершено");
        }

        public void ReadFile(string path)
        {
            
            if (File.Exists(path))
            {
                using (FileStream fs = new FileStream(path, FileMode.Open))
                {
                    //MessageBox.Show("Поток прочитал "+path);
                    //MessageBox.Show("Поток прочитал " + Convert.ToString(++readings) + " раз");
                    fs.Close();
                }
            }
            else
            {
                MessageBox.Show("chill,dude");
            }
        }


        public void ReadPost(int count)
        {
            List<IWebElement> srcFeedrow = (from item in chrome.FindElementsByClassName("post") where item.Displayed select item).ToList();
            while (count> srcFeedrow.Count)
            {
                //class flat_button secondary_dark more_link    id wall_more_link
                srcFeedrow = chrome.FindElementsById("show_more_link").ToList();//ищем кнопку с увеличением постов
                if (!srcFeedrow.Any()) { srcFeedrow = chrome.FindElementsById("wall_more_link").ToList(); }//если для бесед, то этот вариант

                while (srcFeedrow.Any()) 
                {
                    try
                    {
                        srcFeedrow[0].Click();
                        break;
                    }
                    catch (ElementClickInterceptedException)
                    {
                        Thread.Sleep(100);
                    }
                    catch(ElementNotInteractableException)
                    {
                        //MessageBox.Show("Достигнут конец ленты");
                        srcFeedrow = new List<IWebElement>();//обнуляем массив
                        break;
                    }

                }
                
                //если такой кнопки нет и постов меньше назначенного, переходим к следующему шагу
                if (!srcFeedrow.Any())
                {
                    TryToChoose(ref srcFeedrow);
                        //srcFeedrow = (from item in chrome.FindElementsByClassName("post") where item.Displayed select item).ToList();
                        break;

                }
                TryToChoose(ref srcFeedrow);
                //StaleElementReferenceException

            }

            foreach (IWebElement item in srcFeedrow)
            {


                if (item.Displayed && Feedrow.Count <count)
                {
                    Feedrow.Add(item);
                    //MessageBox.Show("Поток считал пост № " + Convert.ToString(++Cnt));
                }




            }
        }

      
        public void Fresh()
        {
            chrome.Navigate().Refresh();
        }

    }
}
