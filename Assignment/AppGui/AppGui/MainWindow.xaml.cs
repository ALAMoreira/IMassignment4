using System;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Xml.Linq;
using System.Xml.XPath;
using mmisharp;
using Newtonsoft.Json;
using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using Microsoft.Speech.Synthesis;
using multimodal;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;

namespace AppGui
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private MmiCommunication mmiC;

        public TimeSpan MyDefaultTimeout { get; private set; }
        private Tts t;
        private ChromeDriver driver;
        private string[] commandsNotUnderstand = new string[5];
        private Random rnd = new Random();
        private Thread thread;
        private string direcao;

        private MmiEventArgs e;

        public MainWindow()
        {

            driver = new ChromeDriver();

            thread = new Thread(scrollSmooth);
            thread.Start();


            openUberEatsChrome(driver);

            mmiC = new MmiCommunication("localhost", 8000, "User1", "GUI");
            mmiC.Message += MmiC_Message;

            mmiC.Start();


            Console.WriteLine("\nbefore driver...");

            Console.WriteLine("after driver function...\n");

        }

        private void MmiC_Message(object sender, MmiEventArgs e)
        {
            var doc = XDocument.Parse(e.Message);

            var com = doc.Descendants("command").FirstOrDefault().Value;
            dynamic json = JsonConvert.DeserializeObject(com);


            var confirmation = (string)json.recognized[1].ToString();
            double confidence = double.Parse(json.recognized[2].ToString());

            switch (confirmation) //confimation
            {
                case "esvaziarC":
                    try
                    {
                        esvaziarCarrinho();
                    }
                    catch { }
                    break;
                case "RecuarR":
                    //if (confidence > 0.8)
                    driver.Navigate().Back();
                    break;
                case "AvancarL":
                    //if (confidence > 0.8)
                    driver.Navigate().Forward();
                    break;
                case "StopS":
                    direcao = "parado";
                    break;
                case "ScrollDR":
                    direcao = "baixo";
                    break;
                case "ScrollU":
                    direcao = "cima";
                    break;
                case "verC":
                    try
                    {
                        verCarrinho();
                    }
                    catch { }
                    break;
                case "homePC":
                    homepage();
                    break;
                default:
                    break;
            }
        }

        private void sairAplicacao()
        {
            driver.Close();
            System.Environment.Exit(1);
        }

        private void homepage()
        {
            var str = "https://www.ubereats.com/pt-PT/feed/?d=" + DateTime.Now.ToString("yyyy-M-dd") + "&et=870&pl=JTdCJTIyYWRkcmVzcyUyMiUzQSUyMkRFVEklMjAtJTIwRGVwYXJ0YW1lbnRvJTIwZGUlMjBFbGVjdHIlQzMlQjNuaWNhJTJDJTIwVGVsZWNvbXVuaWNhJUMzJUE3JUMzJUI1ZXMlMjBlJTIwSW5mb3JtJUMzJUExdGljYSUyMiUyQyUyMnJlZmVyZW5jZSUyMiUzQSUyMkNoSUpzVjdhcjZxaUl3MFJidHRlelhxZVI3YyUyMiUyQyUyMnJlZmVyZW5jZVR5cGUlMjIlM0ElMjJnb29nbGVfcGxhY2VzJTIyJTJDJTIybGF0aXR1ZGUlMjIlM0E0MC42MzMxNzMxMDAwMDAwMSUyQyUyMmxvbmdpdHVkZSUyMiUzQS04LjY1OTQ5MzMlN0Q%3D&ps=1&st=840";

            driver.Navigate().GoToUrl(str);

        }

        private void fecharCarrinho()
        {
            driver.FindElementByXPath("//button[count(ancestor::*)=count(//div[text()='O seu pedido']/ancestor::*)]").Click();
        }

        private void verCarrinho()
        {
            driver.FindElementByXPath("//button[@aria-label='checkout']").Click();
        }

        //private static void esvaziarCarrinho(ChromeDriver driver, bool cartClicked) // Remover STATIC e parametro de entrada para deployment!!! STATIC é apenas utilizado para testes sem Kinect
        private void esvaziarCarrinho()
        {
            verCarrinho();

            //IList<IWebElement> itensCarrinho = driver.FindElementsByCssSelector("li[class='al am']"); // !!! CLASS MUDA !!! ARRANJAR ALTERNATIVA
            //IList<IWebElement> itensCarrinho = driver.FindElementsByCssSelector("li[class='ao ap']"); // !!! CLASS MUDA !!! ARRANJAR ALTERNATIVA
            IList<IWebElement> itensCarrinho = driver.FindElementsByXPath("//ul[count(ancestor::*)=count(//div[text()='O seu pedido']/ancestor::*)]/li");
            
            Console.Write("\n\nCOUNT: " + itensCarrinho.Count + "\n\n");
            foreach (IWebElement item in itensCarrinho)
            {
                //var drpCarrinho = driver.FindElementByCssSelector("select[class='b8 b9 cn bb gs cq cs ae aj gt gu gv gw b2']");
                //var drpCarrinho = driver.FindElementByCssSelector("select[class='b5 b6 c1 b8 jm c5 c7 ae aj jn jo jp jq az']");
                //var drpCarrinho = driver.FindElementByCssSelector("select[class='b8 b9 cs bb jo cv cx ae aj dh di dj jp b2']"); // !!! CLASS MUDA !!! ARRANJAR ALTERNATIVA
                //var drpCarrinho = driver.FindElementByCssSelector("select[class='b8 b9 cs bb kx cv cx ae aj fw fx fy ky b2']"); // !!! CLASS MUDA !!! ARRANJAR ALTERNATIVA
                var drpCarrinho = driver.FindElementByXPath("//option[text()='Remover']/parent::select");
                var selectElement = new SelectElement(drpCarrinho);
                selectElement.SelectByValue("0");
            }
            //var selectElement = new SelectElement(drpCarrinho);
            //selectElement.SelectByValue("0");

            //driver.FindElementByCssSelector("button[class='af eh ei ej ek el em ao aq dt b2']").Click();
        }

        private void scrollSmooth()
        {
            while (true)
            {
                if (direcao == "cima")
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0,-1)", "");
                else if (direcao == "baixo")
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0,1)", ""); 
            }
        }

        private void changeDate(ChromeDriver driver)
        {
            WebDriverWait wait;
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            string action = "Entregar agora";
            driver.FindElementByXPath("//parent::*[contains(text(), '" + action + "')]").Click();
            //driver.FindElement(By.CssSelector("button[class='ao aq bi bj bk ah b2'")).Click();
            action = "Agendar para mais tarde";
            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//parent::*[contains(text(), '" + action + "')]")));
            driver.FindElementByXPath("//parent::*[contains(text(), '" + action + "')]").Click();

            // Adicionar switch para opções
            action = "Definir hora de entrega";
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(text(), '" + action + "')]")));
            driver.FindElementByXPath("//button[contains(text(), '" + action + "')]").Click();
            driver.Navigate().Refresh();
        }

        private void foodOptions(ChromeDriver driver, string itemName)
        {
            WebDriverWait wait;
            IWebElement element;

            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), '" + itemName + "')]")));

            element = driver.FindElementByXPath("//*[contains(text(), '" + itemName + "')]");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            System.Threading.Thread.Sleep(500);

            driver.FindElementByXPath("//*[contains(text(), '" + itemName + "')]").Click();
            //driver.FindElementByXPath("//div[contains(text(), '" + itemName + "')]/ancestor::label").Click();
        }

        public void openUberEatsChrome(ChromeDriver driver)
        {
            // Initialize the Chrome Driver

            WebDriverWait wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            // 1. Go to the "UberEats" homepage           

            homepage();


            // 2. Maximize the browser
            driver.Manage().Window.Maximize();

            changeDate(driver);
            driver.Manage().Window.Minimize();
            driver.Manage().Window.Maximize();
            //driver.SwitchTo().Window(driver.WindowHandles.Last());
            //((IJavaScriptExecutor)driver).ExecuteScript("window.blur();");
            //((IJavaScriptExecutor)driver).ExecuteScript("window.focus();");
            
            // 3. Fill shopping cart
            //driver.FindElementByXPath("//parent::*[contains(text(), 'Procurar')]").Click();
            driver.FindElementByXPath("//div[contains(text(), 'Procurar')]/parent::button").Click();

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//input[@placeholder='O que deseja?']")));

            var searchBox = driver.FindElementByXPath("//input[@placeholder='O que deseja?']");
            string place = "(Aveiro Universidade)";
            searchBox.SendKeys("universidade");
            searchBox.SendKeys(Keys.Enter);

            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='bz c4 bx c0 c3']")));
            //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='ds dt du dv dw dx']")));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//div/*[contains(text(), '" + place + "')]")));

            driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//a[contains(text(), 'Entendido')]")));
            driver.FindElementByXPath("//a[contains(text(), 'Entendido')]").Click();


            string[] food = new string[3];
            food[0] = "Signature Classic";
            food[1] = "Chicken Delights";
            food[2] = "Sundae Morango";
            //food[3] = "Batatas";
            //food[4] = "Chicken Bacon";


            string itemName;
            
            for (int i = 0; i < food.Count(); i++)
            {
                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), '" + food[i] + "')]")));

                Console.Write(food[i]);

                driver.FindElementByXPath("//*[contains(text(), '" + food[i] + "')]").Click();
                switch (i) {
                    case 1:
                        itemName = "Sem molho";
                        foodOptions(driver, itemName);
                        break;
                    case 2:
                        itemName = "Media";
                        foodOptions(driver, itemName);
                        break;
                    case 3:
                        itemName = "Media";
                        foodOptions(driver, itemName);
                        itemName = "Sem molho";
                        foodOptions(driver, itemName);
                        break;
                    default:
                        break;
                }

                string action = "Adicionar";

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//parent::*[contains(text(), '" + action + "') and contains(text(), 'ao pedido')]")));

                driver.FindElementByXPath("//parent::*[contains(text(), '" + action + "') and contains(text(), 'ao pedido')]").Click();
            }


            // 4. Close shopping cart
            fecharCarrinho();


            // 5. Homepage
            homepage();

            /*
            // 6. Clear shopping cart
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//button[@aria-label='checkout']")));

            bool cartClicked = false;
            esvaziarCarrinho(driver, cartClicked);*/
        }

    }
}
