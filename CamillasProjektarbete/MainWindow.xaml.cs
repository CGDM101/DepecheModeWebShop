using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace CamillasProjektarbete
{
    public class DepecheModeAlbum
    {
        public string Title;
        public string Description;
        public decimal Price;
        public string Image;
    }

    public class Discount
    {
        public string Code;
        public decimal Percentage;
    }
    public partial class MainWindow : Window
    {

        // globala variabler och objekt:
        TextBox enterDiscountTextbox = new TextBox { Text = "Skriv in eventuell rabattkod här" };
        Button buyButton = new Button { Content = "Köp!" };
        Button clearCartButton = new Button { Content = "Ta bort allt från varukorgen!" };
        Button removeItemFromCartButton = new Button { Content = "Jag vill ta bort en vara" };
        CheckBox saveCartCheckbox = new CheckBox { Content = "Spara varukorg till senare?" };

        string title, description; decimal price; string image;

        string[] imagePaths = { "Images/ss.jpg", "Images/abf.jpg", "Images/cta.jpg", "Images/sgr.jpg", "Images/bc.jpg", "Images/mftm.jpg", "Images/violator.jpg", "Images/sofad.jpg", "Images/ultra.jpg", "Images/exciter.jpg", "Images/Images/pta.jpg", "Images/sotu.jpg", "Images/dm.jpg", "Images/spirit.jpg" }; // borde läsas från textfilen

        TextBlock albumInfoTextblock;
        ListBox cartListbox;
        ListBox assortmentListbox;
        Button addToCartButton;
        Button addDiscountButton;
        TextBlock receiptTextblock;
        Grid grid;

        string userInputDiscount;
        int amount;
        decimal sum; decimal totalDiscount; decimal totalSumInclDiscount;

        public const string ProductFilePath = "Products.csv";
        public const string CartPath = @"C:\Windows\Temp\Cart.csv";
        public const string DiscountPath = "DiscountCodes.csv";

        List<DepecheModeAlbum> albumList = new List<DepecheModeAlbum>();
        public Dictionary<DepecheModeAlbum, int> cart;
        List<string> linesCart;
        List<DepecheModeAlbum> cartList = new List<DepecheModeAlbum>();

        string[] linesDiscount = File.ReadAllLines(DiscountPath);
        string codeFromCsv; decimal percentage;
        List<Discount> discountList = new List<Discount>();
        Discount[] discountArray;

        public MainWindow()
        {
            CultureInfo.CurrentCulture = CultureInfo.InvariantCulture;

            InitializeComponent();
            Start();
        }

        private void Start()
        {
            if (File.Exists(CartPath)) // Om kunden tidigare sparat sin varukorg
            {
                string[] savedCart = File.ReadAllLines(CartPath); // ...läses denna in.
                foreach (string line in savedCart)
                {
                    string[] parts = line.Split(',');
                    title = parts[0];
                    price = decimal.Parse(parts[1]);

                    cartList.Add(new DepecheModeAlbum
                    {
                        Title = title,
                        Price = price
                    });
                }
            }

            // PROBLEM: Bara en bild läses in här! Nu används ImagePaths[] istället för att variera bilderna
            string[] linesProductFile = File.ReadAllLines(ProductFilePath);
            foreach (string line in linesProductFile)
            {
                string[] parts = line.Split(',');
                title = parts[0];
                description = parts[1];
                price = decimal.Parse(parts[2]);
                image = parts[3]; //  ===================== här ska image få sitt värde från csv:n.

                albumList.Add(new DepecheModeAlbum
                {
                    Title = title,
                    Description = description,
                    Price = price,
                    Image = image // ======================= här ska image få sitt värde från csv:n
                });
            }

            foreach (string lineDiscount in linesDiscount)
            {
                string[] parts = lineDiscount.Split(',');
                codeFromCsv = parts[0];
                percentage = decimal.Parse(parts[1]);

                discountList.Add(new Discount // första objektet får percentage 0.1, andra får percentage 0.2
                {
                    Code = codeFromCsv,
                    Percentage = percentage
                });
            }
            discountArray = discountList.ToArray();

            Title = "The DEPECHE MODE Record Shop";
            Width = 1000;
            Height = 600;
            WindowStartupLocation = WindowStartupLocation.CenterScreen;

            // Scrolling
            ScrollViewer root = new ScrollViewer();
            root.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            Content = root;

            grid = new Grid();
            root.Content = grid;
            grid.Margin = new Thickness(5);

            // 4 x 4 rutnät
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            // COLUMN 0
            Label butikLabel = new Label { Content = "Butik", FontSize = 20 };
            grid.Children.Add(butikLabel);
            Grid.SetRow(butikLabel, 0);
            Grid.SetColumn(butikLabel, 0);

            assortmentListbox = new ListBox { MaxHeight = 110 };
            foreach (DepecheModeAlbum item in albumList) { assortmentListbox.Items.Add(item.Title); }
            grid.Children.Add(assortmentListbox);
            Grid.SetRow(assortmentListbox, 1);
            Grid.SetColumn(assortmentListbox, 0);
            assortmentListbox.SelectionChanged += AssortmentListbox_SelectionChanged;

            albumInfoTextblock = new TextBlock { Text = "Varuinfo", TextWrapping = TextWrapping.Wrap, };
            grid.Children.Add(albumInfoTextblock);
            Grid.SetRow(albumInfoTextblock, 3);
            Grid.SetColumn(albumInfoTextblock, 0);

            // COLUMN 1
            addToCartButton = new Button { Content = "Lägg i varukorg!" };
            grid.Children.Add(addToCartButton);
            Grid.SetRow(addToCartButton, 1);
            Grid.SetColumn(addToCartButton, 1);
            addToCartButton.Click += AddToCartButton_Click;

            cartListbox = new ListBox { MaxHeight = 110 };
            foreach (DepecheModeAlbum item in cartList) { cartListbox.Items.Add(item.Title); } // om cartpath finns
            grid.Children.Add(cartListbox);
            Grid.SetRow(cartListbox, 2);
            Grid.SetColumn(cartListbox, 1);

            grid.Children.Add(clearCartButton);
            Grid.SetRow(clearCartButton, 3);
            Grid.SetColumn(clearCartButton, 1);
            clearCartButton.Click += ClearCartButton_Click;

            // COLUMN 2
            grid.Children.Add(enterDiscountTextbox);
            Grid.SetRow(enterDiscountTextbox, 1);
            Grid.SetColumn(enterDiscountTextbox, 2);
            enterDiscountTextbox.TextChanged += EnterDiscountTextbox_TextChanged;

            grid.Children.Add(removeItemFromCartButton);
            Grid.SetRow(removeItemFromCartButton, 2);
            Grid.SetColumn(removeItemFromCartButton, 2);
            removeItemFromCartButton.Click += RemoveItemFromCartButton_Click;

            grid.Children.Add(saveCartCheckbox);
            Grid.SetRow(saveCartCheckbox, 3);
            Grid.SetColumn(saveCartCheckbox, 2);
            saveCartCheckbox.Checked += SaveCartCheckbox_Checked;

            // COLUMN 3
            Label kassaLabel = new Label { Content = "Kassa", FontSize = 20 };
            grid.Children.Add(kassaLabel);
            Grid.SetRow(kassaLabel, 0);
            Grid.SetColumn(kassaLabel, 3);

            addDiscountButton = new Button { Content = "Använd kod!" };
            grid.Children.Add(addDiscountButton);
            Grid.SetRow(addDiscountButton, 1);
            Grid.SetColumn(addDiscountButton, 3);
            addDiscountButton.Click += AddDiscountButton_Click;

            grid.Children.Add(buyButton);
            Grid.SetRow(buyButton, 2);
            Grid.SetColumn(buyButton, 3);
            buyButton.Click += BuyButton_Click;

            receiptTextblock = new TextBlock { Text = "Kvitto / Annan information", TextWrapping = TextWrapping.Wrap };
            grid.Children.Add(receiptTextblock);
            Grid.SetRow(receiptTextblock, 3);
            Grid.SetColumn(receiptTextblock, 3);
        }

        private void AddDiscountButton_Click(object sender, RoutedEventArgs e)
        {
            if (userInputDiscount == discountArray[0].Code)
            {
                foreach (DepecheModeAlbum w in cartList)
                {
                    totalDiscount = w.Price * discountArray[0].Percentage; // procent är 0.1
                    totalSumInclDiscount = w.Price - totalDiscount;
                }
                addDiscountButton.IsEnabled = false;
                receiptTextblock.Text = "Du har valt 10%";
            }
            else if (userInputDiscount == discountArray[1].Code)
            {
                foreach (DepecheModeAlbum q in cartList)
                {
                    totalDiscount = q.Price * discountArray[1].Percentage; // procent är 0.2
                    totalSumInclDiscount = q.Price - totalDiscount;
                }
                addDiscountButton.IsEnabled = false;
                receiptTextblock.Text = "Du har valt 20%";
            }
            else if (userInputDiscount == null) // ingen rabatt
            {
                foreach (DepecheModeAlbum u in cartList)
                {
                    sum = sum + u.Price;
                }
            }
            else
            {
                MessageBox.Show("Detta var en ogiltig kod. Försök igen eller handla utan rabatt!");
                addDiscountButton.IsEnabled = true;
            }
        }

        private void AddToCartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                int indexxx = assortmentListbox.SelectedIndex; // hitta index
                DepecheModeAlbum y = albumList[indexxx]; // kolla upp vilket album detta index representerar

                cartListbox.Items.Add(y.Title + " " + y.Price + "kr.");

                sum += y.Price;
                receiptTextblock.Text = "Summa: " + sum + "kr"; // Visar användaren summan av varukorgen

                cartList.Add(y); // varorna i listboxen läggs till i listobjektet.

                // Valt index ska också in i dictionary:
                for (int i = 0; i < cartList.Count; i++)
                {
                    cart = new Dictionary<DepecheModeAlbum, int>
                    {
                        [y] = amount
                    };

                    // om objektet redan finns ska amount ökas med 1:
                    if (cart.ContainsKey(y))
                    {
                        cart[y] += amount;
                    }
                    else
                    {
                        cart[y] = amount;
                    }
                }

                // Uppdatera textfilen med informationen från dictionary:
                linesCart = new List<string>();
                foreach (KeyValuePair<DepecheModeAlbum, int> pair in cart)
                {
                    y = pair.Key;
                    amount = pair.Value;
                    linesCart.Add(y.Title + "," + amount); // amount blir 0
                }
                File.WriteAllLines(CartPath, linesCart);// här kommer inte alla rader med, bara en? om inte trycker på spara?
            }
            catch (Exception)
            {
                MessageBox.Show("Välj något ur sortimentet först!");
            }
        }

        private void AssortmentListbox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int indexx = assortmentListbox.SelectedIndex;
            DepecheModeAlbum z = albumList[indexx];
            // Visa text-info för aktuellt objekt:
            albumInfoTextblock.Text = z.Title + " är från " + z.Description + " och kostar " + z.Price + "kr.";
            // Visa bild-info för aktuellt objekt:
            string imagePathForListboxItemSelected = "";
            string x = imagePaths[indexx]; // WIP - filvägen borde komma från Products.csv
            imagePathForListboxItemSelected = x;

            ImageSource mySource = new BitmapImage(new Uri(imagePathForListboxItemSelected, UriKind.Relative));
            Image imageForListboxSelected = new Image
            {
                Source = mySource,
                Width = 100,
                Height = 100,
                Stretch = Stretch.UniformToFill,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Margin = new Thickness(5)
            };
            RenderOptions.SetBitmapScalingMode(imageForListboxSelected, BitmapScalingMode.HighQuality);
            grid.Children.Add(imageForListboxSelected);
            Grid.SetRow(imageForListboxSelected, 2);
            Grid.SetColumn(imageForListboxSelected, 0);

            // Se till att kunden kan handla igen efter ett köp, utan att avsluta programmet:
            addDiscountButton.IsEnabled = true;
            removeItemFromCartButton.IsEnabled = true;
            clearCartButton.IsEnabled = true;
            buyButton.IsEnabled = true;
        }

        private void EnterDiscountTextbox_TextChanged(object sender, TextChangedEventArgs e)
        {
            userInputDiscount = enterDiscountTextbox.Text;
        }

        private void SaveCartCheckbox_Checked(object sender, RoutedEventArgs e)
        {
            List<string> linesCart = new List<string>();
            foreach (DepecheModeAlbum o in cartList)
            {
                Title = o.Title;
                linesCart.Add(o.Title + "," + amount);
            }
            File.WriteAllLines(CartPath, linesCart);
            MessageBox.Show("Varukorgen är sparad.");
        }

        private void RemoveItemFromCartButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Välja en item att ta bort:
                int selectedInCartListbox = cartListbox.SelectedIndex;
                DepecheModeAlbum r = cartList[selectedInCartListbox];
                cartList.Remove(r); // uppdaterar listobjektet
                sum -= r.Price;
                cartListbox.Items.RemoveAt(selectedInCartListbox); // uppdaterar GUI.

                receiptTextblock.Text = r.Title + " är borttagen från din varukorg. Summan är minskad med " + r.Price + "kr. Summan är nu: " + sum; // informerar användaren
            }
            catch (Exception)
            {
                MessageBox.Show("Du måste välja något att ta bort!"); // Förhindra programmet att krascha om de inte valt något
            }
        }

        private void BuyButton_Click(object sender, RoutedEventArgs e)
        {
            if (cartListbox.Items.Count == 0)
            {
                MessageBox.Show("Det finns inget i varukorgen!");
            }

            else if (true) // Detta ska bara hända om de lagt till något i varukorgen:
            {
                string s = "";
                foreach (DepecheModeAlbum depeche in cartList)
                {
                    s = depeche.Title;
                }

                receiptTextblock.Text = "Total summa: " + sum + "\nSumma inklusive rabatt: " + totalSumInclDiscount + "\nRabatten i kronor: " + totalDiscount + ", vilket är" + (percentage * 100) + "%s rabatt.  \nDu köpte dessa varor: \n" + s + " - " + amount + " st.";

                addDiscountButton.IsEnabled = false;
                removeItemFromCartButton.IsEnabled = false;
                clearCartButton.IsEnabled = false;
                buyButton.IsEnabled = false;

                // Ta bort allt från varukorgen:
                cartListbox.Items.Clear();
                cartList.Clear();
                sum = 0; totalSumInclDiscount = 0; totalDiscount = 0;
                File.Delete(CartPath);
                //File.WriteAllText(CartPath, "");
            }
        }

        private void ClearCartButton_Click(object sender, RoutedEventArgs e)
        {
            if (cartListbox.Items.Count <= 0)
            {
                MessageBox.Show("Det finns inget i varukorgen!");
            }
            else if (cartListbox.Items.Count >= 1)
            {
                foreach (var item in cartList)
                {
                    sum -= item.Price; // Minska summan så att den till slut blir 0.
                }
                cartList.Clear(); // Därefter tömma listan
                //cart.Clear(); // och tömma dictionary. =========== Kraschar - OM varukorg är sparad OCH programmet återöppnat
                File.Delete(CartPath);

                saveCartCheckbox.IsChecked = false;
                cartListbox.Items.Clear();
                receiptTextblock.Text = "Varukorgen är tom nu!\nKostnad: " + sum + "kr.";
            }
        }

        // Methods needed for unit testing:
        public static List<DepecheModeAlbum> ReadCartFile(string path) 
        {
            List<DepecheModeAlbum> aCart = new List<DepecheModeAlbum>();
            string title; decimal price;
            string[] savedCart = File.ReadAllLines(path);
            foreach (string line in savedCart)
            {
                string[] parts = line.Split(',');
                title = parts[0];
                price = decimal.Parse(parts[1]);

                aCart.Add(new DepecheModeAlbum
                {
                    Title = title,
                    Price = price
                });
            }
            return aCart;
        }
        public static List<DepecheModeAlbum> ReadProductFile(string path) 
        {
            List<DepecheModeAlbum> aList = new List<DepecheModeAlbum>();
            string title, description, image; decimal price;
            string[] linesProductFile = File.ReadAllLines(path);
            foreach (string line in linesProductFile)
            {
                string[] parts = line.Split(',');
                title = parts[0];
                description = parts[1];
                price = decimal.Parse(parts[2]);
                image = parts[3];

                aList.Add(new DepecheModeAlbum
                {
                    Title = title,
                    Description = description,
                    Price = price,
                    Image = image
                });
            }
            return aList;
        }
        public static List<Discount> ReadDiscountFile(string path) 
        {
            List<Discount> dM = new List<Discount>();
            string[] lines = File.ReadAllLines(path);

            foreach (string line in lines)
            {
                string[] parts = line.Split(',');
                string code = parts[0];
                decimal percentage = decimal.Parse(parts[1]);

                dM.Add(new Discount
                {
                    Code = code,
                    Percentage = percentage
                });
            }
            return dM;         
        }
        public static string WriteToCartfile() 
        {
            string path = "EmptyCartfile.csv";
            List<DepecheModeAlbum> listToCart = new List<DepecheModeAlbum> { }; 
            DepecheModeAlbum dm = new DepecheModeAlbum { Title = "101", Description = "1989", Price = 1000, Image = "101.jpg" };
            DepecheModeAlbum ddmm = new DepecheModeAlbum { Title = "Devotional", Description = "1994", Price = 900, Image = "devotional.jpg" };
            listToCart.Add(dm);
            listToCart.Add(ddmm);
            List<string> stringList = new List<string> { };
            foreach (var item in listToCart)
            {
                stringList.Add(item.Title + "," + "1");
            }
            File.WriteAllLines(path, stringList);
            return "cart is saved";
        }
        public static string CalculateSum() 
        {
            decimal sum = 0, sumInclDiscount = 0, percentage = 0.1m;
            List<DepecheModeAlbum> DepecheModeForever = new List<DepecheModeAlbum>();
            DepecheModeForever.Add(new DepecheModeAlbum { Title = "One Night in Paris", Description = "2002", Price = 500, Image = "onip.jpg" });
            DepecheModeForever.Add(new DepecheModeAlbum { Title = "tba", Description = "2022", Price = 250, Image = "-.jpg" });
            foreach (var item in DepecheModeForever)
            {
                sum = sum + item.Price;
                sumInclDiscount = sum - (sum * percentage);
            }
            return "without discount: " + sum + " with discount: " + sumInclDiscount;
        }
    }
}
