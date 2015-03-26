using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Storage;
using SQLite;
using Windows.UI.Popups;


// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkID=390556

namespace BookMyShowTasks_Ravindra
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class Placeinfo : Page
    {
        public Placeinfo()
        {
            this.InitializeComponent();
        }
        
        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected async override void OnNavigatedTo(NavigationEventArgs e)
        {
            FinalData roj = (FinalData)e.Parameter;
            nameblk.Text = roj.Name;
            addressblk.Text = roj.Address;
            StorageFile file123 = await StorageFile.GetFileFromPathAsync(roj.Path);
            
            using (IRandomAccessStream str = await file123.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapImage bmp = new BitmapImage();
                bmp.SetSource(str);
                img.Source = bmp;

            }
            var path = ApplicationData.Current.LocalFolder.Path + "/mydatabase.db";
            var con = new SQLiteAsyncConnection(path);
            await con.CreateTableAsync<Favorites>();
        }

        private async void favbutton_Click(object sender, RoutedEventArgs e)
        {
            var path = ApplicationData.Current.LocalFolder.Path + "/mydatabase.db";
            var con = new SQLiteAsyncConnection(path);
            Favorites f = new Favorites();
            f.Name = nameblk.Text;
            f.Address = addressblk.Text;
            await con.InsertAsync(f);
            MessageDialog m = new MessageDialog("Added to favorites");
            m.ShowAsync();
        }

        private async void viewfavs_Click(object sender, RoutedEventArgs e)
        {
            var path = ApplicationData.Current.LocalFolder.Path + "/mydatabase.db";
            var con = new SQLiteAsyncConnection(path);
            List<Favorites> mylist = await con.QueryAsync<Favorites>("select * from Favorites");
            favoritesview.ItemsSource = mylist;
        }
    }
}
