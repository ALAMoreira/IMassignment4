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
        private WebDriverWait wait;
        private string[] commandsNotUnderstand = new string[5];
        private Random rnd = new Random();
        private Thread thread;
        private string direcao;

        private bool orderStart = false;
        private bool cartClicked = false, leaving = false;

        private MmiEventArgs e;

        public MainWindow()
        {

            // Initialize the Chrome Driver
            driver = new ChromeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

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
            //double confidence = double.Parse(json.recognized[2].ToString());

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

            // VOZ ----------------------------------------

            if ((string)json.recognized[1].ToString() == "KEY")
            {
                orderStart = true;
                t.Speak("Olá! O que gostaria de fazer?");
            }
            else if ((string)json.recognized[1].ToString() == "EXIT")
            {
                t.Speak("Tem a certeza que deseja sair?");
                leaving = true;
            }

            if (leaving == true)
            {
                confirmation = (string)json.recognized[8].ToString();
                switch (confirmation) //confimation
                {
                    case "":
                        break;
                    case "sim":
                        sairAplicacao();
                        break;
                    case "nao":
                        leaving = false;
                        break;
                    default:
                        t.Speak("Não percebi");
                        break;
                }
            }

            if (orderStart)
            {
                string action;
                switch ((string)json.recognized[2].ToString())
                {

                    case "scroll":
                        scrollSmooth();
                        break;
                    case "search":
                        t.Speak("O que deseja procurar?");
                        break;
                    case "return":
                        homepage();
                        break;

                    case "addtocart":
                        /*action = "Adiciona";
                        driver.FindElementByXPath("//parent::*[contains(text(), '" + action + "') and contains(text(), 'ao pedido')]").Click();
                        */
                        adicionarCarrinho();
                        t.Speak("Adicionado ao carrinho com sucesso!");
                        break;

                    case "changedate":
                        /*action = "Entregar agora";
                        //driver.FindElementByXPath("//parent::*[contains(text(), '" + action + "')]").Click();
                        driver.FindElement(By.CssSelector("button[class='ao aq bi bj bk ah b2'")).Click();
                        action = "Agendar para mais tarde";
                        driver.FindElementByXPath("//parent::*[contains(text(), '" + action + "')]").Click();
                        
                        // Adicionar switch para opções
                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                        action = "Definir hora de entrega";
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//button[contains(text(), '" + action + "')]")));
                        driver.FindElementByXPath("//button[contains(text(), '" + action + "')]").Click();
                        driver.Navigate().Refresh();
                        */
                        changeDate();

                        t.Speak("Ok, a data de entrega foi alterada!");
                        break;

                    case "viewcart":
                        /*//driver.Navigate().Refresh();
                        cartClicked = !cartClicked;
                        if (!cartClicked)
                        {
                            driver.FindElementByXPath("//button[@aria-label='checkout']").Click();
                        }
                        */
                        verCarrinho();
                        t.Speak("Aqui tem o seu carrinho!");
                        break;
                    case "closecart":
                        //driver.FindElementByCssSelector("button[class='af eh ei ej ek el em ao aq dt b2']").Click();
                        fecharCarrinho();
                        t.Speak("O carrinho foi fechado com sucesso!");
                        break;
                    default:
                        break;
                }

                string place, restaurante;
                bool enviar = false, local = false;

                switch ((string)json.recognized[3].ToString()) //restaurants
                {

                    case "MCDONALDS":
                        //search mcdonalds
                        /*driver.FindElementByXPath("//parent::*[contains(text(), 'Procurar')]").Click();
                        var searchBox = driver.FindElementByXPath("//input[@placeholder='O que deseja?']");
                        for (int i = 0; i < 20; i++)
                        {
                            searchBox.SendKeys(Keys.Backspace);
                        }
                        searchBox.SendKeys("mcdonalds ");*/

                        restaurante = "mcdonalds ";
                        enviar = false; local = false;
                        procurar(restaurante, enviar, local);

                        switch ((string)json.recognized[4].ToString()) //place
                        {
                            case "UNIVERSIDADE":
                                place = "(Aveiro Universidade)";
                                /*searchBox.SendKeys("universidade");
                                searchBox.SendKeys(Keys.Enter);*/

                                //t.Speak("Carregue no botão");

                                /*wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='bz c4 bx c0 c3']")));
                                driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();*/
                                enviar = true; local = true;
                                procurar(place, enviar, local);
                                verRestaurante(place);
                                break;
                            case "FORUM":
                                place = "(Aveiro Fórum)";
                                /*searchBox.SendKeys("fórum");
                                searchBox.SendKeys(Keys.Enter);

                                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='bz c4 bx c0 c3']")));
                                driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();
                                */
                                enviar = true; local = true;
                                procurar(place, enviar, local);
                                verRestaurante(place);
                                break;
                            case "GLICINIAS":
                                place = "(Aveiro Glicinias)";
                                /*searchBox.SendKeys("glicinias");
                                searchBox.SendKeys(Keys.Enter);

                                wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='bz c4 bx c0 c3']")));
                                driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();*/
                                enviar = true; local = true;
                                procurar(place, enviar, local);
                                verRestaurante(place);
                                break;
                            case "":
                                place = "";
                                enviar = true; local = true;
                                procurar(place, enviar, local);
                                //tts escolha a opção desejada
                                break;
                        }

                        if ((string)json.recognized[4].ToString() == "")
                        {
                            t.Speak("De qual restaurante?");
                        }
                        else
                        {
                            t.Speak("Que produto quer adquirir?");
                        }

                        break;
                    case "MONTADITOS":
                        /*driver.FindElementByXPath("//parent::*[contains(text(), 'Procurar')]").Click();
                        searchBox = driver.FindElementByXPath("//input[@placeholder='O que deseja?']");
                        for (int i = 0; i < 20; i++)
                        {
                            searchBox.SendKeys(Keys.Backspace);
                        }
                        searchBox.SendKeys("100 montaditos ");

                        searchBox.SendKeys(Keys.Enter);

                        place = "100 Montaditos";

                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='bz c4 bx c0 c3']")));
                        driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();*/
                        place = "100 Montaditos";
                        enviar = true; local = false;
                        procurar(place, enviar, local);
                        verRestaurante(place);
                        break;
                    case "PIZZAHUT":
                        /*driver.FindElementByXPath("//parent::*[contains(text(), 'Procurar')]").Click();
                        searchBox = driver.FindElementByXPath("//input[@placeholder='O que deseja?']");
                        for (int i = 0; i < 20; i++)
                        {
                            searchBox.SendKeys(Keys.Backspace);
                        }
                        searchBox.SendKeys("pizza hut ");

                        searchBox.SendKeys(Keys.Enter);


                        place = "Pizza Hut";

                        wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.CssSelector("div[class='bz c4 bx c0 c3']")));
                        driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();*/

                        place = "Pizza Hut";
                        enviar = true; local = false;
                        procurar(place, enviar, local);
                        verRestaurante(place);
                        break;
                }


                switch ((string)json.recognized[5].ToString()) //options
                {

                    case "":
                        break;
                    case ".":
                        //search mcdonalds
                        //pergunta ao user o que quer?
                        break;
                    case "-":

                        break;
                    case "-.":

                        break;
                }

                switch ((string)json.recognized[6].ToString()) //food on mcdonalds
                {
                    case "":
                        break;
                    default:

                        var food = (string)json.recognized[6].ToString();
                        //wait = new WebDriverWait(driver, TimeSpan.FromSeconds(5));
                        //wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), '" + food + "')]")));

                        //driver.FindElementByXPath("//*[contains(text(), '" + food + "')]").Click();
                        //t.Speak("Deseja alterar o seu pedido?");
                        foodRestaurant(food);
                        break;
                }

                string itemName = "";
                switch ((string)json.recognized[7].ToString()) //food options on mcdonalds
                {
                    case "":
                        break;
                    default:
                        itemName = (string)json.recognized[7].ToString();

                        foodOptions(itemName);
                        break;
                }

            }
    }

        private void procurar(string restaurante, bool enviar, bool local)
        {
            if (local == false)
            {
                driver.FindElementByXPath("//div[contains(text(), 'Procurar')]/parent::button").Click();
            }

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//input[@placeholder='O que deseja?']")));

            var searchBox = driver.FindElementByXPath("//input[@placeholder='O que deseja?']");
            if (enviar == true && local == false)
            {
                for (int i = 0; i < 20; i++)
                {
                    searchBox.SendKeys(Keys.Backspace);
                }
                searchBox.SendKeys(restaurante);
            }

            if (enviar == true)
            {
                searchBox.SendKeys(Keys.Enter);
            }
        }

        private void verRestaurante(String place)
        {
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//div/*[contains(text(), '" + place + "')]")));

            driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//a[contains(text(), 'Entendido')]")));
            driver.FindElementByXPath("//a[contains(text(), 'Entendido')]").Click();
        }

        private void adicionarCarrinho()
        {
            string action = "Adicionar";

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//parent::*[contains(text(), '" + action + "') and contains(text(), 'ao pedido')]")));

            driver.FindElementByXPath("//parent::*[contains(text(), '" + action + "') and contains(text(), 'ao pedido')]").Click();
        }

        private void sairAplicacao()
        {
            t.Speak("Ok, até breve!");
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

        private void esvaziarCarrinho()
        {
            verCarrinho();

            IList<IWebElement> itensCarrinho = driver.FindElementsByXPath("//ul[count(ancestor::*)=count(//div[text()='O seu pedido']/ancestor::*)]/li");
            
            Console.Write("\n\nCOUNT: " + itensCarrinho.Count + "\n\n");
            foreach (IWebElement item in itensCarrinho)
            {
                var drpCarrinho = driver.FindElementByXPath("//option[text()='Remover']/parent::select");
                var selectElement = new SelectElement(drpCarrinho);
                selectElement.SelectByValue("0");
            }
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

        private void changeDate()
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

        private void foodRestaurant(string food)
        {
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), '" + food + "')]")));

            driver.FindElementByXPath("//*[contains(text(), '" + food + "')]").Click();
            t.Speak("Deseja alterar o seu pedido?");
        }

        private void foodOptions(string itemName)
        {
            //WebDriverWait wait;
            IWebElement element;

            //wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), '" + itemName + "')]")));

            element = driver.FindElementByXPath("//*[contains(text(), '" + itemName + "')]");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            System.Threading.Thread.Sleep(500);

            driver.FindElementByXPath("//*[contains(text(), '" + itemName + "')]").Click();
            //driver.FindElementByXPath("//div[contains(text(), '" + itemName + "')]/ancestor::label").Click();
        }

        public void openUberEatsChrome(ChromeDriver driver)
        {

            // 1. Go to the "UberEats" homepage           

            homepage();


            // 2. Maximize the browser
            driver.Manage().Window.Maximize();

            changeDate();
            driver.Manage().Window.Minimize();
            driver.Manage().Window.Maximize();
            //driver.SwitchTo().Window(driver.WindowHandles.Last());
            //((IJavaScriptExecutor)driver).ExecuteScript("window.blur();");
            //((IJavaScriptExecutor)driver).ExecuteScript("window.focus();");
            
            /*
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
            */
            /*
            // 6. Clear shopping cart
            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//button[@aria-label='checkout']")));

            bool cartClicked = false;
            esvaziarCarrinho(driver, cartClicked);*/
        }

    }
}
