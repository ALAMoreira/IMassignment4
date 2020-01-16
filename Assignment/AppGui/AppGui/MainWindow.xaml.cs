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
        private int velocidade = 1, produtos = 0;

        private bool orderStart = false;
        private bool pedidoGrupo = false, leaving = false, esvaziar = false;
        private string confirmationValue;

        private MmiEventArgs e;

        public MainWindow()
        {

            // Initialize the Chrome Driver
            driver = new ChromeDriver();
            wait = new WebDriverWait(driver, TimeSpan.FromSeconds(15));

            thread = new Thread(scrollSmooth);
            thread.Start();

            t = new Tts();

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

            int sizeJson = com.Split(',').Length;

            var gesto = (string)json.recognized[1].ToString();




            switch (gesto) //confimation
            {
                case "scrollDownRapido":
                    velocidade = 5;
                    direcao = "baixo";
                    break;
                case "scrollDownDevagar":
                    velocidade = 1;
                    direcao = "baixo";
                    break;
                case "scrollUpRapido":
                    velocidade = 5;
                    direcao = "cima";
                    break;
                case "scrollUpDevagar":
                    velocidade = 1;
                    direcao = "cima";
                    break;
                case "esvaziarC":
                    if (orderStart == true)
                    {
                        if (produtos > 0)
                        {
                            verCarrinho();
                            t.Speak("Tem a certeza que deseja apagar o carrinho de compras?");
                            esvaziar = true;
                            try
                            {
                                esvaziarCarrinho();
                            }
                            catch { }
                        }
                        else
                        {
                            t.Speak("O carrinho está vazio");
                        }
                    }
                    break;
                case "RecuarR":
                    driver.Navigate().Back();
                    break;
                case "AvancarL":
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
                    if (produtos > 0)
                    {
                        try
                        {
                            verCarrinho();
                        }
                        catch { }
                        t.Speak("Aqui tem o seu carrinho!");
                    }
                    else
                    {
                        t.Speak("O carrinho está vazio");
                    }
                    break;
                case "homePC":
                    homepage();
                    break;
                default:
                    break;
            }

            confirmationValue = (string)json.recognized[7].ToString();
            if (esvaziar == true)
            {
                switch (confirmationValue) //confimation
                {
                    case "":
                        break;
                    case "sim":
                        esvaziarCarrinho();
                        break;
                    case "nao":
                        t.Speak("O carrinho de compras não foi apagado!");
                        esvaziar = false;
                        break;
                    default:
                        t.Speak("Não percebi");
                        break;
                }
            }

            if (sizeJson > 3) {

                if ((string)json.recognized[0].ToString() == "KEY")
                {
                    orderStart = true;
                    t.Speak("Olá! O que gostaria de fazer?");
                }
                else if ((string)json.recognized[0].ToString() == "EXIT")
                {
                    t.Speak("Tem a certeza que deseja sair?");
                    leaving = true;
                }

                confirmationValue = (string)json.recognized[7].ToString();
                if (leaving == true)
                {
                    switch (confirmationValue) //confimation
                    {
                        case "":
                            break;
                        case "sim":
                            sairAplicacao();
                            break;
                        case "nao":
                            leaving = false;
                            t.Speak("Ok!");
                            break;
                        default:
                            t.Speak("Não percebi");
                            break;
                    }
                }

                if (orderStart)
                {

                    switch ((string)json.recognized[1].ToString())
                    {
                        
                        case "scroll":
                            scrollSmooth();
                            break;
                        case "search":
                            if((string)json.recognized[2].ToString()=="")
                                t.Speak("O que deseja procurar?");
                            break;
                        case "return":
                            homepage();
                            break;

                        case "addtocart":
                            adicionarCarrinho();
                            t.Speak("Adicionado ao carrinho com sucesso!");
                            break;

                        case "changedate":
                            changeDate();

                            t.Speak("Ok, a data de entrega foi alterada!");
                            break;

                        case "viewcart":
                            if (produtos > 0)
                            {
                                verCarrinho();
                                t.Speak("Aqui tem o seu carrinho!");
                            }
                            else
                            {
                                t.Speak("O carrinho está vazio");
                            }
                            break;
                        case "closecart":
                            fecharCarrinho();
                            t.Speak("O carrinho foi fechado com sucesso!");
                            break;
                        case "emptycart":
                            if (produtos > 0)
                            {
                                verCarrinho();
                                t.Speak("Tem a certeza que deseja apagar o carrinho de compras?");
                                esvaziar = true;
                            }
                            else
                            {
                                t.Speak("O carrinho está vazio");
                            }
                            break;
                        default:
                            break;
                    }

                    confirmationValue = (string)json.recognized[7].ToString();
                    if (esvaziar == true)
                    {
                        switch (confirmationValue) //confimation
                        {
                            case "":
                                break;
                            case "sim":
                                esvaziarCarrinho();
                                break;
                            case "nao":
                                t.Speak("O carrinho de compras não foi apagado!");
                                esvaziar = false;
                                break;
                            default:
                                t.Speak("Não percebi");
                                break;
                        }
                    }

                    string place;
                    switch ((string)json.recognized[2].ToString()) //restaurants
                    {
                    case "MCDONALDS":
                        //search mcdonalds
                        driver.FindElementByXPath("//div[contains(text(), 'Procurar')]/parent::button").Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//input[@placeholder='O que deseja?']")));
                        var searchBox = driver.FindElementByXPath("//input[@placeholder='O que deseja?']");
                        for (int i = 0; i < 20; i++)
                        {
                            searchBox.SendKeys(Keys.Backspace);
                        }
                        searchBox.SendKeys("mcdonalds ");

                        switch ((string)json.recognized[3].ToString()) //place
                        {
                            case "UNIVERSIDADE":
                                place = "(Aveiro Universidade)";
                                searchBox.SendKeys("universidade");
                                searchBox.SendKeys(Keys.Enter);

                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//div/*[contains(text(), '" + place + "')]")));
                                driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();

                                if (pedidoGrupo == false)
                                {
                                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//a[contains(text(), 'Entendido')]")));
                                    driver.FindElementByXPath("//a[contains(text(), 'Entendido')]").Click();
                                    pedidoGrupo = true;
                                }
                                break;
                            case "FORUM":
                                place = "(Aveiro Fórum)";
                                searchBox.SendKeys("fórum");
                                searchBox.SendKeys(Keys.Enter);

                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//div/*[contains(text(), '" + place + "')]")));
                                driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();

                                if (pedidoGrupo == false)
                                {
                                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//a[contains(text(), 'Entendido')]")));
                                    driver.FindElementByXPath("//a[contains(text(), 'Entendido')]").Click();
                                    pedidoGrupo = true;
                                }
                                break;
                            case "GLICINIAS":
                                place = "(Aveiro Glicinias)";
                                searchBox.SendKeys("glicinias");
                                searchBox.SendKeys(Keys.Enter);

                                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//div/*[contains(text(), '" + place + "')]")));
                                driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();

                                if (pedidoGrupo == false)
                                {
                                    wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//a[contains(text(), 'Entendido')]")));
                                    driver.FindElementByXPath("//a[contains(text(), 'Entendido')]").Click();
                                    pedidoGrupo = true;
                                }
                                break;
                            case "":
                                searchBox.SendKeys(Keys.Enter);
                                break;
                        }

                        if ((string)json.recognized[3].ToString() == "")
                        {
                            t.Speak("De qual restaurante?");
                        }
                        else
                        {
                            t.Speak("Que produto quer adquirir?");
                        }

                        break;
                    case "MONTADITOS":
                        driver.FindElementByXPath("//div[contains(text(), 'Procurar')]/parent::button").Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//input[@placeholder='O que deseja?']")));
                        searchBox = driver.FindElementByXPath("//input[@placeholder='O que deseja?']");
                        for (int i = 0; i < 20; i++)
                        {
                            searchBox.SendKeys(Keys.Backspace);
                        }
                        searchBox.SendKeys("100 montaditos ");

                        searchBox.SendKeys(Keys.Enter);

                        place = "100 Montaditos";

                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//div/*[contains(text(), '" + place + "')]")));
                        driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();

                        if (pedidoGrupo == false)
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//a[contains(text(), 'Entendido')]")));
                            driver.FindElementByXPath("//a[contains(text(), 'Entendido')]").Click();
                            pedidoGrupo = true;
                        }
                        break;
                    case "PIZZAHUT":
                        driver.FindElementByXPath("//div[contains(text(), 'Procurar')]/parent::button").Click();
                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//input[@placeholder='O que deseja?']")));
                        searchBox = driver.FindElementByXPath("//input[@placeholder='O que deseja?']");
                        for (int i = 0; i < 20; i++)
                        {
                            searchBox.SendKeys(Keys.Backspace);
                        }
                        searchBox.SendKeys("pizza hut ");

                        searchBox.SendKeys(Keys.Enter);

                        place = "Pizza Hut";

                        wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//div/*[contains(text(), '" + place + "')]")));
                        driver.FindElementByXPath("//div/*[contains(text(), '" + place + "')]").Click();

                        if (pedidoGrupo == false)
                        {
                            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementIsVisible(By.XPath("//a[contains(text(), 'Entendido')]")));
                            driver.FindElementByXPath("//a[contains(text(), 'Entendido')]").Click();
                            pedidoGrupo = true;
                        }
                        break;
                    }


                    switch ((string)json.recognized[4].ToString()) //options
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

                    switch ((string)json.recognized[5].ToString()) //food on mcdonalds
                    {
                        case "":
                            break;
                        default:

                            var food = (string)json.recognized[5].ToString();
                            foodRestaurant(food);
                            break;
                    }

                    string itemName = "";
                    switch ((string)json.recognized[6].ToString()) //food options on mcdonalds
                    {
                        case "":
                            break;
                        default:
                            itemName = (string)json.recognized[6].ToString();

                            foodOptions(itemName);
                            break;
                    }
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
                
            }

            searchBox.SendKeys(restaurante);

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

            produtos++;
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
            IList<IWebElement> itensCarrinho = driver.FindElementsByXPath("//ul[count(ancestor::*)=count(//div[text()='O seu pedido']/ancestor::*)]/li");
            
            Console.Write("\n\nCOUNT: " + itensCarrinho.Count + "\n\n");
            foreach (IWebElement item in itensCarrinho)
            {
                var drpCarrinho = driver.FindElementByXPath("//option[text()='Remover']/parent::select");
                var selectElement = new SelectElement(drpCarrinho);
                selectElement.SelectByValue("0");
            }

            t.Speak("Carrinho de compras apagado com sucesso!");

            produtos = 0;
        }

        private void scrollSmooth()
        {
            while (true)
            {
                if (direcao == "cima")
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0,-"+ velocidade + ")", "");
                else if (direcao == "baixo")
                    ((IJavaScriptExecutor)driver).ExecuteScript("window.scrollBy(0,"+velocidade+")", ""); 
            }
        }
        

        private void changeDate()
        {
            string action = "Entregar agora";
            driver.FindElementByXPath("//parent::*[contains(text(), '" + action + "')]").Click();

            action = "Agendar para mais tarde";
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
            IWebElement element;

            wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.XPath("//*[contains(text(), '" + itemName + "')]")));

            element = driver.FindElementByXPath("//*[contains(text(), '" + itemName + "')]");
            ((IJavaScriptExecutor)driver).ExecuteScript("arguments[0].scrollIntoView(true);", element);
            System.Threading.Thread.Sleep(500);

            driver.FindElementByXPath("//*[contains(text(), '" + itemName + "')]").Click();
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
        }

    }
}
